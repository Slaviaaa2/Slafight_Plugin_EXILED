using System;
using System.Collections.Generic;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;

namespace Slafight_Plugin_EXILED.Extensions;

public static class PlayerRoleHelpers
{
    // UniqueRole文字列 → CRoleTypeId のマップ
    private static readonly Dictionary<string, CRoleTypeId> UniqueToCustomMap
        = new(StringComparer.OrdinalIgnoreCase)
    {
        // SCiPs
        { "Scp096_Anger", CRoleTypeId.Scp096Anger },
        { "Scp3114",      CRoleTypeId.Scp3114 },           // 3114はRoleTypeIdで扱ってるならNoneでもOK
        { "Scp966",       CRoleTypeId.Scp966 },
        { "Scp682",       CRoleTypeId.Scp682 },           // 682ロールを使うなら CRoleTypeId を追加してもよい
        { "Zombified",    CRoleTypeId.Zombified },
        { "Scp106",       CRoleTypeId.Scp106 },
        { "Scp999",       CRoleTypeId.Scp999 },

        // Fifthists
        { "SCP-3005",        CRoleTypeId.Scp3005 },
        { "FIFTHIST",        CRoleTypeId.FifthistRescure },
        { "F_Priest",        CRoleTypeId.FifthistPriest },
        { "FifthistConvert", CRoleTypeId.FifthistConvert },

        // Chaos Insurgency
        { "CI_Commando",  CRoleTypeId.ChaosCommando },
        { "ChaosSignal",  CRoleTypeId.ChaosSignal },

        // Foundation Forces
        { "NtfAide",      CRoleTypeId.NtfLieutenant },
        { "NtfGeneral",   CRoleTypeId.NtfGeneral },
        { "HdInfantry",   CRoleTypeId.HdInfantry },
        { "HdCommander",  CRoleTypeId.HdCommander },
        { "HdMarshal",    CRoleTypeId.HdMarshal },

        // Scientists
        { "ZoneManager",      CRoleTypeId.ZoneManager },
        { "FacilityManager",  CRoleTypeId.FacilityManager },
        { "Engineer",         CRoleTypeId.Engineer},

        // Facility Guards
        { "EvacuationGuard",  CRoleTypeId.EvacuationGuard },

        // Class-D
        { "Janitor",      CRoleTypeId.Janitor },

        // Other
        { "SnowWarrier",  CRoleTypeId.SnowWarrier },
    };

    /// <summary>
    /// 現在のプレイヤーのカスタムロール(CRoleTypeId)を取得する。
    /// 対応するUniqueRoleが無ければ null。
    /// </summary>
    public static CRoleTypeId GetCustomRole(this Player player)
    {
        if (string.IsNullOrEmpty(player.UniqueRole))
            return CRoleTypeId.None;

        return UniqueToCustomMap.TryGetValue(player.UniqueRole, out var cr)
            ? cr
            : CRoleTypeId.None;
    }

    /// <summary>
    /// 通常ロール/カスタムロール両方まとめて欲しい場合の情報。
    /// </summary>
    public struct PlayerRoleInfo
    {
        public RoleTypeId Vanilla;
        public CRoleTypeId? Custom;
    }

    public static PlayerRoleInfo GetRoleInfo(this Player player)
    {
        return new PlayerRoleInfo
        {
            Vanilla = player.Role.Type,
            Custom  = player.GetCustomRole()
        };
    }

    /// <summary>
    /// 現在のプレイヤーのカスタムチーム(CTeam)を取得する。
    /// 対応するCRoleTypeIdが無ければCTeam.Othersになる。
    /// </summary>
    public static CTeam GetTeam(this Player player)
    {
        if (player == null) return CTeam.Null;
        var role = player.GetRoleInfo();
        if (role.Custom == CRoleTypeId.None)
        {
            var defaultTeams = player.Role.Team;
            switch (defaultTeams)
            {
                case Team.SCPs:
                    return CTeam.SCPs;
                case Team.FoundationForces:
                    return CTeam.FoundationForces;
                case Team.ChaosInsurgency:
                    return CTeam.ChaosInsurgency;
                case Team.Scientists:
                    return CTeam.Scientists;
                case Team.ClassD:
                    return CTeam.ClassD;
                case Team.Dead:
                    return CTeam.Others;
                case Team.OtherAlive:
                    return CTeam.Others;
            }
        }
        else
        {
            switch (role.Custom)
            {
                // None
                case CRoleTypeId.None:
                    return CTeam.Others;
                // SCPs
                case CRoleTypeId.Scp096Anger:
                    return CTeam.SCPs;
                case CRoleTypeId.Scp3005:
                    return CTeam.SCPs;
                case CRoleTypeId.Scp966:
                    return CTeam.SCPs;
                case CRoleTypeId.Scp682:
                    return CTeam.SCPs;
                case CRoleTypeId.Scp999:
                    return CTeam.SCPs;
                case CRoleTypeId.Scp106:
                    return CTeam.SCPs;
                case CRoleTypeId.Scp3114:
                    return CTeam.SCPs;
                // Fifthists
                case CRoleTypeId.FifthistRescure:
                    return CTeam.Fifthists;
                case CRoleTypeId.FifthistPriest:
                    return CTeam.Fifthists;
                case CRoleTypeId.FifthistConvert:
                    return CTeam.Fifthists;
                // Chaos Insurgency
                case CRoleTypeId.ChaosCommando:
                    return CTeam.ChaosInsurgency;
                case CRoleTypeId.ChaosSignal:
                    return CTeam.ChaosInsurgency;
                // Foundation Forces
                case CRoleTypeId.NtfLieutenant:
                    return CTeam.FoundationForces;
                case CRoleTypeId.NtfGeneral:
                    return CTeam.FoundationForces;
                case CRoleTypeId.HdInfantry:
                    return CTeam.FoundationForces;
                case CRoleTypeId.HdCommander:
                    return CTeam.FoundationForces;
                case CRoleTypeId.HdMarshal:
                    return CTeam.FoundationForces;
                // Guards
                case CRoleTypeId.EvacuationGuard:
                    return CTeam.Guards;
                case CRoleTypeId.SecurityChief:
                    return CTeam.Guards;
                // Class-D Personnel
                case CRoleTypeId.Janitor:
                    return CTeam.ClassD;
                // Scientists
                case CRoleTypeId.ZoneManager:
                    return CTeam.Scientists;
                case CRoleTypeId.FacilityManager:
                    return CTeam.Scientists;
                case CRoleTypeId.Engineer:
                    return CTeam.Scientists;
                // Other Threads
                case CRoleTypeId.SnowWarrier:
                    return CTeam.Others;
            }
        }
        // Fail Safe
        return CTeam.Others;
    }
}
