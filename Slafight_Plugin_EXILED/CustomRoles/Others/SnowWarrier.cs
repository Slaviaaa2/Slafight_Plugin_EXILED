using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;

namespace Slafight_Plugin_EXILED.CustomRoles.Others
{
    public class SnowWarrier : CRole
    {
        protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.SnowWarrier;
        protected override CTeam Team { get; set; } = CTeam.Others;
        protected override string UniqueRoleKey { get; set; } = "SnowWarrier";

        public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
        {
            base.SpawnRole(player, roleSpawnFlags);

            player.Role.Set(RoleTypeId.ChaosRifleman, RoleSpawnFlags.All);
            player.Role.Set(RoleTypeId.Tutorial, RoleSpawnFlags.AssignInventory);
            Plugin.Singleton.LabApiHandler.SchemSnowWarrier(LabApi.Features.Wrappers.Player.Get(player.ReferenceHub));

            int maxHealth = 1000;

            Timing.CallDelayed(0.05f, () =>
            {
                player.UniqueRole = UniqueRoleKey;
                player.CustomInfo = "<color=#FFFFFF>SNOW WARRIER</color>";
                player.InfoArea |= PlayerInfoArea.Nickname;
                player.InfoArea &= ~PlayerInfoArea.Role;
                player.MaxHealth = maxHealth;
                player.Health = maxHealth;
                player.EnableEffect(EffectType.Slowness, 10);

                player.ShowHint(
                    "<color=white>SNOW WARRIER</color>\n非常に<color=#ffffff>雪玉的</color>である。そうは思わんかね？",
                    10);

                player.AddItem(ItemType.SCP1509);
                player.AddItem(ItemType.GunCOM18);
                player.AddItem(ItemType.ArmorHeavy);
                player.AddItem(ItemType.SCP500);
                player.AddItem(ItemType.SCP500);
                player.AddItem(ItemType.KeycardO5);

                player.AddAmmo(AmmoType.Nato9, 50);
            });
        }
    }
}
