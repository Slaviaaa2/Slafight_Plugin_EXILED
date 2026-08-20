using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Core.Extensions;
using Slafight_Plugin_EXILED.API.Core.Structs;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// 何人に、どの役職を、何人ずつ割り当てるかをまとめて宣言するものです。
/// ラウンド開始時の一括割り当てや、イベント開始時の陣営配布に使います。
/// </summary>
/// <remarks>
/// <b>これは旧 <c>MainHandlers/SpawnSystem</c> 一式を置き換えるためのものです。</b>
/// 波を表す中間基底クラスや <c>SpawnTypeId</c> のような enum を挟まないでください。
/// 波は <see cref="SpawnSet"/> の派生クラスそのものとして宣言し、
/// 陣営・重み・比率・テーマはその派生クラスのプロパティとして持たせます。
/// enum と引き表に分けた瞬間、旧構造へ逆戻りします。
/// </remarks>
/// <example>
/// <code>
/// public class ChaosRaidSet : SpawnSet
/// {
///     public override string Name => "Chaos Raid";
///
///     public override int AllowedPlayerCount => 6;
///
///     public override IReadOnlyList&lt;SpawnSetRoleDefinition&gt; SpawnRoles =>
///     [
///         SpawnSetRoleDefinition.Custom&lt;ChaosCommando&gt;(count: 1, isForced: true),
///         SpawnSetRoleDefinition.Vanilla(RoleTypeId.ChaosRifleman, count: 99),
///     ];
/// }
/// </code>
/// </example>
public abstract class SpawnSet
{
    /// <summary>
    /// 表示名です。
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// 説明です。
    /// </summary>
    public virtual string Description => string.Empty;

    /// <summary>
    /// この SpawnSet が割り当てる最大人数です。
    /// -1 なら対象プレイヤーが居る限り割り当てます。
    /// </summary>
    public virtual int AllowedPlayerCount => -1;

    /// <summary>
    /// 全員をこの位置に飛ばします。null なら役職側の指定に任せます。
    /// </summary>
    public virtual Vector3? OverridePosition => null;

    /// <summary>
    /// 割り当てる役職の一覧です。
    /// </summary>
    public abstract IReadOnlyList<SpawnSetRoleDefinition> SpawnRoles { get; }

    /// <summary>
    /// この SpawnSet の割り当て対象になりうるプレイヤーです。
    /// 既定では「まだ役職が割り当てられていない実プレイヤー」を返します。
    /// </summary>
    /// <remarks>
    /// ラウンド開始時はバニラの役職割り当てより前に走るため、その時点のプレイヤーは
    /// <see cref="RoleTypeId.Spectator"/> ではなく <see cref="RoleTypeId.None"/> (ロビー) です。
    /// ラウンド中に呼ばれた場合は Spectator が対象になります。
    /// Overwatch は <see cref="RoleTypeId.Overwatch"/> なのでここには含まれません。
    ///
    /// 既定は <c>IsSafePlayer</c> で実プレイヤーに絞ります。NPC も対象にしたい SpawnSet は
    /// ここを override して明示的に広げてください。
    /// </remarks>
    protected virtual List<Player> TargetPlayers()
    {
        return Player.List
            .Where(player => player.IsSafePlayer() &&
                             player.Role.Type is RoleTypeId.None or RoleTypeId.Spectator)
            .Shuffled();
    }

    /// <summary>
    /// 割り当てを実行します。
    /// </summary>
    /// <returns>実際に割り当てた人数。</returns>
    public int Spawn()
    {
        bool alreadyLocked = Round.IsLocked;

        if (!alreadyLocked)
            Round.IsLocked = true;

        // 途中で return してもラウンドロックを必ず戻す。
        try
        {
            return SpawnInternal();
        }
        finally
        {
            if (!alreadyLocked)
                Round.IsLocked = false;
        }
    }

