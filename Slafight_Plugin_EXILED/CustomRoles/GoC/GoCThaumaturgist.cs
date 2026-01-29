using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.CustomRoles.GoC;

public class GoCThaumaturgist : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.GoCThaumaturgist;
    protected override CTeam Team { get; set; } = CTeam.GoC;
    protected override string UniqueRoleKey { get; set; } = "GoCThaumaturgist";

    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.NtfSpecialist);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        player.AddItem(ItemType.GunE11SR);
        player.AddItem(ItemType.KeycardMTFOperative);
        player.TryAddCustomItem(2018);
        player.AddItem(ItemType.GrenadeHE);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.SCP500);
        player.AddItem(ItemType.Radio);
        player.TryAddCustomItem(10);
            
        player.AddAmmo(AmmoType.Nato556,140);

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Infantry");
        player.SetCustomInfo("Global Occult Collision: Broken Dagger Thaumaturgist");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#0000c8>GoC: Broken Dagger 超常技術スペシャリスト</color>\n",10f);
        });
    }
}