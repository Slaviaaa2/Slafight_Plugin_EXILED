using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Mirror;
using Slafight_Plugin_EXILED.API.Enums;
using UnityEngine;
using Firearm = Exiled.API.Features.Items.Firearm;
using FirearmPickup = Exiled.API.Features.Pickups.FirearmPickup;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

[CustomItem(ItemType.MicroHID)]
public class GunGoCTurret : CustomWeapon
{
    public override uint Id { get; set; } = 10001;
    public override string Name { get; set; } = "GoC Turret";
    public override string Description { get; set; } = "テストアイテム。";
    public override float Weight { get; set; } = 1.15f;
    public override ItemType Type { get; set; } = ItemType.MicroHID;
    public override SpawnProperties SpawnProperties { get; set; } = new();
    
    public override float Damage { get; set; } = 45f;
    public override Vector3 Scale { get; set; } = new (1.15f,1f,1.15f);
    public override byte ClipSize { get; set; } = 1;

    public Color glowColor = CustomColor.Gold.ToUnityColor();
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> ActiveLights = [];

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting += Debug_HurtingDamage;
        
        Exiled.Events.Handlers.Player.PickingUpItem += LimitPatch;
        Exiled.Events.Handlers.Player.DroppingItem += LimitDestroy;
        
        Exiled.Events.Handlers.Map.PickupAdded += AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;
        
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting -= Debug_HurtingDamage;
        
        Exiled.Events.Handlers.Player.PickingUpItem -= LimitPatch;
        Exiled.Events.Handlers.Player.DroppingItem -= LimitDestroy;
        
        Exiled.Events.Handlers.Map.PickupAdded -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;
        
        base.UnsubscribeEvents();
    }

    private void Debug_HurtingDamage(HurtingEventArgs ev)
    {
        if (Check(ev.Attacker?.CurrentItem))
        {
            ev.Player.ExplodeEffect(ProjectileType.FragGrenade);
            ev.Player.Hurt(2000f,DamageType.Explosion);
        }
    }

    private void LimitPatch(PickingUpItemEventArgs ev)
    {
        if (Check(ev.Pickup))
        {
            if (ev.Pickup is FirearmPickup item)
            {
                item.MaxAmmo = 1;
                item.Ammo = 1;
            }
        }
    }

    private void LimitDestroy(DroppingItemEventArgs ev)
    {
        if (Check(ev.Item))
        {
            if (ev.Item is Firearm item)
            {
                if (item.TotalAmmo != 1)
                {
                    item.Destroy();
                }
            }
        }
    }
    
    private void RemoveGlow(PickupDestroyedEventArgs ev)
    {
        if (Check(ev.Pickup))
        {
            if (ev.Pickup != null)
            {
                if (ev.Pickup?.Base?.gameObject == null) return;
                if (TryGet(ev.Pickup.Serial, out CustomItem ci) && ci != null)
                {
                    if (ev.Pickup == null || !ActiveLights.ContainsKey(ev.Pickup)) return;
                    Exiled.API.Features.Toys.Light light = ActiveLights[ev.Pickup];
                    if (light != null && light.Base != null)
                    {
                        NetworkServer.Destroy(light.Base.gameObject);
                    }
                    ActiveLights.Remove(ev.Pickup);
                }
            }
        }

    }
    private void AddGlow(PickupAddedEventArgs ev)
    {
        if (Check(ev.Pickup) && ev.Pickup.PreviousOwner != null)
        {
            if (ev.Pickup?.Base?.gameObject == null) return;
            TryGet(ev.Pickup, out CustomItem ci);
            Log.Debug($"Pickup is CI: {ev.Pickup.Serial} | {ci.Id} | {ci.Name}");

            var light = Exiled.API.Features.Toys.Light.Create(ev.Pickup.Position);
            light.Color = glowColor;

            light.Intensity = 0.7f;
            light.Range = 5f;
            light.ShadowType = LightShadows.None;

            light.Base.gameObject.transform.SetParent(ev.Pickup.Base.gameObject.transform);
            ActiveLights[ev.Pickup] = light;
        }
    }
}