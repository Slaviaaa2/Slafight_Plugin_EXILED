using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.Scientist;

public class ObjectObserver : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.ObjectObserver;
    protected override CTeam Team { get; set; } = CTeam.Scientists;
    protected override string UniqueRoleKey { get; set; } = "ObjectObserver";

    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scientist);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        Log.Debug("Giving Items to ObjectObserver");
        player.AddItem(ItemType.KeycardScientist);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.ArmorLight);
        var pos = Door.Get(DoorType.Scp173Connector).Position;
        pos += new Vector3(0f,0.35f,0f);
        player.Position = pos;
        Log.Debug($"RoomPos: {pos},ObjectObserver pos: {player.Position}");
            
        player.SetCustomInfo("Object Observer");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#00b7eb>オブジェクト観測者</color>\nオブジェクトの異常を監視する。",10f);
        });
    }
}