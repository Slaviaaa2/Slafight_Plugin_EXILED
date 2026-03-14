using System.Collections.Generic;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.MainHandlers;

namespace Slafight_Plugin_EXILED.API.Features;

/// <summary>
/// 特定の SpawnTypeId に対する部隊編成（ロール構成）セット。
/// 再利用可能な「部隊プリセット」。
/// </summary>
public class UnitPack
{
    public string Name { get; }

    // SpawnTypeId ごとの RoleTable（SpawnSystem が期待する形式）
    public Dictionary<SpawnTypeId, Dictionary<SpawnSystem.SpawnRoleKey, (float maxCount, bool guaranteed)>> RoleTables { get; }

    public UnitPack(
        string name,
        Dictionary<SpawnTypeId, Dictionary<SpawnSystem.SpawnRoleKey, (float maxCount, bool guaranteed)>> roleTables)
    {
        Name = name;
        RoleTables = roleTables;
    }
}