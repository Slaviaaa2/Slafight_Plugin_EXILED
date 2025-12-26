using System.Collections.Generic;
using Exiled.API.Features;

namespace Slafight_Plugin_EXILED.API.Features;

public static class AbilityManager
{
    public static readonly Dictionary<int, AbilityLoadout> Loadouts = new();

    public static AbilityLoadout GetLoadout(Player player)
    {
        if (!Loadouts.TryGetValue(player.Id, out var loadout))
        {
            loadout = new AbilityLoadout();
            Loadouts[player.Id] = loadout;
        }
        return loadout;
    }

    public static void ClearPlayer(Player player) => Loadouts.Remove(player.Id);
    
    public static void ClearAllLoadouts() => Loadouts.Clear();
}
