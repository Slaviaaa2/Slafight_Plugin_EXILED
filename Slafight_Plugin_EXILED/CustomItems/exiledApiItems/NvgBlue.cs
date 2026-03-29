using Exiled.API.Features.Spawn;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

public class NvgBlue : NvgGogglesBase
{
    public override uint   Id          { get; set; } = 2040;
    public override string Name        { get; set; } = "ナイトビジョンゴーグル - 青";
    public override string Description { get; set; } =
        "遠くや暗い場所まで見えるようになる暗視ゴーグル。電池を消費するが、周りの情報が分かる。";
    public override float  Weight      { get; set; } = 1f;

    public override bool CanBeRemoveSafely { get; set; } = true;
    public override bool Remove1344Effect  { get; set; } = false;

    public override SpawnProperties SpawnProperties { get; set; } = new();

    protected override NvgProfile NvgProfile => new()
    {
        DrainPerSecond = 10f,        // 無限電池
        LightColor     = Color.blue,
        LightRange     = 50f,
        LightIntensity = 10200f,
        UseBlackout    = true,     // 電池切れしないので不要だが念のため
    };

    protected override Color GlowColor => Color.blue;
}