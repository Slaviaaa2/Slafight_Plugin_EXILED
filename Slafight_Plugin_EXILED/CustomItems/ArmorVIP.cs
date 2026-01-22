using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Roles;
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
using Slafight_Plugin_EXILED.API.Enums;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Slafight_Plugin_EXILED.CustomItems;

[CustomItem(ItemType.ArmorCombat)]
public class ArmorVip : CustomArmor
{
    public override uint Id { get; set; } = 12;
    public override string Name { get; set; } = "要人用アーマー";
    public override string Description { get; set; } = "要人の命を守るために、防護に超特化したアーマー。";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.ArmorHeavy;
    public override SpawnProperties SpawnProperties { get; set; } = new();

    public override int VestEfficacy { get; set; } = 100;
    public override int HelmetEfficacy { get; set; } = 100;
    public override float StaminaUseMultiplier { get; set; } = 0.2f;

    public Color glowColor = CustomColor.Purple.ToUnityColor();
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
            ev.Player.SetAmmoLimit(AmmoType.Nato9,400);
            ev.Player.SetAmmoLimit(AmmoType.Nato556,400);
            ev.Player.SetCategoryLimit(ItemCategory.Firearm,3);
            ev.Player.SetCategoryLimit(ItemCategory.Grenade,3);
        }
    }

    private void LimitDestroy(DroppingItemEventArgs ev)
    {
        if (Check(ev.Item))
        {
            ev.Player.ResetAmmoLimit(AmmoType.Nato9);
            ev.Player.ResetAmmoLimit(AmmoType.Nato556);
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