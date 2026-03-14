using System;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.ProximityChat;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.API.Features;

public class AbilityInputHandler
{
    public AbilityInputHandler()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
    }

    ~AbilityInputHandler()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSettingValueReceived;
    }

    private void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase @base)
    {
        // キーバインド以外は無視
        if (@base is not SSKeybindSetting keybind || !keybind.SyncIsPressed)
            return;

        var player = Player.Get(hub);
        if (player == null || !player.IsConnected)
            return;

        // Log.Debug($"[Input] {player.Nickname} role={player.Role?.Type} team={player.Role?.Team} setting={keybind.SettingId}");

        // VCトグルは常に許可
        if (keybind.SettingId == 1)
        {
            ActivateHandler.ToggleProximityChat(player);
            return;
        }

        // 能力処理するのは「生きてるロールだけ」
        if (player.Role == null ||
            player.Role.Type == RoleTypeId.None ||
            player.Role.Type == RoleTypeId.Spectator ||
            player.Role.Team == Team.Dead)
        {
            //Log.Debug($"[Input] Ignore ability input from {player.Nickname} (role={player.Role?.Type}, team={player.Role?.Team})");
            return;
        }

        // ロードアウト取得（ないなら何もしない）
        if (!AbilityManager.Loadouts.TryGetValue(player.Id, out var loadout) || loadout == null)
        {
            //Log.Debug($"[Input] No loadout for {player.Nickname}");
            return;
        }

        try
        {
            if (keybind.SettingId == 3)
            {
                // アクティブ能力使用
                //Log.Debug($"[Input] Try use active ability for {player.Nickname}");
                loadout.ActiveAbility?.TryActivateFromInput(player);
            }
            else if (keybind.SettingId == 4)
            {
                // スロット切り替え＋HUD更新
                //Log.Debug($"[Input] Cycle ability slot for {player.Nickname}");
                loadout.CycleNext();
                AbilityManager.UpdateAbilityHint(player, loadout);
            }
        }
        catch (Exception e)
        {
            Log.Warn($"[Input] Ability handling error for {player.Nickname}: {e}");
        }
    }
}
