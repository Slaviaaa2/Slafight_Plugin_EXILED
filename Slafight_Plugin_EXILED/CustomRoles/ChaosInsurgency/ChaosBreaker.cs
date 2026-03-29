using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.CustomRoles.ChaosInsurgency;

public class ChaosBreaker : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.ChaosBreaker;
    protected override CTeam Team { get; set; } = CTeam.ChaosInsurgency;
    protected override string UniqueRoleKey { get; set; } = "ChaosBreaker";

    public override void SpawnRole(Player? player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player!.Role.Set(RoleTypeId.ChaosConscript);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        
        player.ClearInventory();
        player.AddItem(ItemType.ArmorCombat);
        player.SetCategoryLimit(ItemCategory.Grenade, 4);
        player.TryAddCustomItem(700); // Fake Grenade
        player.TryAddCustomItem(700); // Fake Grenade
        player.TryAddCustomItem(700); // Fake Grenade
        player.TryAddCustomItem(700); // Fake Grenade
        player.TryAddCustomItem(1101); // Conscripts Card
        player.AddItem(ItemType.Painkillers);
            
        player.SetCustomInfo("Chaos Insurgency Breaker");
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<size=24><color=#228b22>カオス・インサージェンシー 現地工作員</color>\n貴方は何かの間違いでカオスに連れてこられてしまった哀れな現地住民だ。\n不良品かもしれないグレネードを用いて道を切り開け！",10f);
        });
    }
}