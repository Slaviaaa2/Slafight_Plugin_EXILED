using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.CustomRoles;

namespace Slafight_Plugin_EXILED.Extensions
{
    public static class RoundUtils
    {
        /// <summary>
        /// 特殊勝利としてラウンドを終了させる。
        /// 呼び出し時に IsSpecialWinEnding を true にし、RoundLock 解除は呼び出し側で行う想定。
        /// </summary>
        public static void EndRound(this CTeam team, string specificReason = null)
        {
            // CustomRolesHandler があれば特殊勝利フラグを立てる
            var handler = Plugin.Singleton?.CustomRolesHandler;
            if (handler != null)
                handler.IsSpecialWinEnding = true;

            switch (team)
            {
                case CTeam.SCPs:
                    Round.KillsByScp = 999;
                    Round.EndRound(true);
                    break;

                case CTeam.Fifthists:
                    Round.KillsByScp = 555;
                    Round.EndRound(true);
                    foreach (Player player in Player.List)
                        player.ShowHint("<b><size=80><color=#ff00fa>第五教会</color>の勝利</size></b>", 555f);
                    break;

                case CTeam.ChaosInsurgency:
                    Round.EscapedDClasses = 999;
                    Round.EndRound(true);
                    break;

                case CTeam.ClassD:
                    Round.EscapedDClasses = 999;
                    Round.EndRound(true);
                    break;

                case CTeam.FoundationForces:
                case CTeam.Scientists:
                    Round.EscapedScientists = 999;
                    Round.EndRound(true);
                    break;

                case CTeam.Others:
                    Round.EscapedDClasses = 999;
                    Round.EndRound(true);
                    if (specificReason == "SW_WIN")
                    {
                        foreach (Player player in Player.List)
                            player.ShowHint("<b><size=80><color=#ffffff>雪の戦士達</color>の勝利</size></b>", 555f);
                    }
                    else
                    {
                        foreach (Player player in Player.List)
                            player.ShowHint("<b><size=80><color=#ffffff>UNKNOWN TEAM</color>の勝利</size></b>", 555f);
                    }
                    break;

                case CTeam.GoC:
                    Round.EscapedDClasses = 999;
                    Round.EndRound(true);
                    if (specificReason == "SavedHumanity")
                    {
                        foreach (var player in Player.List)
                            player.ShowHint("<b><size=80><color=#0000c8>人類</color>の勝利</size></b>", 555f);
                    }
                    else
                    {
                        foreach (var player in Player.List)
                            player.ShowHint("<b><size=80><color=#0000c8>世界オカルト連合</color>の勝利</size></b>", 555f);
                    }
                    break;

                default:
                    Round.EndRound(true);
                    break;
            }
        }
    }
}
