using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Mirror;
using Slafight_Plugin_EXILED.API.Enums;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

public class Veritas : CustomGoggles
{
    public override uint Id { get; set; } = 2037;
    public override string Name { get; set; } = "VERITAS";
    public override string Description { get; set; } =
        "hogehoge";
    public override float Weight { get; set; } = 1f;
    public override bool CanBeRemoveSafely { get; set; } = true;
    public override bool Remove1344Effect { get; set; } = false;

    private readonly Color glowColor = CustomColor.NinetailedBlue.ToUnityColor();
    private readonly Dictionary<Pickup, Light> _activeLights = new();
    public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded     += AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded     -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;
        base.UnsubscribeEvents();
    }

    private void RemoveGlow(PickupDestroyedEventArgs ev)
    {
        if (ev?.Pickup == null) return;
        if (!Check(ev.Pickup)) return;
        if (ev.Pickup.Base?.gameObject == null) return;
        if (!TryGet(ev.Pickup.Serial, out var ci) || ci == null) return;
        if (!_activeLights.TryGetValue(ev.Pickup, out var light)) return;

        if (light?.Base != null)
        {
            try { NetworkServer.Destroy(light.Base.gameObject); }
            catch (Exception ex) { Log.Warn($"[NVG_Normal] RemoveGlow Destroy 失敗: {ex.Message}"); }
        }
        _activeLights.Remove(ev.Pickup);
    }

    private void AddGlow(PickupAddedEventArgs ev)
    {
        if (ev?.Pickup == null) return;
        if (!Check(ev.Pickup)) return;
        if (ev.Pickup.PreviousOwner == null) return;
        if (ev.Pickup.Base?.gameObject == null) return;
        if (!TryGet(ev.Pickup, out var ci) || ci == null) return;

        try
        {
            var light = Light.Create(ev.Pickup.Position);
            if (light == null) return;

            light.Color      = glowColor;
            light.Intensity  = 0.7f;
            light.Range      = 5f;
            light.ShadowType = LightShadows.None;

            if (ev.Pickup.Base?.gameObject != null)
                light.Base.gameObject.transform.SetParent(ev.Pickup.Base.gameObject.transform);

            _activeLights[ev.Pickup] = light;
        }
        catch (Exception ex)
        {
            Log.Error($"[NVG_Normal] AddGlow エラー: {ex.Message}");
        }
    }
}
