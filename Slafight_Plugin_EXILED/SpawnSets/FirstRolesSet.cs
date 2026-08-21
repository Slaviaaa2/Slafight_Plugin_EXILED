using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Core.Features;
using Slafight_Plugin_EXILED.API.Core.Structs;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.SpawnSets;

public class FirstRolesHumanSet : SpawnSet
{
    public override string Name => "First Spawn Human Roles";
    public override IReadOnlyList<SpawnSetRoleDefinition> SpawnRoles =>
    [
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.ClassD, count: 99, weight: 4f),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.Scientist, count: 99, weight: 2f),
        SpawnSetRoleDefinition.Vanilla(RoleTypeId.FacilityGuard, count: 99, weight: 3f),
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