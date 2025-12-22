using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class EvacuationGuard : CRole
{
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.FacilityGuard);
        player.UniqueRole = "EvacuationGuard";
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        Log.Debug("Giving Items to EvacuationGuard");
        player.AddItem(ItemType.GunFSP9);
        player.AddItem(ItemType.KeycardGuard);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.Painkillers);
        player.AddItem(ItemType.ArmorCombat);
        player.AddItem(ItemType.Radio);
        var pos = Room.Get(RoomType.LczArmory).WorldPosition(new Vector3(0f,1f,0f));
        player.Position = pos;
        Log.Debug($"RoomPos: {pos},EvacuationManager pos: {player.Position}");
            
        player.CustomInfo = "Emergency Evacuation Guard";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#00b7eb>避難支援警備員</color>\n下層の秩序を守り、職員の避難を助ける。",10f);
        });
    }
}