using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Jailbird;
using PlayerStatsSystem;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class SchwarzschildRailbreaker : CItemHybrid
{
    public override string DisplayName => "シュバルツシルト・レイルブレイカァー";
    public override string Description => "???";
    protected override string UniqueKey => "SchwarzschildRailbreaker";
    protected override ItemType BaseItem => ItemType.Jailbird;
    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor => CustomColor.Purple.ToUnityColor();

    protected override List<Type> SubModes => [typeof(SchwarzschildQuasar),typeof(GunGoCRailgunFull)];
    protected override string GetModeName(int modeIndex) => modeIndex switch
    {
        0 => "🔥 Quasar",
        1 => "⚡ Railgun",
        _ => "[?] Undefined Mode"
    };
}