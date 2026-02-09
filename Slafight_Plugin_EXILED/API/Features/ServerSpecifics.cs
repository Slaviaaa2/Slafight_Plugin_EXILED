using System.Collections.Generic;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.API.Features;

public abstract class ServerSpecifics
{
    public static ServerSpecificSettingBase[] Settings()
    {
        var settingsList = new List<ServerSpecificSettingBase>()
        {
            new SSGroupHeader(0,"シャープ鯖"),
            new SSKeybindSetting(1,"近接チャット",KeyCode.V,true,false,hint:"一部の利用可能ロールで、近接チャットを使用するのに必要です。Vを推奨します"),
            new SSPlaintextSetting(2,"キャラクター名","",20,hint:"RPのキャラ名です。設定した名前の後に本当の名前が表示されます。"),
            new SSKeybindSetting(3,"アビリティ使用",KeyCode.LeftAlt,true,false,hint:"一部の利用可能ロールで、アビリティを使用するのに必要です。左Altを推奨します"),
            new SSKeybindSetting(4,"アビリティ切り替え",KeyCode.Mouse2,true,false,hint:"複数アビリティを所持している際の切り替えボタンです。中マウスボタンを推奨します"),
            new SSPlaintextSetting(5, "シークレットパスコード","00000", 5, hint:"特別な場面で必要となるかもしれません・・・")
        };
        return settingsList.ToArray();
    }
}
