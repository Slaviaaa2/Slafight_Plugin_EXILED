using System.Collections.Generic;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED;

public class ServerSpecifics
{
    public static ServerSpecificSettingBase[] Settings()
    {
        List<ServerSpecificSettingBase> SettingsList = new List<ServerSpecificSettingBase>()
        {
            new SSGroupHeader(0,"シャープ鯖"),
            new SSKeybindSetting(1,"近接チャット",KeyCode.LeftAlt,true,false)
        };
        return SettingsList.ToArray();
    }
}