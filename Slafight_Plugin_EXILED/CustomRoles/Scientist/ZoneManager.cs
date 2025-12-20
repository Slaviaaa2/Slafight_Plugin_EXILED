using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class ZoneManager
{
    public void SpawnRole(Player player)
    {
        player.Role.Set(RoleTypeId.Scientist);
        player.UniqueRole = "ZoneManager";
        Timing.CallDelayed(0.05f, () =>
        {
            player.MaxHealth = 100;
            player.Health = player.MaxHealth;
            player.ClearInventory();
            Log.Debug("Giving Items to ZoneManager");
            player.AddItem(ItemType.GunFSP9);
            player.AddItem(ItemType.KeycardZoneManager);
            player.AddItem(ItemType.KeycardScientist);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.ArmorLight);
            player.AddItem(ItemType.Radio);
            var SelectZone = Random.Range(0,2);
            if (SelectZone==0)
            {
                var pos = Room.Get(RoomType.LczCafe).WorldPosition(new Vector3(0f,1f,0f));
                player.Position = pos;
                Log.Debug($"RoomPos: {pos},ZoneManager pos: {player.Position}");
            }
            else if (SelectZone==1)
            {
                var pos = Room.Get(RoomType.HczHid).WorldPosition(new Vector3(0f,1f,0f));
                player.Position = pos;
                Log.Debug($"RoomPos: {pos},ZoneManager pos: {player.Position}");
            }
            
            player.CustomInfo = "Zone Manager";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            Timing.CallDelayed(0.05f, () =>
            {
                player.ShowHint("<color=#00ffff>区画管理者</color>\n各区画に割り当てられた軽度な権限をもつ科学者",10f);
            });
        });
    }
}