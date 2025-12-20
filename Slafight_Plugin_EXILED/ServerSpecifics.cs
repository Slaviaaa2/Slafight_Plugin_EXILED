using System;
using System.Collections.Generic;
using System.Net.Mime;
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
            new SSKeybindSetting(1,"近接チャット",KeyCode.LeftAlt,true,false,hint:"一部の利用可能ロールで、近接チャットを使用するのに必要です。左Altを推奨します"),
            new SSPlaintextSetting(2,"キャラクター名","",20,hint:"RPのキャラ名です。設定した名前の後に本当の名前が表示されます。")
        };
        return SettingsList.ToArray();
    }
}