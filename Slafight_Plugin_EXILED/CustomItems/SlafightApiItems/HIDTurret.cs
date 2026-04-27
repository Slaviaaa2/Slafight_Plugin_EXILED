using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.MicroHID.Modules;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class HIDTurret : CItem
{
    public override string DisplayName => "H.I.D. Turret";
    public override string Description =>
        "このH.I.D.は小チャージのみ使用可能で、無限に撃つことが出来ます！";

    protected override string UniqueKey => "HIDTurret";
    protected override ItemType BaseItem => ItemType.MicroHID;

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.yellow;

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.ChangingMicroHIDState += OnChangingMicroHIDState;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.ChangingMicroHIDState -= OnChangingMicroHIDState;
        base.UnregisterEvents();
    }

    /// <summary>
    /// チャージ撃ち (大チャージ) を禁止し、小チャージのみ使用可能にする。
    /// </summary>
    private void OnChangingMicroHIDState(ChangingMicroHIDStateEventArgs ev)
    {
        if (!Check(ev.Item)) return;
        if (ev.MicroHID.LastFiringMode == MicroHidFiringMode.ChargeFire
            && ev.NewPhase == MicroHidPhase.Firing)
        {
            ev.IsAllowed = false;
        }
    }
}
