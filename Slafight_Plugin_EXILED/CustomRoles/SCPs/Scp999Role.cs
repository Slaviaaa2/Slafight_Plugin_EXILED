using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp999Role : CRole
{
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
        player.Role.Set(RoleTypeId.Tutorial,RoleSpawnFlags.All);
        player.UniqueRole = "Scp999";
        player.MaxHealth = 99999;
        player.Health = player.MaxHealth;
        player.ClearInventory();

        player.SetCustomInfo("SCP-999");

        player.Position = Door.Get(DoorType.Scp173NewGate).Position + new Vector3(0f, 0.1f, 0f);
        player.Scale = new Vector3(0.01f, 0.8f, 0.01f);
    }
}