using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Toys;
using MEC;
using Slafight_Plugin_EXILED.SpecialEvents;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;

namespace Slafight_Plugin_EXILED.MapExtensions;

public static class OmegaWarhead
{
    private static readonly SpecialEventsHandler SpecialEventsHandler = Plugin.Singleton.SpecialEventsHandler;
    private static int warheadPID;

    public static bool IsWarheadStarted;
    static Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
    
    public static void StartProtocol(int pid, float triggerTime = 0f)
    {
        if (IsWarheadStarted) return;
        warheadPID = pid;
        if (Warhead.IsInProgress) Warhead.Stop();
        Plugin.Singleton.EventHandler.DeadmanDisable = true;
        Plugin.Singleton.EventHandler.WarheadLocked = true;
        //Log.Debug($"OW: Starting OMEGA WARHEAD Protocol. local:{pid}, global:{SpecialEventsHandler.EventPID}");
        if (pid != SpecialEventsHandler.EventPID) return;
        Timing.CallDelayed(triggerTime, () =>
        {
            if (pid != SpecialEventsHandler.EventPID || Warhead.IsInProgress) return;
            IsWarheadStarted = true;
            foreach (Room rooms in Room.List)
            {
                rooms.Color = Color.blue;
            }
            foreach (Door door in Door.List)
            {
                if (door.Type != DoorType.ElevatorGateA && door.Type != DoorType.ElevatorGateB && door.Type != DoorType.ElevatorLczA && door.Type != DoorType.ElevatorLczB && door.Type != DoorType.ElevatorNuke && door.Type != DoorType.ElevatorScp049 && door.Type != DoorType.ElevatorServerRoom)
                {
                    door.IsOpen = true;
                    door.PlaySound(DoorBeepType.InteractionAllowed);
                    door.Lock(DoorLockType.Warhead);
                }
            }
            Exiled.API.Features.Cassie.MessageTranslated($"By Order of O5 Command . Omega Warhead Sequence Activated . All Facility Detonated in T MINUS {Plugin.Singleton.Config.OwBoomTime} Seconds.",$"O5評議会の指令に基づいた操作により、<color=blue>OMEGA WARHEAD</color>シーケンスが開始されました。施設の全てを{Plugin.Singleton.Config.OwBoomTime}秒後に爆破します。",true);
            CreateAndPlayAudio("omega_v2.ogg","OmegaWarhead",Vector3.zero,true,null,false,999999999,0);
            Timing.CallDelayed(Plugin.Singleton.Config.OwBoomTime, () =>
            {
                if (pid != SpecialEventsHandler.EventPID) return;
                foreach (Player player in Player.List)
                {
                    if (player == null) continue;
                    player.ExplodeEffect(ProjectileType.FragGrenade);
                    player.Kill("OMEGA WARHEADに爆破された");
                }
            });
        });
    }
}