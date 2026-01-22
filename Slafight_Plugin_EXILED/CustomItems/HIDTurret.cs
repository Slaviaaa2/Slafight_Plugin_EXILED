using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using InventorySystem.Items.MicroHID.Modules;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems;

[CustomItem(ItemType.MicroHID)]
public class HIDTurret : CustomWeapon
{
    public override uint Id { get; set; } = 1;
    public override string Name { get; set; } = "H.I.D. Turret";
    public override string Description { get; set; } = "このH.I.D.は小チャージのみ使用可能で、無限に撃つことが出来ます！";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.MicroHID;

    public Color glowColor = CustomColor.Purple.ToUnityColor();
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> ActiveLights = [];

    public override SpawnProperties SpawnProperties { get; set; } = new();

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingMicroHIDEnergy += RightChargeDisable;
        Exiled.Events.Handlers.Player.ChangingMicroHIDState += disRight;
        Exiled.Events.Handlers.Map.PickupAdded += AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;
        
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingMicroHIDEnergy -= RightChargeDisable;
        Exiled.Events.Handlers.Player.ChangingMicroHIDState -= disRight;
        Exiled.Events.Handlers.Map.PickupAdded -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;
        
        base.UnsubscribeEvents();
    }

    private void disRight(ChangingMicroHIDStateEventArgs ev)
    {
        if (!Check(ev.Item)) { return; }
        if (ev.MicroHID.LastFiringMode == MicroHidFiringMode.ChargeFire && ev.NewPhase == MicroHidPhase.Firing)
        {
            ev.IsAllowed = false;
        }
    }
    
    private void RightChargeDisable(UsingMicroHIDEnergyEventArgs ev)
    {
        if (!Check(ev.Item)) { return; }
        if (ev.MicroHID.LastFiringMode == MicroHidFiringMode.PrimaryFire)
        {
            Log.Debug(ev.MicroHID.LastFiringMode);
            ev.IsAllowed = false;
        }
    }

    protected override void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Player == null) return;
        var info = ev.Player.GetRoleInfo();
        if (info.Vanilla == RoleTypeId.Scp106 && (info.Custom == CRoleTypeId.None || info.Custom == CRoleTypeId.Scp106))
        {
            ev.Amount = 80f;
        }
        else if (ev.Player?.GetTeam() == CTeam.SCPs)
        {
            ev.Amount = 25f;
        }
        else
        {
            ev.IsAllowed = false;
        }
        base.OnHurting(ev);
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