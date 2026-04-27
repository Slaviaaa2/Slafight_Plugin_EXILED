using System.Collections.Generic;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.MainHandlers;

public class RPNameSetter
{
    public RPNameSetter()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
    }

    ~RPNameSetter()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSettingValueReceived;
    }

    public static Dictionary<Player, string> PlayerInputNames = new();
    public static Dictionary<Player, string> Passcodes = new();
    
    public static void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase @base)
    {
        // 型チェック & 空文字チェック
        if (@base is not SSPlaintextSetting textSettings || textSettings.SyncInputText == null)
            return;

        Log.Debug("SSPlainText Setting");

        var player = Player.Get(hub);
        if (player == null || !player.IsConnected)
            return;

        // Sergey等でRPName無効化中なら安全にスキップ
        var flags = player.Get();
        if (flags == null)
        {
            // ラウンド開始直後やDestroy直後に来た場合用の保険
            Log.Debug($"[RPNameSetter] Flags null for {player.Nickname}, skipping.");
        }

        if (textSettings.SettingId == 2)
        {
            // RPNameDisabledなら触らない

            Log.Debug("nickname updated");

            var realName = player.Nickname;
            var rp = textSettings.SyncInputText;


            if (!(flags != null && flags.Contains(SpecificFlagType.RPNameDisabled)))
            {
                if (!string.IsNullOrEmpty(rp))
                    player.CustomName = $"{rp} ({realName})";
                else
                    player.CustomName = realName;   
            }
            
            PlayerInputNames[player] = player.CustomName;
        }
        else if (textSettings.SettingId == 5)
        {
            Log.Debug("passcode updated");
            Passcodes[player] = textSettings.SyncInputText;
        }
    }
}