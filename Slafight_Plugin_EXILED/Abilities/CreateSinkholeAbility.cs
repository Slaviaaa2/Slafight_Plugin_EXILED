using System;
using System.Collections.Generic;
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
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.Abilities;

public class CreateSinkholeAbility : AbilityBase
{
    // AbilityBase の抽象プロパティを実装
    protected override float DefaultCooldown => 30f;
    protected override int DefaultMaxUses => 3;

    // 完全デフォルト
    public CreateSinkholeAbility(Player owner)
        : base(owner) { }

    // コマンドなどから上書きしたいとき用
    public CreateSinkholeAbility(Player owner, float cooldownSeconds)
        : base(owner, cooldownSeconds) { }

    public CreateSinkholeAbility(Player owner, float cooldownSeconds, int maxUses)
        : base(owner, cooldownSeconds, maxUses) { }

    protected override void ExecuteAbility(Player player)
    {
        try
        {
            var position = player.Position
                           + player.CameraTransform.forward * 3f
                           + Vector3.up * 0.5f;

            var sinkholePrefabId = PrefabType.Sinkhole;
            var sinkhole = PrefabHelper.Spawn(sinkholePrefabId, position, Quaternion.identity);
            NetworkServer.Spawn(sinkhole);
            Timing.CallDelayed(10f, () => UnityEngine.Object.Destroy(sinkhole));
        }
        catch (Exception ex)
        {
            Log.Error($"Sinkhole Prefabスポーン失敗: {ex.Message}");
        }
    }
}