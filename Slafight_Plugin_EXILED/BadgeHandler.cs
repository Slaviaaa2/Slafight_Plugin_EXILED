using Exiled.Events.EventArgs.Player;
using Exiled.Permissions.Features;

namespace Slafight_Plugin_EXILED;

public class BadgeHandler
{
    public BadgeHandler()
    {
        Exiled.Events.Handlers.Player.Verified += _BadgeHandler;
    }

    ~BadgeHandler()
    {
        Exiled.Events.Handlers.Player.Verified -= _BadgeHandler;
    }

    public void _BadgeHandler(VerifiedEventArgs ev)
    {
        long steamId = long.Parse(ev.Player.RawUserId.Split('@')[0]);
    }
}