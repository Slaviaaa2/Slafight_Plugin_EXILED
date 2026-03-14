using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Mirror;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

[CustomItem(ItemType.GunRevolver)]
public class GunTacticalRevolver : CustomWeapon
{
    public override uint Id { get; set; } = 2032;
    public override string Name { get; set; } = "Tactical Revolver";
    public override string Description { get; set; } = "ヘッドショットをすると暫く毒を与えられるリボルバー。\nリロード時暫くは精度良く扱える";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.GunRevolver;
    public override SpawnProperties SpawnProperties { get; set; } = new();

    public override float Damage { get; set; } = 30f;
    public override Vector3 Scale { get; set; } = new (1f,1f,1.15f);
    public override byte ClipSize { get; set; } = 7;

    public Color glowColor = Color.yellow;
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> ActiveLights = [];

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded += AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;
        
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;
        
        base.UnsubscribeEvents();
    }

    protected override void OnHurting(HurtingEventArgs ev)
    {
        if (ev.DamageHandler.Base is GenericDamageHandler handler && handler.Hitbox == HitboxType.Headshot)
        {
            ev.Player?.EnableEffect(EffectType.Poisoned, 5, 3f);
        }
        base.OnHurting(ev);
    }

    protected override void OnReloaded(ReloadedWeaponEventArgs ev)
    {
        ev.Player?.EnableEffect(EffectType.Scp1853, 2, 3f);
        base.OnReloaded(ev);
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