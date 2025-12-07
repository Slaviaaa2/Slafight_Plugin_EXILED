using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class HdCommander
{
    public void SpawnRole(Player player)
    {
        player.Role.Set(RoleTypeId.NtfSergeant);
        player.UniqueRole = "HdCommander";
        Timing.CallDelayed(0.01f, () =>
        {
            player.MaxHealth = 100;
            player.Health = player.MaxHealth;
            player.ClearInventory();
            player.AddItem(ItemType.ArmorHeavy);
            player.AddItem(ItemType.KeycardMTFOperative);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.GrenadeHE);
            player.AddItem(ItemType.GrenadeHE);
            player.AddItem(ItemType.Radio);
            CustomItem.TryGive(player, 12,false);

            player.CustomInfo = "<color=#252525>Nu-7 Infantry - ハンマーダウン 歩兵</color>";
            Timing.CallDelayed(0.05f, () =>
            {
                player.ShowHint("<color=#252525>ハンマーダウン 指揮官</color>\nNu-7の歩兵たちを指揮し、制圧を進める。\n偉大なる我らが元帥の指示に従え！",10f);
            });
        });
    }
}