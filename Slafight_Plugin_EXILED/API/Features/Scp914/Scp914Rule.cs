#nullable enable
using System;
using Exiled.CustomItems.API.Features;

namespace Slafight_Plugin_EXILED.API.Features.Scp914;

public enum Scp914RuleKind
{
    /// <summary>入力を消す。何もスポーンしない。</summary>
    Destroy,
    /// <summary>入力をそのまま OutputPosition に残す (self)。</summary>
    Keep,
    /// <summary>vanilla SCP-914 のデフォルト処理に任せる。</summary>
    Passthrough,
    /// <summary>Vanilla ItemType をスポーン。</summary>
    ToVanilla,
    /// <summary>Exiled CustomItem をスポーン。</summary>
    ToCustomItem,
    /// <summary>CItem をスポーン。</summary>
    ToCItem,
    /// <summary>任意の Action を実行（ev.IsAllowed = false）。</summary>
    Custom,
}

/// <summary>
/// 1 つの KnobSetting に対するアップグレードルール。
/// static factory で生成し、<see cref="WithChance"/> / <see cref="Times"/> でチェインできる。
/// </summary>
public readonly struct Scp914Rule
{
    public Scp914RuleKind Kind { get; init; }
    public float Chance { get; init; }
    public int Count { get; init; }

    public ItemType VanillaItem { get; init; }
    public Func<Scp914Context, ItemType>? VanillaSelector { get; init; }

    public Type? CustomItemType { get; init; }
    public Type? CItemType { get; init; }

    public Action<Scp914Context>? CustomAction { get; init; }

    // ==== Static factories ====

    public static Scp914Rule Destroy => new()
    {
        Kind = Scp914RuleKind.Destroy, Chance = 1f, Count = 1,
    };

    public static Scp914Rule Keep => new()
    {
        Kind = Scp914RuleKind.Keep, Chance = 1f, Count = 1,
    };

    public static Scp914Rule Passthrough => new()
    {
        Kind = Scp914RuleKind.Passthrough, Chance = 1f, Count = 1,
    };

    public static Scp914Rule ToVanilla(ItemType type, int count = 1) => new()
    {
        Kind = Scp914RuleKind.ToVanilla, VanillaItem = type, Count = count, Chance = 1f,
    };

    public static Scp914Rule ToVanilla(Func<Scp914Context, ItemType> selector, int count = 1) => new()
    {
        Kind = Scp914RuleKind.ToVanilla, VanillaSelector = selector, Count = count, Chance = 1f,
    };

    public static Scp914Rule ToCustomItem<T>(int count = 1) where T : CustomItem => new()
    {
        Kind = Scp914RuleKind.ToCustomItem, CustomItemType = typeof(T), Count = count, Chance = 1f,
    };

    public static Scp914Rule ToCItem<T>(int count = 1) where T : CItem => new()
    {
        Kind = Scp914RuleKind.ToCItem, CItemType = typeof(T), Count = count, Chance = 1f,
    };

    public static Scp914Rule Custom(Action<Scp914Context> action) => new()
    {
        Kind = Scp914RuleKind.Custom, CustomAction = action, Chance = 1f, Count = 1,
    };

    // ==== Chain modifiers ====

    public Scp914Rule WithChance(float chance) => this with { Chance = chance };
    public Scp914Rule Times(int count) => this with { Count = count };
}
