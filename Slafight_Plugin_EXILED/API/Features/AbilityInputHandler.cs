using System.Text;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Hints;
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
        var keybind = @base as SSKeybindSetting;
        if (keybind == null || !keybind.SyncIsPressed)
            return;

        var player = Player.Get(hub);
        if (player == null)
            return;

        Log.Debug($"[Input] {player.Nickname} role={player.Role.Type}, team={player.Role.Team}, setting={keybind.SettingId}");

        if (keybind.SettingId == 1)
        {
            ActivateHandler.ToggleProximityChat(player);
            return;
        }

        if (!AbilityManager.Loadouts.TryGetValue(player.Id, out var loadout))
        {
            Log.Debug($"[Input] No loadout for {player.Nickname}");
            return;
        }

        if (keybind.SettingId == 3)
        {
            Log.Debug($"[Input] Try use active ability for {player.Nickname}");
            loadout.ActiveAbility?.TryActivateFromInput(player);
        }
        else if (keybind.SettingId == 4)
        {
            loadout.CycleNext();  // これだけでOK
            // UpdateAbilityHint(player, loadout);  ← これを削除！
            AbilityManager.UpdateAbilityHint(player, loadout);  // これに変更
        }
    }

    // HUD 更新（必要に応じて使う）
    private void UpdateAbilityHint(Player player, AbilityLoadout loadout)
    {
        if (player == null || !player.IsConnected || player.Role == RoleTypeId.None)
            return;
    
        // AbilityManager.UpdateAbilityHint に任せる
        AbilityManager.NextSlot(player);  // こっちは既に安全
    }
}