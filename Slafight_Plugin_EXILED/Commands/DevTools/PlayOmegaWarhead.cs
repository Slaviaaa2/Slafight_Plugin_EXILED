using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class PlayOmegaWarhead : ICommand
{
    public string Command => "playomega";
    public string[] Aliases { get; } = { "omega","p_ow","p_1" };
    public string Description => "Play Omega Warhead Scene";
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        DevToolFunctionHandler devToolFunctionHandler = new DevToolFunctionHandler();
        devToolFunctionHandler.PlayOmegaWarhead();

        response = ("Good luck.");
        return true;
    }
}