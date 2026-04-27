using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp0492;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MainHandlers;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp3005Role : CRole
{
    protected override string RoleName { get; set; } = "SCP-3005";
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.Scp3005;
    protected override CTeam Team { get; set; } = CTeam.SCPs;
    protected override string UniqueRoleKey { get; set; } = "SCP-3005";

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.Hurting += OnHurting;
        Exiled.Events.Handlers.Player.SpawningRagdoll += CancelRagdoll;
        Exiled.Events.Handlers.Scp0492.ConsumedCorpse += OnConsumed;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.Hurting -= OnHurting;
        Exiled.Events.Handlers.Player.SpawningRagdoll -= CancelRagdoll;
        Exiled.Events.Handlers.Scp0492.ConsumedCorpse -= OnConsumed;
        base.UnregisterEvents();
    }

    public override void SpawnRole(Player? player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);

        player!.Role.Set(RoleTypeId.Scp0492);
        player.UniqueRole = "SCP-3005";
        player.SetCustomInfo("SCP-3005");
        const int maxHealth = 55556;
        player.MaxHealth = maxHealth;
        player.Health = maxHealth-1;
        player.EnableEffect(EffectType.MovementBoost, 50);

        var spawnRoom = Room.Get(RoomType.LczPlants);
        Vector3 offset = new(0f, 7.35f, 0f);
        player.Position = spawnRoom.Position + spawnRoom.Rotation * offset;
        player.Rotation = spawnRoom.Rotation;

        // ★ Scale は触らない
        LabApiHandler.Schem3005(LabApi.Features.Wrappers.Player.Get(player.ReferenceHub));

        player.AddAbility(new MagicMissileAbility(player));
        player.AddAbility(new SoundOfFifthAbility(player));

        Timing.RunCoroutine(Scp3005Coroutine(player));
        Timing.CallDelayed(0.05f, () => player.ShowHint(
            "<size=24><color=red>SCP-3005</color>\n第五的な光を放つ存在。\n普通はダメージを受けることはなく、\nアビリティで第五的なミサイルや閃光を引き起こせる。\n<color=#ff00fa>第五教会に道を示せ</color>",
            10));
    }
    
    protected override void OnDying(DyingEventArgs ev)
    {
        if (Plugin.Singleton.LabApiHandler.ActivatedAntiMemeProtocol && ev.Attacker is null)
        {
            Exiled.API.Features.Cassie.MessageTranslated("SCP 3 0 0 5 Successfully neutralized by $pitch_.85 Anti- $pitch_1 Me mu Protocol.", $"<color={Team.GetTeamColor()}>{RoleName}</color> は<color={CTeam.Fifthists.GetTeamColor()}>アンチミームプロトコル</color>により正常に無効化されました。");
        }
        else
        {
            CassieHelper.AnnounceTermination(ev, "SCP 3 0 0 5", $"<color={Team.GetTeamColor()}>{RoleName}</color>", true);
        }
        base.OnDying(ev);
    }

    private static void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Player?.GetCustomRole() == CRoleTypeId.Scp3005 && ev.Attacker != null && ev.Attacker?.GetCustomRole() != CRoleTypeId.Scp3005)
        {
            var hasGoggles = ev.Attacker != null && ev.Attacker.Items
                .OfType<Scp1344>()
                .Any(i => i.TryGetCustomItem(out var ci) && ci is AntiMemeGoggle && i.IsWorn);
            if (ev.Player.IsEffectActive<CustomPlayerEffects.Sinkhole>() || hasGoggles) return;
            ev.IsAllowed = false;
            ev.Attacker?.Hurt(ev.Player, 5f, DamageType.Unknown,null,  "<color=#ff00fa>第五的</color>な力による影響");

            if (ev.Attacker != null && ev.Attacker.GetTeam() == CTeam.Fifthists)
                ev.Attacker.ShowHint("第五的な存在に反逆するとは何事か！？");
        }
    }

    private void OnConsumed(ConsumedCorpseEventArgs ev)
    {
        if (!Check(ev.Player) || ev.Ragdoll.Owner.IsAlive) return;
        ev.ConsumeHeal = 0f;
        var target = ev.Ragdoll.Owner;
        target?.SetRole(CRoleTypeId.FifthistMarionette);
        Timing.CallDelayed(0.1f, () => target?.Position = ev.Ragdoll.Position + Vector3.up * 0.15f);
    }

    private static void CancelRagdoll(SpawningRagdollEventArgs ev)
    {
        if (ev.Player?.GetCustomRole() == CRoleTypeId.Scp3005)
            ev.IsAllowed = false;
    }

    private static IEnumerator<float> Scp3005Coroutine(Player player)
    {
        for (;;)
        {
            if (player.GetCustomRole() != CRoleTypeId.Scp3005)
                yield break;

            foreach (var target in Player.List)
            {
                if (target == null || target == player || !target.IsAlive)
                    continue;

                if (target.GetTeam() == CTeam.SCPs || target.GetTeam() == CTeam.Fifthists)
                    continue;
                    
                var hasGoggles = target.Items
                    .OfType<Scp1344>()
                    .Any(i => i.TryGetCustomItem(out var ci) && ci is AntiMemeGoggle && i.IsWorn);
                if (hasGoggles)  continue;

                var distance = Vector3.Distance(player.Position, target.Position);
                if (!(distance <= 2.75f)) continue;
                target.Hurt(player, 25f, DamageType.Unknown,null,  "<color=#ff00fa>第五的</color>な力による影響");
                player.ShowHitMarker();
            }

            if (Plugin.Singleton.LabApiHandler.ActivatedAntiMemeProtocolInPast)
            {
                player.DisableEffect(EffectType.Slowness);
                player.EnableEffect(EffectType.MovementBoost, 25);
            }
            else
            {
                player.DisableEffect(EffectType.MovementBoost);
                player.EnableEffect(EffectType.Slowness, 25);
            }

            if (Plugin.Singleton.LabApiHandler.ActivatedAntiMemeProtocol)
                player.Hurt(100f, "<color=#ff00fa>アンチミームプロトコロル</color>により終了された");

            yield return Timing.WaitForSeconds(1.5f);
        }
    }
}