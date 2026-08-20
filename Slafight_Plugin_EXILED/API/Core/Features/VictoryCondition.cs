using System;
using System.Linq;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// チームの勝利条件です。<see cref="CustomTeam.Victory"/> がこれを返します。
///
/// レジストリにも DefinitionSource にも登録しません。<b>チームが自分で持ちます。</b>
/// </summary>
/// <example>
/// <code>
/// public sealed class ScpTeam : CustomTeam
/// {
///     public override string Name => "SCP";
///     protected override bool IncludesVanilla(Player player) => player.IsScp;
///
///     // 敵対する側が全滅したら勝ち
///     public override VictoryCondition Victory => VictoryCondition.LastStanding(priority: 10);
/// }
/// </code>
/// </example>
public sealed class VictoryCondition
{
    private readonly Func<CustomTeam, bool> isMet;

    private VictoryCondition(Func<CustomTeam, bool> isMet, int priority, string reason)
    {
        this.isMet = isMet;
        Priority = priority;
        Reason = reason;
    }

    /// <summary>
    /// 複数チームが同時に条件を満たしたときの優先度です。大きいほど優先されます。
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// 勝因の表示テキストです。null ならチーム名から組み立てます。
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// 敵対する側に生存者がいなくなったら勝ち、という最も普通の条件です。
    ///
    /// 「敵対する側」は <see cref="CustomTeam.IsSameSide"/> が false を返すチームで、
    /// 自チームと <see cref="CustomTeam.Allies"/> は味方として数えます。
    /// </summary>
    public static VictoryCondition LastStanding(int priority = 0, string reason = null)
    {
        return new VictoryCondition(
            team => team.Members.Any() &&
                    !CustomTeam.All.Any(other => !team.IsSameSide(other) && other.Members.Any()),
            priority,
            reason);
    }

    /// <summary>
    /// 生存に関係なく、独自の条件で勝敗を決めます。
    /// ボス撃破・脱出達成・時間切れなど、<see cref="LastStanding"/> で表せないものに使います。
    /// </summary>
    public static VictoryCondition Custom(Func<CustomTeam, bool> isMet, int priority = 0, string reason = null)
    {
        if (isMet is null)
            throw new ArgumentNullException(nameof(isMet));

        return new VictoryCondition(isMet, priority, reason);
    }

    /// <summary>
    /// 指定したチームがこの条件を満たしているか判定します。
    /// </summary>
    public bool IsMet(CustomTeam team) => team is not null && isMet(team);
}
