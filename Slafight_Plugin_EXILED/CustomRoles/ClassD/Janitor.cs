using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class Janitor : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.Janitor;
    protected override CTeam Team { get; set; } = CTeam.ClassD;
    protected override string UniqueRoleKey { get; set; } = "Janitor";

    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.ClassD);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        Log.Debug("Giving Items to Janitor");
        CustomItem.TryGive(player, 700,false);
        CustomItem.TryGive(player, 700,false);
        CustomItem.TryGive(player, 700,false);
        CustomItem.TryGive(player, 700,false);
        CustomItem.TryGive(player, 700,false);
        CustomItem.TryGive(player, 700,false);
        player.AddItem(ItemType.KeycardJanitor);
        player.AddItem(ItemType.Radio);
        var pos = Door.Get(DoorType.Scp173Connector).Position;
        pos += new Vector3(0f,0.35f,0f);
        player.Position = pos;
        Log.Debug($"RoomPos: {pos},Janitor pos: {player.Position}");
            
        player.CustomInfo = "Janitor";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#ee7600>用務員</color>\n特殊グレネードで近くの汚れを清掃できる",10f);
        });
    }
}