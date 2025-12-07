using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class ReRollSetQueue : ICommand
{
    public string Command => "rerollqueue";
    public string[] Aliases { get; } = { "rerollspq","rrspq","rrq",".0" };
    public string Description => "Reroll Position Zero Queue Event.";
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.SetQueueRandomEvent();

        response = ("Special Events now Rerolled!\nNew Event: "+Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.localizedEventName);
        return true;
    }
}