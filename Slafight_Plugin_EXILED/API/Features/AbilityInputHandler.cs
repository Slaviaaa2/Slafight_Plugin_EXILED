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
            loadout.CycleNext();
            UpdateAbilityHint(player, loadout);
        }
    }

    // HUD 更新（必要に応じて使う）
    private void UpdateAbilityHint(Player player, AbilityLoadout loadout)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < AbilityLoadout.MaxSlots; i++)
        {
            var ability = loadout.Slots[i];
            if (ability == null)
                continue;

            string name = ability.GetType().Name;
            string marker = (i == loadout.ActiveIndex) ? "<color=#ffff00>▶</color>" : "　";
            sb.AppendLine($"{marker} {name}");
        }

        string text = sb.Length > 0 ? sb.ToString() : string.Empty;
        Plugin.Singleton.PlayerHUD.HintSync(SyncType.PHUD_Specific, text, player);
    }
}