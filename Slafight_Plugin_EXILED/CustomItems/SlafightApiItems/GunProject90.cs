using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class GunProject90 : CItemWeapon
{
    public override string DisplayName => "Project-90";
    public override string Description => "昔ながらの、安定した撃ちどけ";

    protected override string UniqueKey => "GunProject90";
    protected override ItemType BaseItem => ItemType.GunCrossvec;

    protected override float Damage => 36f;
    protected override byte MagazineSize => 42;
    protected override Vector3 Scale => new(1f, 1f, 1.15f);

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.cyan;

    protected override void OnAcquired(ItemAddedEventArgs ev, bool displayMessage)
    {
        ev.Player.SetAmmoLimit(AmmoType.Nato9, 200);
        ev.Player.SetCategoryLimit(ItemCategory.Firearm, 3);
        ev.Player.SetCategoryLimit(ItemCategory.Grenade, 3);
    }

    protected override void OnDropping(DroppingItemEventArgs ev)
    {
        ev.Player.ResetAmmoLimit(AmmoType.Nato9);
        ev.Player.ResetCategoryLimit(ItemCategory.Firearm);
        ev.Player.ResetCategoryLimit(ItemCategory.Grenade);
    }
}
