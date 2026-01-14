using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Slafight_Plugin_EXILED.API.Enums;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class GetQueue : ICommand
{
    public string Command => "getqueue";
    public string[] Aliases { get; } = { "getq", "gq", "showq", ".3" };
    public string Description => "Get Queued Events.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = "You don't have permission to execute this command.";
            return false;
        }

        var seh = Plugin.Singleton.SpecialEventsHandler;
        var queues = seh.EventQueue.ToList();
        var result = queues.Any() ? String.Join(",", queues) : "none";

        response = $"Now queued Special Events: {result}";
        return true;
    }
}