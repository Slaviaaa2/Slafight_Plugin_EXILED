using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.MicroHID.Modules;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Player = Exiled.API.Features.Player;

namespace Slafight_Plugin_EXILED.CustomItems.newEve;

public class HIDTurret : CItem
{
    protected override ItemType ItemType => ItemType.MicroHID;
    public override bool UseLight { get; } = true;
    public override Color GlowColor { get; } = CustomColor.Purple.ToUnityColor();

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.ChangingMicroHIDState += DisRight;
        Exiled.Events.Handlers.Player.UsingMicroHIDEnergy += RightChargeDisable;
        Exiled.Events.Handlers.Player.Hurting += OnHurting;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.ChangingMicroHIDState -= DisRight;
        Exiled.Events.Handlers.Player.UsingMicroHIDEnergy -= RightChargeDisable;
        Exiled.Events.Handlers.Player.Hurting -= OnHurting;
        base.UnregisterEvents();
    }

    protected override void OnObtained(Player player)
    {
        player.ShowHint("<size=20>あなたはHID Turretを拾いました！</size>\n超絶あるちめっと光線");
        base.OnObtained(player);
    }
    
    private void DisRight(ChangingMicroHIDStateEventArgs ev)
    {
        if (!CheckItem(ev.Item)) { return; }
        if (ev.MicroHID.LastFiringMode == MicroHidFiringMode.ChargeFire && ev.NewPhase == MicroHidPhase.Firing)
        {
            ev.IsAllowed = false;
        }
    }
    
    private void RightChargeDisable(UsingMicroHIDEnergyEventArgs ev)
    {
        if (!CheckItem(ev.Item)) { return; }
        if (ev.MicroHID.LastFiringMode == MicroHidFiringMode.PrimaryFire)
        {
            Log.Debug(ev.MicroHID.LastFiringMode);
            ev.IsAllowed = false;
        }
    }

    private void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Player == null || CheckItem(ev.Attacker.CurrentItem)) return;
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
    }
}