using System;
using Exiled.API.Extensions;
using Exiled.API.Features;

namespace Slafight_Plugin_EXILED.Extensions;

public static class CassieExtensions
{
    public static void CassieTranslated(string words, string translated, bool isBell)
    {
        foreach (Player player in Player.List)
        {
            player.MessageTranslated(words,String.Empty,translated,isBell,false);
        }
    }
}