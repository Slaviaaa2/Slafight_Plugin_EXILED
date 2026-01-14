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

public class ChaosCommando : CRole
{
    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.Dying += OnDying;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.Dying -= OnDying;
        base.UnregisterEvents();
    }

    public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.ChaosRepressor);
        player.UniqueRole = "CI_Commando";
        player.MaxHealth = 150;
        player.Health = player.MaxHealth;
        
        player.ClearInventory();
        Log.Debug("Giving Items to ChaosCommando");
        player.AddItem(ItemType.KeycardChaosInsurgency);
        player.AddItem(ItemType.Adrenaline);
        player.AddItem(ItemType.Medkit);
        player.TryAddCustomItem(2009);
        player.TryAddCustomItem(10);
        player.TryAddCustomItem(2006);
        
        player.AddAmmo(AmmoType.Nato762, 350);
            
        player.SetCustomInfo("Chaos Insurgency Commando");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#228b22>カオス・インサージェンシー コマンドー</color>\nカオスの実戦部隊の中でのエリート中のエリート。\n抑圧兵よりも階級は上で、基本的に秩序のない、襲撃部隊を指揮する。",10f);
        });
    }

    private void OnDying(DyingEventArgs ev)
    {
        if (ev.Player.GetCustomRole() != CRoleTypeId.ChaosCommando) return;
        Projectile.CreateAndSpawn(ProjectileType.FragGrenade,ev.Player.Position + Vector3.up * 0.5f);
    }
}