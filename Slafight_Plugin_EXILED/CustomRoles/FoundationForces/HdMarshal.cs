using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class HdMarshal : CRole
{
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.NtfCaptain);
        player.UniqueRole = "HdMarshal";
        player.MaxHealth = 180;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        Log.Debug("Giving Items to HdMarshal");
        player.AddItem(ItemType.KeycardMTFCaptain);
        player.TryAddCustomItem(2010);
        player.TryAddCustomItem(2009);
        player.AddItem(ItemType.GrenadeHE);
        player.AddItem(ItemType.GrenadeHE);
        player.AddItem(ItemType.Radio);
        CustomItem.TryGive(player, 12,false);
        CustomItem.TryGive(player, 2011, false);
            
        player.AddAmmo(AmmoType.Nato556,250);

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Commander");
        player.CustomInfo = "<color=#727472>Hammer Down Marshal</color>";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#151515>ハンマーダウン 元帥</color>\nNu-7の師団を指揮し、勝利へと導く。\n敗北など許されない。突き進め！",10f);
        });
    }
}