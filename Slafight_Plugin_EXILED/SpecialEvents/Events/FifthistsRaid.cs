using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Pickups;
using LightContainmentZoneDecontamination;
using PlayerRoles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class FifthistsRaid
{
    public void FifthistsRaidEvent()
    {
        var EventHandler = Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler;
        var SpecialEventHandler = Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler;
        Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
        int eventPID = SpecialEventHandler.EventPID;
        Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.isFifthistsRaidActive = true;
        
        if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;

        int i=0;
        foreach (Player player in Player.List)
        {
            if (player.Role.Team != Team.SCPs)
            {
                Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.SpawnFifthist(player);
                i++;
            }
            if (i >= Math.Truncate(Player.List.Count/4f)) break;
        }

        foreach (Player player in Player.List)
        {
            if (player.Role.Team == Team.SCPs)
            {
                Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.Spawn3005(player);
                break;
            }

            if (player.UniqueRole == "SCP-3005")
            {
                break;
            }
        }
        
        Cassie.MessageTranslated($"Attention, All personnel. Detected {i} Fifthist Forces in Gate B .",$"全職員に通達。Gate Bに{i}人の第五主義者が検出されました。",false,true);
    }
}
