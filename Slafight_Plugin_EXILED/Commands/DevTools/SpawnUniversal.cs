using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class SpawnUniversal : ICommand
{
    public string Command => "spawn";
    public string[] Aliases { get; } = { "spawn","us" };
    public string Description => "Universal Customrole Spawner";
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }
        if (arguments.Count == 0)
        {
            List<string> customRoles = new()
            {
                "173","3114","3005","966","NtfLieutenant","CI_Commando",
                "Fifthist","FifthistPriest","ZoneManager","FacilityManager",
                "Janitor","mp","debug","SnowWarrier"
            };

            response = "Failed to load Role. List of available roles: \n" + string.Join(", ", customRoles);
            return true; // 一覧表示なので true でも false でもお好みで
        }

        Player player = Player.Get(sender);
        var roleId = arguments[0];
        if (roleId=="173")
        {
            player?.Role.Set(RoleTypeId.Scp173,RoleSpawnFlags.All);
            player?.Position = Plugin.Singleton.EventHandler.Scp173SpawnPoint;
            player?.EnableEffect(EffectType.Slowness, 95,60f);
            player?.UniqueRole = null;
            response = $"You're now Scp173 - Old Room Positioned";
            return true;
        }
        if (roleId=="3114")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.CRScp3114Role.SpawnRole(player);   
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="3005")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.Spawn3005(player);   
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="966")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.CR_Scp966Role.SpawnRole(player);   
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="NtfLieutenant")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.CR_NtfAide.SpawnRole(player);   
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="CI_Commando")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.SpawnChaosCommando(player);   
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="Fifthist")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.SpawnFifthist(player);   
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="FifthistPriest")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.SpawnF_Priest(player);   
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="ZoneManager")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.CR_ZoneManager.SpawnRole(player);   
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="FacilityManager")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.CR_FacilityManager.SpawnRole(player);   
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="Janitor")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.CR_Janitor.SpawnRole(player);   
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="mp")
        {
            player.UniqueRole = "MapEditor";
            Slafight_Plugin_EXILED.Plugin.Singleton.PlayerHUD.DestroyHints();
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="debug")
        {
            player.UniqueRole = "Debug";
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        if (roleId=="SnowWarrier")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.SpawnSnowWarrier(player);   
            response = $"You're now {player.UniqueRole}";
            return true;
        }
        response = $"Failed to load Role.";
        return false;
        //Npc a = Npc.Spawn("a",RoleTypeId.ClassD); // for Debug
        //Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.Spawn3005(a);
    }
}