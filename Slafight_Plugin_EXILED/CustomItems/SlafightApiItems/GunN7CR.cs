using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class GunN7CR : CItemWeapon
{
    public override string DisplayName => "MTF-N7-CR";
    public override string Description => "Nu-7 Commanderが使用する銃。";

    protected override string UniqueKey => "GunN7CR";
    protected override ItemType BaseItem => ItemType.GunE11SR;

    protected override float Damage => 40f;
    protected override byte MagazineSize => 100;
    protected override Vector3 Scale => new(1f, 1f, 1.15f);

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.cyan;
}
