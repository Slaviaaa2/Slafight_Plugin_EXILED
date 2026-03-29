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
}