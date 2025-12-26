using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp682Role : CRole
{
    // プレイヤーごとのspeedLevel管理
    private static readonly Dictionary<Player, float> SpeedLevels = new();
    
    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.Dying += DiedCassie;
        Exiled.Events.Handlers.Player.Hurting += Hurting;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.Dying -= DiedCassie;
        Exiled.Events.Handlers.Player.Hurting -= Hurting;
        base.UnregisterEvents();
    }
    
    public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scp173);
        player.Role.Set(RoleTypeId.Scp939, RoleSpawnFlags.None);
        player.UniqueRole = "Scp682";
        player.MaxHealth = 999;
        player.Health = player.MaxHealth;
        player.MaxHumeShield = 10000;
        player.ClearInventory();

        player.SetFakeScale(new Vector3(1.75f, 0.75f, 3f), Player.List);
        player.CustomInfo = "<color=#C50000>SCP-682</color>";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
        
        // Dictionaryに初期化
        SpeedLevels[player] = 1f;
        
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=red>SCP-682</color>\n長く眠っていた為視界がぼやけている。でも頑張って無双しろ！！！", 10f);
        });
        Timing.RunCoroutine(Coroutine(player));
    }

    private IEnumerator<float> Coroutine(Player player)
    {
        for (;;)
        {
            if (player.UniqueRole != "Scp682")
            {
                SpeedLevels.Remove(player);  // クリーンアップ
                yield break;
            }

            float speedLevel = SpeedLevels[player];
            player.EnableEffect(EffectType.FocusedVision);
            player.EnableEffect(EffectType.NightVision, 255);
            player.SetFakeScale(new Vector3(1.75f, 0.75f, 3f) * speedLevel, Player.List);
            
            // プレイヤーごとのspeedLevel更新
            SpeedLevels[player] *= 1.00001f;
            
            Plugin.Singleton.PlayerHUD.HintSync(
                SyncType.PHUD_Specific, 
                ("Big Multiplier: " + SpeedLevels[player].ToString("F2")), 
                player
            );
            yield return Timing.WaitForSeconds(1f);
        }
    }

    private void Hurting(HurtingEventArgs ev)
    {
        if (ev.Attacker?.UniqueRole == "Scp682")
        {
            ev.Amount *= SpeedLevels[ev.Attacker];
        }
    }
    
    private void DiedCassie(DyingEventArgs ev)
    {
        if (ev.Player?.UniqueRole == "Scp682")  // "Scp682"に統一
        {
            SpeedLevels.Remove(ev.Player);  // クリーンアップ
            Plugin.Singleton.PlayerHUD.HintSync(SyncType.PHUD_Specific, "", ev.Player);
            Exiled.API.Features.Cassie.Clear();
            Exiled.API.Features.Cassie.MessageTranslated(
                "SCP 6 8 2 Successfully Neutralized .",
                "<color=red>SCP-682</color>の無力化に成功しました。"  // メッセージ修正
            );
        }
    }
}
