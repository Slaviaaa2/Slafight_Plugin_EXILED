using Exiled.API.Enums;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Exiled.CustomItems.API.Features;

namespace Slafight_Plugin_EXILED.CustomRoles.Fifthist
{
    public class FifthistGuidance : CRole
    {
        protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.FifthistGuidance;
        protected override CTeam Team { get; set; } = CTeam.Fifthists;
        protected override string UniqueRoleKey { get; set; } = "FifthistGuidance";

        public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
        {
            base.SpawnRole(player, roleSpawnFlags);

            player.Role.Set(RoleTypeId.Tutorial);
            int maxHealth = 150;

            player.UniqueRole = UniqueRoleKey;
            player.SetCustomInfo("<color=#FF0090>Fifthist Guidance</color>");
            player.MaxHealth = maxHealth;
            player.Health = maxHealth;

            player.ShowHint(
                "<color=#ff00fa>第五教会 案内人</color>\n第五主義を広め、人々を第五世界へと誘う案内人。\n杖を使って相手を倒すと第五主義者に出来る。",
                10);

            Room spawnRoom = Room.Get(RoomType.Surface);
            Vector3 offset = Vector3.zero;
            player.Position = new Vector3(124f, 289f, 21f);
            // player.Rotation = spawnRoom.Rotation;

            player.ClearInventory();
            player.TryAddCustomItem(2020);
            player.AddItem(ItemType.ArmorHeavy);
            CustomItem.TryGive(player, 5, false);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.GrenadeHE);
        }
    }
}