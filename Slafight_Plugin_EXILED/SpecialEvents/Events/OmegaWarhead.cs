using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using MEC;
using UnityEngine;
using CameraType = Exiled.API.Enums.CameraType;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class OmegaWarhead
{
    public void OmegaWarheadEvent()
    {
        var EventHandler = Plugin.Singleton.EventHandler;
        var SpecialEventHandler = Plugin.Singleton.SpecialEventsHandler;
        Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
        int eventPID = SpecialEventHandler.EventPID;

        if (eventPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;
        
        EventHandler.SpecialWarhead = true;
        EventHandler.WarheadLocked = true;
        EventHandler.DeadmanDisable = true;
        
        if (eventPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;
        
        Timing.CallDelayed(30, () =>
        {
            if (eventPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;
            //Exiled.API.Features.Cassie.MessageTranslated("O5 Command has decided to halt containment breaches using alpha warheads. Continue evacuation.","O5評議会が<color=red>ALPHA WARHEAD</color>を用いた収容違反の一時解決を決定しました。起動までに引き続き非難をしてください。",true,true,false);
            foreach (Player player in Player.List)
            {
                MirrorExtensions.MessageTranslated(player,$"O5 Command has decided to halt containment breaches using alpha warhead . Continue evacuation .","",$"O5評議会が<color=red>ALPHA WARHEAD</color>を用いた収容違反の一時解決を決定しました。起動までに引き続き非難をしてください。",true,false);
            }
            Timing.CallDelayed(35, () =>
            {
                if (eventPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                Exiled.API.Features.Cassie.MessageTranslated("New Status for Containment Breach by O5 Command : Using OMEGA WARHEAD","O5による収容違反対応ステータス更新：<color=blue>OMEGA WARHEAD</color>を用いた対応",true,true);
                Exiled.API.Features.Cassie.MessageTranslated("New Status Accepted .","新ステータス：承認",false,false);
                MapExtensions.OmegaWarhead.StartProtocol(eventPID,555f);
            });
        });
    }
}