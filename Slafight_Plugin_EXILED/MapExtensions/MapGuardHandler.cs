using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Warhead;
using Interactables.Interobjects.DoorUtils;
using MEC;
using UnityEngine;

namespace Slafight_Plugin_EXILED;

public class MapGuardHandler
{
    public MapGuardHandler()
    {
        Exiled.Events.Handlers.Player.DamagingDoor += OnExplodingDoor;
    }

    ~MapGuardHandler()
    {
        Exiled.Events.Handlers.Player.DamagingDoor -= OnExplodingDoor;
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
        }
    }

    private void OnWarheadDoor(StartingEventArgs ev)
    {
        Timing.CallDelayed(10f, () =>
        {
            foreach (var door in Door.List)
            {
                if (!(Vector3.SqrMagnitude(door.Position - CustomMap.OWJoin) > 0.75 * 0.75))
                {
                    door.IsOpen = false;
                    door.ChangeLock(DoorLockType.AdminCommand);
                }
            }
        });
    }

    private void Message(Player player)
    {
        player.ShowHint("この扉は特殊な素材で守られているようだ・・・");
    }
}