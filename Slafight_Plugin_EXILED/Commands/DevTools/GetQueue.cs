using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using Slafight_Plugin_EXILED.SpecialEvents;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class GetQueue : ICommand
{
    public string Command => "getqueue";
    public string[] Aliases { get; } = ["getq", "gq", "showq", ".3"];
    public string Description => "Get Queued Events.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = "You don't have permission to execute this command.";
            return false;
        }

        var seh = SpecialEventsHandler.Instance;
        var result = seh.EventQueue.Count > 0 ? string.Join(", ", seh.EventQueue) : "none";

        response = $"Now queued Special Events: {result}";
        return true;
    }
}