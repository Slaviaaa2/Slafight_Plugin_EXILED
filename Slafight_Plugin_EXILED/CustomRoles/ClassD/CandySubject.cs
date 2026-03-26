using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.CustomItems.API.Features;
using InventorySystem.Items.Usables.Scp330;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.ClassD;

public class CandySubject : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.CandySubject;
    protected override CTeam Team { get; set; } = CTeam.ClassD;
    protected override string UniqueRoleKey { get; set; } = "CandySubject";

    public override void SpawnRole(Player? player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player!.Role.Set(RoleTypeId.ClassD);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        player.AddItem(ItemType.KeycardJanitor);
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
        
        Door.Get(DoorType.Scp330Chamber).IsOpen = true;
        player.Position = Door.Get(DoorType.Scp330Chamber).Position + (Vector3.up * 0.8f);
            
        player.SetCustomInfo("Candy Subject");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<size=24><color=#ee7600>菓子被験者</color>\nレアなキャンディーいっぱい！！！",10f);
        });
    }
}