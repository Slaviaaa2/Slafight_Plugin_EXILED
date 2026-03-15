using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Extensions;

public static class CustomItemExtensions
{
    // ───────────────────────────────
    // TrySpawn
    // ───────────────────────────────

    public static bool TrySpawn<T>(Vector3 position, out Pickup? pickup) where T : CustomItem
    {
        pickup = null;
        foreach (var item in CustomItem.Registered)
            if (item is T) return CustomItem.TrySpawn(item.Id, position, out pickup);
        return false;
    }

    public static bool TrySpawn<T>(Player player, out Pickup? pickup) where T : CustomItem
        => TrySpawn<T>(player.Position, out pickup);

    public static bool TrySpawn<T>(float x, float y, float z, out Pickup? pickup) where T : CustomItem
        => TrySpawn<T>(new Vector3(x, y, z), out pickup);

    // ───────────────────────────────
    // TryGive
    // ───────────────────────────────

    public static bool TryGive<T>(Player player, bool displayMessage = true) where T : CustomItem
    {
        foreach (var item in CustomItem.Registered)
            if (item is T) return CustomItem.TryGive(player, item.Id, displayMessage);
        return false;
    }

    // ───────────────────────────────
    // TryGet
    // ───────────────────────────────

    public static bool TryGet<T>(out T? customItem) where T : CustomItem
    {
        foreach (var item in CustomItem.Registered)
            if (item is T t) { customItem = t; return true; }
        customItem = null;
        return false;
    }

    // ───────────────────────────────
    // Check
    // ───────────────────────────────

    public static bool IsCustomItem<T>(this Exiled.API.Features.Items.Item item) where T : CustomItem
    {
        foreach (var ci in CustomItem.Registered)
            if (ci is T && ci.Check(item)) return true;
        return false;
    }

    public static bool IsCustomItem<T>(this Pickup pickup) where T : CustomItem
    {
        foreach (var ci in CustomItem.Registered)
            if (ci is T && ci.Check(pickup)) return true;
        return false;
    }

    public static bool IsCustomItem<T>(this Player player) where T : CustomItem
    {
        foreach (var ci in CustomItem.Registered)
            if (ci is T && ci.Check(player)) return true;
        return false;
    }
}