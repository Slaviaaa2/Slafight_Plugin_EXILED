using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using InventorySystem;
using MEC;
using PlayerRoles;
using ProjectMER.Commands.Utility;
using Slafight_Plugin_EXILED.API.Enums;
using UnityEngine;

namespace Slafight_Plugin_EXILED.API.Features;

public abstract class CRole
{
    public virtual void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        if (player == null)
        {
            Log.Error($"CRole: SpawnRole failed in {player.Nickname}. Reason: Player is Null");
            return;
        }
        if (roleSpawnFlags == RoleSpawnFlags.None)
        {
            Vector3 savePosition = player.Position + new Vector3(0f,0.1f,0f);
            var items = player.Items.ToList(); 
            var ammos = player.Ammo.ToList();
            Timing.CallDelayed(1f, () =>
            {
                player.Position = savePosition;
                player.ClearInventory();
                foreach (var item in items)
                {
                    player.AddItem(item);
                }
                foreach (var ammo in ammos)
                {
                    player.AddAmmo((AmmoType)ammo.Key, ammo.Value);
                }
            });
        }
        else if (roleSpawnFlags == RoleSpawnFlags.AssignInventory)
        {
            Vector3 savePosition = player.Position + new Vector3(0f,0.1f,0f);
            Timing.CallDelayed(1f, () =>
            {
                player.Position = savePosition;
            });
        }
        else if (roleSpawnFlags == RoleSpawnFlags.UseSpawnpoint)
        {
            var items = player.Items.ToList(); 
            var ammos = player.Ammo.ToList();
            Timing.CallDelayed(1f, () =>
            {
                player.ClearInventory();
                foreach (var item in items)
                {
                    player.AddItem(item);
                }
                foreach (var ammo in ammos)
                {
                    player.AddAmmo((AmmoType)ammo.Key, ammo.Value);
                }
            });
        }
    }
}