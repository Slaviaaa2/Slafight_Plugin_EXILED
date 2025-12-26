using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Hazards;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp106;
using HintServiceMeow.Core.Utilities;
using MEC;
using Mirror;
using PlayerRoles;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp106Role : CRole
{
    public override void RegisterEvents()
    {
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        base.UnregisterEvents();
    }
    
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scp106);
        player.UniqueRole = "Scp106";
        //player.MaxHealth = 1400;
        //player.Health = player.MaxHealth;
        //player.MaxHumeShield = 500;
        player.ClearInventory();
        player.SetCustomInfo("SCP-106");
        
        // 状態を作る
        AbilityBase.GrantAbility(player.Id, cooldown: 30f, maxUses: 3);

        // アビリティをスロットに追加
        player.AddAbility<CreateSinkholeAbility>();

        // クールダウン終了時の Hint を設定
        AbilityBase.SetOnCooldownEnd(player.Id, p =>
        {
            if (p != null && p.IsConnected && AbilityBase.HasAbility<CreateSinkholeAbility>(p))
                p.ShowHint("<color=yellow>Sinkhole のクールダウンが終了しました。</color>", 3f);
        });
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=red>SCP-106</color>\n若者の叫び声大好き爺。いっぱいPDに送り込もう！\nアビリティで陥没穴を創り出せるぞ！陥没穴は中に\n人を引き込めるから沢山作れ！",10f);
        });
    }

    private void CreateSmallSinkhole(Player player)
    {
        try
        {
            var position = player.Position + new Vector3(0f,-0.5f,0f);
        
            // SinkholeのPrefabIdを指定（実際のIDは要確認）
            var sinkholePrefabId = PrefabType.Sinkhole;
        
            // PrefabHelperでスポーン
            var sinkhole = PrefabHelper.Spawn(sinkholePrefabId, position, Quaternion.identity);
            
            // 必要に応じてNetwork同期
            NetworkServer.Spawn(sinkhole);
        
            // 10秒後に削除
            Timing.CallDelayed(5f, () => UnityEngine.Object.Destroy(sinkhole));
        }
        catch (System.Exception ex)
        {
            Log.Error($"Sinkhole Prefabスポーン失敗: {ex.Message}");
        }
    }
}