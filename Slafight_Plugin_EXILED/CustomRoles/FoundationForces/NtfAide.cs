using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class NtfAide : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.NtfLieutenant;
    protected override CTeam Team { get; set; } = CTeam.FoundationForces;
    protected override string UniqueRoleKey { get; set; } = "NtfAide";

    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.NtfSergeant);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        Log.Debug("Giving Items to NtfAide");
        player.AddItem(ItemType.GunE11SR);
        player.AddItem(ItemType.KeycardMTFCaptain);
        player.AddItem(ItemType.Adrenaline);
        player.AddItem(ItemType.Adrenaline);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.GrenadeFlash);
        player.AddItem(ItemType.ArmorHeavy);
        player.AddItem(ItemType.Radio);

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Nine-tailed Fox Aide");
        player.CustomInfo = "Nine-tailed Fox Lieutenant";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#00b7eb>九尾狐 補佐官</color>\n隊長の補佐を目的とし、万一の際は代理・臨時隊長として指示を下せる。",10f);
        });
    }
}