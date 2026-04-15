using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.EventArgs;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Mirror;
using Scp914;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

[CustomItem(ItemType.SCP500)]
public class ClassXMemoryForcePil : CustomItem
{
    public override uint Id { get; set; } = 2028;
    public override string Name { get; set; } = "クラスX-記憶補強剤";
    public override string Description { get; set; } = "反ミーム性の現象等に対抗するために使用される薬。\n反ミームの影響を軽減する。\n効果時間：1分";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.SCP500;

    public Color glowColor = Color.yellow;
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> ActiveLights = [];

    public override SpawnProperties SpawnProperties { get; set; } = new();

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem += OnUsing;
        Exiled.Events.Handlers.Player.UsedItem += OnUsed;
        
        Exiled.Events.Handlers.Map.PickupAdded += AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;
        
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem -= OnUsing;
        Exiled.Events.Handlers.Player.UsedItem -= OnUsed;
        
        Exiled.Events.Handlers.Map.PickupAdded -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;
        
        base.UnsubscribeEvents();
    }

    private void OnUsing(UsingItemEventArgs ev)
    {
        if (!Check(ev.Item) || ev.Player == null) return;
        if (ev.Player.HasFlag(SpecificFlagType.AntiMemeEffectDisabled))
        {
            ev.IsAllowed = false;
            ev.Player.ShowHint("既に耐性を得ている為、使用できません。");
        }
    }
    
    private void OnUsed(UsedItemEventArgs ev)
    {
        if (!Check(ev.Item) || ev.Player == null) return;
        ev.Player.EnableEffect(EffectType.Invigorated, 60);
        ev.Player.TryAddFlag(SpecificFlagType.AntiMemeEffectDisabled);
        ev.Player.WaitAndRemove(SpecificFlagType.AntiMemeEffectDisabled, 60f);
    }
    
    protected override void OnUpgrading(UpgradingEventArgs ev)
    {
        switch (ev.KnobSetting)
        {
            case Scp914KnobSetting.Coarse:
                Pickup.CreateAndSpawn(ItemType.SCP500, ev.OutputPosition);
                break;
            case Scp914KnobSetting.OneToOne:
                return;
            case Scp914KnobSetting.Fine:
            case Scp914KnobSetting.VeryFine:
                CustomItemExtensions.TrySpawn<ClassZMemoryForcePil>(ev.OutputPosition, out _);
                break;
        }

        ev.IsAllowed = false;
        ev.Item.DestroySelf();
        base.OnUpgrading(ev);
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
