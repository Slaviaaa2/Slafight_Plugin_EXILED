using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using UnityEngine;

namespace Slafight_Plugin_EXILED.API.Features;

/// <summary>
/// Armor 系 CItem の中間基底。Spawn 経路で <see cref="Item.Create"/> + 個別カスタマイズ +
/// <see cref="Item.CreatePickup"/> 経由に切り替えて、装着効力 (VestEfficacy /
/// HelmetEfficacy / StaminaUseMultiplier) を BodyArmor へ事前焼き付けする。
/// 派生は値を override するだけで CustomArmor 互換の動作になる。
/// </summary>
public abstract class CItemArmor : CItem
{
    /// <summary>
    /// Spawn 経路: Item を作って Armor 効力を焼き込んでから Pickup に変換する。
    /// CustomizeItem 既定実装が <see cref="Armor"/> 値を反映するので、ここでは
    /// 単純に <see cref="CItem.CustomizeItem"/> を呼んでから CreatePickup する。
    /// </summary>
    protected override Pickup? CreatePickupForSpawn(Vector3 position)
    {
        var item = Item.Create(BaseItem);
        if (item == null) return null;

        CustomizeItem(item);
        return item.CreatePickup(position);
    }
}
