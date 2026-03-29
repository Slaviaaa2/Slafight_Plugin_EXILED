using Exiled.API.Features.Spawn;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

public class NvgRed : NvgGogglesBase
{
    public override uint   Id          { get; set; } = 2039;
    public override string Name        { get; set; } = "ナイトビジョンゴーグル - 赤";
    public override string Description { get; set; } =
        "遠くや暗い場所まで見えるようになる暗視ゴーグル。電池を消費しない。";
    public override float  Weight      { get; set; } = 1f;

    public override bool CanBeRemoveSafely { get; set; } = true;
    public override bool Remove1344Effect  { get; set; } = true;

    public override SpawnProperties SpawnProperties { get; set; } = new();

    protected override NvgProfile NvgProfile => new()
    {
        DrainPerSecond = 0f,        // 無限電池
        LightColor     = Color.red,
        LightRange     = 30f,
        LightIntensity = 10000f,
        UseBlackout    = false,     // 電池切れしないので不要だが念のため
    };

    protected override Color GlowColor => Color.red;
}