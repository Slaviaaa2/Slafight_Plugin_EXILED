using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using LightContainmentZoneDecontamination;
using MEC;
using UnityEngine;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class DeltaWarhead
{
    public void DeltaWarheadEvent()
    {
        var EventHandler = Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler;
        var SpecialEventHandler = Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler;
        Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
        int eventPID = SpecialEventHandler.EventPID;

        if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
        
        EventHandler.SpecialWarhead = true;
        EventHandler.WarheadLocked = true;
        EventHandler.DeadmanDisable = true;
        //EventHandler.DeconCancellFlag = true;
        
        if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
        Exiled.API.Features.Cassie.MessageTranslated("Detected Danger SCPs Containment Breach in The Heavy Containment Zone. Thinking approaches...","危険なSCiPの収容違反が中層で確認されました。対応策を考案します・・・");
        Timing.CallDelayed(60, () =>
        {
            if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
            //DecontaminationController.Singleton.DecontaminationOverride = DecontaminationController.DecontaminationStatus.Disabled;
            //DecontaminationController.Singleton.TimeOffset = int.MinValue;
            //DecontaminationController.DeconBroadcastDeconMessage = "除染は取り消されました";
            Exiled.API.Features.Cassie.MessageTranslated("My Approaches confirmed by O5 Command, Setup Delta System...","対応策がO5評議会に承認されました。<color=green>DELTAシステム</color>を準備しています・・・");
            Timing.CallDelayed(180, () =>
            {
                if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                Exiled.API.Features.Cassie.MessageTranslated($"By Order of O5 Command . Delta Warhead Sequence Activated . Heavy Containment Zone Detonated in T MINUS {Slafight_Plugin_EXILED.Plugin.Singleton.Config.DwBoomTime} Seconds.",$"O5評議会の決定により、<color=green>DELTA WARHEAD</color>シーケンスが開始されました。重度収容区画を{Slafight_Plugin_EXILED.Plugin.Singleton.Config.DwBoomTime}秒後に爆破します。",true);
                foreach (Room rooms in Room.List)
                {
                    if (rooms.Zone == ZoneType.HeavyContainment)
                    {
                        rooms.Color = Color.green;
                    }
                }
                CreateAndPlayAudio("delta.ogg","Exiled.API.Features.Cassie",Vector3.zero,true,null,false,999999999,0);
                Timing.CallDelayed(Slafight_Plugin_EXILED.Plugin.Singleton.Config.DwBoomTime, () =>
                {
                    if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                    Log.Debug("Delta Passed EventPID Checker");
                    List<ElevatorType> lockEvTypes = new List<ElevatorType>() { ElevatorType.LczA,ElevatorType.LczB };
                    foreach (Lift lift in Lift.List){
                        Log.Debug("sendforeach:"+lift.Type);
                        if (lockEvTypes.Contains(lift.Type))
                        {
                            Log.Debug("foreach catched: "+lift.Type);
                            lift.TryStart(0,true);
                        }
                    }
                    Log.Debug("Delta Passed TryStart Elevator Foreach.");
                    List<DoorType> lockEvDoorTypes = new List<DoorType>() { DoorType.CheckpointGateA,DoorType.CheckpointGateB,DoorType.ElevatorLczA,DoorType.ElevatorLczB };
                    foreach (Door door in Door.List)
                    {
                        Log.Debug("lockforeach:"+door.Type);
                        if (door.Type == DoorType.CheckpointGateA || door.Type == DoorType.CheckpointGateB)
                            door.IsOpen = false;
                        if (lockEvDoorTypes.Contains(door.Type))
                        {
                            Log.Debug("foreach catched: "+door.Type);
                            door.Lock(DoorLockType.Warhead);
                        }
                    }
                    Log.Debug("Delta Passed Lock Elevator Foreach.");
                    foreach (Player player in Player.List)
                    {
                        Log.Debug("playerforeach:"+player.Zone);
                        if (player.Zone == ZoneType.Entrance || player.Zone == ZoneType.HeavyContainment)
                        {
                            player.ExplodeEffect(ProjectileType.FragGrenade);
                            player.Kill("DELTA WARHEADに爆破された");
                        }
                    }
                    Log.Debug("Delta Passed Kill Player Foreach");
                });
            });
        });
    }
}