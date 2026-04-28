using Exiled.API.Features;
using Exiled.API.Features.Items;
using Slafight_Plugin_EXILED.CustomItems;

namespace Slafight_Plugin_EXILED.API.Features;

/// <summary>
/// 暗視ゴーグル (Scp1344 ベース) の中間基底。
/// 装着完了で <see cref="NvgManager.StartNvg"/>、取り外しで <see cref="NvgManager.StopNvg"/>
/// を呼ぶ共通配線を提供する。派生は <see cref="NvgProfile"/> と PickupLight 色だけを上書きすれば済む。
/// </summary>
public abstract class CItemNvg : CItem
{
    protected override ItemType BaseItem => ItemType.SCP1344;
    protected override bool IsGoggles => true;

    /// <summary>NVG の挙動 (色 / 範囲 / バッテリー減衰など)。デフォルトは NvgProfile.Default。</summary>
    protected virtual NvgProfile NvgProfile => NvgProfile.Default;

    protected override void OnGogglesWorn(Player player, Scp1344 goggles)
    {
        if (player == null) return;
        NvgManager.StartNvg(player, goggles.Serial, NvgProfile);
    }

    protected override void OnGogglesRemoved(Player player, Scp1344 goggles)
    {
        if (player == null) return;
        NvgManager.StopNvg(player, goggles.Serial);
    }
}
