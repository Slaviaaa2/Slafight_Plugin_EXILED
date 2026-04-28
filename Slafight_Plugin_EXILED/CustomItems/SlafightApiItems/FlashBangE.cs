using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;
using MapHandlers = Exiled.Events.Handlers.Map;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class FlashBangE : CItem
{
    public override string DisplayName => "Flashbang-E";
    public override string Description => "SCPオブジェクトにのみ当たるように改良されたフラッシュバン。";

    protected override string UniqueKey => "FlashBangE";
    protected override ItemType BaseItem => ItemType.GrenadeFlash;

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.green;

    public override void RegisterEvents()
    {
        MapHandlers.ExplodingGrenade += OnExploding;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        MapHandlers.ExplodingGrenade -= OnExploding;
        base.UnregisterEvents();
    }

    protected override void OnThrowingRequest(ThrowingRequestEventArgs ev)
    {
        if (ev.Item is FlashGrenade fg)
            fg.FuseTime = 0.5f;
    }

    protected override void OnPickingUp(PickingUpItemEventArgs ev)
    {
        ev.Player.SetCategoryLimit(ItemCategory.Grenade,
            (sbyte)(ev.Player.GetCategoryLimit(ItemCategory.Grenade) + 1));
    }

    protected override void OnDropping(DroppingItemEventArgs ev)
    {
        ev.Player.SetCategoryLimit(ItemCategory.Grenade,
            (sbyte)(ev.Player.GetCategoryLimit(ItemCategory.Grenade) - 1));
    }

    private void OnExploding(ExplodingGrenadeEventArgs ev)
    {
        if (!Check(ev.Projectile)) return;
        ev.TargetsToAffect.RemoveWhere(player => player.IsHuman);
    }
}
