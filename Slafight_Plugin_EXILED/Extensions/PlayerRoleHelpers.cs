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
        { "Scp3114",      CRoleTypeId.None },           // 3114はRoleTypeIdで扱ってるならNoneでもOK
        { "Scp966",       CRoleTypeId.Scp966 },
        { "Scp682",       CRoleTypeId.None },           // 682ロールを使うなら CRoleTypeId を追加してもよい
        { "Zombified",    CRoleTypeId.None },

        // Fifthists
        { "SCP-3005",     CRoleTypeId.Scp3005 },
        { "FIFTHIST",     CRoleTypeId.FifthistRescure },
        { "F_Priest",     CRoleTypeId.FifthistPriest },

        // Chaos Insurgency
        { "CI_Commando",  CRoleTypeId.ChaosCommando },

        // Foundation Forces
        { "NtfAide",      CRoleTypeId.NtfLieutenant },
        { "HdInfantry",   CRoleTypeId.HdInfantry },
        { "HdCommander",  CRoleTypeId.HdCommander },

        // Scientists
        { "ZoneManager",      CRoleTypeId.ZoneManager },
        { "FacilityManager",  CRoleTypeId.FacilityManager },

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
}
