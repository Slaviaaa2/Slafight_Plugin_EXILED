using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;

namespace Slafight_Plugin_EXILED.CustomRoles.Fifthist
{
    public class FifthistPriest : CRole
    {
        protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.FifthistPriest;
        protected override CTeam Team { get; set; } = CTeam.Fifthists;
        protected override string UniqueRoleKey { get; set; } =  "F_Priest";

        private CoroutineHandle _auraHandle;

        public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
        {
            base.SpawnRole(player, roleSpawnFlags);

            player.Role.Set(RoleTypeId.Tutorial);
            int maxHealth = 555;

            player.UniqueRole = UniqueRoleKey;
            player.Scale = new Vector3(1.1f, 1.1f, 1.1f);
            player.CustomInfo = "<color=#FF0090>Fifthist Priest</color>";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.MaxHealth = maxHealth;
            player.Health = maxHealth;

            player.ShowHint(
                "<color=#ff00fa>第五教会 司祭</color>\n非常に<color=#ff00fa>第五的</color>な存在の恩寵を受けた第五主義者。\n施設を占領せよ！",
                10);

            Room spawnRoom = Room.Get(RoomType.Surface);
            Vector3 offset = Vector3.zero;
            player.Position = new Vector3(124f, 289f, 21f);
            // player.Rotation = spawnRoom.Rotation;

            player.ClearInventory();
            Log.Debug("Giving Items to F_Priest");
            player.AddItem(ItemType.GunSCP127);
            player.AddItem(ItemType.ArmorHeavy);
            CustomItem.TryGive(player, 6, false);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.GrenadeHE);

            var light = Light.Create(Vector3.zero);
            light.Position = player.Transform.position + new Vector3(0f, -0.08f, 0f);
            light.Transform.parent = player.Transform;
            light.Scale = new Vector3(1f, 1f, 1f);
            light.Range = 10f;
            light.Intensity = 1.25f;
            light.Color = Color.magenta;

            if (_auraHandle.IsRunning)
                Timing.KillCoroutines(_auraHandle);
            _auraHandle = Timing.RunCoroutine(Scp3005AuraCoroutine(player));
        }

        private IEnumerator<float> Scp3005AuraCoroutine(Player player)
        {
            for (;;)
            {
                if (player == null || !player.IsAlive || player.GetCustomRole() != CRoleTypeId.FifthistPriest)
                    yield break;

                foreach (Player target in Player.List)
                {
                    if (target == null || target == player || !target.IsAlive)
                        continue;

                    if (target.GetTeam() == CTeam.Fifthists ||
                        target.GetCustomRole() == CRoleTypeId.Scp3005)
                        continue;

                    float distance = Vector3.Distance(player.Position, target.Position);
                    if (distance <= 2.75f)
                    {
                        target.Hurt(25f, "<color=#ff00fa>第五的</color>な力による影響");
                        player.ShowHitMarker();
                    }
                }

                yield return Timing.WaitForSeconds(1.5f);
            }
        }

        protected override void OnDying(DyingEventArgs ev)
        {
            if (_auraHandle.IsRunning)
                Timing.KillCoroutines(_auraHandle);

            base.OnDying(ev);
        }
    }
}
