using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using MEC;
using UnityEngine;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class OmegaWarhead
{
    public void OmegaWarheadEvent()
    {
        var EventHandler = Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler;
        var SpecialEventHandler = Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler;
        Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
        int eventPID = SpecialEventHandler.EventPID;

        if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
        
        EventHandler.SpecialWarhead = true;
        EventHandler.WarheadLocked = true;
        EventHandler.DeadmanDisable = true;
        
        if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
        
        Exiled.API.Features.Cassie.MessageTranslated("Emergency , emergency , A large containment breach is currently started within the site. All personnel must immediately begin evacuation.","緊急、緊急、現在大規模な収容違反がサイト内で発生しています。全職員は警備隊の指示に従い、避難を開始してください。", true);
        Timing.CallDelayed(30, () =>
        {
            if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
            Exiled.API.Features.Cassie.MessageTranslated("O5 Command has decided to halt containment breaches using alpha warheads. Continue evacuation.","O5評議会が<color=red>ALPHA WARHEAD</color>を用いた収容違反の一時解決を決定しました。起動までに引き続き非難をしてください。");
            Timing.CallDelayed(35, () =>
            {
                if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                Exiled.API.Features.Cassie.MessageTranslated("New Status for Containment Breach by O5 Command: Using OMEGA WARHEAD","O5による収容違反対応ステータス更新：<color=blue>OMEGA WARHEAD</color>を用いた対応");
                Exiled.API.Features.Cassie.MessageTranslated("New Status Accepted .","新ステータス：承認",false,false);
                Timing.CallDelayed(300, () =>
                {
                    if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                    foreach (Room rooms in Room.List)
                    {
                        rooms.Color = Color.blue;
                    }

                    foreach (Door door in Door.List)
                    {
                        if (door.Type != DoorType.ElevatorGateA || door.Type != DoorType.ElevatorGateB || door.Type != DoorType.ElevatorLczA || door.Type != DoorType.ElevatorLczB || door.Type != DoorType.ElevatorNuke || door.Type != DoorType.ElevatorScp049 || door.Type != DoorType.ElevatorServerRoom)
                        {
                            door.IsOpen = true;
                            door.Lock(DoorLockType.Warhead);
                        }
                    }
                    Exiled.API.Features.Cassie.MessageTranslated($"By Order of O5 Command . Omega Warhead Sequence Activated . All Facility Detonated in T MINUS {Slafight_Plugin_EXILED.Plugin.Singleton.Config.OwBoomTime} Seconds.",$"O5評議会の決定により、<color=blue>OMEGA WARHEAD</color>シーケンスが開始されました。施設の全てを{Slafight_Plugin_EXILED.Plugin.Singleton.Config.OwBoomTime}秒後に爆破します。",true);
                    CreateAndPlayAudio("omega_v2.ogg","Exiled.API.Features.Cassie",Vector3.zero,true,null,false,999999999,0);
                    Timing.CallDelayed(Slafight_Plugin_EXILED.Plugin.Singleton.Config.OwBoomTime, () =>
                    {
                        if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                        foreach (Player player in Player.List)
                        {
                            if (player == null) continue;
                            player.ExplodeEffect(ProjectileType.FragGrenade);
                            player.Kill("OMEGA WARHEADに爆破された");
                        }
                    });
                });
            });
        });
    }
}