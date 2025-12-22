using Exiled.API.Features;
using PlayerRoles;

namespace Slafight_Plugin_EXILED.API.Features;

public static class CustomBooleans
{
    public static bool IsRoleUnassigned(this Player player)
    {
        return player.Role.Type == RoleTypeId.Spectator
               || player.Role.Type == RoleTypeId.None;
    }
}
