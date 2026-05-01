using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Warhead;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Random = System.Random;
using Slafight_Plugin_EXILED.API.Interface;

namespace Slafight_Plugin_EXILED.CustomRoles.Scientist;

public class MarionWheeler : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.MarionWheeler;
    protected override CTeam Team { get; set; } = CTeam.Scientists;
    protected override string UniqueRoleKey { get; set; } = "MarionWheeler";

    public override void SpawnRole(Player? player, RoleSpawnFlags flags = RoleSpawnFlags.All)
    {
        if (player == null) return;
        player.Role.Set(RoleTypeId.Scientist);
        base.SpawnRole(player, flags);

        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 120;
        player.Health = player.MaxHealth;

        player.ClearInventory();
        player.AddItem(ItemType.KeycardContainmentEngineer);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.Medkit);
        player.GiveCItem<GunScp7381>();
        player.Position = Door.Get(DoorType.Intercom).Position + Vector3.up * 1.25f;

        player.SetCustomInfo("Marion Wheeler");

        Timing.CallDelayed(0.1f, () =>
        {
            Timing.RunCoroutine(Coroutine(player));
            player.ShowHint("<size=24><color=#00ffff>マリオン・ホイーラー</color>\nW.I.P", 8f);
        });
    }

    private IEnumerator<float> Coroutine(Player player)
    {
        while (true)
        {
            if (!Check(player)) yield break;
            player.EnableEffect<Slowness>(20, 5f);
            yield return Timing.WaitForSeconds(1f);
        }
    }
}