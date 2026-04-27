using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;

namespace Slafight_Plugin_EXILED.CustomRoles.Others.SergeyMakarov;

public static class SergeySharedContents
{
    public static IEnumerator<float> SergeySharedCoroutine(Player player)
    {
        while (true)
        {
            // まず player 自体のnull/接続確認
            if (player == null || !player.IsConnected)
                yield break;

            // 生存＆Sergey継続＆ラウンド中か
            if (!player.IsSergeyMarkov() || !Round.InProgress)
            {
                try
                {
                    player.DisableEffect(EffectType.Ghostly);
                }
                catch
                {
                    // ここは握りつぶしでOK
                }
                yield break;
            }

            // ここから先も毎回nullガード
            if (Warhead.IsDetonated)
            {
                player.Kill("施設の呪縛");
                yield break;
            }

            if (!player.IsEffectActive<Ghostly>())
                player.EnableEffect(EffectType.Ghostly);

            var flags = player.Get();
            if (flags != null && !flags.Contains(SpecificFlagType.RPNameDisabled))
                player.TryAddFlag(SpecificFlagType.RPNameDisabled);

            yield return Timing.WaitForSeconds(1f);
        }
    }
}