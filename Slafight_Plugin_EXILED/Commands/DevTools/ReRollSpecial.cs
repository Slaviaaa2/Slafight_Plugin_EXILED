using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class ReRollSpecial : ICommand
{
    public string Command => "rerollspecial";
    public string[] Aliases { get; } = { "rerollsp","rrsp" };
    public string Description => "Reroll Special Events.\n<color=red>(Attention)If you use this command, now running events with a few exceptions will be cancelled!!!</color>";
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler.RerollSpecial();
        
        response = ("Special Events now Rerolled!");
        return true;
    }
}