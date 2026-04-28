using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class NvgNormal : CItemNvg
{
    public override string DisplayName => "ナイトビジョンゴーグル";
    public override string Description =>
        "遠くや暗い場所まで見えるようになる暗視ゴーグル。電池を消費する。";

    protected override string UniqueKey => "NvgNormal";

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => new(0.6f, 1f, 0.6f);
}
