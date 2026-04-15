using System;
using Exiled.API.Features;
using Exiled.API.Features.Items;
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
    // TryAddCustomItem (Player 拡張)
    // ───────────────────────────────

    /// <summary>ID 指定でカスタムアイテムを付与する</summary>
    public static bool TryAddCustomItem(this Player player, uint itemId, bool showMessage = false)
    {
        if (!CustomItem.TryGet(itemId, out var item) || item is null)
            return false;

        try
        {
            item.Give(player, showMessage);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>型引数指定でカスタムアイテムを付与する</summary>
    public static bool TryAddCustomItem<T>(this Player player, bool showMessage = false) where T : CustomItem
    {
        if (!TryGet<T>(out var item) || item is null)
            return false;

        try
        {
            item.Give(player, showMessage);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // ───────────────────────────────
    // TryGetCustomItem (Item & Pickup 拡張)
    // ───────────────────────────────

    /// <summary>カスタムアイテムの取得を試みます。取得できた場合は customItem に格納します。</summary>
    public static bool TryGetCustomItem(this Item item, out CustomItem? customItem)
    {
        var result = CustomItem.TryGet(item, out var ci);
        customItem = ci;
        return result;
    }
    
    /// <summary>Pickup が持つカスタムアイテムを取得します。</summary>
    public static bool TryGetCustomItem(this Pickup pickup, out CustomItem? customItem)
    {
        return CustomItem.TryGet(pickup, out customItem);
    }

    // ───────────────────────────────
    // HasCustomItem (Player 拡張)
    // ───────────────────────────────

    /// <summary>型引数で指定したカスタムアイテムを所持しているか確認します。</summary>
    public static bool HasCustomItem<T>(this Player player) where T : CustomItem
    {
        if (player == null) return false;
        foreach (var item in player.Items)
        {
            if (item == null) continue;
            if (item.TryGetCustomItem(out var ci) && ci is T) return true;
        }
        return false;
    }

    public static bool HasCustomItem(this Player player, CustomItem expectedItem)
    {
        if (player == null || expectedItem == null) return false;
        foreach (var item in player.Items)
        {
            if (item == null) continue;
            if (item.TryGetCustomItem(out var ci) && ci!.Id == expectedItem.Id) return true;
        }
        return false;
    }

    public static bool HasCustomItem(this Player player, uint expectedId)
    {
        if (player == null) return false;
        foreach (var item in player.Items)
        {
            if (item == null) continue;
            if (item.TryGetCustomItem(out var ci) && ci!.Id == expectedId) return true;
        }
        return false;
    }

    public static bool HasCustomItem(this Player player, string expectedName)
    {
        if (player == null) return false;
        foreach (var item in player.Items)
        {
            if (item == null) continue;
            if (item.TryGetCustomItem(out var ci) && ci!.Name == expectedName) return true;
        }
        return false;
    }

    // ───────────────────────────────
    // IsCustomItem (型チェック拡張)
    // ───────────────────────────────

    public static bool IsCustomItem<T>(this Item item) where T : CustomItem
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