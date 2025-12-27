using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.ProximityChat;

namespace Slafight_Plugin_EXILED.API.Features;

public static class AbilityParseHelper
{
    public static IEnumerable<string> GetAllAbilityNames() =>
        new[] { "CreateSinkholeAbility", "MagicMissileAbility" };

    public static bool TryGiveAbility(string id, Player target, float? cooldownOverride, int? maxUsesOverride)
    {
        switch (id.ToLower())
        {
            case "createsinkholeability":
            case "sinkhole":
            case "sh":
                return GiveAbilityWithOptionalParams<CreateSinkholeAbility>(
                    target,
                    cooldownOverride,
                    maxUsesOverride);

            case "magicmissileability":
            case "magicmissile":
                return GiveAbilityWithOptionalParams<MagicMissileAbility>(
                    target,
                    cooldownOverride,
                    maxUsesOverride);

            default:
                return false;
        }
    }

    /// <summary>
    /// AbilityBase の「デフォルト値抽象プロパティ＋nullableコンストラクタ」を前提にした共通ヘルパー。
    /// TAbility 側には以下のコンストラクタがある前提:
    /// - TAbility(Player owner)                             // 完全デフォルト
    /// - TAbility(Player owner, float cooldownSeconds)      // CDだけ上書き
    /// - TAbility(Player owner, float cooldownSeconds, int maxUses) // 両方上書き
    /// </summary>
    private static bool GiveAbilityWithOptionalParams<TAbility>(
        Player target,
        float? cooldownOverride,
        int? maxUsesOverride)
        where TAbility : AbilityBase
    {
        if (cooldownOverride == null && maxUsesOverride == null)
        {
            // TAbility(Player)
            target.AddAbility((TAbility)Activator.CreateInstance(typeof(TAbility), target));
        }
        else if (cooldownOverride != null && maxUsesOverride == null)
        {
            // TAbility(Player, float)
            var cd = cooldownOverride.Value;
            target.AddAbility((TAbility)Activator.CreateInstance(typeof(TAbility), target, cd));
        }
        else
        {
            // TAbility(Player, float, int)
            var cd = cooldownOverride ?? default(float);   // null は来ない前提だが保険
            var uses = maxUsesOverride ?? default(int);
            target.AddAbility((TAbility)Activator.CreateInstance(typeof(TAbility), target, cd, uses));
        }

        return true;
    }
}