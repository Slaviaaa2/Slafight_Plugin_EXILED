using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Scp096;
using Utils.NonAllocLINQ;

namespace Slafight_Plugin_EXILED.Extensions;

public static class Scp096Extensions
{
    public static void Register()
    {
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        Exiled.Events.Handlers.Scp096.AddingTarget += OnTriggered;
        Exiled.Events.Handlers.Scp096.RemovingTarget += OnRemoving;
    }

    public static void Unregister()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        Exiled.Events.Handlers.Scp096.AddingTarget -= OnTriggered;
        Exiled.Events.Handlers.Scp096.RemovingTarget -= OnRemoving;
    }

    private static Dictionary<Scp096Role, List<Player>> Scp096RolesData;

    private static void OnRoundStarted()
    {
        Scp096RolesData.Clear();
    }

    private static void OnTriggered(AddingTargetEventArgs ev)
    {
        if (!ev.IsAllowed) return;
        if (Scp096RolesData.TryGetValue(ev.Scp096, out var roles))
        {
            roles.AddIfNotContains(ev.Target);
        }
    }

    private static void OnRemoving(RemovingTargetEventArgs ev)
    {
        if (ev.Target.IsAlive) return;
        if (Scp096RolesData.TryGetValue(ev.Scp096, out var roles))
        {
            roles.Remove(ev.Target);
        }
    }
    
    public static List<Player> GetReallyTriggeredPlayers(this Scp096Role role)
    {
        return Scp096RolesData[role];
    }
}