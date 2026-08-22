using System.Collections.Generic;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Core.Features;
using Slafight_Plugin_EXILED.API.Core.Structs;

namespace Slafight_Plugin_EXILED.SpawnSets.ChaosInsurgents;

/// <summary>
/// カオス・インサージェンシーの予備部隊 (ミニウェーブ) です。
/// </summary>
public sealed class ChaosBackupWaveSet : SpawnSet
{
    /// <inheritdoc />
    public override string Name => "Chaos Insurgency Backup";

    /// <inheritdoc />
    public override string Description => "カオス・インサージェンシーの予備部隊です。";

    /// <inheritdoc />
    public override Faction RespawnFaction => Faction.FoundationEnemy;

    /// <inheritdoc />
    public override bool IsMiniWave => true;

    /// <summary>
    /// master の <c>FoundationEnemyMiniWaveWeights</c> に準拠しています。
    /// </summary>
    public override int RespawnWeight => 100;

    /// <inheritdoc />
    public override float RespawnRatio => 1.0f;

    /// <inheritdoc />
    public override string Theme => "./WaveThemes/_w_chaos.ogg";

    /// <inheritdoc />
    public override (string Cassie, string Subtitle) Announcement(int spawnCount, string unitName) =>
        ($"Attention All personnel . Detected {spawnCount} Chaos Insurgency Forces in Gate A . Please Terminate Them",
         $"全職員に通達。Gate Aに{spawnCount}人の<color=#228b22>カオス・インサージェンシー</color>部隊が検出されました。" +
         "<split>見つけ次第終了してください。");

    /// <summary>
    /// master の <c>GOI_ChaosBackup</c> UnitPack に準拠した構成です。
    /// </summary>
    public override IReadOnlyList<SpawnSetRoleDefinition> SpawnRoles =>
    [
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.ChaosMarauder, count: 2, weight: 1.5f),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.ChaosRifleman, count: 99, weight: 4f),

        // ▼ カスタム役職を実装したらここを開ける (master の GOI_ChaosBackup 相当)
        // SpawnSetRoleDefinition.Custom<ChaosSignal>(count: 1, isForced: true),
        // SpawnSetRoleDefinition.Custom<ChaosPenal>(count: 1),
    ];
}
