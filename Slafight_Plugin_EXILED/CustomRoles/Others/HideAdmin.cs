using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MainHandlers;

namespace Slafight_Plugin_EXILED.CustomRoles.Others;

public class HideAdmin : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.HideAdmin;
    protected override CTeam Team { get; set; } = CTeam.Others;
    protected override string UniqueRoleKey { get; set; } = "HideAdmin";

    public override void SpawnRole(Player? player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player!.Role.Set(RoleTypeId.Tutorial);
        player.UniqueRole = UniqueRoleKey;

        const int maxHealth = 99999;

        Timing.CallDelayed(0.05f, () =>
        {
            player.SetCustomInfo("<color=black>THE ADMINISTRATOR</color>");
            player.MaxHealth = maxHealth;
            player.Health = maxHealth;
            player.EnableEffect(EffectType.DamageReduction, 255);
            player.IsBypassModeEnabled = true;
            player.IsNoclipPermitted = true;

            player.ShowHint(
                "<size=24><color=black><b>THE ADMINISTRATOR</b></color>\nhaha",
                10);

            CItem.Get<CloakGenerator>()?.Give(player);
            player.AddItem(ItemType.KeycardO5);
            player.IsSpectatable = false;
        });
    }
}