namespace Slafight_Plugin_EXILED.API.Features;

public static class CassieHelper
{
    public static void AnnounceNtfArrival(string callsignCassie, string callsignLocalized)
    {
        Exiled.API.Features.Cassie.MessageTranslated(
            $"MtfUnit Epsilon 11 Designated {callsignCassie} HasEntered AllRemaining",
            $"<color=#5bc5ff>機動部隊Epsilon-11 \"九尾狐\"-{callsignLocalized}</color>が施設に到着しました。残存する全職員は、機動部隊が目的地に到着するまで、標準避難プロトコルに従って行動してください。",
            true);
    }

    public static void AnnounceNtfBackup()
    {
        Exiled.API.Features.Cassie.MessageTranslated(
            "Ninetailedfox Backup unit has entered the facility .",
            "<color=#5bc5ff>九尾狐 予備部隊</color>が施設に到着しました。",
            true);
    }
    
    public static void AnnounceHdArrival(string callsignCassie, string callsignLocalized)
    {
        Exiled.API.Features.Cassie.MessageTranslated(
            $"MtfUnit Nu 7 Designated {callsignCassie} HasEntered AllRemaining This Forces Work Epsilon 11 Task and operated by O5 Command . for Big Containment Breachs .",
            $"<b><color=#353535>機動部隊Nu-7 \"下される鉄槌 - ハンマーダウン\"-{callsignLocalized}</color></b>が施設に到着しました。残存する全職員は、機動部隊が目的地に到着するまで、標準避難プロトコルに従って行動してください。" +
            $"<split>本部隊は<color=#5bc5ff>Epsilon-11 \"九尾狐\"</color>の任務の代替として大規模な収容違反の対応の為O5評議会に招集されました。",
            true);
    }

    public static void AnnounceHdBackup()
    {
        Exiled.API.Features.Cassie.MessageTranslated(
            "Her man down Backup unit has entered the facility .",
            "<b><color=#353535>下される鉄槌 - ハンマーダウン 予備部隊</color></b>が施設に到着しました。",
            true);
    }
    
    public static void AnnounceChaos(int count)
    {
        Exiled.API.Features.Cassie.MessageTranslated(
            $"Attention All personnel . Detected {count} Chaos Insurgency Forces in Gate A .",
            $"全職員に通達。Gate Aに{count}人のカオス・インサージェンシー部隊が検出されました。",
            true);
    }

    public static void AnnounceFifthist(int count)
    {
        Exiled.API.Features.Cassie.MessageTranslated(
            $"Attention All personnel . Detected {count} $pitch_1.05 5 5 5 $pitch_1 Forces in Gate B .",
            $"全職員に通達。Gate Bに{count}人の第五主義者が検出されました。",
            true);
    }
}