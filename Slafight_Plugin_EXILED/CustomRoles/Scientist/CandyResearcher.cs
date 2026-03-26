using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using InventorySystem.Items.Usables.Scp330;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.Scientist;

public class CandyResearcher : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.CandyResearcher;
    protected override CTeam Team { get; set; } = CTeam.Scientists;
    protected override string UniqueRoleKey { get; set; } = "CandyResearcher";

    public override void SpawnRole(Player? player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player!.Role.Set(RoleTypeId.Scientist);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        player.AddItem(ItemType.KeycardScientist);
        List<CandyKindID> rareCandies =
        [
            CandyKindID.Black,
            CandyKindID.Brown,
            CandyKindID.Gray,
            CandyKindID.Orange,
            CandyKindID.White,
        ];
        for (var i = 0; i < 8; i++)
        {
            player.TryAddCandy(rareCandies.RandomItem()); 
        }
        
        Door.Get(DoorType.Scp330).IsOpen = true;
        player.Position = Door.Get(DoorType.Scp330).Position + (Vector3.up * 0.8f);
            
        player.SetCustomInfo("Candy Researcher");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<size=24><color=#00b7eb>菓子研究員</color>\nレアなキャンディーいっぱい！！！",10f);
        });
    }
}