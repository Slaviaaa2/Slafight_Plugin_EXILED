using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.ForceSystem;

/// <summary>
/// いま生きている隊と隊員をまとめて持ちます。
/// </summary>
/// <remarks>
/// 隊員の引きは netId キーです。AGENTS.md が求めるとおり、
/// 破棄され得る <see cref="Player"/> を辞書のキーにしません。
/// </remarks>
public static class ForceRegistry
{
    private static readonly List<ForceBase> Forces = [];
    private static readonly Dictionary<uint, ForceMember> MembersByNetId = new();
    private static readonly Dictionary<(System.Type Kind, bool IsMainForce), int> Ordinals = new();

    /// <summary>
    /// いま存在する隊です。本隊も分隊も含みます。
    /// </summary>
    public static IReadOnlyList<ForceBase> All => Forces;

    /// <summary>
    /// このプレイヤーの隊員状態です。まだ登録されていなければ作ります。
    /// </summary>
    public static ForceMember MemberOf(Player player)
    {
        if (!player.IsSafePlayer()) return null;

        uint netId = player.GetNetId();

        if (netId == 0) return null;

        // 同じ枠に別人が入っている場合は作り直す。netId まで見ないと取り違える。
        if (MembersByNetId.TryGetValue(netId, out ForceMember existing) &&
            ReferenceEquals(existing.Player, player))
            return existing;

        ForceMember member = new ForceMember(player);
        MembersByNetId[netId] = member;

        return member;
    }

    /// <summary>
    /// このプレイヤーの隊員状態です。無ければ null。新しくは作りません。
    /// </summary>
    public static ForceMember FindMember(Player player)
    {
        if (player is null) return null;

        return MembersByNetId.TryGetValue(player.GetNetId(), out ForceMember member) &&
               ReferenceEquals(member.Player, player)
            ? member
            : null;
    }

    /// <summary>
    /// このプレイヤーが属している隊です。無ければ null。
    /// </summary>
    public static ForceBase ForceOf(Player player) => FindMember(player)?.Force;

    /// <summary>
    /// 隊を登録します。
    /// </summary>
    public static void Register(ForceBase force)
    {
        if (force is null || Forces.Contains(force)) return;

        // 名前は遅延生成なので、番号は登録の時点で決めておく。
        if (!force.IsMainForce && force.Parent is { } parent)
        {
            // 親が居る分隊はその本隊ごとの番号。「第 1 分隊」が本隊ごとに始まる。
            force.Ordinal = parent.NextSquadOrdinal();
        }
        else
        {
            (System.Type, bool) key = (force.GetType(), force.IsMainForce);

            Ordinals.TryGetValue(key, out int issued);
            Ordinals[key] = ++issued;
            force.Ordinal = issued;
        }

        Forces.Add(force);
    }

    /// <summary>
    /// 隊を解散します。所属していた隊員は無所属に戻ります。
    /// </summary>
    public static void Dissolve(ForceBase force)
    {
        if (force is null || !Forces.Remove(force)) return;

        foreach (ForceMember member in force.Members.ToArray())
            force.Remove(member);

        // この隊を親にしていた分隊は独立させる。昇格の対象になる。
        foreach (ForceBase squad in Forces.Where(candidate => ReferenceEquals(candidate.Parent, force)))
            squad.Parent = null;
    }

    /// <summary>
    /// この陣営の隊です。HUD の表示範囲を絞るのに使います。
    /// </summary>
    public static IEnumerable<ForceBase> OfFaction(Faction faction) =>
        Forces.Where(force => force.Faction == faction);

    /// <summary>
    /// この隊に属する分隊です。
    /// </summary>
    public static IEnumerable<ForceBase> SquadsOf(ForceBase parent) =>
        Forces.Where(force => !force.IsMainForce && ReferenceEquals(force.Parent, parent));

    /// <summary>
    /// 役職が変わった人の所属を見直します。いまの役職に合わなくなっていれば捨てます。
    /// </summary>
    /// <remarks>
    /// <b>死亡だけを見ていると足りません。</b>隊員が SCP-049-2 にされたり、
    /// 管理者に SCP へ変えられたりした場合、生きたままなので
    /// <see cref="ForceBase.PruneDead"/> では外れず、隊に残って分隊を率い、
    /// バフまで受け取り続けます。役職が入れ替わった時点でここが捨てます。
    ///
    /// 階級と貢献度は「その役職でその隊に居たあいだ」のものなので、
    /// 隊から外すだけでなく隊員状態ごと捨てます。残すと、部隊を率いていた人が
    /// 別陣営で復帰した瞬間にまた隊長になります。
    /// </remarks>
    internal static void Refresh(Player player)
    {
        if (player is null) return;

        uint netId = player.GetNetId();

        if (netId == 0 || !MembersByNetId.TryGetValue(netId, out ForceMember member)) return;

        // 同じ枠に別人が入っているなら、それは前の人の残骸。
        if (!ReferenceEquals(member.Player, player))
        {
            MembersByNetId.Remove(netId);

            return;
        }

        if (IsStale(member))
            Discard(member);
    }

    /// <summary>
    /// この隊員状態が、もう持ち主の役職と合っていないかどうか。
    /// </summary>
    /// <remarks>
    /// 隊に属しているなら、居られるかどうかは隊自身 (<see cref="ForceBase.Accepts"/>) が決めます。
    /// 属していないなら、作られたときの種類 (<see cref="ForceMember.Kind"/>) から
    /// 変わっていないかだけを見ます。
    /// </remarks>
    private static bool IsStale(ForceMember member)
    {
        if (!member.IsAlive) return true;

        if (member.Force is { } force) return !force.Accepts(member.Player);

        return ForceKinds.For(member.Player) is not { } kind || kind != member.Kind;
    }

    /// <summary>
    /// 隊員状態を捨てます。所属していた隊からも外れます。
    /// </summary>
    /// <remarks>
    /// 名札もここで描き直します。隊名と階級は <c>CustomInfo</c> に焼き付いていて、
    /// 隊員状態を失った人は以後 <see cref="ForceEvaluator"/> の描き直しが触らないので、
    /// ここで消さないと SCP になっても「隊長」のまま残ります。
    /// </remarks>
    private static void Discard(ForceMember member)
    {
        member.Force?.Remove(member);
        MembersByNetId.Remove(member.NetId);

        // 退出済みなら描き直す名札も無い。破棄済みのハブに触らない。
        if (member.IsAlive)
            ForceNameplate.Refresh(member.Player);
    }

    /// <summary>
    /// 役職が変わった隊員・死亡した隊員・退出した隊員と、空になった隊を掃除します。
    /// </summary>
    /// <remarks>
    /// <see cref="Refresh"/> の取りこぼし (役職変更イベントを通らない経路) をここで拾います。
    /// 評価の tick ごとに回るので、遅くとも数秒で辻褄が合います。
    /// </remarks>
    internal static void Prune()
    {
        foreach (KeyValuePair<uint, ForceMember> pair in MembersByNetId.ToArray())
        {
            if (IsStale(pair.Value))
                Discard(pair.Value);
        }

        foreach (ForceBase force in Forces.ToArray())
        {
            force.PruneDead();

            if (force.Members.Count == 0)
                Dissolve(force);
        }
    }

    /// <summary>
    /// ラウンドをまたいで状態を持ち越さないようにします。
    /// </summary>
    internal static void Reset()
    {
        foreach (ForceBase force in Forces)
        {
            foreach (ForceMember member in force.Members)
                member.Force = null;
        }

        Forces.Clear();
        MembersByNetId.Clear();
        Ordinals.Clear();
        ForceNaming.Reset();
    }
}
