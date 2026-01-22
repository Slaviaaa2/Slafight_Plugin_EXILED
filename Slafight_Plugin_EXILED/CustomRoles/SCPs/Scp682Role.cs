using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp682Role : CRole
{
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
        player.MaxHumeShield = 2000;
        player.HumeShieldRegenerationMultiplier = 55f;
        player.ClearInventory();

        SpeedLevels[player] = 1f;
        player.SetScale(new Vector3(0.7f, 0.65f, 1.2f));

        player.SetCustomInfo("SCP-682");
        
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
            if (player.GetCustomRole() != CRoleTypeId.Scp682)
            {
                SpeedLevels.Remove(player);
                RoleSpecificTextProvider.Clear(player);
                yield break;
            }

            if (!SpeedLevels.TryGetValue(player, out float speedLevel))
                speedLevel = SpeedLevels[player] = 1f;

            player.EnableEffect(EffectType.FocusedVision);
            player.EnableEffect(EffectType.NightVision, 255);
            
            SpeedLevels[player] *= 1.001f;
            
            // ★ 修正: RoleSpecificTextProvider を使用
            RoleSpecificTextProvider.Set(
                player,
                "Awaken Status: " + SpeedLevels[player].ToString("F2")
            );
            
            yield return Timing.WaitForSeconds(1f);
        }
    }

    private void Hurting(HurtingEventArgs ev)
    {
        if (ev.Attacker != null &&
            ev.Attacker.GetCustomRole() == CRoleTypeId.Scp682 &&
            SpeedLevels.TryGetValue(ev.Attacker, out float level))
        {
            ev.Amount *= level;
            SpeedLevels[ev.Attacker] = level + ev.Amount / 10000;
        }
    }
    
    private void DiedCassie(DyingEventArgs ev)
    {
        if (ev.Player?.GetCustomRole() == CRoleTypeId.Scp682)
        {
            SpeedLevels.Remove(ev.Player);
            // ★ 修正: クリア時も RoleSpecificTextProvider を使用
            RoleSpecificTextProvider.Clear(ev.Player);
            
            Exiled.API.Features.Cassie.Clear();
            Exiled.API.Features.Cassie.MessageTranslated(
                "SCP 6 8 2 Successfully Neutralized .",
                "<color=red>SCP-682</color>の無力化に成功しました。"
            );
        }
    }
}