using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.Permissions.Extensions;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class GiveUniversal : ICommand
{
    public string Command => "give";
    public string[] Aliases { get; } = ["give", "gi"];
    public string Description => "Universal CustomItem Giver";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"No permission: slperm.{Command}";
            return false;
        }

        var executor = Player.Get(sender);
        if (executor == null)
        {
            response = "Player not found.";
            return false;
        }

        if (arguments.Count == 0)
        {
            response = $"Usage: give <itemId> [targetId] [drop]\n{CItem.GetAllItemNames().Aggregate((a,b) => $"{a}, {b}")}";
            return false;
        }

        var itemId = arguments.At(0);
        if (!CItem.TryParseItem(itemId, out var itemType))
        {
            response = $"Unknown item: {itemId}\nAvailable: {string.Join(", ", CItem.GetAllItemNames())}";
            return false;
        }

        Player target = executor;
        bool forceDrop = false;
        if (arguments.Count >= 2)
        {
            if (arguments.At(1).Equals("drop", StringComparison.OrdinalIgnoreCase))
                forceDrop = true;
            else if (int.TryParse(arguments.At(1), out int targetId))
            {
                target = Player.Get(targetId) ?? throw new Exception($"Player {targetId} not found");
            }
        }

        Pickup result;
        if (forceDrop || target.IsInventoryFull)
        {
            result = CItem.GiveOrDropItem(target, itemType, target.Position + Vector3.up * 0.5f);
            response = result != null ? $"Dropped {itemId} for {target.Nickname}" : $"Drop failed: {itemId}";
        }
        else
        {
            var item = CItem.GiveItem(target, itemType);
            response = item != null ? $"Gave {itemId} to {target.Nickname}" : $"Give failed: {itemId}";
        }

        return true;
    }
}