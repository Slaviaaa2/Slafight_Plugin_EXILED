using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class HdInfantry
{
    public void SpawnRole(Player player)
    {
        player.Role.Set(RoleTypeId.NtfPrivate);
        player.UniqueRole = "HdInfantry";
        Timing.CallDelayed(0.01f, () =>
        {
            player.MaxHealth = 100;
            player.Health = player.MaxHealth;
            player.ClearInventory();
            Log.Debug("Giving Items to HdInfantry");
            player.AddItem(ItemType.GunCrossvec);
            player.AddItem(ItemType.KeycardMTFOperative);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.GrenadeFlash);
            player.AddItem(ItemType.GrenadeHE);
            player.AddItem(ItemType.Radio);
            CustomItem.TryGive(player, 10,false);
            
            player.AddAmmo(AmmoType.Nato9,140);

            //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Infantry");
            player.CustomInfo = "<color=#727472>Hammer Down Infantry</color>";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            Timing.CallDelayed(0.05f, () =>
            {
                player.ShowHint("<color=#353535>ハンマーダウン 歩兵</color>\nNu-7の最下級兵だが、それでも強い装備が持たされている。\nNu-7とはこういう奴らなのだ",10f);
            });
        });
    }
}