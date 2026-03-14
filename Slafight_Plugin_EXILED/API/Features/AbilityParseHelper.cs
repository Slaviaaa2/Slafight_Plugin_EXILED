#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.API.Features;

public static class AbilityParseHelper
{
    /// <summary>
    /// 全Abilityの名前リスト（動的取得）
    /// </summary>
    public static IEnumerable<string> GetAllAbilityNames()
    {
        return FindAllAbilityTypes()
            .Select(t => t.Name)
            .Concat(new[] { "sh", "sinkhole", "magicmissile" }); // ショートエイリアスも追加
    }

    /// <summary>
    /// 名前でAbility付与（完全動的・ゼロメンテ！）
    /// .give @me CreateSinkholeAbility 5 3 → CD5秒/3回制限
    /// .give @me sh → Sinkhole（エイリアス対応）
    /// .give @me NewAbility → 新規追加Abilityも即対応
    /// </summary>
    public static bool TryGiveAbility(string id, Player target, float? cooldownOverride = null, int? maxUsesOverride = null)
    {
        var abilityType = ResolveAbilityType(id);
        return abilityType != null && GiveAbilityWithOptionalParams(abilityType, target, cooldownOverride, maxUsesOverride);
    }

    /// <summary>
    /// 名前→Type解決（エイリアス＋動的検索）
    /// </summary>
    private static Type? ResolveAbilityType(string id)
    {
        var lowerId = id.ToLower();

        // エイリアス直接対応
        return lowerId switch
        {
            "sh" or "sinkhole" => typeof(CreateSinkholeAbility),
            "magicmissile" => typeof(MagicMissileAbility),
            "fifth" or "soundoffifth" => typeof(SoundOfFifthAbility),
            "escape" or "allowescape" => typeof(AllowEscapeAbility),
            _ => FindAbilityType(lowerId) // 動的検索
        };
    }

    /// <summary>
    /// Type名でAbilityType検索（アセンブリ走査）
    /// </summary>
    private static Type? FindAbilityType(string name)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = asm.GetTypes()
                .FirstOrDefault(t => t.IsSubclassOf(typeof(AbilityBase)) &&
                                   (t.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                                    t.FullName?.EndsWith("." + name, StringComparison.OrdinalIgnoreCase) == true));
            if (type != null) return type;
        }
        return null;
    }

    /// <summary>
    /// 全AbilityType取得
    /// </summary>
    private static IEnumerable<Type> FindAllAbilityTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(AbilityBase)));
    }

    /// <summary>
    /// Type指定で汎用付与（Activatorで動的インスタンス化）
    /// </summary>
    private static bool GiveAbilityWithOptionalParams(Type abilityType, Player target,
        float? cooldownOverride, int? maxUsesOverride)
    {
        if (!abilityType.IsSubclassOf(typeof(AbilityBase)))
            return false;

        try
        {
            object? instance;

            // コンストラクタ自動判定
            if (cooldownOverride == null && maxUsesOverride == null)
            {
                instance = Activator.CreateInstance(abilityType, target);
            }
            else if (cooldownOverride != null && maxUsesOverride == null)
            {
                instance = Activator.CreateInstance(abilityType, target, cooldownOverride.Value);
            }
            else
            {
                var cd = cooldownOverride ?? 10f;
                var uses = maxUsesOverride ?? -1;
                instance = Activator.CreateInstance(abilityType, target, cd, uses);
            }

            if (instance is AbilityBase ability)
            {
                // AbilityBaseの状態も自動登録
                AbilityBase.GrantAbility(target.Id, abilityType, 
                    cooldownOverride ?? 10f,
                    maxUsesOverride ?? -1); 
                
                target.AddAbility(ability);
                return true;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[AbilityParse] Failed to create {abilityType.Name}: {ex.Message}");
        }

        return false;
    }
}
