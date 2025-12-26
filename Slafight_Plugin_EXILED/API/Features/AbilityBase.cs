using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;

namespace Slafight_Plugin_EXILED.API.Features;

public abstract class AbilityBase
{
    protected static readonly Dictionary<int, AbilityState> playerStates = new();

    protected class AbilityState
    {
        public bool CanUse { get; set; } = true;
        public int MaxUses { get; set; } = -1;
        public int UsedCount { get; set; } = 0;
        public float CooldownSeconds { get; set; } = 10f;

        public Action<Player>? OnCooldownEnd { get; set; }
        public CoroutineHandle CooldownHandle;
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
    // プレイヤーごとの状態取得・設定用のヘルパー
    protected static bool TryGetState(int playerId, out AbilityState state) =>
        playerStates.TryGetValue(playerId, out state);

    // 外部からコールバックをセットするAPI
    public static void SetOnCooldownEnd(int playerId, Action<Player>? onCooldownEnd)
    {
        if (playerStates.TryGetValue(playerId, out var state))
            state.OnCooldownEnd = onCooldownEnd;
    }

    public static void GrantAbility(
        int playerId,
        float cooldown = 10f,
        int maxUses = -1,
        Action<Player>? onCooldownEnd = null)
    {
        playerStates[playerId] = new AbilityState
        {
            CanUse = true,
            CooldownSeconds = cooldown,
            MaxUses = maxUses,
            OnCooldownEnd = onCooldownEnd,
        };
    }

    public static void RevokeAbility(int playerId)
    {
        if (playerStates.TryGetValue(playerId, out var state))
        {
            if (state.CooldownHandle.IsRunning)
                Timing.KillCoroutines(state.CooldownHandle);

            // OnCooldownEnd は参照を捨てるだけでOK
            state.OnCooldownEnd = null;
        }

        playerStates.Remove(playerId);
    }

    public static void RevokeAllPlayers() => playerStates.Clear();

    public static void ResetCooldown(int playerId)
    {
        if (playerStates.TryGetValue(playerId, out var state))
            state.CanUse = true;
    }

    // Player拡張
    public static bool GrantAbility(
        Player player,
        float cooldown = 10f,
        int maxUses = -1,
        Action<Player>? onCooldownEnd = null)
    {
        GrantAbility(player.Id, cooldown, maxUses, onCooldownEnd);
        return true;
    }

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

    protected AbilityBase(float cooldownSeconds = 10f, int maxUses = -1)
    {
        _defaultCooldown = cooldownSeconds;
        _defaultMaxUses = maxUses;

        if (_initialized)
            return;

        Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        Exiled.Events.Handlers.Player.Joined += OnPlayerJoined;
        Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
        _initialized = true;
    }

    private static void OnWaitingForPlayers() => playerStates.Clear();
    private static void OnPlayerJoined(JoinedEventArgs ev) => playerStates[ev.Player.Id] = new AbilityState();
    private static void OnPlayerLeft(LeftEventArgs ev) => playerStates.Remove(ev.Player.Id);

    // ===== 内部使用メソッド =====
    private readonly float _defaultCooldown;
    private readonly int _defaultMaxUses;

    protected bool TryUseAbility(Player player)
    {
        if (!playerStates.TryGetValue(player.Id, out var state))
        {
            state = new AbilityState
            {
                CooldownSeconds = _defaultCooldown,
                MaxUses = _defaultMaxUses,
            };
            playerStates[player.Id] = state;
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

        state.CooldownHandle = Timing.RunCoroutine(CooldownCoroutine(player.Id, state.CooldownSeconds));
        return true;
    }

    private static IEnumerator<float> CooldownCoroutine(int playerId, float duration)
    {
        yield return Timing.WaitForSeconds(duration);

        if (!playerStates.TryGetValue(playerId, out var state))
            yield break;

        state.CanUse = true;

        var player = Player.Get(playerId);
        if (player != null && player.IsConnected)
            state.OnCooldownEnd?.Invoke(player);
    }

    protected abstract void ExecuteAbility(Player player);
}
