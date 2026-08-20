using System;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Structs;

/// <summary>
/// <see cref="SpawnSet"/> に並べる 1 行分の役職定義です。
///
/// カスタム役職は<b>型</b>で指定します。割り当てのたびに新しいインスタンスが作られるので、
/// 同じ役職が複数人に出ても per-player 状態が混ざりません。
/// </summary>
/// <remarks>
/// <paramref name="count"/> が人数上限、<c>Weight</c> が出やすさを兼ねます。
/// 旧実装が別々に持っていた「重み表」と「上限表」がこの 1 行に畳まれています。
/// </remarks>
/// <example>
/// <code>
/// public override IReadOnlyList&lt;SpawnSetRoleDefinition&gt; SpawnRoles =>
/// [
///     SpawnSetRoleDefinition.Custom&lt;ChaosSniper&gt;(count: 2),
///     SpawnSetRoleDefinition.Vanilla(RoleTypeId.ChaosRifleman, count: 99),
///
///     // 出やすさに差を付けたいときは weight を変える
///     SpawnSetRoleDefinition.Custom&lt;Scp096Anger&gt;(count: 1, weight: 0.2f),
/// ];
/// </code>
/// </example>
public readonly struct SpawnSetRoleDefinition
{
    private readonly Func<Player, bool> spawn;

    private SpawnSetRoleDefinition(Func<Player, bool> spawn, int count, bool isForced, float weight)
    {
        this.spawn = spawn;
        Count = count;
        IsForced = isForced;
        Weight = weight;
    }

    /// <summary>
    /// この行を何人まで割り当てるかです。上限の役割も兼ねます。
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// 他の行より先に必ず埋めるかどうかです。
    /// </summary>
    public bool IsForced { get; }

    /// <summary>
    /// 抽選の重みです。既定の 1 なら他の行と等確率。
    /// 0 以下にすると <see cref="IsForced"/> でない限り選ばれません。
    /// </summary>
    public float Weight { get; }

    /// <summary>
    /// バニラ役職を割り当てます。
    /// </summary>
    public static SpawnSetRoleDefinition Vanilla(
        RoleTypeId role,
        int count = 1,
        bool isForced = false,
        float weight = 1f)
    {
        return new SpawnSetRoleDefinition(
            player =>
            {
                player.Role.Set(role);

                return true;
            },
            count,
            isForced,
            weight);
    }

    /// <summary>
    /// カスタム役職を割り当てます。割り当てごとに新しいインスタンスが作られます。
    /// </summary>
    public static SpawnSetRoleDefinition Custom<T>(
        int count = 1,
        bool isForced = false,
        float weight = 1f)
        where T : CustomRole, new()
    {
        return new SpawnSetRoleDefinition(
            player => CustomRole.Spawn<T>(player) is not null,
            count,
            isForced,
            weight);
    }

    /// <summary>
    /// 割り当てを実行します。
    /// </summary>
    public bool SpawnFor(Player player) => spawn is not null && spawn(player);
}
