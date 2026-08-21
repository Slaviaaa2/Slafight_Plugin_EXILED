using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
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
        if (Player.Get(hub) is not { } player || !player.IsSafePlayer()) return;

        // テキスト欄は押下ではなく入力確定で届く。キーバインドとは別に拾う。
        if (setting is SSPlaintextSetting { SyncInputText: not null } text)
        {
            HandleText(player, text.SettingId, text.SyncInputText);

            return;
        }

        if (setting is not SSKeybindSetting { SyncIsPressed: true } keybind) return;

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

            return;
        }

        if (keybind.SettingId == ServerSpecifics.SuicideButtonKeybindSettingId)
        {
            Suicide(player);
        }
    }

    /// <summary>
    /// 銃を構えている間だけ、自害できます。
    /// </summary>
    private static void Suicide(Player player)
    {
        if (player.CurrentItem is not Firearm firearm)
            return;

        try
        {
            if (firearm.FirearmType is not (FirearmType.None or FirearmType.ParticleDisruptor))
            {
                player.PlayGunSound(firearm.FirearmType);
                SpeakerApi.Play("suicide_shot.ogg", $"{player.NetId}_suicideShotSound", player.Position, true);
            }
        }
        catch (Exception exception)
        {
            // 音が出せなくても自害そのものは通す。
            Log.Warn($"[Slafight] 自害音の再生に失敗しました ({player.Nickname}): {exception.Message}");
        }

        player.Kill("自害した");
    }

    /// <summary>
    /// いま選んでいる能力の選択肢を送ります。
    /// </summary>
    private static void SwitchOption(Player player, AbilityOptionDirection direction)
    {
        if (AbilityBase.Active(player) is not { } ability) return;

        if (!ability.TrySwitchOption(direction)) return;

        CoreHints.Show(
            player,
            $"<size=22>{ability.DisplayName}: <color=#8fdcff>{ability.SelectedOption?.Name}</color></size>",
            2f);
    }

    /// <summary>
    /// テキスト設定の入力を受け取ります。
    /// </summary>
    private static void HandleText(Player player, int settingId, string text)
    {
        if (settingId == ServerSpecifics.RpNameSettingId)
        {
            RPNameSetter.SetInputName(player, text);

            return;
        }

        if (settingId == ServerSpecifics.SecretPasscodeSettingId)
        {
            RPNameSetter.SetPasscode(player, text);
        }
    }

    /// <summary>
    /// いま選んでいる能力を使います。使えなかった理由は本人にだけ返します。
    /// </summary>
    private static void UseActiveAbility(Player player)
    {
        if (AbilityBase.Active(player) is not { } ability)
            return;

        if (!ability.TryUse(out string failureReason) && failureReason is { Length: > 0 })
            CoreHints.Show(player, $"<size=22>{ability.DisplayName}: {failureReason}</size>", 2f);
    }
}
