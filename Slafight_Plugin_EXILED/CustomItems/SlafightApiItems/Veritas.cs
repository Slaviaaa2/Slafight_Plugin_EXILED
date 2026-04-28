using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class Veritas : CItem
{
    public override string DisplayName => "VERITAS";
    public override string Description => "hogehoge";

    protected override string UniqueKey => "Veritas";
    protected override ItemType BaseItem => ItemType.SCP1344;
    protected override bool IsGoggles => true;

    /// <summary>装着時に SCP-1344 効果を残しておく (青視野は機能の一部)。</summary>
    protected override bool Remove1344Effect => false;

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => CustomColor.NinetailedBlue.ToUnityColor();
}
