using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Pickups;
using LightContainmentZoneDecontamination;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class FifthistsRaid
{
    public void FifthistsRaidEvent()
    {
        var EventHandler = Plugin.Singleton.EventHandler;
        var SpecialEventHandler = Plugin.Singleton.SpecialEventsHandler;
        Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
        int eventPID = SpecialEventHandler.EventPID;
        Plugin.Singleton.SpecialEventsHandler.isFifthistsRaidActive = true;
        
        if (eventPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;

        int i=0;
        foreach (Player player in Player.List)
        {
            if (player.Role.Team != Team.SCPs)
            {
                Plugin.Singleton.CustomRolesHandler.SpawnFifthist(player,RoleSpawnFlags.All);
                i++;
            }
            if (i >= Math.Truncate(Player.List.Count/4f)) break;
        }

        foreach (Player player in Player.List)
        {
            if (player.Role.Team == Team.SCPs)
            {
                player.SetRole(CRoleTypeId.Scp3005);
                break;
            }

            if (player.GetCustomRole() == CRoleTypeId.Scp3005)
            {
                break;
            }
        }

        Timing.CallDelayed(8f, () =>
        {
            //Exiled.API.Features.Cassie.MessageTranslated($"Attention, All personnel. Detected {i} Fifthist Forces in Gate B .",$"全職員に通達。Gate Bに{i}人の第五主義者が検出されました。",false,true);
            CreateAndPlayAudio("_w_fifthists.ogg","WaveTheme",Vector3.zero,true,null,false,999999999,0);
            CassieHelper.AnnounceFifthist(i);
        });
    }
}
