using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Usables.Scp244;
using MEC;
using PlayerStatsSystem;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class ThrowableScp244 : CItem
{
    public override string DisplayName => "SCP-244 (投擲)";
    public override string Description => "投擲して使用することができるSCP-244";
    protected override string UniqueKey => "Scp244-Throwable";
    protected override ItemType BaseItem => ItemType.GrenadeFlash;
    
    private static readonly Dictionary<Projectile, Scp244Pickup>? TrackedPickups = [];

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.ThrownProjectile += OnThrown;
        Exiled.Events.Handlers.Map.ExplodingGrenade += OnExploding;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.ThrownProjectile -= OnThrown;
        Exiled.Events.Handlers.Map.ExplodingGrenade -= OnExploding;
        base.UnregisterEvents();
    }

    protected override void OnWaitingForPlayers()
    {
        TrackedPickups?.Clear();
        base.OnWaitingForPlayers();
    }

    private void OnThrown(ThrownProjectileEventArgs ev)
    {
        if (!Check(ev.Projectile)) return;
        var bp = Pickup.CreateAndSpawn(ItemType.SCP244a, ev.Projectile.Position);
        bp.Transform.SetParent(ev.Projectile.Transform);
        TrackedPickups?.Add(ev.Projectile, bp.Cast<Scp244Pickup>());
    }

    private void OnExploding(ExplodingGrenadeEventArgs ev)
    {
        if (TrackedPickups == null) return;
        if (!Check(ev.Projectile) || !TrackedPickups.ContainsKey(ev.Projectile)) return;
        ev.TargetsToAffect.Clear();
        TrackedPickups[ev.Projectile]?.Transform.parent = null;
        TrackedPickups[ev.Projectile]?.State = Scp244State.Active;
        Timing.CallDelayed(1f, () => TrackedPickups[ev.Projectile]?.State = Scp244State.Destroyed);
    }
}