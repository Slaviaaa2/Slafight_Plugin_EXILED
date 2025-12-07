using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class NtfAide
{
    public void SpawnRole(Player player)
    {
        player.Role.Set(RoleTypeId.NtfSergeant);
        player.UniqueRole = "NtfAide";
        Timing.CallDelayed(0.01f, () =>
        {
            player.MaxHealth = 100;
            player.Health = player.MaxHealth;
            player.ClearInventory();
            player.AddItem(ItemType.GunCrossvec);
            player.AddItem(ItemType.KeycardMTFOperative);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.GrenadeFlash);
            player.AddItem(ItemType.ArmorCombat);
            player.AddItem(ItemType.Radio);

            player.CustomInfo = ("<color=#00b7eb>Ntf Aide - 補佐官</color>");
            Timing.CallDelayed(0.05f, () =>
            {
                player.ShowHint("<color=#00b7eb>九尾狐 補佐官</color>\n隊長の補佐を目的とし",10f);
            });
        });
    }
}