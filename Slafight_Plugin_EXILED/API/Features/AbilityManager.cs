using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;

namespace Slafight_Plugin_EXILED.API.Features;

public static class AbilityManager
{
    public static readonly Dictionary<int, AbilityLoadout> Loadouts = new();

    // 「必ず作る」用
    public static AbilityLoadout GetOrCreateLoadout(Player player)
    {
        if (!Loadouts.TryGetValue(player.Id, out var loadout))
        {
            loadout = new AbilityLoadout();
            Loadouts[player.Id] = loadout;
        }
        return loadout;
    }

    // 「あれば取るだけ」用
    public static bool TryGetLoadout(Player player, out AbilityLoadout loadout)
        => Loadouts.TryGetValue(player.Id, out loadout);

    // ★ 現在選択中のアビリティ使用可能か
    public static bool CanUseActiveAbility(Player player)
        => AbilityBase.CanUseSelectedAbility(player.Id);

    // ★ スロット切り替え
    public static bool SwitchToSlot(Player player, int slotIndex)
    {
        if (!TryGetLoadout(player, out var loadout))
            return false;

        if (slotIndex < 0 || slotIndex >= AbilityLoadout.MaxSlots)
            return false;

        loadout.ActiveIndex = slotIndex;
        UpdateAbilityHint(player, loadout);
        return true;
    }

    // ★ 次スロットへ（CycleNextのラッパー）
    public static bool NextSlot(Player player)
    {
        if (!TryGetLoadout(player, out var loadout))
            return false;

        loadout.CycleNext();
        UpdateAbilityHint(player, loadout);
        return true;
    }

    // HUD更新ヘルパー
    private static void UpdateAbilityHint(Player player, AbilityLoadout loadout)
    {
        var sb = new System.Text.StringBuilder();

        for (int i = 0; i < AbilityLoadout.MaxSlots; i++)
        {
            var ability = loadout.Slots[i];
            if (ability == null)
                continue;

            string name = ability.GetType().Name;
            string marker = (i == loadout.ActiveIndex) ? "<color=#ffff00>▶</color>" : "　";
            string status = AbilityBase.CanUseNow(player.Id, ability.GetType()) ? 
                "<color=green>OK</color>" : "<color=red>CD</color>";
            sb.AppendLine($"{marker} {name} [{status}]");
        }

        string text = sb.Length > 0 ? sb.ToString() : "<color=gray>アビリティ未設定</color>";
        
        // Plugin.Singleton.PlayerHUD.HintSync があるならそれを使う
        // なければ player.ShowHint
        try
        {
            // Plugin.Singleton?.PlayerHUD?.HintSync(SyncType.PHUD_Specific, text, player);
            player.ShowHint(text, 2f);
        }
        catch
        {
            player.ShowHint(text, 2f);
        }
    }

    // プレイヤー全クリア（AbilityBaseも連動）
    public static void ClearPlayer(Player player)
    {
        AbilityBase.RevokeAbility(player.Id);  // アビリティ状態も削除
        Loadouts.Remove(player.Id);
    }

    // 全員クリア
    public static void ClearAllLoadouts()
    {
        foreach (var kvp in Loadouts.ToArray())
            ClearPlayer(Player.Get(kvp.Key));
        Loadouts.Clear();
    }

    // ★ スロットだけクリア（アビリティ状態は残す）
    public static void ClearSlots(Player player)
    {
        if (!TryGetLoadout(player, out var loadout))
            return;

        for (int i = 0; i < AbilityLoadout.MaxSlots; i++)
            loadout.Slots[i] = null;

        loadout.ActiveIndex = 0;
    }

    // ===== イベント管理 =====
    private static bool _initialized;

    internal static void RegisterEvents()
    {
        if (_initialized) return;

        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
        Exiled.Events.Handlers.Player.Joined += OnPlayerJoined;
        _initialized = true;
    }

    internal static void UnregisterEvents()
    {
        if (!_initialized) return;

        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
        Exiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
        _initialized = false;
    }

    private static void OnRoundStarted()
    {
        // ラウンド開始時にクールダウンリセット
        foreach (var kvp in Loadouts)
        {
            var player = Player.Get(kvp.Key);
            if (player?.IsConnected == true)
                AbilityBase.ResetCooldown(player.Id);
        }
    }

    private static void OnWaitingForPlayers()
    {
        ClearAllLoadouts();  // 待機時に全クリア
    }

    private static void OnPlayerLeft(LeftEventArgs ev)
    {
        ClearPlayer(ev.Player);
    }

    private static void OnPlayerJoined(JoinedEventArgs ev)
    {
        // 新規参加者に空Loadout作成
        GetOrCreateLoadout(ev.Player);
    }

    // ★ デバッグ用：ロードアウト内容表示
    public static string GetLoadoutInfo(Player player)
    {
        if (!TryGetLoadout(player, out var loadout))
            return "No loadout";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Active: {loadout.ActiveIndex}");
        for (int i = 0; i < AbilityLoadout.MaxSlots; i++)
        {
            var ability = loadout.Slots[i];
            sb.AppendLine($"Slot{i}: {ability?.GetType().Name ?? "空"}");
        }
        return sb.ToString();
    }
}
