using System.Collections.Generic;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Core.Features;
using Slafight_Plugin_EXILED.API.Core.Structs;

namespace Slafight_Plugin_EXILED.SpawnSets.ChaosInsurgents;

/// <summary>
/// カオス・インサージェンシーの通常波です。
/// </summary>
/// <remarks>
/// 機動部隊側 (<c>NtfWaveSet</c>) との違いは <see cref="RespawnFaction"/> だけです。
/// <see cref="Faction.FoundationEnemy"/> なので、この波が出ると
/// <c>ForceManager</c> はカオスの隊 (<c>ChaosForce</c>) を作ります。
/// カオスの隊は草案の派生システムどおり、SCP-914 の使用が減点対象から外れ、
/// 継続所属時間の影響が「大」ではなく「中」になります。
///
/// カオス側にはバニラの部隊名 (<c>(ALPHA-01)</c>) が存在しないので、
/// 名札には出ません。部隊名は Slafight 側で採番して HUD にだけ出します。
/// 番号はバニラの採番済み数を基点にずらしてあるので、機動部隊の名前と重なりません。
/// </remarks>
public sealed class ChaosWaveSet : SpawnSet
{
    /// <inheritdoc />
    public override string Name => "Chaos Insurgency";

    /// <inheritdoc />
    public override string Description => "カオス・インサージェンシーの通常波です。";

    /// <inheritdoc />
    public override Faction RespawnFaction => Faction.FoundationEnemy;

    /// <summary>
    /// master の <c>FoundationEnemyWaveWeights</c> に準拠しています。
    /// </summary>
    /// <remarks>
    /// 旧実装はカオス 100 / 第五主義者 0 でした。第五主義者はカスタム役職が要るのでまだ足していません。
    /// </remarks>
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
    /// master の <c>GOI_ChaosNormal</c> UnitPack に準拠した構成です。
    /// </summary>
    public override IReadOnlyList<SpawnSetRoleDefinition> SpawnRoles =>
    [
        // 重機関銃手が隊を率いる。部隊システムではこの役職が TopLead になる。
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.ChaosRepressor, count: 2, weight: 1.5f),

        SpawnSetRoleDefinition.Vanilla(RoleTypeId.ChaosMarauder, count: 2, weight: 1.5f),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.ChaosRifleman, count: 99, weight: 4f),

        // ▼ カスタム役職を実装したらここを開ける (master の GOI_ChaosNormal 相当)
        // SpawnSetRoleDefinition.Custom<ChaosCommando>(count: 1),
        // SpawnSetRoleDefinition.Custom<ChaosSignal>(count: 2, weight: 2f),
        // SpawnSetRoleDefinition.Custom<ChaosTacticalUnit>(count: 2, weight: 2f),
        // SpawnSetRoleDefinition.Custom<ChaosPenal>(count: 2, weight: 2f),
        // SpawnSetRoleDefinition.Custom<ChaosSniper>(count: 2, weight: 2f),
    ];
}
