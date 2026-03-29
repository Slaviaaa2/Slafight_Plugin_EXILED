using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
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
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.Scp3005;
    protected override CTeam Team { get; set; } = CTeam.SCPs;
    protected override string UniqueRoleKey { get; set; } = "SCP-3005";

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.Hurting += OnHurting;
        Exiled.Events.Handlers.Player.SpawningRagdoll += CencellRagdoll;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.Hurting -= OnHurting;
        Exiled.Events.Handlers.Player.SpawningRagdoll -= CencellRagdoll;
        base.UnregisterEvents();
    }

    public override void SpawnRole(Player? player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);

        player!.Role.Set(RoleTypeId.Scp0492);
        player.UniqueRole = "SCP-3005";
        player.SetCustomInfo("SCP-3005");
        const int maxHealth = 55555;
        player.MaxHealth = maxHealth;
        player.Health = maxHealth;
        player.EnableEffect(EffectType.MovementBoost, 50);

        Room spawnRoom = Room.Get(RoomType.LczPlants);
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

    private void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Player?.GetCustomRole() == CRoleTypeId.Scp3005 && ev.Attacker != null && ev.Attacker?.GetCustomRole() != CRoleTypeId.Scp3005)
        {
            bool hasGoggles = ev.Attacker.Items
                .OfType<Scp1344>()
                .Any(i => i.TryGetCustomItem(out var ci) && ci is AntiMemeGoggle && i.IsWorn);
            if (ev.Player.IsEffectActive<CustomPlayerEffects.Sinkhole>() || hasGoggles) return;
            ev.IsAllowed = false;
            ev.Attacker.Hurt(ev.Player, 5f, DamageType.Unknown,null,  "<color=#ff00fa>第五的</color>な力による影響");

            if (ev.Attacker.GetTeam() == CTeam.Fifthists)
                ev.Attacker.ShowHint("第五的な存在に反逆するとは何事か！？");
        }
    }

    protected override void OnDying(DyingEventArgs ev)
    {
        Exiled.API.Features.Cassie.MessageTranslated("SCP 3 0 0 5 contained successfully by $pitch_.85 Anti- $pitch_1 Me mu Protocol.", "<color=red>SCP-3005</color> は、アンチミームプロトコルにより再収用されました", true);
        base.OnDying(ev);
    }

    private void CencellRagdoll(SpawningRagdollEventArgs ev)
    {
        if (ev.Player?.GetCustomRole() == CRoleTypeId.Scp3005)
            ev.IsAllowed = false;
    }

    private IEnumerator<float> Scp3005Coroutine(Player player)
    {
        for (;;)
        {
            if (player.GetCustomRole() != CRoleTypeId.Scp3005)
                yield break;

            foreach (Player target in Player.List)
            {
                if (target == null || target == player || !target.IsAlive)
                    continue;

                if (target.GetTeam() == CTeam.SCPs || target.GetTeam() == CTeam.Fifthists)
                    continue;
                    
                bool hasGoggles = target.Items
                    .OfType<Scp1344>()
                    .Any(i => i.TryGetCustomItem(out var ci) && ci is AntiMemeGoggle && i.IsWorn);
                if (hasGoggles)  continue;

                float distance = Vector3.Distance(player.Position, target.Position);
                if (distance <= 2.75f)
                {
                    target.Hurt(player, 25f, DamageType.Unknown,null,  "<color=#ff00fa>第五的</color>な力による影響");
                    player.ShowHitMarker();
                }
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