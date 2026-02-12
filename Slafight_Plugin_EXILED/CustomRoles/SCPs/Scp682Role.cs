using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
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
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.Scp682;
    protected override CTeam Team { get; set; } = CTeam.SCPs;
    protected override string UniqueRoleKey { get; set; } = "Scp682";
    
    private static readonly Dictionary<Player, float> SpeedLevels = new();
    
    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.Hurting += Hurting;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.Hurting -= Hurting;
        base.UnregisterEvents();
    }
    
    public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scp173);
        player.Role.Set(RoleTypeId.Scp939, RoleSpawnFlags.None);

        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 999;
        player.Health = player.MaxHealth;
        player.MaxHumeShield = 1800;
        player.HumeShieldRegenerationMultiplier = 15f;
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
    protected override void OnDying(DyingEventArgs ev)
    {
        SpeedLevels.Remove(ev.Player);
        Exiled.API.Features.Cassie.Clear();
        Exiled.API.Features.Cassie.MessageTranslated("SCP 6 8 2 Successfully Neutralized .", "<color=red>SCP-682</color>の無力化に成功しました。");
        base.OnDying(ev);
    }
}