using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp999Role : CRole
{
    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.SpawningRagdoll += CencellRagdoll;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.SpawningRagdoll -= CencellRagdoll;
        base.UnregisterEvents();
    }

    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Tutorial,RoleSpawnFlags.All);
        player.UniqueRole = "Scp999";
        player.MaxHealth = 99999;
        player.Health = player.MaxHealth;
        player.ClearInventory();

        player.SetCustomInfo("SCP-999");

        player.Position = Door.Get(DoorType.Scp173NewGate).Position + new Vector3(0f, 1f, 0f);
        
        Plugin.Singleton.LabApiHandler.Schem999(LabApi.Features.Wrappers.Player.Get(player.ReferenceHub));
    }
    
    private void CencellRagdoll(SpawningRagdollEventArgs ev)
    {
        if (ev.Player?.GetCustomRole() == CRoleTypeId.Scp999)
            ev.IsAllowed = false;
    }
}