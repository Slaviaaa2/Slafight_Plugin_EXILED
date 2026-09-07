using System;
using System.Collections.Generic;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces.MTFs.Epsilon11;

public class NtfCaptain : CustomRole
{
    public override string Name => "Nine-tailed Fox Captain";
    public override RoleTypeId BaseRole => RoleTypeId.NtfCaptain;
    public override int ForceRolePower => 3;
    public override IReadOnlyList<ItemType> Items =>
    [
        
    ];
    public override IReadOnlyList<Type> CustomItems =>
    [
        
    ];
}