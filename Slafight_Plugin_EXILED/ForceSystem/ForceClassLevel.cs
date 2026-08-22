namespace Slafight_Plugin_EXILED.ForceSystem;

/// <summary>
/// 部隊内での階級です。
/// </summary>
/// <remarks>
/// <b>ビットフラグではありません。</b>1 人が同時に 2 つの階級を持つことはないので、
/// 組み合わせを表現する必要がありません。
///
/// <see cref="Alone"/> だけは他と性質が違い、<b>保持する階級ではなく表示上の状態</b>です。
/// 草案が「Alone は TopLead/SubLead の階級状態に影響しない」と定めているため、
/// <see cref="ForceMember.Rank"/> が持つのは <see cref="TopLead"/> /
/// <see cref="SubLead"/> / <see cref="Member"/> の 3 つだけで、
/// <see cref="Alone"/> は <see cref="ForceMember.Level"/> が計算して返します。
/// </remarks>
public enum ForceClassLevel
{
    /// <summary>
    /// 隊長級。本隊を率います。一人になっても本隊であり続けます。
    /// </summary>
    TopLead,

    /// <summary>
    /// 軍曹・補佐官級。分隊編成時のバフを強化します。
    /// </summary>
    SubLead,

    /// <summary>
    /// 部隊を構成する一般的な隊員です。
    /// </summary>
    Member,

    /// <summary>
    /// 隊から外れて行動していて、分隊も組めていない状態です。
    /// </summary>
    /// <remarks>
    /// <see cref="ForceMember.Rank"/> には入りません。表示用の射影としてだけ現れます。
    /// </remarks>
    Alone,
}
