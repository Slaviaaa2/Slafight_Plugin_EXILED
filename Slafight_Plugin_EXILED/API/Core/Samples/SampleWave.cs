using System.Collections.Generic;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Core.Features;
using Slafight_Plugin_EXILED.API.Core.Structs;

namespace Slafight_Plugin_EXILED.API.Core.Samples;

/// <summary>
/// リスポーンウェーブの書き方の見本です。
///
/// <b>これは <see cref="SpawnSet"/> の派生クラスそのものです。</b>
/// 波専用の基底クラスも、波を指す enum も経由していません。
/// 陣営・重み・割合・アナウンスは、この波が自分で名乗ります。
/// </summary>
/// <remarks>
/// <see cref="SpawnSet.RespawnWeight"/> を 0 より大きくしたことで、
/// この <see cref="SpawnSet"/> は抽選対象の波になります。
/// 逆に <see cref="SampleSpawnSet"/> は 0 のままなので波にはなりません。
/// </remarks>
public sealed class SampleWave : SpawnSet
{
    public override string Name => "Sample Wave";

    public override string Description => "動作確認用のリスポーンウェーブです。";

    public override Faction RespawnFaction => Faction.FoundationStaff;

    public override int RespawnWeight => 0;

    /// <summary>
    /// 待機者の 6 割だけ出します。0 人になることはありません。
    /// </summary>
    public override float RespawnRatio => 0.6f;

    public override (string Cassie, string Subtitle) Announcement(int spawnCount, string unitName) =>
        ("MtfUnit Epsilon 11 Designated Sample HasEntered AllRemaining",
         $"<color=#00b7eb>見本部隊</color>が施設に到着しました。({spawnCount}名)");

    public override IReadOnlyList<SpawnSetRoleDefinition> SpawnRoles =>
    [
        SpawnSetRoleDefinition.Custom<SampleRole>(count: 1, isForced: true),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.NtfPrivate, count: 99),
    ];
}
