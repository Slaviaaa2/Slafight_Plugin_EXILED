using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class FifthistConvert : CRole
{
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Tutorial);
        Vector3 offset;
        int MaxHealth = 150;
        
        player.UniqueRole = "FifthistConvert";
        player.SetCustomInfo("<color=#FF0090>Fifthist Convert</color>");
        player.MaxHealth = MaxHealth;
        player.Health = MaxHealth;
            
        player.ShowHint("<color=#ff5ffa>第五教会 改宗者</color>\n貴方は新たに第五教会に加わった。全てを第五に捧げるのです。\nSCP-1425を使って、更に第五を広めろ！",10f);
        Room SpawnRoom = Room.Get(RoomType.Surface);
        Log.Debug(SpawnRoom.Position);
        offset = new Vector3(0f,0f,0f);
        player.Position = new Vector3(124f,289f,21f);//SpawnRoom.Position + SpawnRoom.Rotation * offset;
        //player.Rotation = SpawnRoom.Rotation;
            
        player.ClearInventory();
        Log.Debug("Giving Items to FifthistConvert");
        player.AddItem(ItemType.GunCrossvec);
        //player.AddItem(ItemType.KeycardFacilityManager);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.ArmorCombat);
        player.TryAddCustomItem(5); // Fifthist Keycard
        player.TryAddCustomItem(1102); // Scp1425
        player.AddAmmo(AmmoType.Nato9,170);
    }
}