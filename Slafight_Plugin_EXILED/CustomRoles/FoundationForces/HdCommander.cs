using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Features;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class HdCommander : CRole
{
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.NtfSergeant);
        player.UniqueRole = "HdCommander";
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        Log.Debug("Giving Items to HdCommander");
        player.AddItem(ItemType.KeycardMTFOperative);
        player.AddItem(ItemType.Adrenaline);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.GrenadeHE);
        player.AddItem(ItemType.GrenadeHE);
        player.AddItem(ItemType.Radio);
        CustomItem.TryGive(player, 12,false);
        CustomItem.TryGive(player, 11, false);
            
        player.AddAmmo(AmmoType.Nato556,200);

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Commander");
        player.CustomInfo = "<color=#727472>Hammer Down Commander</color>";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=#252525>ハンマーダウン 指揮官</color>\nNu-7の歩兵たちを指揮し、制圧を進める。\n偉大なる我らが元帥の指示に従え！",10f);
        });
    }
}