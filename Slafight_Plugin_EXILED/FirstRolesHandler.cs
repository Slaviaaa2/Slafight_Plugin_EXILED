using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED;

public class FirstRolesHandler
{
    public FirstRolesHandler()
    {
        Exiled.Events.Handlers.Server.WaitingForPlayers += RoundLocker;
        Exiled.Events.Handlers.Player.ChangingRole += CancelRoundStartedRole;
        Exiled.Events.Handlers.Server.RoundStarted += SetupRandomRoles;
        Exiled.Events.Handlers.Server.RoundStarted += RoundUnlocker;
    }

    ~FirstRolesHandler()
    {
        Exiled.Events.Handlers.Server.WaitingForPlayers -= RoundLocker;
        Exiled.Events.Handlers.Player.ChangingRole -= CancelRoundStartedRole;
        Exiled.Events.Handlers.Server.RoundStarted -= SetupRandomRoles;
        Exiled.Events.Handlers.Server.RoundStarted -= RoundUnlocker;
    }

    public void RoundLocker()
    {
        Round.IsLocked = true;
    }

    public void CancelRoundStartedRole(ChangingRoleEventArgs ev)
    {
        if (ev.Reason == SpawnReason.RoundStart)
        {
            ev.IsAllowed = false;
        }
    }

    private void AssignRole(Player player, List<object> table, RoleSpawnFlags flags)
    {
        const int maxTries = 20;
        object choice = null;

        for (int i = 0; i < maxTries; i++)
        {
            choice = table.RandomItem();
            if (RoleLimitManager.CanAssign(choice))
                break;

            choice = null;
        }

        if (choice == null)
        {
            Log.Debug($"[FirstRoles] No available role for {player.Nickname} (all limited)");
            return;
        }

        RoleLimitManager.Consume(choice);

        switch (choice)
        {
            case RoleTypeId r:
                player.SetRole(r, flags);
                break;
            case CRoleTypeId cr:
                player.SetRole(cr, flags);
                break;
        }

        Log.Debug($"[FirstRoles] Assigned {choice} to {player.Nickname}");
    }

    private void _LimitChecker()
    {
        RoleLimitManager.ClearAll();

        var nowEvent = Plugin.Singleton.SpecialEventsHandler.EventQueue[0];
        if (nowEvent == SpecialEventType.None)
        {
            RoleLimitManager.SetLimit(CRoleTypeId.ZoneManager, 2);
            RoleLimitManager.SetLimit(CRoleTypeId.FacilityManager, 1);
            RoleLimitManager.SetLimit(CRoleTypeId.EvacuationGuard, 1);
        }

        Log.Debug("[FirstRoles] _LimitChecker called");
    }

    public void SetupRandomRoles()
    {
        Log.Debug("[FirstRoles] SetupRandomRoles called");
        
        _LimitChecker();

        // RoundStartをキャンセルしている前提なので、接続プレイヤー全員を対象
        var players = Player.List.Where(p => p != null && p.IsConnected && p.IsRoleUnassigned()).ToList();
        int playerCount = players.Count;
        if (playerCount == 0)
        {
            Round.IsLocked = false;
            return;
        }

        var shuffledPlayers = players.OrderBy(_ => UnityEngine.Random.value).ToList();

        // ===== 1) SCP人数（5人に1人、最低1人） =====
        int scpCount = Mathf.Max(1, playerCount / 5);

        var scpPlayers   = shuffledPlayers.Take(scpCount).ToList();
        var humanPlayers = shuffledPlayers.Skip(scpCount).ToList();

        // SCP
        foreach (var pl in scpPlayers)
            AssignRole(pl, RoleTables.ScpRoles, RoleSpawnFlags.All);

        // 人間 3人に一人
        for (int i = 0; i < humanPlayers.Count; i++)
        {
            var pl = humanPlayers[i];
            var table = (i % 3) switch
            {
                0 => RoleTables.ClassDRoles,
                1 => RoleTables.ScientistRoles,
                _ => RoleTables.GuardRoles,
            };
            AssignRole(pl, table, RoleSpawnFlags.All);
        }

        Round.IsLocked = false;
    }

    public void RoundUnlocker()
    {
        Timing.CallDelayed(5f, () =>
        {
            Round.IsLocked = false;
        });
    }
}
