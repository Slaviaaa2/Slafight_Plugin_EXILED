using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class SpawnDebugToolRole : ICommand
{
    public string Command => "debugmode";
    public string[] Aliases { get; } = { "debug","spawndebug" };
    public string Description => "Debugging Mode. You can know Interacted Door Info and Coin Flipping Pos Info.";
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        Player player = Player.Get(sender);
        player.UniqueRole = "Debug";
        response = "You're now Entered Debugger Mode!";
        return true;
    }
}