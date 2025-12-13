using System.Collections.Generic;
using AdvancedMERTools;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.API.Structs;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using InventorySystem.Items.Armor;
using InventorySystem.Items.MicroHID.Modules;
using MEC;
using Mirror;
using PlayerStatsSystem;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Slafight_Plugin_EXILED.CustomItems;

[CustomItem(ItemType.GunE11SR)]
public class GunN7CR : CustomWeapon
{
    public override uint Id { get; set; } = 11;
    public override string Name { get; set; } = "MTF-N7-CR";
    public override string Description { get; set; } = "Nu-7 Commanderが使用する銃。";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.GunE11SR;
    public override SpawnProperties SpawnProperties { get; set; } = new();

    public override float Damage { get; set; } = 45f;
    public override Vector3 Scale { get; set; } = new (1f,1f,1.15f);
    public override byte ClipSize { get; set; } = 100;

    public Color glowColor = Color.magenta;
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> ActiveLights = [];

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.PickingUpItem += LimitPatch;
        Exiled.Events.Handlers.Player.DroppingItem += LimitDestroy;
        
        Exiled.Events.Handlers.Map.PickupAdded += AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;
        
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.PickingUpItem -= LimitPatch;
        Exiled.Events.Handlers.Player.DroppingItem -= LimitDestroy;
        
        Exiled.Events.Handlers.Map.PickupAdded -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;
        
        base.UnsubscribeEvents();
    }

    private void LimitPatch(PickingUpItemEventArgs ev)
    {
        if (Check(ev.Pickup))
        {
            ev.Player.SetAmmoLimit(AmmoType.Nato9,200);
            ev.Player.SetCategoryLimit(ItemCategory.Firearm,3);
            ev.Player.SetCategoryLimit(ItemCategory.Grenade,3);
        }
    }

    private void LimitDestroy(DroppingItemEventArgs ev)
    {
        if (Check(ev.Item))
        {
            ev.Player.ResetAmmoLimit(AmmoType.Nato9);
            ev.Player.ResetCategoryLimit(ItemCategory.Firearm);
            ev.Player.ResetCategoryLimit(ItemCategory.Grenade);
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