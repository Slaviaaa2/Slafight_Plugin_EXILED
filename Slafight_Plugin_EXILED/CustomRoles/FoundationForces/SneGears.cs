using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class SneGears : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.SneGears;
    protected override CTeam Team { get; set; } = CTeam.FoundationForces;
    protected override string UniqueRoleKey { get; set; } = "SneGears";

    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.NtfSergeant);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 125;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        player.AddItem(ItemType.GunE11SR);
        player.AddItem(ItemType.KeycardMTFOperative);
        player.TryAddCustomItem(2010);
        player.AddItem(ItemType.Medkit);
        player.TryAddCustomItem(2026);
        player.AddItem(ItemType.Radio);
        player.AddItem(ItemType.ArmorHeavy);
            
        player.AddAmmo(AmmoType.Nato556,140);

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Infantry");
        player.SetCustomInfo("<color=#FF1493>See No Evil Gears</color>");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#FF1493>シー・ノー・イービル 対圧兵</color>\n気狂いどもに一撃を与えろ！",10f);
        });
    }
}