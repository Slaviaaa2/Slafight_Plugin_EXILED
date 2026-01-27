using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class NtfGeneral : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.NtfGeneral;
    protected override CTeam Team { get; set; } = CTeam.FoundationForces;
    protected override string UniqueRoleKey { get; set; } = "NtfGeneral";

    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.NtfCaptain);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        Log.Debug("Giving Items to NtfGeneral");
        player.AddItem(ItemType.KeycardMTFCaptain);
        player.TryAddCustomItem(2008);
        player.TryAddCustomItem(2009);
        player.AddItem(ItemType.GrenadeHE);
        player.AddItem(ItemType.GrenadeHE);
        player.AddItem(ItemType.Radio);
        CustomItem.TryGive(player, 12,false);
        CustomItem.TryGive(player, 2007, false);
            
        player.AddAmmo(AmmoType.Nato556,350);

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Commander");
        player.CustomInfo = "Nine-tailed Fox General";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=blue>九尾狐 司令官</color>\nEpsilon-11を率いる高位の司令官。\n隊長等と連携し、確実に施設に安定をもたらせ！",10f);
        });
    }
}