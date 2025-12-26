using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.Hints;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class SpawnMapEditRole : ICommand
{
    public string Command => "mapeditmode";
    public string[] Aliases { get; } = { "map","pmer","mer","editmap","mp","spawnmap" };
    public string Description => "<color=red>It's Command doesnt working! please disable hsm plugin!</color>";
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        Player player = Player.Get(sender);
        player.UniqueRole = "MapEditor";
        Plugin.Singleton.PlayerHUD.DestroyHints();
        response = "You're now Entered Map Editor Mode!";
        return true;
    }
}