    private int SpawnInternal()
    {
        if (AllowedPlayerCount < -1)
        {
            Log.Error($"[Slafight] SpawnSet '{Name}' の AllowedPlayerCount が不正です: {AllowedPlayerCount} (-1 以上にしてください)");

            return 0;
        }

        if (SpawnRoles is null || SpawnRoles.Count == 0 || AllowedPlayerCount == 0)
            return 0;

        List<Player> candidates = TargetPlayers();

        if (candidates.Count == 0)
            return 0;

        // TargetPlayers はシャッフル済みなので、先頭から必要人数を取るだけでランダム選出になる。
        if (AllowedPlayerCount > 0 && candidates.Count > AllowedPlayerCount)
            candidates = candidates.Take(AllowedPlayerCount).ToList();

        List<SpawnRoleState> states = SpawnRoles
            .Shuffled()
            .Select(role => new SpawnRoleState(role))
            .ToList();

        // 必須の行を先に埋めてから、残りを回す。
        int assigned = Assign(states.Where(state => state.Definition.IsForced).ToList(), candidates);

        assigned += Assign(states.Where(state => !state.Definition.IsForced).ToList(), candidates);

        return assigned;
    }

    /// <returns>この呼び出しで割り当てた人数。</returns>
    private int Assign(IReadOnlyList<SpawnRoleState> states, List<Player> candidates)
    {
        int assigned = 0;

        if (states.Count == 0)
            return assigned;

        while (candidates.Count > 0)
        {
            if (PickNext(states) is not { } state)
                break;

            // TargetPlayers はシャッフル済みなので、先頭から取れば無作為に選んだのと同じ。
            Player target = candidates[0];
            candidates.RemoveAt(0);

            if (!TrySpawn(state, target))
            {
                // 割り当てに失敗した行は以後選ばない。無限ループを避ける。
                state.IsBroken = true;

                // この対象はまだ役職を持っていない。別の行が拾えるよう列の先頭へ戻す。
                // 全行が壊れたら PickNext が null を返して while が抜けるので、回り続けない。
                candidates.Insert(0, target);

                continue;
            }

            // 役職側の SpawnPosition より SpawnSet 側の指定を優先する。
            if (OverridePosition is { } position)
                target.Position = position;

            state.AssignedCount++;
            assigned++;
        }

        return assigned;
    }

    /// <summary>
    /// 1 行ぶんの割り当てを行います。
    /// </summary>
    /// <remarks>
    /// <b>1 つの役職が落ちても割り当て全体を巻き添えにしません。</b>
    /// ここで例外を通すと <see cref="Spawn"/> ごと抜けてしまい、
    /// 「SCP だけ配られて人間側が誰も配られない」ような、
    /// 原因の見えない中断になります。落ちた行だけを諦めて先へ進みます。
    /// </remarks>
    private bool TrySpawn(SpawnRoleState state, Player target)
    {
        try
        {
            return state.Definition.SpawnFor(target);
        }
        catch (Exception exception)
        {
            Log.Error($"[Slafight] SpawnSet '{Name}' の役職割り当てで例外が発生しました: {exception}");

            return false;
        }
    }

    /// <summary>
    /// まだ枠が残っている行から 1 つ選びます。<see cref="SpawnSetRoleDefinition.Weight"/> で
    /// 出やすさに差を付けられます。全部同じ重みなら等確率です。
    /// </summary>
    private static SpawnRoleState PickNext(IReadOnlyList<SpawnRoleState> states)
    {
        float total = 0f;

        foreach (SpawnRoleState state in states)
        {
            if (state.IsAvailable)
                total += Mathf.Max(0f, state.Definition.Weight);
        }

        if (total <= 0f)
            return null;

        float roll = UnityEngine.Random.value * total;

        foreach (SpawnRoleState state in states)
        {
            if (!state.IsAvailable)
                continue;

            roll -= Mathf.Max(0f, state.Definition.Weight);

            if (roll <= 0f)
                return state;
        }

        // 浮動小数の誤差で漏れたときの保険。
        return states.LastOrDefault(state => state.IsAvailable);
    }

    private sealed class SpawnRoleState(SpawnSetRoleDefinition definition)
    {
        public SpawnSetRoleDefinition Definition { get; } = definition;

        public int AssignedCount { get; set; }

        public bool IsBroken { get; set; }

        public bool IsAvailable => !IsBroken && AssignedCount < Definition.Count;
    }
}
