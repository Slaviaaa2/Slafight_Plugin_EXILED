using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class GunSuperLogicer : CItemWeapon
{
    public override string DisplayName => "Logicer SUPER";
    public override string Description => "最新式のLogicer。SUPERに強化されている。";

    protected override string UniqueKey => "GunSuperLogicer";
    protected override ItemType BaseItem => ItemType.GunLogicer;

    protected override float Damage => 30f;
    protected override byte MagazineSize => 255;
    protected override Vector3 Scale => new(1.08f, 1f, 1.35f);

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => CustomColor.ChaoticGreen.ToUnityColor();
}
