using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class ChamberGuard : CRole
{
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.FacilityGuard);
        player.UniqueRole = "ChamberGuard";
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        Log.Debug("Giving Items to ChamberGuard");
        player.AddItem(ItemType.GunFSP9);
        player.AddItem(ItemType.KeycardGuard);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.ArmorLight);
        player.AddItem(ItemType.Radio);
        player.AddAmmo(AmmoType.Nato9,100);
        var pos = Door.Get(DoorType.Scp173Connector).Position;
        pos += new Vector3(0f,0.35f,0f);
        player.Position = pos;
        Log.Debug($"RoomPos: {pos},CGuard pos: {player.Position}");
            
        player.SetCustomInfo("Chamber Guard");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#00b7eb>収容室警備</color>\nDクラス職員やオブジェクトの異常を監視する。",10f);
        });
    }
}