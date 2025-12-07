using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class PlaySurfaceAttack : ICommand
{
    public string Command => "playsurfaceattack";
    public string[] Aliases { get; } = { "playsurfaceattack","p_SfAtk","p_0" };
    public string Description => "Play Surface Attack Nuke Scene";
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        DevToolFunctionHandler devToolFunctionHandler = new DevToolFunctionHandler();
        devToolFunctionHandler.PlaySurfaceAttack();

        response = ("Good luck.");
        return true;
    }
}