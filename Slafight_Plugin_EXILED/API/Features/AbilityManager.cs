using System.Collections.Generic;
using Exiled.API.Features;

namespace Slafight_Plugin_EXILED.API.Features;

public static class AbilityManager
{
    public static readonly Dictionary<int, AbilityLoadout> Loadouts = new();

    // 「必ず作る」用
    public static AbilityLoadout GetOrCreateLoadout(Player player)
    {
        if (!Loadouts.TryGetValue(player.Id, out var loadout))
        {
            loadout = new AbilityLoadout();
            Loadouts[player.Id] = loadout;
        }
        return loadout;
    }

    // 「あれば取るだけ」用
    public static bool TryGetLoadout(Player player, out AbilityLoadout loadout)
        => Loadouts.TryGetValue(player.Id, out loadout);

    public static void ClearPlayer(Player player) => Loadouts.Remove(player.Id);

    public static void ClearAllLoadouts() => Loadouts.Clear();
    
    // ★ 追加：特定プレイヤーのスロットだけクリア
    public static void ClearSlots(Player player)
    {
        if (!Loadouts.TryGetValue(player.Id, out var loadout))
            return;

        for (int i = 0; i < AbilityLoadout.MaxSlots; i++)
            loadout.Slots[i] = null;

        loadout.ActiveIndex = 0;
    }
}