using System;
using Slafight_Plugin_EXILED.API.Enums;

namespace Slafight_Plugin_EXILED.Extensions;

public struct CustomTeamInfo
{
    public CTeam Team;
    public string TeamName;
    public string CassieString;
    public string TeamColor;
}
public static class CustomTeamUtils
{
    public static CustomTeamInfo GetTeamInfo(CTeam team)
    {
        return new CustomTeamInfo
        {
            Team = team,
            TeamName = GetTeamName(team),
            CassieString = GetTeamCassie(team),
            TeamColor = GetTeamColor(team)
        };
    }
    public static string GetTeamName(CTeam team)
    {
        return team switch
        {
            CTeam.Null => "不明な勢力",
            CTeam.FoundationForces => "機動部隊",
            CTeam.Scientists => "科学者",
            CTeam.ClassD => "Dクラス職員",
            CTeam.Guards => "警備員",
            CTeam.ChaosInsurgency => "カオス・インサージェンシー",
            CTeam.Fifthists => "第五教会",
            CTeam.GoC => "世界オカルト連合",
            CTeam.UIU => "連邦捜査局(FBI)異常事件課",
            CTeam.SerpentsHand => "サーペント・ハンド",
            CTeam.SCPs => "SCP",
            CTeam.Others => "不明な勢力",
            CTeam.BrokenGodChurch => "壊れた神の教会",
            CTeam.O5 => "O5評議会",
            CTeam.Sarkic => "サーキック・カルト",
            CTeam.AWCY => "Are We Cool Yet?",
            CTeam.BlackQueen => "黒の女王",
            _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
        };
    }
    public static string GetTeamCassie(CTeam team)
    {
        return team switch
        {
            CTeam.Null => "Unknown Forces",
            CTeam.FoundationForces => "MtfUnit",
            CTeam.Scientists => "Scientist Personnel",
            CTeam.ClassD => "Class D Personnel",
            CTeam.Guards => "Facility Guard Personnel",
            CTeam.ChaosInsurgency => "Chaos Insurgency",
            CTeam.Fifthists => "$pitch_1.05 5 5 5 $pitch_1 Forces",
            CTeam.GoC => "G O C",
            CTeam.UIU => "U I U",
            CTeam.SerpentsHand => "Serpents Hand",
            CTeam.SCPs => "SCP",
            CTeam.Others => "Unknown Forces",
            CTeam.BrokenGodChurch => "Black God Charge",
            CTeam.O5 => "O5 Command",
            CTeam.Sarkic => "SAW KEY CARD",
            CTeam.AWCY => "Are were code yet",
            CTeam.BlackQueen => "Black Q been",
            _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
        };
    }
    public static string GetTeamColor(CTeam team)
    {
        return team switch
        {
            CTeam.Null => "#ffffff",
            CTeam.FoundationForces => "#00b7eb",
            CTeam.Scientists => "#faff86",
            CTeam.ClassD => "#ee7600",
            CTeam.Guards => "#00b7eb",
            CTeam.ChaosInsurgency => "#228b22",
            CTeam.Fifthists => "#ff00fa",
            CTeam.GoC => "#0000c8",
            CTeam.UIU => "",
            CTeam.SerpentsHand => "",
            CTeam.SCPs => "#c50000",
            CTeam.Others => "#ffffff",
            CTeam.BrokenGodChurch => "",
            CTeam.O5 => "#000000",
            CTeam.Sarkic => "",
            CTeam.AWCY => "",
            CTeam.BlackQueen => "#000000",
            _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
        };
    }
}