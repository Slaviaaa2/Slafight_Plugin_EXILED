using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Hazards;
using Exiled.Events.EventArgs.Player;
using Hazards;
using MEC;
using Mirror;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomMaps.ObjectPrefabs;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.ProximityChat;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.Abilities;

public class Scp035TentacleAbility : AbilityBase
{
    // AbilityBase の抽象プロパティを実装
    protected override float DefaultCooldown => 30f;
    protected override int DefaultMaxUses => -1;

    // 完全デフォルト
    public Scp035TentacleAbility(Player owner)
        : base(owner) { }

    // コマンドなどから上書きしたいとき用
    public Scp035TentacleAbility(Player owner, float cooldownSeconds)
        : base(owner, cooldownSeconds) { }

    public Scp035TentacleAbility(Player owner, float cooldownSeconds, int maxUses)
        : base(owner, cooldownSeconds, maxUses) { }

    protected override void ExecuteAbility(Player player)
    {
        try
        {
            var position = player.Position
                           + player.CameraTransform.forward * 3f
                           + Vector3.up * 0.45f;
            var tentacle = new Tentacle(){Position = position}.Create();
            Timing.CallDelayed(60f, () => tentacle.Destroy());
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to spawn Tentacle:\n{ex}");
        }
    }
}