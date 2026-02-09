using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using ProjectMER.Events.Arguments;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Changes;

public class EscapeHandler
{
    public EscapeHandler()
    {
        ProjectMER.Events.Handlers.Schematic.SchematicSpawned += SetEscapePoint;

        Exiled.Events.Handlers.Player.Escaping += CancelDefaultEscape;
        Exiled.Events.Handlers.Server.RoundStarted += AddEscapeCoroutine;
    }

    ~EscapeHandler()
    {
        ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= SetEscapePoint;

        Exiled.Events.Handlers.Player.Escaping -= CancelDefaultEscape;
        Exiled.Events.Handlers.Server.RoundStarted -= AddEscapeCoroutine;
    }

    // 逃走判定半径の二乗
    private const float EscapeRadius = 1.75f;
    private const float EscapeRadiusSqr = EscapeRadius * EscapeRadius;

    // アイテム拾い直しの半径二乗
    private const float ItemPickupRadius = 1.05f;
    private const float ItemPickupRadiusSqr = ItemPickupRadius * ItemPickupRadius;

    public readonly List<Vector3> EscapePoints = new();

    public void SetEscapePoint(SchematicSpawnedEventArgs ev)
    {
        if (ev.Schematic.Name != "EscapePoint")
            return;

        Vector3 pos = ev.Schematic.gameObject.transform.position;
        EscapePoints.Add(pos);
        ev.Schematic.Destroy();
    }

    public void SaveItems(Player player)
    {
        var nowPos = player.Position;

        // 先にドロップ
        player.DropItems();

        // LINQで候補だけ抽出→リストにして1回だけループ
        var saveItems = Pickup.List
            .Where(p =>
                p != null &&
                p.PreviousOwner == player &&
                (p.Position - nowPos).sqrMagnitude <= ItemPickupRadiusSqr)
            .ToList();

        if (saveItems.Count == 0)
            return;

        Timing.CallDelayed(0.5f, () =>
        {
            if (player == null || !player.IsConnected)
                return;

            var newPos = player.Position + new Vector3(0f, 0.15f, 0f);
            foreach (var item in saveItems)
            {
                if (item == null || !item.IsSpawned)
                    continue;

                item.Position = newPos;
            }
        });
    }

    public struct EscapeTargetRole
    {
        public RoleTypeId? Vanilla;   // 通常ロールに変身したいとき用
        public CRoleTypeId? Custom;   // カスタムロールに変身したいとき用
    }

    private EscapeTargetRole GetEscapeTarget(CTeam myTeam, CTeam cufferTeam)
    {
        return (myTeam, cufferTeam) switch
        {
            // Class-D Personnel
            (CTeam.ClassD, CTeam.FoundationForces or CTeam.Scientists or CTeam.Guards)
                => new EscapeTargetRole { Vanilla = RoleTypeId.NtfPrivate, Custom = null },
            (CTeam.ClassD, CTeam.Fifthists)
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.FifthistConvert },
            (CTeam.ClassD, CTeam.GoC) // ← orより上！
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.GoCOperative },
            (CTeam.ClassD, _)
                => new EscapeTargetRole { Vanilla = RoleTypeId.ChaosConscript, Custom = null },

