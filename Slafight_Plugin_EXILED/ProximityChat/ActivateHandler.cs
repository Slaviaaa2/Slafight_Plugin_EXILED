using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.ProximityChat;

public class ActivateHandler
{
    public ActivateHandler()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
    }

    ~ActivateHandler()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSettingValueReceived;
    }

    public static void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase @base)
    {
        var keybindSetting = @base as SSKeybindSetting;
        if (keybindSetting == null || !keybindSetting.SyncIsPressed)
        {
            return;
        }
        Log.Debug(keybindSetting.SettingId);
        if (keybindSetting.SettingId == 1)
        {
            var player = Player.Get(hub);
            if (player != null)
            {
                if (Handler.CanUsePlayers.Contains(player))
                {
                    if (Handler.ActivatedPlayers.Contains(player))
                    {
                        if (player?.UniqueRole != "Zombified")
                        {
                            Handler.ActivatedPlayers.Remove(player);
                            player.ShowHint("近接チャットが<color=red>無効化</color>されました",5f);
                        }
                        else
                        {
                            if (!Handler.ActivatedPlayers.Contains(player))
                            {
                                Handler.ActivatedPlayers.Add(player);
                            }
                            player.ShowHint("近接チャットが無効化出来ませんでした",5f);
                        }
                    }
                    else
                    {
                        Handler.ActivatedPlayers.Add(player);
                        player.ShowHint("近接チャットが<color=green>有効化</color>されました",5f);
                    }
                }
            }
        }
    }
}