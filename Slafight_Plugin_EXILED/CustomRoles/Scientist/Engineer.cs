using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
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

namespace Slafight_Plugin_EXILED.CustomRoles.Scientist;

[CRoleAutoRegisterIgnore]
public class Engineer : CRole
{
    // TODO: need rework
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.Engineer;
    protected override CTeam Team { get; set; } = CTeam.Scientists;
    protected override string UniqueRoleKey { get; set; } = "Engineer";

    public override void SpawnRole(Player? player, RoleSpawnFlags flags = RoleSpawnFlags.All)
    {
        if (player == null) return;
        player.Role.Set(RoleTypeId.Scientist);
        base.SpawnRole(player, flags);

        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;

        player.ClearInventory();
        player.AddItem(ItemType.KeycardContainmentEngineer);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.Medkit);
        CItem.Get<Toolbox>()?.Give(player);

        var room = Room.Get(RoomType.HczTestRoom);
        var pos = room != null ? room.WorldPosition(new Vector3(0f, 1f, 0f)) : player.Position;
        player.Position = pos;

        player.SetCustomInfo("Engineer");

        Timing.CallDelayed(0.1f, () =>
        {
            player.ShowHint("<size=24><color=#00ffff>エンジニア</color>\nToolboxを用いて施設中を駆け巡れ！", 8f);
        });
    }
}