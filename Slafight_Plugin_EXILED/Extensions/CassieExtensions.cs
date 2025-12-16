using System;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;

namespace Slafight_Plugin_EXILED.Extensions;

public static class CassieExtensions
{
    public static void CassieTranslated(string words, string translated, bool isBell)
    {
        float waitTime = translated.Length / 8f;
        if (waitTime <= 0f) waitTime = 1f;
        Timing.CallDelayed(0f, () =>
        {
            foreach (Player player in Player.List)
            {
                player.MessageTranslated(words,String.Empty,translated,isBell,false);
            }
        });
    }
}