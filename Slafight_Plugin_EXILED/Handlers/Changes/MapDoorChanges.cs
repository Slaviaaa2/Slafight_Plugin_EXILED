using Exiled.API.Enums;
using Exiled.API.Features.Doors;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.Handlers.Changes;

public class MapDoorChanges : EventHandlerBase
{
    public override void OnServerRoundStarted()
    {
        Door.Get(DoorType.EscapeFinal)?.Unlock();
        base.OnServerRoundStarted();
    }
}