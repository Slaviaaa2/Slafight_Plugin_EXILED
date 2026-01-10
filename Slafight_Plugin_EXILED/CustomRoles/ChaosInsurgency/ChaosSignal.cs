using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.ChaosInsurgency;

public class ChaosSignal : CRole
{
    public override void RegisterEvents()
    {
        //Exiled.Events.Handlers.Player.Dying += OnDying;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        //Exiled.Events.Handlers.Player.Dying -= OnDying;
        base.UnregisterEvents();
    }

    public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.ChaosRifleman);
        player.UniqueRole = "ChaosSignal";
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        
        player.SetCategoryLimit(ItemCategory.Radio, 2);
        
        player.ClearInventory();
        Log.Debug("Giving Items to ChaosCommando");
        player.AddItem(ItemType.KeycardChaosInsurgency);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.Painkillers);
        player.AddItem(ItemType.ArmorCombat);
        player.AddItem(ItemType.Radio);
        player.TryAddCustomItem(2012);
            
        player.SetCustomInfo("Chaos Insurgency Signal");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#228b22>カオス・インサージェンシー 通信兵</color>\nS-Nav 300を用いてユニークな部屋を捜索する。",10f);
        });
    }
}