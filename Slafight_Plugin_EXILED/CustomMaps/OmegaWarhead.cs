using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using MEC;
using Slafight_Plugin_EXILED.SpecialEvents;
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.CustomMaps;

public class OmegaWarheadStartingEventArgs : EventArgs
{
    /// <summary>
    /// 起動しようとしているプレイヤー
    /// </summary>
    public Player Player { get; }
    public bool IsAllowed { get; }
    public OmegaWarheadStartingEventArgs(Player player, bool isAllowed) => Player = player;
}
public static class OmegaWarhead
{
    private static readonly SpecialEventsHandler SpecialEventsHandler = Plugin.Singleton.SpecialEventsHandler;
    private static int warheadPID;

    public static bool IsWarheadStarted;
    public static Player StartedPlayer = null;
    static Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
    public static event EventHandler<OmegaWarheadStartingEventArgs> OmegaWarheadStarting; 
    
    public static void StartProtocol(int pid, float triggerTime = 0f, Player startedBy = null)
    {
        if (IsWarheadStarted) return;
        warheadPID = pid;
        if (Warhead.IsInProgress) Warhead.Stop();
        Plugin.Singleton.EventHandler.DeadmanDisable = true;
        Warhead.IsLocked = true;
        if (pid != SpecialEventsHandler.EventPID) return;
        Timing.CallDelayed(triggerTime, () =>
        {
            if (pid != SpecialEventsHandler.EventPID || Warhead.IsInProgress) return;
            StartedPlayer = startedBy;
            var ev = new OmegaWarheadStartingEventArgs(startedBy, true);
            OmegaWarheadStarting?.Invoke(null, ev);
            if (!ev.IsAllowed) return;
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
                AlphaWarheadController.Singleton.RpcShake(false);
                Player.List.Where(p => p.IsAlive).ToList().ForEach(p =>
                {
                    p.ExplodeEffect(ProjectileType.FragGrenade);
                    p.Kill("OMEGA WARHEADに爆破された");
                });
            });
        });
    }
}