using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.ProximityChat;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs
{
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

        public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
        {
            base.SpawnRole(player, roleSpawnFlags);

            player.Role.Set(RoleTypeId.Scp0492);
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
            Plugin.Singleton.LabApiHandler.Schem3005(LabApi.Features.Wrappers.Player.Get(player.ReferenceHub));

            player.AddAbility(new MagicMissileAbility(player));
            player.AddAbility(new SoundOfFifthAbility(player));

            Timing.RunCoroutine(Scp3005Coroutine(player));
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Player?.GetCustomRole() == CRoleTypeId.Scp3005 && ev.Attacker != null)
            {
                ev.IsAllowed = false;
                ev.Attacker.Hurt(5f, "第五的な力による影響");

                if (ev.Attacker.GetTeam() == CTeam.Fifthists)
                    ev.Attacker.ShowHint("第五的な存在に反逆するとは何事か！？");
            }
        }

        protected override void OnDying(DyingEventArgs ev)
        {
            Exiled.API.Features.Cassie.MessageTranslated("SCP 3 0 0 5 contained successfully by $pitch_.85 Anti- $pitch_1 Me mu Protocol.", "<color=red>SCP-3005</color> は、アンチミームプロトコルにより再収用されました");
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

                    float distance = Vector3.Distance(player.Position, target.Position);
                    if (distance <= 2.75f)
                    {
                        target.Hurt(25f, "<color=#ff00fa>第五的</color>な力による影響");
                        player.ShowHitMarker();
                    }
                }

                if (Plugin.Singleton.LabApiHandler._activatedAntiMemeProtocolInPast)
                {
                    player.DisableEffect(EffectType.Slowness);
                    player.EnableEffect(EffectType.MovementBoost, 25);
                }
                else
                {
                    player.DisableEffect(EffectType.MovementBoost);
                    player.EnableEffect(EffectType.Slowness, 25);
                }

                if (Plugin.Singleton.LabApiHandler.activatedAntiMemeProtocol)
                    player.Hurt(100f, "<color=#ff00fa>アンチミームプロトコロル</color>により終了された");

                yield return Timing.WaitForSeconds(1.5f);
            }
        }
    }
}
