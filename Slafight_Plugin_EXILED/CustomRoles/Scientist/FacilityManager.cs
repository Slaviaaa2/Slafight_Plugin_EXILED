using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class FacilityManager : CRole
{
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scientist);
        player.UniqueRole = "FacilityManager";
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        Log.Debug("Giving Items to FacilityManager");
        player.AddItem(ItemType.GunCrossvec);
        player.AddItem(ItemType.KeycardFacilityManager);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.ArmorCombat);
        player.AddItem(ItemType.Radio);
        var pos = Room.Get(RoomType.EzChef).WorldPosition(new Vector3(0f,1f,0f));
        player.Position = pos;
        Log.Debug($"RoomPos: {pos},FacilityManager pos: {player.Position}");
            
        player.CustomInfo = "Facility Manager";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#dc143c>施設管理者</color>\n施設を統括する重要な科学者",10f);
        });
    }
}