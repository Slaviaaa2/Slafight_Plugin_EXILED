#nullable enable
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Scp914;
using UnityEngine;

namespace Slafight_Plugin_EXILED.API.Features.Scp914;

/// <summary>
/// SCP-914 アップグレードルールを評価・実行する際に渡されるコンテキスト。
/// 床由来なら <see cref="Pickup"/>、インベントリ由来なら <see cref="Item"/> と <see cref="Owner"/> が入る。
/// </summary>
public readonly struct Scp914Context
{
    public Scp914KnobSetting Setting { get; }
    public Pickup? Pickup { get; }
    public Item? Item { get; }
    public Player? Owner { get; }
    public Vector3 OutputPosition { get; }

    public Scp914Context(
        Scp914KnobSetting setting,
        Pickup? pickup,
        Item? item,
        Player? owner,
        Vector3 outputPosition)
    {
        Setting = setting;
        Pickup = pickup;
        Item = item;
        Owner = owner;
        OutputPosition = outputPosition;
    }

    public bool IsInventory => Item != null && Owner != null;
}
