using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class SecurityChief : CRole
{
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.FacilityGuard);
        player.UniqueRole = "SecurityChief";
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        Log.Debug("Giving Items to SecurityChief");
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.ArmorCombat);
        player.AddItem(ItemType.Radio);
        player.TryAddCustomItem(1100);
        player.TryAddCustomItem(2000);
        player.AddAmmo(AmmoType.Nato9,180);
        var pos = new Vector3(125f, 296f, -65f);
        player.Position = pos;
            
        player.SetCustomInfo("Security Chief");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#00b7eb>警備主任</color>\n施設内の職員を外に脱出させよう！",10f);
        });
    }
}