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
        foreach (var player in checkPlayers.ToList())
        {
            if (player == null || !player.IsAlive) continue;
            var info = player.GetRoleInfo();
            if (player.GetTeam() == team) continue;
            if (info.Vanilla == RoleTypeId.Tutorial && info.Custom == CRoleTypeId.None) continue;
            if (team == CTeam.Fifthists && player.GetCustomRole() == CRoleTypeId.Scp3005) continue;
            if (team == CTeam.Others && specificTrigger == "snow" && player.GetCustomRole() == CRoleTypeId.SnowWarrier) continue;
            if (team == CTeam.GoC && specificTrigger == "humanity" && player.IsHumanitist()) continue;
            if (player.GetCustomRole() == CRoleTypeId.Scp999) continue;
            return false;
        }
        return true;
    }
}