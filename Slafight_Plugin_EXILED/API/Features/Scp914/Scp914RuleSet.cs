#nullable enable
using Scp914;

namespace Slafight_Plugin_EXILED.API.Features.Scp914;

/// <summary>
/// 1 アイテム分のアップグレードテーブル。各 KnobSetting ごとに Rule を割り当てる。
/// null の Setting は「このテーブルで扱わない（別経路 / vanilla fallthrough）」扱い。
/// </summary>
public sealed class Scp914RuleSet
{
    public Scp914Rule? Rough { get; init; }
    public Scp914Rule? Coarse { get; init; }
    public Scp914Rule? OneToOne { get; init; }
    public Scp914Rule? Fine { get; init; }
    public Scp914Rule? VeryFine { get; init; }

    /// <summary>全 Setting を同じルールで埋めるためのセッター。</summary>
    public Scp914Rule? All
    {
        init
        {
            Rough = value;
            Coarse = value;
            OneToOne = value;
            Fine = value;
            VeryFine = value;
        }
    }

    public Scp914Rule? Get(Scp914KnobSetting setting) => setting switch
    {
        Scp914KnobSetting.Rough => Rough,
        Scp914KnobSetting.Coarse => Coarse,
        Scp914KnobSetting.OneToOne => OneToOne,
        Scp914KnobSetting.Fine => Fine,
        Scp914KnobSetting.VeryFine => VeryFine,
        _ => null,
    };
}
