using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class DebugStart : ICommand
{
    public string Command => "debugstart";
    public string[] Aliases { get; } = { "debug2","dmode","startd","ds" };
    public string Description => "Start Round Debug Mode.";
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }
        if (!Round.IsLobby)
        {
            response = "You cannot start a round without a Lobby!";
            return false;
        }
        var executor = Player.Get(sender);
        if (executor == null)
        {
            response = "Executed Player not found.";
            return false;
        }

        DevToolFunctionHandler devToolFunctionHandler = new DevToolFunctionHandler();
        devToolFunctionHandler.DebugRoundStart(executor);

        response = ("Started Debug Round Successfully!");
        return true;
    }
}