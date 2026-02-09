using Exiled.Events.EventArgs.Scp1509;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.Changes;

public class Scp1509Handler
{
    public Scp1509Handler()
    {
        Exiled.Events.Handlers.Scp1509.Resurrecting += OnReincarnating;
    }

    ~Scp1509Handler()
    {
        Exiled.Events.Handlers.Scp1509.Resurrecting -= OnReincarnating;
    }

    private void OnReincarnating(ResurrectingEventArgs ev)
    {
        var player = ev.Target;
        Timing.CallDelayed(0.1f, () =>
        {
            if (ev.Player?.GetTeam() == CTeam.FoundationForces)
            {
                player?.SetRole(RoleTypeId.NtfPrivate, RoleSpawnFlags.None);
            }
            else if (ev.Player?.GetTeam() == CTeam.Guards)
            {
                player?.SetRole(RoleTypeId.FacilityGuard, RoleSpawnFlags.None);
            }
            else if (ev.Player?.GetTeam() == CTeam.Scientists)
            {
                player?.SetRole(RoleTypeId.Scientist, RoleSpawnFlags.None);
            }
            else if (ev.Player?.GetTeam() == CTeam.ClassD)
            {
                player?.SetRole(RoleTypeId.ClassD, RoleSpawnFlags.None);
            }
            else if (ev.Player?.GetTeam() == CTeam.ChaosInsurgency)
            {
                player?.SetRole(RoleTypeId.ChaosConscript, RoleSpawnFlags.None);
            }
            else if (ev.Player?.GetTeam() == CTeam.Fifthists)
            {
                player?.SetRole(CRoleTypeId.FifthistConvert, RoleSpawnFlags.None);
            }
            else if (ev.Player?.GetTeam() == CTeam.GoC)
            {
                player?.SetRole(CRoleTypeId.GoCOperative, RoleSpawnFlags.None);
            }
            else
            {
                if (ev.Player == null) return;
                var state = ev.Player.GetRoleInfo();
                if (state.Custom != CRoleTypeId.None) player?.SetRole((CRoleTypeId)state.Custom, RoleSpawnFlags.None);
                else player?.SetRole(state.Vanilla, RoleSpawnFlags.None);
            }
        });
    }
}