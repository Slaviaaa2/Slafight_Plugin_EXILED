using Exiled.API.Features;

namespace Slafight_Plugin_EXILED.ForceSystem;

/// <summary>
/// 部隊システムをプレイヤー側から引くための拡張メソッドです。
/// </summary>
public static class ForceSystemExtensions
{
    /// <summary>
    /// このプレイヤーが属している隊です。無所属なら null。
    /// </summary>
    public static ForceBase GetForce(this Player player) => ForceRegistry.ForceOf(player);

    /// <summary>
    /// このプレイヤーの隊員状態です。まだ一度も隊に入っていなければ null。
    /// </summary>
    public static ForceMember GetForceMember(this Player player) => ForceRegistry.FindMember(player);

    /// <summary>
    /// このプレイヤーの表示上の階級です。
    /// </summary>
    /// <remarks>
    /// 隊員状態が無い (部隊システムの対象外) 場合も <see cref="ForceClassLevel.Alone"/> を返します。
    /// 表示の都合上、無所属と対象外を区別しないためです。
    /// </remarks>
    public static ForceClassLevel GetForceLevel(this Player player) =>
        player.GetForceMember()?.Level ?? ForceClassLevel.Alone;

    /// <summary>
    /// この階級の呼称です。隊が分かっているなら隊に聞き、分からなければ既定を返します。
    /// </summary>
    /// <remarks>
    /// 呼称は隊ごとに変わる (ギャングなら「ボス」) ので、隊があるときは必ずそちらを優先します。
    /// </remarks>
    public static string NameOf(this ForceClassLevel level, ForceBase force) =>
        force?.RankNameOf(level) ?? level.DefaultName();

    /// <summary>
    /// 隊に属していないときに使う既定の呼称です。
    /// </summary>
    public static string DefaultName(this ForceClassLevel level) => level switch
    {
        ForceClassLevel.TopLead => "隊長",
        ForceClassLevel.SubLead => "補佐",
        ForceClassLevel.Member => "隊員",
        _ => "単独行動",
    };

    /// <summary>
    /// このプレイヤーが隊を率いられるかどうか。
    /// </summary>
    /// <remarks>
    /// 「率いられる」= 生きていて、役職優先度を名乗っていること。
    /// TopLead が死んだときの昇格候補を絞るのに使います。
    /// </remarks>
    public static bool IsLeadable(this Player player) =>
        player is { IsAlive: true } && ForceRolePower.Of(player) > 0;

    /// <summary>
    /// 2 人が同じ隊に属しているかどうか。
    /// </summary>
    public static bool IsInSameForce(this Player player, Player other)
    {
        ForceBase force = player.GetForce();

        return force is not null && ReferenceEquals(force, other.GetForce());
    }
}
