using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class SnePurify : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.SnePurify;
    protected override CTeam Team { get; set; } = CTeam.FoundationForces;
    protected override string UniqueRoleKey { get; set; } = "SnePurify";

    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.NtfPrivate);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 110;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        player.AddItem(ItemType.SCP1509);
        player.AddItem(ItemType.KeycardMTFOperative);
        player.AddItem(ItemType.Adrenaline);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.GrenadeFlash);
        player.AddItem(ItemType.GrenadeHE);
        player.AddItem(ItemType.Radio);
        player.AddItem(ItemType.ArmorCombat);
            
        player.AddAmmo(AmmoType.Nato9,140);

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Infantry");
        player.SetCustomInfo("<color=#6a00ff>See No Evil Purify</color>");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#6a00ff>シー・ノー・イービル 浄化師</color>\n気狂いどもを正常しろ！",10f);
        });
    }
}