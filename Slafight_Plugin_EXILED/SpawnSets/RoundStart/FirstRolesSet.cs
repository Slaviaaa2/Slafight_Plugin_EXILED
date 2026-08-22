using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Core.Features;
using Slafight_Plugin_EXILED.API.Core.Structs;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.SpawnSets.RoundStart;

public class FirstRolesHumanSet : SpawnSet
{
    public override string Name => "First Spawn Human Roles";
    public override IReadOnlyList<SpawnSetRoleDefinition> SpawnRoles =>
    [
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.ClassD, count: 99),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.Scientist, count: 99),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.FacilityGuard, count: 99),
    ];
}

public class FirstRolesSCPsSet : SpawnSet
{
    public override string Name => "First Spawn SCPs Roles";
    public override int AllowedPlayerCount => Player.List.Count(p => p.IsSafePlayer()) switch
    {
        < 4 => 1,
        < 8 => 2,
        < 12 => 3,
        _ => 4,
    };

    public override IReadOnlyList<SpawnSetRoleDefinition> SpawnRoles =>
    [
        // 重みは master の RoleTableContext.DefaultScpRoles() に準拠。
        // SCP は 1 体につき 1 人なので count はすべて 1。
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.Scp173, count: 1, weight: 1.15f),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.Scp106, count: 1, weight: 1.1f),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.Scp939, count: 1, weight: 1.1f),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.Scp049, count: 1, weight: 1.08f),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.Scp079, count: 1, weight: 1.05f),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.Scp3114, count: 1, weight: 0.95f),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.Scp096, count: 1, weight: 0.85f),

        // ▼ カスタム SCP を実装したらここを開ける
        // SpawnSetRoleDefinition.Custom<Scp173>(),
        // SpawnSetRoleDefinition.Custom<Scp106>(),
        // SpawnSetRoleDefinition.Custom<Scp049>(),
        // SpawnSetRoleDefinition.Custom<Scp079>(),
        // SpawnSetRoleDefinition.Custom<Scp3114>(),
        //
        // // 特殊な SCP は出現頻度を抑える。
        // SpawnSetRoleDefinition.Custom<Scp682>(weight: 0.5f),
        // SpawnSetRoleDefinition.Custom<Scp966>(weight: 0.5f),
        // SpawnSetRoleDefinition.Custom<Scp035>(weight: 0.4f),
        // SpawnSetRoleDefinition.Custom<Scp610>(weight: 0.3f),
        // SpawnSetRoleDefinition.Custom<Scp3005>(weight: 0.3f),
    ];
}