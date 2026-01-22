using System.Collections.Generic;

namespace Slafight_Plugin_EXILED.API.Features;

public static class AbilityLocalization
{
    private static readonly Dictionary<string, string> JaNames = new()
    {
        ["CreateSinkholeAbility"] = "シンクホール",
        ["MagicMissileAbility"] = "マジックミサイル",
        ["AllowEscapeAbility"] = "腐蝕からの解放",
        ["SoundOfFifthAbility"] = "第五からの音",
    };

    public static string GetDisplayName(string key, Exiled.API.Features.Player player)
    {
        if (JaNames.TryGetValue(key, out var name))
            return name;
        
        // フォールバック
        return key.Replace("Ability", string.Empty);
    }
}