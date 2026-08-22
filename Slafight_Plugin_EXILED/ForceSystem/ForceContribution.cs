namespace Slafight_Plugin_EXILED.ForceSystem;

/// <summary>
/// 貢献度の増減幅です。
/// </summary>
/// <remarks>
/// 草案が定める 大 10 / 中 5 / 小 1 をそのまま持ちます。
/// 個々のルールがこの 3 段階のどれを使うかは <see cref="ForceBase"/> 側が決めます。
/// </remarks>
public enum ForceImpact
{
    /// <summary>影響なし。派生システムが特定のルールを無効化するときに使います。</summary>
    None = 0,

    /// <summary>影響：小。</summary>
    Small = 1,

    /// <summary>影響：中。</summary>
    Medium = 5,

    /// <summary>影響：大。</summary>
    Large = 10,
}

/// <summary>
/// 貢献度の出入りをまとめます。
/// </summary>
/// <remarks>
/// バニラの <c>FactionInfluenceManager</c> は<b>陣営単位</b>でしか値を持たず、
/// 誰がどれだけ稼いだかの内訳がありません。草案の
/// 「分隊ごとの貢献したスポーンウェーブポイントを比較する」を実現できないので、
/// こちらで独自に持ちます。
/// </remarks>
public static class ForceContribution
{
    /// <summary>
    /// 貢献度を加えます。負の値なら減点です。
    /// </summary>
    /// <remarks>
    /// <b>貢献度は 0 未満にしません。</b>負のまま持つと、
    /// 昇格の比較で「一番マシな減点者」が選ばれるだけの席になり、
    /// 減点が「昇格しづらくなる」という意味を失います。
    /// </remarks>
    public static void Add(ForceMember member, int amount)
    {
        if (member is null || amount == 0) return;

        member.Contribution = System.Math.Max(0, member.Contribution + amount);
    }

    /// <summary>
    /// 加点します。
    /// </summary>
    public static void Reward(ForceMember member, ForceImpact impact) => Add(member, (int)impact);

    /// <summary>
    /// 減点します。
    /// </summary>
    public static void Penalize(ForceMember member, ForceImpact impact) => Add(member, -(int)impact);

    /// <summary>
    /// 隊全体の貢献度に占める、この隊員の割合です。隊が空なら 0。
    /// </summary>
    /// <remarks>
    /// 草案の「隊全体の貢献度の中での内訳の 70%」がこれです。
    /// </remarks>
    public static float ShareOf(ForceMember member)
    {
        if (member?.Force is not { } force) return 0f;

        int total = force.TotalContribution;

        return total <= 0 ? 0f : (float)member.Contribution / total;
    }
}
