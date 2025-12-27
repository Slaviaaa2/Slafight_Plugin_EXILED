using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;

namespace Slafight_Plugin_EXILED.API.Features;

public abstract class AbilityBase
{
    // プレイヤーIDごとのアビリティ状態
    protected static readonly Dictionary<int, AbilityState> playerStates = new();

    protected class AbilityState
    {
        public bool CanUse { get; set; } = true;
        public int MaxUses { get; set; } = -1;   // -1 = 無制限
        public int UsedCount { get; set; } = 0;
        public float CooldownSeconds { get; set; } = 10f;

        public CoroutineHandle CooldownHandle;
        public AbilityBase? OwnerAbility { get; set; }
    }

    // ===== 外部API =====
    public static bool HasAbility(int playerId) => playerStates.ContainsKey(playerId);

    public static bool CanUseNow(int playerId) =>
        playerStates.TryGetValue(playerId, out var state) && state.CanUse;

    public static bool IsOnCooldown(int playerId) =>
        playerStates.TryGetValue(playerId, out var state) && !state.CanUse;

    public static int GetUsedCount(int playerId) =>
        playerStates.TryGetValue(playerId, out var state) ? state.UsedCount : 0;

    public static bool HasUsesLeft(int playerId) =>
        playerStates.TryGetValue(playerId, out var state) &&
        (state.MaxUses < 0 || state.UsedCount < state.MaxUses);

    // プレイヤーごとの状態取得
    protected static bool TryGetState(int playerId, out AbilityState state) =>
        playerStates.TryGetValue(playerId, out state);

    // AbilityState 作成（外部から細かく触る必要が無ければこれだけでOK）
    public static void GrantAbility(int playerId, float cooldown = 10f, int maxUses = -1)
    {
        playerStates[playerId] = new AbilityState
        {
            CanUse = true,
            CooldownSeconds = cooldown,
            MaxUses = maxUses,
            OwnerAbility = null, // 最初の発動時に TryUseAbility で埋める
        };
    }
    // 「クールダウンだけ変えたい」用の糖衣 (任意)
    public static void GrantAbility(int playerId, float cooldown)
        => GrantAbility(playerId, cooldown, -1);

    public static void RevokeAbility(int playerId)
    {
        if (playerStates.TryGetValue(playerId, out var state))
        {
            if (state.CooldownHandle.IsRunning)
                Timing.KillCoroutines(state.CooldownHandle);
        }

        playerStates.Remove(playerId);
    }

    public static void RevokeAllPlayers()
    {
        // 全員分のコルーチンを殺してから状態クリア
        foreach (var state in playerStates.Values)
        {
            if (state.CooldownHandle.IsRunning)
                Timing.KillCoroutines(state.CooldownHandle);
        }

        playerStates.Clear();
    }

    public static void ResetCooldown(int playerId)
    {
        if (playerStates.TryGetValue(playerId, out var state))
            state.CanUse = true;
    }

    // Player 拡張
    public static bool GrantAbility(Player player, float cooldown = 10f, int maxUses = -1)
    {
        GrantAbility(player.Id, cooldown, maxUses);
        return true;
    }

    public static bool GrantAbility(Player player, float cooldown)
        => GrantAbility(player, cooldown, -1);

    public static bool HasAbility(Player player) => HasAbility(player.Id);

    public static bool HasAbility<TAbility>(Player player) where TAbility : AbilityBase
    {
        if (!AbilityManager.Loadouts.TryGetValue(player.Id, out var loadout))
            return false;

        foreach (var ability in loadout.Slots)
        {
            if (ability is TAbility)
                return true;
        }

        return false;
    }

    // AbilityInputHandler から叩く入口
    public void TryActivateFromInput(Player player)
    {
        Log.Debug($"[Ability] TryActivateFromInput called for {player.Nickname}, role={player.Role.Type}, team={player.Role.Team}");

        if (player.Role.Team == Team.Dead)
        {
            Log.Debug("TryActivateFromInput: Dead Blocked");
            return;
        }

        var canUse = TryUseAbility(player);
        Log.Debug($"[Ability] TryUseAbility result={canUse} for {player.Nickname}");

        if (canUse)
            ExecuteAbility(player);
    }

    // ===== 初期化（ラウンド跨ぎのクールダウンクリア用）=====
    private static bool _initialized;
    
    // 抽象デフォルト値
    protected abstract float DefaultCooldown { get; }
    protected abstract int DefaultMaxUses { get; }

    private readonly float _defaultCooldown;
    private readonly int _defaultMaxUses;

    // ここを「nullable で受ける」コンストラクタにする
    protected AbilityBase(Player owner, float? cooldownSeconds = null, int? maxUses = null)
    {
        _defaultCooldown = cooldownSeconds ?? DefaultCooldown;
        _defaultMaxUses  = maxUses ?? DefaultMaxUses;

        GrantAbility(owner.Id, _defaultCooldown, _defaultMaxUses);
    }
    
    // AbilityBase 内
    internal static void RegisterEvents()
    {
        if (_initialized) return;

        Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        Exiled.Events.Handlers.Player.Joined += OnPlayerJoined;
        Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
        _initialized = true;
    }

    internal static void UnregisterEvents()
    {
        if (!_initialized) return;

        Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        Exiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
        Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
        _initialized = false;
    }

    protected AbilityBase(Player owner)
        : this(owner, null, null)
    {
    }

    private static void OnWaitingForPlayers() => RevokeAllPlayers();

    private static void OnPlayerJoined(JoinedEventArgs ev) =>
        playerStates[ev.Player.Id] = new AbilityState();

    private static void OnPlayerLeft(LeftEventArgs ev) =>
        RevokeAbility(ev.Player.Id);

    protected bool TryUseAbility(Player player)
    {
        if (!playerStates.TryGetValue(player.Id, out var state))
        {
            state = new AbilityState
            {
                CooldownSeconds = _defaultCooldown,
                MaxUses = _defaultMaxUses,
                OwnerAbility = this,
            };
            playerStates[player.Id] = state;
        }
        else if (state.OwnerAbility == null)
        {
            state.OwnerAbility = this;
        }

        if (state.MaxUses > 0 && state.UsedCount >= state.MaxUses)
            return false;

        if (!state.CanUse)
            return false;

        state.CanUse = false;
        state.UsedCount++;

        // 既存コルーチンがあれば殺す（多重起動防止）
        if (state.CooldownHandle.IsRunning)
            Timing.KillCoroutines(state.CooldownHandle);

        state.CooldownHandle = Timing.RunCoroutine(
            CooldownCoroutine(player.Id, state.CooldownSeconds));

        return true;
    }

    private static IEnumerator<float> CooldownCoroutine(int playerId, float duration)
    {
        yield return Timing.WaitForSeconds(duration);

        if (!playerStates.TryGetValue(playerId, out var state))
            yield break;

        state.CanUse = true;

        var player = Player.Get(playerId);
        if (player == null || !player.IsConnected || state.OwnerAbility == null)
            yield break;

        state.OwnerAbility.OnCooldownEnd(player);
    }

    protected abstract void ExecuteAbility(Player player);

    // クールダウン終了時のデフォルト挙動
    protected virtual void OnCooldownEnd(Player player)
    {
        if (player != null && player.IsConnected)
        {
            var abilityName = GetType().Name;
            player.ShowHint(
                $"<color=yellow>{abilityName} のクールダウンが終了しました。</color>",
                3f);
        }
    }
}