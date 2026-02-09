using Exiled.Events.EventArgs.Player;

namespace Slafight_Plugin_EXILED.MainHandlers;

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