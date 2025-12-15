using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Toys;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Server;
using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MapGeneration;
using MEC;
using PlayerRoles;
using ProjectMER.Events.Arguments;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using UnityEngine;

using Scp049Handler = Exiled.Events.Handlers.Scp049;
using Scp096Handler = Exiled.Events.Handlers.Scp096;
using ServerHandler = Exiled.Events.Handlers.Server;
using Scp330Handler = Exiled.Events.Handlers.Scp330;
using Warhead = Exiled.API.Features.Warhead;
using WarheadHandler = Exiled.Events.Handlers.Warhead;
using MapHandler = Exiled.Events.Handlers.Map;
using PlayerHandler = Exiled.Events.Handlers.Player;
using CassieHandler = Exiled.Events.Handlers.Cassie;

namespace Slafight_Plugin_EXILED
{
    public class CustomMap : CustomEventsHandler
    {
        public CustomMap()
        {
            
            Exiled.Events.Handlers.Server.RoundStarted += SetDoorState;
            Exiled.Events.Handlers.Map.SpawningTeamVehicle += ChaosAnimation;

            LabApi.Events.Handlers.PlayerEvents.SearchedToy += InteractionButton;

            ProjectMER.Events.Handlers.Schematic.SchematicSpawned += GetSchems;
        }
        ~CustomMap()
        {
            
            Exiled.Events.Handlers.Server.RoundStarted -= SetDoorState;
            Exiled.Events.Handlers.Map.SpawningTeamVehicle -= ChaosAnimation;

            LabApi.Events.Handlers.PlayerEvents.SearchedToy -= InteractionButton;

            ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= GetSchems;
        }

        public void SetDoorState()
        {
            foreach (Door door in Door.List)
            {
                if (door.Type == DoorType.SurfaceGate)
                {
                    door.RequireAllPermissions = true;
                    door.RequiredPermissions
                        = DoorPermissionFlags.ExitGates;
                }
                else if (door.Type == DoorType.EscapeFinal)
                {
                    door.Unlock();
                }
            }
        }

        private SchematicObject ChaosBar = null;
        private Vector3 ChaosBarNormalPos;

        public void GetSchems(SchematicSpawnedEventArgs ev)
        {
            if (ev.Schematic.Name == "Surface_CarStopper_Bar")
            {
                ChaosBar = ev.Schematic;
                ChaosBarNormalPos = ev.Schematic.Position;
            }
        }
        public void ChaosAnimation(SpawningTeamVehicleEventArgs ev)
        {
            if (ev.Team.TargetFaction == Faction.FoundationEnemy)
            {
                Timing.CallDelayed(2.25f, () =>
                {
                    Timing.RunCoroutine(PlayBarAnim(ChaosBar,22f));
                });
            }
        }

        private IEnumerator<float> PlayBarAnim(SchematicObject schem, float waitTime)
        {
            // 上に 4 上げる
            yield return Timing.WaitUntilDone(MoveBar(schem, ChaosBarNormalPos,new Vector3(0, 4f, 0), 0.8f));

            // 待機
            yield return Timing.WaitForSeconds(waitTime);

            // 下に 4 下げる
            yield return Timing.WaitUntilDone(MoveBar(schem, ChaosBarNormalPos+new Vector3(0f,4f,0f),new Vector3(0, -4f, 0), 1.5f));
        }

        private IEnumerator<float> MoveBar(SchematicObject schem, Vector3 startpos,Vector3 offset, float duration)
        {
            float elapsedTime = 0f;
            Vector3 startPos = startpos;
            Vector3 endPos = startPos + offset;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                schem.transform.position = Vector3.Lerp(startPos, endPos, progress);
                yield return 0f;
            }

            schem.transform.position = endPos;
        }


        public void InteractionButton(PlayerSearchedToyEventArgs ev)
        {
            // 許容誤差
            const float PositionTolerance = 0.15f; // 好きな距離に調整
            if (Vector3.Distance(ev.Interactable.Position,new Vector3(-17.25f, 291.60f, -36.89f)) <= PositionTolerance)
            {
                Timing.RunCoroutine(PlayBarAnim(ChaosBar, 3f));
            }
        }
    }
}