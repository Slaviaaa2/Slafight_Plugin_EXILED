using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class OmegaWarheadAccess : CItem
{
    public override string DisplayName => "<color=blue>OMEGA WARHEAD</color>アクセスパス";
    public override string Description =>
        "<color=blue>OMEGA WARHEAD</color>サイロにアクセスできる使い捨てのカード。\n担当職員へ：間違ってもゲート解放に使わないように！";

    protected override string UniqueKey => "OmegaWarheadAccess";
    protected override ItemType BaseItem => ItemType.SurfaceAccessPass;

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.blue;
}
