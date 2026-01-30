using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp173;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class Sculpture : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.Sculpture;
    protected override CTeam Team { get; set; } = CTeam.FoundationForces;
    protected override string UniqueRoleKey { get; set; } = "Sculpture";

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Scp173.Blinking += OnBlinking;
        Exiled.Events.Handlers.Scp173.AddingObserver += OnObserving;
        Exiled.Events.Handlers.Player.Hurting += OnNecking;
        Exiled.Events.Handlers.Map.AnnouncingScpTermination += OnDying;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Scp173.Blinking -= OnBlinking;
        Exiled.Events.Handlers.Scp173.AddingObserver -= OnObserving;
        Exiled.Events.Handlers.Player.Hurting -= OnNecking;
        Exiled.Events.Handlers.Map.AnnouncingScpTermination -= OnDying;
        base.UnregisterEvents();
    }
    
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.NtfPrivate);
        player.Role.Set(RoleTypeId.Scp173, RoleSpawnFlags.AssignInventory);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 300f;
        player.Health = player.MaxHealth;
        player.MaxHumeShield = 200f;
        player.HumeShield = player.MaxHumeShield;
        player.SetScale(new Vector3(0.8f, 1f, 0.8f));
        player.ClearInventory();
        player.SetCustomInfo("<color=#00B7EB>Sculpture</color>");

        player.EnableEffect(EffectType.Slowness, 20);
        
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#00b7eb>Sculpture</color>\n相手が瞬きしたときに高速で移動し、痛めつける。\n財団の味方である。",10f);
        });
    }

    private void OnBlinking(BlinkingEventArgs ev)
    {
        if (!Check(ev.Player)) return;
        if (ev.Targets.Count >= 3)
        {
            ev.Scp173.BlinkReady = false;
        }
    }

    private void OnObserving(AddingObserverEventArgs ev)
    {
        if (!Check(ev.Player)) return;
        if (ev.Observer.GetTeam() == CTeam.FoundationForces || ev.Observer.GetTeam() == CTeam.Guards)
        {
            ev.IsAllowed = false;
            return;
        }

        ev.Scp173.BlinkCooldown = 3f;
    }

    private void OnNecking(HurtingEventArgs ev)
    {
        if (!Check(ev.Attacker)) return;
        if (ev.DamageHandler.Type == DamageType.Scp173 && ev.IsInstantKill)
        {
            ev.IsAllowed = false;
            ev.Player.Hurt(ev.Attacker, 35f, DamageType.Scp173);
            ev.Attacker.ShowHitMarker();
        }
    }

    private void OnDying(AnnouncingScpTerminationEventArgs ev)
    {
        if (!Check(ev.Player)) return;
        ev.IsAllowed = false;
    }
}