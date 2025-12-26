using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using ProjectMER.Commands.Utility;
using ProjectMER.Events.Arguments;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED;

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
    
    public List<Vector3> EscapePoints = new List<Vector3>() { };

    public void SetEscapePoint(SchematicSpawnedEventArgs ev)
    {
        if (ev.Schematic.Name == "EscapePoint")
        {
            Vector3 pos = ev.Schematic.gameObject.transform.position;
            EscapePoints.Add(pos);
            ev.Schematic.Destroy();
        }
    }

    public void SaveItems(Player player)
    {
        var nowPos = player.Position;
        player.DropItems();
        List<Pickup> saveItems = new();
        foreach (var items in Pickup.List)
        {
            if (items.PreviousOwner == player)
            {
                if (Vector3.Distance(nowPos,items.Position) <= 1.05f)
                {
                    saveItems.Add(items);
                }
            }
        }

        Timing.CallDelayed(0.5f, () =>
        {
            var newPos = player.Position + new Vector3(0f,0.15f,0f);
            foreach (var item in saveItems)
            {
                if (item == null) continue;
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
            (CTeam.ClassD, _)
                => new EscapeTargetRole { Vanilla = RoleTypeId.ChaosConscript, Custom = null },

            // Scientists
            (CTeam.Scientists, CTeam.ChaosInsurgency or CTeam.ClassD)
                => new EscapeTargetRole { Vanilla = RoleTypeId.ChaosConscript, Custom = null },
            (CTeam.Scientists, CTeam.Fifthists)
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.FifthistConvert },
            (CTeam.Scientists, _)
                => new EscapeTargetRole { Vanilla = RoleTypeId.NtfSpecialist, Custom = null },

            // Chaos Insurgency
            (CTeam.ChaosInsurgency, CTeam.FoundationForces or CTeam.Scientists or CTeam.Guards)
                => new EscapeTargetRole { Vanilla = RoleTypeId.NtfPrivate, Custom = null },
            (CTeam.ChaosInsurgency, CTeam.Fifthists)
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.FifthistConvert },

            // Foundation Forces (with Guards)
            (CTeam.FoundationForces or CTeam.Guards, CTeam.ChaosInsurgency or CTeam.ClassD)
                => new EscapeTargetRole { Vanilla = RoleTypeId.ChaosConscript, Custom = null },
            (CTeam.FoundationForces or CTeam.Guards, CTeam.Fifthists)
                => new EscapeTargetRole { Vanilla = null, Custom = CRoleTypeId.FifthistConvert },
            
            // Fifthists
            (CTeam.Fifthists, CTeam.ChaosInsurgency or CTeam.ClassD)
                => new EscapeTargetRole { Vanilla = RoleTypeId.ChaosConscript, Custom = null },
            (CTeam.Fifthists, CTeam.FoundationForces or CTeam.Scientists or CTeam.Guards)
                => new EscapeTargetRole { Vanilla = RoleTypeId.NtfPrivate, Custom = null },

            // デフォルト：変身しない
            _ => new EscapeTargetRole { Vanilla = null, Custom = null },
        };
    }
    
    public void Escape(Player player)
    {
        Log.Debug($"Escape Triggered. by: "+player.Nickname+$", CTeam: {player.GetTeam()}");
        
        var myTeam     = player.GetTeam();
        var cufferTeam = player.Cuffer.GetTeam();

        var target = GetEscapeTarget(myTeam, cufferTeam);

        // 何も指定されていないなら変身しない
        if (target.Vanilla is null && target.Custom is null)
            return;

        SaveItems(player);

        if (target.Custom is { } customRole)
        {
            player.SetRole(customRole);
        }
        else if (target.Vanilla is { } vanillaRole)
        {
            player.SetRole(vanillaRole);
        }
    }

    public void AddEscapeCoroutine()
    {
        Timing.RunCoroutine(EscapeCoroutine());
    }

    private IEnumerator<float> EscapeCoroutine()
    {
        for (;;)
        {
            if (Round.IsLobby) yield break;
            //Log.Debug("Coroutine is Alive!");
            foreach (Player player in Player.List)
            {
                if (player == null || !player.IsAlive) continue;
                //Log.Debug("P Foreach is Alive! Player: "+player.Nickname);
                foreach (Vector3 escapePoint in EscapePoints)
                {
                    //Log.Debug("E Foreach is Alive! EscapePoint: "+escapePoint);
                    if (Vector3.Distance(player.Position, escapePoint) <= 1.75f)
                    {
                        //Log.Debug("Trigger!");
                        Escape(player);
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