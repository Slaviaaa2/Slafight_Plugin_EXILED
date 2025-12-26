using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Hazards;
using Exiled.Events.EventArgs.Player;
using Hazards;
using MEC;
using Mirror;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.Abilities;

public class CreateSinkholeAbility : AbilityBase
{
    public CreateSinkholeAbility() : base(3,30f,3) { }

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
    }
}