using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Warhead;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using UnityEngine;

namespace Slafight_Plugin_EXILED;

public class MapGuardHandler
{
    public MapGuardHandler()
    {
        Exiled.Events.Handlers.Player.DamagingDoor += OnExplodingDoor;
        Exiled.Events.Handlers.Warhead.Starting += OnWarheadDoor;
    }

    ~MapGuardHandler()
    {
        Exiled.Events.Handlers.Player.DamagingDoor -= OnExplodingDoor;
        Exiled.Events.Handlers.Warhead.Starting -= OnWarheadDoor;
    }

    private void OnExplodingDoor(DamagingDoorEventArgs ev)
    {
        if (ev.DamageType != DoorDamageType.ServerCommand)
        {
            if (!(Vector3.SqrMagnitude(ev.Door.Position - CustomMap.OWJoin) > 0.75 * 0.75))
            {
                ev.IsAllowed = false;
                Message(ev.Player);
            }

            if (ev.Door.Type == DoorType.LightContainmentDoor)
            {
                var _ = ev.Door.Position.TryGetRoom(out var room);
                if (room.Name != RoomName.EzGateB) return;
                ev.IsAllowed = false;
                Message(ev.Player);
            }
        }
    }

    private void OnWarheadDoor(StartingEventArgs ev)
    {
        Timing.CallDelayed(30f, () =>
        {
            Log.Debug("abbabbabbaba");
            foreach (var door in Door.List)
            {
                if (!(Vector3.SqrMagnitude(door.Position - CustomMap.OWJoin) > 0.75 * 0.75))
                {
                    door.Unlock();
                    door.IsOpen = false;
                    door.Lock(DoorLockType.AdminCommand);
                    continue;
                }

                if (door.Type == DoorType.LightContainmentDoor)
                {
                    var _ = door.Position.TryGetRoom(out var room);
                    if (room.Name != RoomName.EzGateB) continue;
                    door.Unlock();
                    door.IsOpen = false;
                    door.Lock(DoorLockType.AdminCommand);
                }

                if (!(Vector3.SqrMagnitude(door.Position - new Vector3(139.153f, 300.128f, -65.381f)) > 0.75 * 0.75))
                {
                    door.Unlock();
                    door.IsOpen = false;
                    door.Lock(DoorLockType.AdminCommand);
                    continue;
                }
            }
        });
    }

    private void Message(Player player)
    {
        player.ShowHint("この扉は特殊な素材で守られているようだ・・・");
    }
}