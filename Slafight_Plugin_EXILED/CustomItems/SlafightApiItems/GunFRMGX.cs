using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class GunFRMGX : CItemWeapon
{
    public override string DisplayName => "FRMG-X";
    public override string Description =>
        "財団の無理を押し通して購入された最新式のFRMG-0。全体的に強化されている。";

    protected override string UniqueKey => "GunFRMGX";
    protected override ItemType BaseItem => ItemType.GunFRMG0;

    protected override float Damage => 38f;
    protected override byte MagazineSize => 130;
    protected override Vector3 Scale => new(1.08f, 1f, 1.35f);

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => CustomColor.NinetailedBlue.ToUnityColor();
}
