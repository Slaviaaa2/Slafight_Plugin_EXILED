using Exiled.API.Features;
using PlayerRoles;

namespace Slafight_Plugin_EXILED.API.Features;

public static class CustomBooleans
{
    public static bool IsRoleUnassigned(this Player player)
    {
        return player.Role.Type is RoleTypeId.Spectator or RoleTypeId.None;
    }
}
