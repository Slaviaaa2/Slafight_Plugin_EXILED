using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using ProjectMER.Commands.Utility;
using ProjectMER.Events.Arguments;
using UnityEngine;

namespace Slafight_Plugin_EXILED;

public class EscapeHandler
{
    public EscapeHandler()
    {
        ProjectMER.Events.Handlers.Schematic.SchematicSpawned += SetEscapePoint;

        Exiled.Events.Handlers.Player.Escaping += CancelDefaultEscape;
        Exiled.Events.Handlers.Server.RoundStarted += AddEscapeCoroutine;
    }

    ~EscapeHandler()
    {
        ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= SetEscapePoint;
        
        Exiled.Events.Handlers.Player.Escaping -= CancelDefaultEscape;
        Exiled.Events.Handlers.Server.RoundStarted -= AddEscapeCoroutine;
    }
    
    public List<Vector3> EscapePoints = new List<Vector3>() { };

    public void SetEscapePoint(SchematicSpawnedEventArgs ev)
    {
        if (ev.Schematic.Name == "EscapePoint")
        {
            Vector3 pos = ev.Schematic.gameObject.transform.position;
            EscapePoints.Add(pos);
            ev.Schematic.Destroy();
        }
    }
    
    public void Escape(Player player)
    {
        Log.Debug("Escape Triggered. by: "+player.Nickname);
        if (string.IsNullOrEmpty(player.UniqueRole))
        {
            if (player.Role == RoleTypeId.ClassD)
            {
                if (player.Cuffer?.Role.Team == Team.FoundationForces || player.Cuffer?.Role.Team == Team.Scientists)
                {
                    player.Role.Set(RoleTypeId.NtfSpecialist,RoleSpawnFlags.All);
                }
                else
                {
                    player.Role.Set(RoleTypeId.ChaosConscript,RoleSpawnFlags.All);
                }
            }
            else if (player.Role == RoleTypeId.Scientist)
            {
                if (player.Cuffer?.Role.Team == Team.ChaosInsurgency || player.Cuffer?.Role.Team == Team.ClassD)
                {
                    player.Role.Set(RoleTypeId.ChaosConscript,RoleSpawnFlags.All);
                }
                else
                {
                    player.Role.Set(RoleTypeId.NtfSpecialist,RoleSpawnFlags.All);
                }
            }
            else if (player.Role.Team == Team.ChaosInsurgency)
            {
                if (player.Cuffer?.Role.Team == Team.FoundationForces || player.Cuffer?.Role.Team == Team.Scientists)
                {
                    player.Role.Set(RoleTypeId.NtfPrivate,RoleSpawnFlags.All);
                }
            }
            else if (player.Role.Team == Team.FoundationForces)
            {
                if (player.Cuffer?.Role.Team == Team.ChaosInsurgency || player.Cuffer?.Role.Team == Team.ClassD)
                {
                    player.Role.Set(RoleTypeId.ChaosConscript,RoleSpawnFlags.All);
                }
            }
        }
        else
        {
            if (player.UniqueRole == "SCP-3005")
            {
                if (Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.isFifthistsRaidActive)
                {
                    player.UniqueRole = null;
                    Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.SpawnF_Priest(player,RoleSpawnFlags.All);
                }
            }
        }
    }

    public void AddEscapeCoroutine()
    {
        Timing.RunCoroutine(EscapeCoroutine());
    }

    private IEnumerator<float> EscapeCoroutine()
    {
        for (;;)
        {
            if (Round.IsLobby) yield break;
            //Log.Debug("Coroutine is Alive!");
            foreach (Player player in Player.List)
            {
                if (player == null || !player.IsAlive) continue;
                //Log.Debug("P Foreach is Alive! Player: "+player.Nickname);
                foreach (Vector3 escapePoint in EscapePoints)
                {
                    //Log.Debug("E Foreach is Alive! EscapePoint: "+escapePoint);
                    if (Vector3.Distance(player.Position, escapePoint) <= 1.75f)
                    {
                        //Log.Debug("Trigger!");
                        Escape(player);
                    }
                }
            }
            yield return Timing.WaitForSeconds(0.5f);
        }
    }

    public void CancelDefaultEscape(EscapingEventArgs ev)
    {
        ev.IsAllowed = false;
    }
}