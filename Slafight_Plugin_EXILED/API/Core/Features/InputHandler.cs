using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.ProximityChat;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// Server Specific Settings のキーバインドを、対応する処理へ渡します。
///
/// <b>能力そのものは入力を知りません。</b>
/// <see cref="AbilityBase"/> は「使えるか」「効果は何か」だけを持ち、
/// どのキーで撃つかはここが決めます。入力方法を変えても能力側は無変更で済みます。
/// </summary>
/// <remarks>
/// このクラスはどこからも登録されていません。<see cref="EventHandlerBase"/> を
/// 継承しているだけで <see cref="EventHandlerRegistry"/> が購読させます。
/// </remarks>
public sealed class InputHandler : EventHandlerBase
{
    /// <inheritdoc />
    public override void RegisterEvents()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
    }

    /// <inheritdoc />
    public override void UnregisterEvents()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSettingValueReceived;
    }

    private static void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase setting)
    {
        if (setting is not SSKeybindSetting { SyncIsPressed: true } keybind) return;

        if (Player.Get(hub) is not { } player || !player.IsSafePlayer()) return;

        // ボイスの切り替えは死んでいても許可する。
        if (keybind.SettingId == ServerSpecifics.ProximityChatKeybindSettingId)
        {
            ActivateHandler.ToggleProximityChat(player);

            return;
        }

        if (!player.IsAlive) return;

        if (keybind.SettingId == ServerSpecifics.AbilityUseKeybindSettingId)
        {
            UseActiveAbility(player);

            return;
        }

        if (keybind.SettingId == ServerSpecifics.AbilitySwitchKeybindSettingId)
        {
            AbilityBase.SelectNext(player);

            return;
        }

        if (keybind.SettingId == ServerSpecifics.AbilityOptionPreviousKeybindSettingId)
        {
            SwitchOption(player, AbilityOptionDirection.Previous);

            return;
        }

        if (keybind.SettingId == ServerSpecifics.AbilityOptionNextKeybindSettingId)
        {
            SwitchOption(player, AbilityOptionDirection.Next);
        }
    }

    /// <summary>
    /// いま選んでいる能力の選択肢を送ります。
    /// </summary>
    private static void SwitchOption(Player player, AbilityOptionDirection direction)
    {
        if (AbilityBase.Active(player) is not { } ability) return;

        if (!ability.TrySwitchOption(direction)) return;

        player.ShowHint(
            $"<size=22>{ability.DisplayName}: <color=#8fdcff>{ability.SelectedOption?.Name}</color></size>",
            2f);
    }

    /// <summary>
    /// いま選んでいる能力を使います。使えなかった理由は本人にだけ返します。
    /// </summary>
    private static void UseActiveAbility(Player player)
    {
        if (AbilityBase.Active(player) is not { } ability)
            return;

        if (!ability.TryUse(out string failureReason) && failureReason is { Length: > 0 })
            player.ShowHint($"<size=22>{ability.DisplayName}: {failureReason}</size>", 2f);
    }
}
