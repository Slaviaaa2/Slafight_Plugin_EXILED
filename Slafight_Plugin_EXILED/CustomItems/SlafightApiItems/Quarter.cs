using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class Quarter : CItem
{
    public override string DisplayName => "Quarter";
    public override string Description => "25セント硬貨。特に意味はない";

    protected override string UniqueKey => "Quarter";
    protected override ItemType BaseItem => ItemType.Coin;

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.white;
}
