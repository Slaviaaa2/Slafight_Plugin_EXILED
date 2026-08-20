using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using Slafight_Plugin_EXILED.Extensions;

using ServerHandlers = Exiled.Events.Handlers.Server;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// リスポーンウェーブを引き受けます。バニラのウェーブは常に取り消し、
/// どの <see cref="SpawnSet"/> を出すかをこちらで決めます。
/// </summary>
/// <remarks>
/// 旧実装は <c>SpawnTypeId</c> という 24 個の enum と、
/// <c>Dictionary&lt;SpawnTypeId, Dictionary&lt;SpawnRoleKey, (float, bool)&gt;&gt;</c> という
/// 引き表で波を表していました。ここではその両方を持ちません。
/// <b>波は <see cref="SpawnSet"/> の派生クラスそのものです。</b>
///
/// このクラスはどこからも登録されていません。<see cref="EventHandlerBase"/> を
/// 継承しているだけで <see cref="EventHandlerRegistry"/> が購読させます。
/// </remarks>
public sealed class SpawnSystem : EventHandlerBase
{
    private static readonly List<RegisteredOverride> Overrides = [];

    private static long sequence;

    private static bool spawning;

    /// <summary>
    /// 波を出す直前に呼ばれます。<see cref="SpawningEventArgs.Wave"/> を差し替えれば別の波になり、
    /// <see cref="SpawningEventArgs.IsAllowed"/> を false にすれば出ません。
    /// </summary>
    public static event EventHandler<SpawningEventArgs> Spawning;

    /// <summary>
    /// 波を出した直後に呼ばれます。演出はここで拾ってください。
    /// </summary>
    public static event EventHandler<SpawnedEventArgs> Spawned;

    /// <summary>
    /// 一時的にリスポーンを止めます。
    /// </summary>
    /// <remarks>
    /// 基底の <see cref="EventHandlerBase.Disable"/> と紛らわしい名前にしないこと。
    /// 移植元では <c>public static new bool Disable</c> が基底のメソッドを隠していました。
    /// </remarks>
    public static bool IsSuspended { get; set; }

    /// <summary>
    /// 抽選に使う乱数です。試験で固定したいときに差し替えます。
    /// </summary>
    public static Func<int, int> IndexSelector { get; set; } = max => UnityEngine.Random.Range(0, max);

    /// <inheritdoc />
    public override void RegisterEvents()
    {
        ServerHandlers.RespawningTeam += OnRespawningTeam;
        ServerHandlers.RestartingRound += ResetRuntimeState;
        ServerHandlers.WaitingForPlayers += ResetRuntimeState;
    }

    /// <inheritdoc />
    public override void UnregisterEvents()
    {
        ServerHandlers.RespawningTeam -= OnRespawningTeam;
        ServerHandlers.RestartingRound -= ResetRuntimeState;
        ServerHandlers.WaitingForPlayers -= ResetRuntimeState;
    }

    /// <summary>
    /// 次に条件が合ったウェーブを、指定した波に差し替えます。
    /// </summary>
    /// <returns>取り消しに使う手札。</returns>
    public static Guid AddOverride(SpawnOverride rule)
    {
        if (rule?.Wave is null)
            throw new ArgumentNullException(nameof(rule));

        Guid id = Guid.NewGuid();
        Overrides.Add(new RegisteredOverride(id, rule, ++sequence));

        return id;
    }

    /// <summary>
    /// 予約した差し替えを取り消します。
    /// </summary>
    public static bool RemoveOverride(Guid id) => Overrides.RemoveAll(entry => entry.Id == id) > 0;

    /// <summary>
    /// 予約した差し替えをすべて取り消します。
    /// </summary>
    public static void ClearOverrides() => Overrides.Clear();

    /// <summary>
    /// バニラのタイマーを待たず、いますぐ波を出します。
    /// </summary>
    /// <returns>実際に割り当てた人数。</returns>
    public static int ForceSpawn(SpawnSet wave) => Summon(wave, null);

    /// <summary>
    /// この波の対象になりうるプレイヤーです。
    /// </summary>
    /// <remarks>
    /// 既にカスタム役職を持っている観戦者は対象外です。
    /// 二重に配ると前の役職の後始末が走り、per-player 状態が失われます。
    /// </remarks>
    private static List<Player> Candidates()
    {
        return Player.List
            .Where(player => player.IsSafePlayer() &&
                             player.Role.Type is RoleTypeId.Spectator &&
                             CustomRole.Of(player) is null)
            .ToList();
    }

    private static void OnRespawningTeam(RespawningTeamEventArgs ev)
    {
        // バニラのウェーブは常に取り消す。誰を出すかはこちらが決める。
        ev.IsAllowed = false;

        if (IsSuspended || spawning) return;

        Faction faction = ev.NextKnownTeam;
        bool miniWave = ev.Wave?.IsMiniWave ?? false;

        if (TakeOverride(ev, faction, miniWave) is { } forced)
        {
            Summon(forced, ev);

            return;
        }

        if (faction is not (Faction.FoundationStaff or Faction.FoundationEnemy)) return;

        SpawnContext context = SpawnContext.Active;
        SpawnSet selected = context.Roll(faction, miniWave, IndexSelector);

        if (selected is null) return;

        Summon(selected, ev);
    }

    /// <summary>
    /// 条件に合う差し替えを 1 つ取り出します。優先度が高いものを、同じなら先に予約したものを選びます。
    /// </summary>
    private static SpawnSet TakeOverride(RespawningTeamEventArgs ev, Faction faction, bool miniWave)
    {
        RegisteredOverride match = Overrides
            .Where(entry => entry.Rule.Matches(ev, faction, miniWave))
            .OrderByDescending(entry => entry.Rule.Priority)
            .ThenBy(entry => entry.Sequence)
            .FirstOrDefault();

        if (match is null) return null;

        Overrides.Remove(match);

        return match.Rule.Wave;
    }

    /// <summary>
    /// 波を実際に出します。
    /// </summary>
    private static int Summon(SpawnSet wave, RespawningTeamEventArgs source)
    {
        if (wave is null) return 0;

        List<Player> candidates = Candidates();

        if (candidates.Count == 0) return 0;

        SpawningEventArgs spawningArgs = new SpawningEventArgs(wave, SpawnContext.Active, candidates, source);
        Spawning?.Invoke(null, spawningArgs);

        if (!spawningArgs.IsAllowed || spawningArgs.Wave is null) return 0;

        spawning = true;

        int assigned;

        try
        {
            assigned = spawningArgs.Wave.SpawnFor(candidates);
        }
        catch (Exception exception)
        {
            Log.Error($"[Slafight] ウェーブ '{wave.Name}' の割り当てで例外が発生しました: {exception}");

            return 0;
        }
        finally
        {
            spawning = false;
        }

        if (assigned == 0) return 0;

        Spawned?.Invoke(null, new SpawnedEventArgs(spawningArgs.Wave, assigned));

        return assigned;
    }

    private static void ResetRuntimeState()
    {
        Overrides.Clear();
        IsSuspended = false;
        spawning = false;
        SpawnContext.Reset();
    }

    private sealed class RegisteredOverride(Guid id, SpawnOverride rule, long sequence)
    {
        public Guid Id { get; } = id;

        public SpawnOverride Rule { get; } = rule;

        public long Sequence { get; } = sequence;
    }
}
