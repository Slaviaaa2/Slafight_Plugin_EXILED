using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class Engineer : CRole
{
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scientist);
        player.UniqueRole = "Engineer";
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        Log.Debug("Giving Items to Engineer");
        // ALL WIP
        var pos = Room.Get(RoomType.HczTestRoom).WorldPosition(new Vector3(0f,1f,0f));
        player.Position = pos;
        Log.Debug($"RoomPos: {pos},ZoneManager pos: {player.Position}");
            
        player.CustomInfo = "Engineer";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#00ffff>エンジニア</color>\n色んなタスクをこなして権限をアップグレードする。",10f);
        });
    }
}