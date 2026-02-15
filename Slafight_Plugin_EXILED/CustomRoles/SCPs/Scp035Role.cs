using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Hazards;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp106;
using HintServiceMeow.Core.Utilities;
using MEC;
using Mirror;
using PlayerRoles;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MainHandlers;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp035Role : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.Scp035;
    protected override CTeam Team { get; set; } = CTeam.SCPs;
    protected override string UniqueRoleKey { get; set; } = "Scp035";

    public override void RegisterEvents()
    {
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        base.UnregisterEvents();
    }
    
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scientist);
        player.UniqueRole = UniqueRoleKey;
        player.ClearInventory();
        player.SetCustomInfo("SCP-035");
        
        player.Wear("SCP035", new Vector3(0f, 0.8f, 0f));

        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=red>SCP-035</color>\n愚かな博士が仮面をつけて乗っ取れた！\nアビリティで触手を出すことが出来るぞ！\n脱出してこんなクソッタレた施設から逃げましょう。",10f);
        });
    }
}