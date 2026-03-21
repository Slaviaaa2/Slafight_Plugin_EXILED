using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class SneOperator : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.SneOperator;
    protected override CTeam Team { get; set; } = CTeam.FoundationForces;
    protected override string UniqueRoleKey { get; set; } = "SneOperator";

    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.NtfCaptain);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 150;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        player.AddItem(ItemType.GunFRMG0);
        player.AddItem(ItemType.KeycardMTFCaptain);
        player.TryAddCustomItem(2010);
        player.TryAddCustomItem(2026);
        player.TryAddCustomItem(2027);
        player.TryAddCustomItem(2027);
        player.AddItem(ItemType.Radio);
        player.AddItem(ItemType.ArmorHeavy);
            
        player.AddAmmo(AmmoType.Nato556,220);

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Infantry");
        player.SetCustomInfo("<color=#FF1493>See No Evil Operator</color>");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<size=24><color=#FF1493>シー・ノー・イービル オペレーター</color>\n部隊を指揮し、気狂いどもに正常性による一撃を与えよ！",10f);
        });
    }
}