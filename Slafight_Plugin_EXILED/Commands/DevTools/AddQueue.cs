using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using Slafight_Plugin_EXILED.API.Enums;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class AddQueue : ICommand
{
    public string Command => "addqueue";
    public string[] Aliases { get; } = { "addq","aq","nextq",".2" };
    public string Description => "Add new Queue Event.";
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = "You don't have permission to execute this command.";
            return false;
        }

        if (arguments.Count == 0)
        {
            response = $"Usage: {Command} <SpecialEventType>\n" +
                       "Available: " +
                       string.Join(", ", Enum.GetNames(typeof(SpecialEventType)));
            return false;
        }

        var arg = string.Join(" ", arguments).Trim();

        if (!Enum.TryParse<SpecialEventType>(arg, true, out var eventType) ||
            !Enum.IsDefined(typeof(SpecialEventType), eventType))
        {
            response = $"Unknown SpecialEventType: {arg}\n" +
                       "Available: " +
                       string.Join(", ", Enum.GetNames(typeof(SpecialEventType)));
            return false;
        }

        var seh = Plugin.Singleton.SpecialEventsHandler;
        seh.AddEvent(eventType);

        response = $"New queued special event set to: {eventType}";
        return true;
    }
}