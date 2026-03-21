using System;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomMaps.ObjectPrefabs;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Abilities;

public class Scp035TentacleAbility : AbilityBase
{
    // AbilityBase の抽象プロパティを実装
    protected override float DefaultCooldown => 10f;
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
                           + Vector3.up * 0.5f;
            new Tentacle(){Position = position, AutoDestroyEnabled = true, AutoDestroyTime = 30f}.Create();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to spawn Tentacle:\n{ex}");
        }
    }
}