using System;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;

namespace Slafight_Plugin_EXILED.Extensions;

public static class PlayerCustomItemExtensions
{
    public static bool TryAddCustomItem(this Player player, uint ItemId, bool showMessage = false)
    {
        CustomItem.TryGet(ItemId, out var item);
        try
        {
            item.Give(player,showMessage);
            return true;
        }
        catch (Exception e)
        {
            item = null;
            return false;
        }
    }
}