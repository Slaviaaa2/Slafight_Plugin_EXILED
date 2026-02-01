using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;

namespace Slafight_Plugin_EXILED.Extensions;

public static class OnlyChecker
{
    public static bool IsOnlyTeam(this List<Player> checkPlayers, CTeam team, string specificTrigger = null)
    {
        // 生きている判定対象が1人もいないなら「チーム勝利は発生しない」
        if (!checkPlayers.Any())
        {
            Log.Debug($"[IsOnlyTeam] EMPTY list → false");
            return false;
        }

        Log.Debug($"[IsOnlyTeam] Checking {checkPlayers.Count} players for team {team}");
    
        foreach (var player in checkPlayers.ToList())
        {
            if (player == null || !player.IsAlive || player.Role.Type == RoleTypeId.Spectator)
            {
                Log.Debug($"[IsOnlyTeam] SKIP: {player?.Nickname ?? "null"} (dead/spectator)");
                continue;
            }

            Log.Debug($"[IsOnlyTeam] CHECK: {player.Nickname} Team={player.GetTeam()} Custom={player.GetCustomRole()}");

            if (player.GetTeam() == team) continue;

            if (player.Role.Type == RoleTypeId.Tutorial && player.GetCustomRole() == CRoleTypeId.None) continue;
            if (team == CTeam.Fifthists && player.GetCustomRole() == CRoleTypeId.Scp3005) continue;
            if (team == CTeam.Others && specificTrigger == "snow" && player.GetCustomRole() == CRoleTypeId.SnowWarrier) continue;
            if (team == CTeam.GoC && specificTrigger == "humanity" && player.IsHumanitist()) continue;
            if (player.GetCustomRole() == CRoleTypeId.Scp999) continue;

            Log.Debug($"[IsOnlyTeam] BLOCKED by {player.Nickname} (team={player.GetTeam()})");
            return false;
        }

        Log.Debug($"[IsOnlyTeam] PASSED: Only {team} remaining");
        return true;
    }
}