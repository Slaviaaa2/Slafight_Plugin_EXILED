using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.EventArgs;
using Scp914;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

public class NvgNormal : NvgGogglesBase
{
    public override uint   Id          { get; set; } = 2033;
    public override string Name        { get; set; } = "ナイトビジョンゴーグル";
    public override string Description { get; set; } =
        "遠くや暗い場所まで見えるようになる暗視ゴーグル。電池を消費する。";
    public override float  Weight      { get; set; } = 1f;

    public override bool CanBeRemoveSafely { get; set; } = true;
    public override bool Remove1344Effect  { get; set; } = true;

    public override SpawnProperties SpawnProperties { get; set; } = new();
    
    protected override void OnUpgrading(UpgradingEventArgs ev)
    {
        if (ev.KnobSetting == Scp914KnobSetting.OneToOne)
        {
            CustomItemExtensions.TrySpawn<NvgNormal>(ev.OutputPosition, out _);
        }
        else if (ev.KnobSetting == Scp914KnobSetting.Fine)
        {
            CustomItemExtensions.TrySpawn<NvgRed>(ev.OutputPosition, out _);
        }
        else if (ev.KnobSetting == Scp914KnobSetting.VeryFine)
        {
            CustomItemExtensions.TrySpawn<NvgBlue>(ev.OutputPosition, out _);
        }

        ev.IsAllowed = false;
        ev.Item.DestroySelf();
        base.OnUpgrading(ev);
    }
}