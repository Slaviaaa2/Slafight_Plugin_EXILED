using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class GunN7Weltkrieg : CItemWeapon
{
    public override string DisplayName => "Nu7 Weltkrieg級軽機関銃";
    public override string Description =>
        "Nu-7 Marshalが使用するとても強い軽機関銃。威厳を感じさせる";

    protected override string UniqueKey => "GunN7Weltkrieg";
    protected override ItemType BaseItem => ItemType.GunFRMG0;

    protected override float Damage => 40f;
    protected override byte MagazineSize => 100;
    protected override Vector3 Scale => new(1.15f, 1.3f, 1.25f);

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => CustomColor.Gold.ToUnityColor();
    protected override float PickupLightRange   => 5.5f;
}