            // Scientists
            (CTeam.Scientists, CTeam.ChaosInsurgency or CTeam.ClassD)
                => new EscapeTargetRole { Vanilla = RoleTypeId.ChaosConscript, Custom = null },
            (CTeam.Scientists, CTeam.Fifthists)
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.FifthistConvert },
            (CTeam.Scientists, CTeam.GoC)
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.GoCOperative },
            (CTeam.Scientists, _)
                => new EscapeTargetRole { Vanilla = RoleTypeId.NtfSpecialist, Custom = null },

            // Chaos Insurgency
            (CTeam.ChaosInsurgency, CTeam.FoundationForces or CTeam.Scientists or CTeam.Guards)
                => new EscapeTargetRole { Vanilla = RoleTypeId.NtfPrivate, Custom = null },
            (CTeam.ChaosInsurgency, CTeam.Fifthists)
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.FifthistConvert },
            (CTeam.ChaosInsurgency, CTeam.GoC)
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.GoCOperative },

            // Foundation Forces (with Guards)
            (CTeam.FoundationForces or CTeam.Guards, CTeam.ChaosInsurgency or CTeam.ClassD)
                => new EscapeTargetRole { Vanilla = RoleTypeId.ChaosConscript, Custom = null },
            (CTeam.FoundationForces or CTeam.Guards, CTeam.Fifthists)
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.FifthistConvert },
            (CTeam.FoundationForces or CTeam.Guards, CTeam.GoC)
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.GoCOperative },

            // Fifthists
            (CTeam.Fifthists, CTeam.ChaosInsurgency or CTeam.ClassD)
                => new EscapeTargetRole { Vanilla = RoleTypeId.ChaosConscript, Custom = null },
            (CTeam.Fifthists, CTeam.FoundationForces or CTeam.Scientists or CTeam.Guards)
                => new EscapeTargetRole { Vanilla = RoleTypeId.NtfPrivate, Custom = null },
            
            // GoC
            (CTeam.Fifthists, CTeam.GoC)
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.GoCOperative },

            // デフォルト
            _ => new EscapeTargetRole { Vanilla = null, Custom = null },
        };
    }


    private EscapeTargetRole ApplyEventOverrides(
        EscapeTargetRole baseTarget,
        Player player)
    {
        var nowEvent = Plugin.Singleton.SpecialEventsHandler.NowEvent;

        var myRole       = player.Role.Type;          // RoleTypeId
        var myCustomRole = player.GetCustomRole();    // CRoleTypeId
        var cufferRole   = player.Cuffer?.Role.Type;
        var cufferCustom = player.Cuffer?.GetCustomRole();

        switch (nowEvent)
        {
            case SpecialEventType.None:
            default:
                return baseTarget;

            case SpecialEventType.FifthistsRaid:
            {
                if (myCustomRole is CRoleTypeId.Scp3005)
                    return new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.FifthistPriest };
                break;
            }

            // 他イベントもここへ
        }

        return baseTarget;
    }

    private EscapeTargetRole GetEscapeTarget(Player player)
    {
        var myTeam     = player.GetTeam();
        var cufferTeam = player.Cuffer?.GetTeam() ?? CTeam.Null;

        var baseTarget = GetEscapeTarget(myTeam, cufferTeam);

        return ApplyEventOverrides(baseTarget, player);
    }

    public void Escape(Player player)
    {
        Log.Debug($"Escape Triggered. by: {player.Nickname}, CTeam: {player.GetTeam()}");

        var target = GetEscapeTarget(player);

        if (target.Vanilla is null && target.Custom is null)
            return;

        SaveItems(player);

        if (target.Custom is { } customRole)
            player.SetRole(customRole);
        else if (target.Vanilla is { } vanillaRole)
            player.SetRole(vanillaRole);
    }

    public void AddEscapeCoroutine()
    {
        Timing.RunCoroutine(EscapeCoroutine());
    }

    private IEnumerator<float> EscapeCoroutine()
    {
        // 既に脱出処理済みのプレイヤーを記録して、二重Escapeを防ぐ
        var escapedPlayers = new HashSet<Player>();

        for (;;)
        {
            if (Round.IsLobby)
                yield break;

            if (EscapePoints.Count == 0)
            {
                yield return Timing.WaitForSeconds(0.5f);
                continue;
            }

            // Player.ListとEscapePointsをそのまま二重for
            foreach (var player in Player.List)
            {
                if (player == null || !player.IsAlive || escapedPlayers.Contains(player))
                    continue;

                var playerPos = player.Position;

                // EscapePoints側は距離をsqrで見る
                for (int i = 0; i < EscapePoints.Count; i++)
                {
                    var escPos = EscapePoints[i];
                    if ((playerPos - escPos).sqrMagnitude <= EscapeRadiusSqr)
                    {
                        Escape(player);
                        escapedPlayers.Add(player);
                        break; // このプレイヤーはもう判定不要
                    }
                }
            }

            yield return Timing.WaitForSeconds(0.5f);
        }
    }

    public void CancelDefaultEscape(EscapingEventArgs ev)
    {
        ev.IsAllowed = false;
    }
}
