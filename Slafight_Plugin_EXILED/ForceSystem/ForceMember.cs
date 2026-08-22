using Exiled.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.ForceSystem;

/// <summary>
/// 部隊に属する 1 人ぶんの状態です。
/// </summary>
/// <remarks>
/// キーは <see cref="Player"/> ではなく <see cref="NetId"/> です。
/// AGENTS.md が「破棄され得るオブジェクトを遅延辞書のキーにせず、安定した netId を使い、
/// コールバック実行時に同一性を確認すること」を求めているためです。
/// <see cref="IsAlive"/> がその確認にあたり、再接続で別人が同じ枠に入っても取り違えません。
/// </remarks>
public sealed class ForceMember
{
    internal ForceMember(Player player)
    {
        Player = player;
        NetId = player.GetNetId();
        JoinedAt = Time.time;
        AloneSince = Time.time;
    }

    /// <summary>
    /// 本人です。<see cref="IsAlive"/> が false なら信用してはいけません。
    /// </summary>
    public Player Player { get; }

    /// <summary>
    /// 本人を指す安定したキーです。
    /// </summary>
    public uint NetId { get; }

    /// <summary>
    /// 現在所属している隊です。どこにも属していなければ null。
    /// </summary>
    public ForceBase Force { get; internal set; }

    /// <summary>
    /// 保持している階級です。
    /// </summary>
    /// <remarks>
    /// <b><see cref="ForceClassLevel.Alone"/> はここには入りません。</b>
    /// 草案が「Alone は TopLead/SubLead の階級状態に影響しない」と定めているため、
    /// 隊から離れても保持階級はそのままで、表示だけが <see cref="Level"/> で変わります。
    /// </remarks>
    public ForceClassLevel Rank { get; internal set; } = ForceClassLevel.Member;

    /// <summary>
    /// 表示用の階級です。
    /// </summary>
    /// <remarks>
    /// 隊に属していない一般隊員だけが <see cref="ForceClassLevel.Alone"/> になります。
    /// TopLead は一人でも本隊を形成し続けるので Alone にはなりません。
    /// </remarks>
    public ForceClassLevel Level => Force is null && Rank is ForceClassLevel.Member
        ? ForceClassLevel.Alone
        : Rank;

    /// <summary>
    /// いまの隊に加わった時刻です (<see cref="Time.time"/> 基準)。
    /// </summary>
    public float JoinedAt { get; internal set; }

    /// <summary>
    /// 単独行動になった時刻です。隊に属していれば null。
    /// </summary>
    /// <remarks>
    /// 分隊を組むときの隊長選びに使います。これが無いと候補の順が
    /// <c>Player.List</c> (接続順) のままになり、先に単独だった人ではなく
    /// たまたま並び順が前の人が隊長になります。
    /// </remarks>
    public float? AloneSince { get; internal set; }

    /// <summary>
    /// この隊での累計貢献度です。
    /// </summary>
    public int Contribution { get; internal set; }

    /// <summary>
    /// SubLead への昇進条件を満たし始めた時刻です。満たしていなければ null。
    /// </summary>
    internal float? SubLeadHoldSince { get; set; }

    /// <summary>
    /// 昇進条件が緩和されているかどうか。
    /// </summary>
    /// <remarks>
    /// 隊の昇格で TopLead になった隊員の次点だった者に立ちます。
    /// 草案どおり、通常の「70% を 60 秒」が「60% を 40 秒」になります。
    /// </remarks>
    internal bool HasRelaxedPromotion { get; set; }

    /// <summary>
    /// 本人がまだ同一人物として生きているかどうか。
    /// </summary>
    /// <remarks>
    /// netId まで見るのは、退出した枠に別人が入っても取り違えないためです。
    /// </remarks>
    public bool IsAlive => Player.IsSafePlayer() && Player.GetNetId() == NetId;

    /// <summary>
    /// この隊に加わってからの経過秒数です。
    /// </summary>
    public float MembershipSeconds => Time.time - JoinedAt;
}
