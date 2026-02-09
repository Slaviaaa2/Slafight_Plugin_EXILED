using System.Collections.Generic;
using Exiled.API.Features;
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

    public static Dictionary<Player, string> Passcodes = new();
    
    public static void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase @base)
    {
        var textSettings = @base as SSPlaintextSetting;
        if (textSettings == null || textSettings.SyncInputText == null)
        {
            return;
        }
        Log.Debug("SSPlainText Setting");
        if (textSettings.SettingId == 2)
        {
            var player = Player.Get(hub);
            if (player != null)
            {
                Log.Debug("nickname updated");
                var realName = player.Nickname;
                if (!string.IsNullOrEmpty(textSettings.SyncInputText))
                {
                    player.CustomName = $"{textSettings.SyncInputText} ({realName})";
                }
                else
                {
                    player.CustomName = $"{realName}";
                }
            }
        }
        else if (textSettings.SettingId == 5)
        {
            var player = Player.Get(hub);
            if (player != null)
            {
                Log.Debug("passcode updated");
                Passcodes[player] = textSettings.SyncInputText;
            }
        }
    }
}