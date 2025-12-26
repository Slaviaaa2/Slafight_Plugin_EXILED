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
    public CreateSinkholeAbility() : base(30f,3) { }

    protected override void ExecuteAbility(Player player)
    {
        try 
        {
            var position = player.Position + player.CameraTransform.forward * 3f + Vector3.up * 0.5f;
        
            // SinkholeのPrefabIdを指定（実際のIDは要確認）
            var sinkholePrefabId = PrefabType.Sinkhole;
        
            // PrefabHelperでスポーン
            var sinkhole = PrefabHelper.Spawn(sinkholePrefabId, position, Quaternion.identity);
        
            // 必要に応じてNetwork同期
            NetworkServer.Spawn(sinkhole);
        
            // 10秒後に削除
            Timing.CallDelayed(10f, () => UnityEngine.Object.Destroy(sinkhole));
        }
        catch (System.Exception ex)
        {
            Log.Error($"Sinkhole Prefabスポーン失敗: {ex.Message}");
        }
        // クールダウン秒数を AbilityBase から取ってこれるように小ヘルパーを置く
        float cd = AbilityBase.CanUseNow(player.Id) ? 0f : 30f; // 例: 30秒固定なら直書きでもOK

        // 実際は _defaultCooldown を外から見えるようにするか、
        // CreateSinkholeAbility 側で const Cooldown = 30f を持っておく
        const float cooldownSeconds = 30f;

        Timing.CallDelayed(cooldownSeconds, () =>
        {
            if (player != null && player.IsConnected && HasAbility<CreateSinkholeAbility>(player))
                player.ShowHint("<color=yellow>Sinkhole のクールダウンが終了しました。</color>", 3f);
        });
    }
}