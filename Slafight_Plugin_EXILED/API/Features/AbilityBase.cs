using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.API.Features;

public abstract class AbilityBase
{
    protected static Dictionary<int, AbilityState> playerStates = new Dictionary<int, AbilityState>();
    private static bool _initialized = false;

    // ★AbilityState（状態管理クラス）
    protected class AbilityState
    {
        public bool CanUse { get; set; } = true;
        public int MaxUses { get; set; } = -1;  // -1 = 無制限
        public int UsedCount { get; set; } = 0;
        public float CooldownSeconds { get; set; } = 10f;
    }

    // ===== 外部API（静的メソッド）=====
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
    
    public static void GrantAbility(int playerId, float cooldown = 10f, int maxUses = -1)
    {
        playerStates[playerId] = new AbilityState 
        { CanUse = true, CooldownSeconds = cooldown, MaxUses = maxUses };
    }
    
    public static void RevokeAbility(int playerId) => playerStates.Remove(playerId);
    public static void SetCooldown(int playerId, float seconds)
    {
        if (playerStates.TryGetValue(playerId, out var state))
            state.CooldownSeconds = seconds;
    }
    public static void ResetCooldown(int playerId)
    {
        if (playerStates.TryGetValue(playerId, out var state))
            state.CanUse = true;
    }
    
    public static void GrantAllPlayers(float cooldown = 10f, int maxUses = -1)
    {
        foreach (var player in Player.List)
            GrantAbility(player.Id, cooldown, maxUses);
    }
    
    public static void RevokeAllPlayers() => playerStates.Clear();

    // Player拡張
    public static bool GrantAbility(Player player, float cooldown = 10f, int maxUses = -1)
    {
        GrantAbility(player.Id, cooldown, maxUses); return true;
    }
    public static bool HasAbility(Player player) => HasAbility(player.Id);
    public void TryActivateFromInput(Player player)
    {
        if (TryUseAbility(player))
            ExecuteAbility(player);
    }
    // ===== コンストラクタ・初期化 =====
    private readonly int _settingId;
    private readonly float _defaultCooldown;
    private readonly int _defaultMaxUses;

    protected AbilityBase(int settingId, float cooldownSeconds = 10f, int maxUses = -1)
    {
        _settingId = settingId; _defaultCooldown = cooldownSeconds; _defaultMaxUses = maxUses;
        
        if (!_initialized)
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoined;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
            _initialized = true;
        }
    }

    private static void OnWaitingForPlayers() => playerStates.Clear();
    private static void OnPlayerJoined(JoinedEventArgs ev) => playerStates[ev.Player.Id] = new AbilityState();
    private static void OnPlayerLeft(LeftEventArgs ev) => playerStates.Remove(ev.Player.Id);

    private void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase @base)
    {
        var keybind = @base as SSKeybindSetting;
        if (keybind?.SyncIsPressed != true || keybind.SettingId != _settingId) return;

        var player = Player.Get(hub);
        if (player == null || !TryUseAbility(player)) return;
        ExecuteAbility(player);
    }

    // ===== 内部使用メソッド =====
    protected bool TryUseAbility(Player player)
    {
        if (!playerStates.TryGetValue(player.Id, out var state))
        {
            state = new AbilityState { CooldownSeconds = _defaultCooldown, MaxUses = _defaultMaxUses };
            playerStates[player.Id] = state;
        }

        if (state.MaxUses > 0 && state.UsedCount >= state.MaxUses) return false;
        if (!state.CanUse) return false;

        state.CanUse = false; state.UsedCount++;
        Timing.RunCoroutine(CooldownCoroutine(player.Id, state.CooldownSeconds));
        return true;
    }

    private static IEnumerator<float> CooldownCoroutine(int playerId, float duration)
    {
        yield return Timing.WaitForSeconds(duration);
        if (playerStates.TryGetValue(playerId, out var state)) state.CanUse = true;
    }

    protected abstract void ExecuteAbility(Player player);
}