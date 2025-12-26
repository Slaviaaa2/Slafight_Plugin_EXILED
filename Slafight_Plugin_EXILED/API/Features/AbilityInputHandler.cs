using System.Text;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Hints;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.API.Features;

public static class AbilityInputHandler
{
    // クラスが最初に参照されたタイミングで1回だけ呼ばれる
    static AbilityInputHandler()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
    }

    // HUD 操作用に PlayerHUD へアクセスする前提
    // Plugin.Singleton.PlayerHUD みたいなプロパティがある想定
    private static PlayerHUD Hud => Plugin.Singleton.PlayerHUD;

    private static void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase @base)
    {
        var keybind = @base as SSKeybindSetting;
        if (keybind == null || !keybind.SyncIsPressed)
            return;

        var player = Player.Get(hub);
        if (player == null)
            return;

        // ロードアウトが無ければアビリティ無し扱い
        if (!AbilityManager.Loadouts.TryGetValue(player.Id, out var loadout))
            return;

        // アビリティ使用
        if (keybind.SettingId == 3)
        {
            loadout.ActiveAbility?.TryActivateFromInput(player);
            return;
        }

        // アビリティ切り替え
        if (keybind.SettingId == 4)
        {
            loadout.CycleNext();
            UpdateAbilityHint(player, loadout);
        }
    }

    /// <summary>
    /// 現在のアビリティ一覧＋選択中をHUDに表示。
    /// </summary>
    private static void UpdateAbilityHint(Player player, AbilityLoadout loadout)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < AbilityLoadout.MaxSlots; i++)
        {
            var ability = loadout.Slots[i];
            if (ability == null)
                continue;

            // 表示名（必要なら AbilityBase に Name プロパティを追加してもよい）
            string name = ability.GetType().Name;

            // 選択中スロットにはマーカーを付ける
            string marker = (i == loadout.ActiveIndex) ? "<color=#ffff00>▶</color>" : "　";

            sb.AppendLine($"{marker} {name}");
        }

        // 何もなければ消す
        string text = sb.Length > 0 ? sb.ToString() : string.Empty;

        // PHUD_Specific に流す（PlayerHUD.HintSync を利用）
        Hud.HintSync(SyncType.PHUD_Specific, text, player);
    }

    // 必要なら解除用API
    public static void Unregister()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSettingValueReceived;
    }
}