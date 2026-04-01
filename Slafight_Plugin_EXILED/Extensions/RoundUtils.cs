using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.CustomRoles;

namespace Slafight_Plugin_EXILED.Extensions;

public static class RoundUtils
{
    /// <summary>
    /// 特殊勝利としてラウンドを終了させる。
    /// 呼び出し時に IsSpecialWinEnding を true にし、RoundLock 解除は呼び出し側で行う想定。
    /// </summary>
    public static void EndRound(this CTeam team, string specificReason = null)
    {
        CustomRolesHandler.EndRound(team, specificReason);
    }

    public static bool HasSpecificWinMethod(this Player player)
    {
        var info = player.GetRoleInfo();
        if (info is { Vanilla: RoleTypeId.Tutorial, Custom: CRoleTypeId.None }) return false;
        if (!player.IsAlive) return false;
        return player.GetTeam() is not (CTeam.SCPs or CTeam.Null or CTeam.FoundationForces or CTeam.Guards
            or CTeam.Scientists or CTeam.ChaosInsurgency or CTeam.ClassD);
    }
}