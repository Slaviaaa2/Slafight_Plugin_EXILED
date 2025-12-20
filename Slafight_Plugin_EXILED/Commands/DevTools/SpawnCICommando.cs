using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class SpawnCiCommando : ICommand
{
    public string Command => "spawnCICommando";
    public string[] Aliases { get; } = { "spawnCIC","CIC" };
    public string Description => "Reroll Special Events.\n<color=red>(Attention)If you use this command, now running events with a few exceptions will be cancelled!!!</color>";
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        Player player = Player.Get(sender);
        Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.SpawnChaosCommando(player);
        //Npc a = Npc.Spawn("a",RoleTypeId.ClassD); // for Debug
        //Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.Spawn3005(a);
        response = "You're now Scp966.";
        return true;
    }
}