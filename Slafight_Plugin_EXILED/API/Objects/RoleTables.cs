using System.Collections.Generic;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;

namespace Slafight_Plugin_EXILED;

public static class RoleTables
{
    // FirstSpawnHandler時のスポーンテーブル。
    public static readonly List<object> ScpRoles = new()
    {
        RoleTypeId.Scp173,
        RoleTypeId.Scp049,
        RoleTypeId.Scp079,
        RoleTypeId.Scp096,
        RoleTypeId.Scp106,
        RoleTypeId.Scp939,
        RoleTypeId.Scp3114,
        CRoleTypeId.Scp3005,
        CRoleTypeId.Scp966,
        CRoleTypeId.Scp682
    };

    public static readonly List<object> ScientistRoles = new()
    {
        RoleTypeId.Scientist,
        CRoleTypeId.ZoneManager,
        CRoleTypeId.FacilityManager,
        CRoleTypeId.Engineer
    };

    public static readonly List<object> GuardRoles = new()
    {
        RoleTypeId.FacilityGuard,
        CRoleTypeId.EvacuationGuard,
        CRoleTypeId.SecurityChief
    };

    public static readonly List<object> ClassDRoles = new()
    {
        RoleTypeId.ClassD,
        CRoleTypeId.Janitor
    };
}