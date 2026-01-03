using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Toys;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Server;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MapGeneration;
using MEC;
using PlayerRoles;
using ProjectMER.Events.Arguments;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MapExtensions;
using Slafight_Plugin_EXILED.SpecialEvents;
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
using Light = Exiled.API.Features.Toys.Light;

namespace Slafight_Plugin_EXILED
{
    public class CustomMap : CustomEventsHandler
    {
        public CustomMap()
        {
            // RoundStarted は1つのハンドラに統合
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Map.SpawningTeamVehicle += ChaosAnimation;
            LabApi.Events.Handlers.PlayerEvents.SearchedToy += InteractionButton;
            LabApi.Events.Handlers.PlayerEvents.InteractedDoor += DoorInteracted;
            ProjectMER.Events.Handlers.Schematic.SchematicSpawned += GetSchems;
        }

        ~CustomMap()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Map.SpawningTeamVehicle -= ChaosAnimation;
            LabApi.Events.Handlers.PlayerEvents.SearchedToy -= InteractionButton;
            LabApi.Events.Handlers.PlayerEvents.InteractedDoor -= DoorInteracted;
            ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= GetSchems;
        }

        /// <summary>
        /// RoundStarted 統合ハンドラ
        /// </summary>
        private void OnRoundStarted()
        {
            SetDoorState();
            SetupMaps();
            HolidaySeasonMapLoader();
        }

        public void SetDoorState()
        {
            const float PositionTolerance = 0.75f;
            foreach (Door door in Door.List)
            {
                if (door == null) continue;
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
                else if (Vector3.Distance(door.Position, OWJoin) <= PositionTolerance)
                {
                    door.Lock(DoorLockType.AdminCommand);
                }
            }
        }

        private void SetupMaps()
        {
            Timing.RunCoroutine(FemurBreaker());
        }

        private SchematicObject ChaosBar = null;
        private Vector3 ChaosBarNormalPos;
        private Vector3 FBJoin;
        private SchematicObject FBDoor;
        private static bool FemurSetup = false;
        private SchematicObject FBButton;
        private static bool FemurBreaked = false;
        private Vector3 FBCP;
        private Vector3 OWB;
        private Vector3 OWJoin;

        // APIs
        public static Vector3 PDExJoin;
        public static Vector3 PDExJoinKing;
        public static bool _femurSetup => FemurSetup;
        public static bool _femurBreaked => FemurBreaked;
        
        Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;

        public void GetSchems(SchematicSpawnedEventArgs ev)
        {
            switch (ev.Schematic.Name)
            {
                case "Surface_CarStopper_Bar":
                    ChaosBar = ev.Schematic;
                    ChaosBarNormalPos = ev.Schematic.Position;
                    break;

                case "FemurBreaker_JoinPoint":
                    FBJoin = ev.Schematic.Position;
                    ev.Schematic.Destroy();
                    break;

                case "FemurBreaker_Door":
                    FBDoor = ev.Schematic;
                    break;

                case "FemurBreakerButton":
                    FBButton = ev.Schematic;
                    break;

                case "FemurBreaker_CapybaraPoint":
                    FBCP = ev.Schematic.Position;
                    ev.Schematic.Destroy();
                    break;

                case "PDEX_JoinPoint":
                    PDExJoin = ev.Schematic.Position;
                    ev.Schematic.Destroy();
                    break;

                case "PDEX_JoinPointKing":
                    PDExJoinKing = ev.Schematic.Position;
                    ev.Schematic.Destroy();
                    break;
                case "OWB":
                    OWB = ev.Schematic.Position;
                    ev.Schematic.Destroy();
                    break;
                case "OWJoin":
                    OWJoin = ev.Schematic.Position;
                    ev.Schematic.Destroy();
                    break;
            }

            FemurSetup = false;
            FemurBreaked = false;
            femuredPlayers.Clear();
        }

        public void ChaosAnimation(SpawningTeamVehicleEventArgs ev)
        {
            if (ev.Team.TargetFaction == Faction.FoundationEnemy)
            {
                Timing.CallDelayed(2.25f, () =>
                {
                    Timing.RunCoroutine(PlayBarAnim(ChaosBar, 22f));
                });
            }
        }

        private IEnumerator<float> PlayBarAnim(SchematicObject schem, float waitTime)
        {
            // 上に 4 上げる
            yield return Timing.WaitUntilDone(Anim(schem, ChaosBarNormalPos, new Vector3(0, 4f, 0), 0.8f));

            // 待機
            yield return Timing.WaitForSeconds(waitTime);

            // 下に 4 下げる
            yield return Timing.WaitUntilDone(Anim(schem, ChaosBarNormalPos + new Vector3(0f, 4f, 0f), new Vector3(0, -4f, 0), 1.5f));
        }

        private IEnumerator<float> Anim(SchematicObject schem, Vector3 startpos, Vector3 offset, float duration)
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

        private List<Player> femuredPlayers = new();
        
        private void InteractionButton(PlayerSearchedToyEventArgs ev)
        {
            var specialEventsHandler = Plugin.Singleton.SpecialEventsHandler;
            const float PositionTolerance = 0.75f;
            
            //Log.Debug($"Interacted: {ev.Interactable.Position}, OWB: {OWB}, Distance: {Vector3.Distance(ev.Interactable.Position, OWB)}");
    
            if (Vector3.Distance(ev.Interactable.Position, new Vector3(-17.25f, 291.60f, -36.89f)) <= PositionTolerance)
            {
                Timing.RunCoroutine(PlayBarAnim(ChaosBar, 3f));
            }

            if (Vector3.Distance(ev.Interactable.Position, FBButton.Position) <= PositionTolerance)
            {
                if (FemurSetup && !FemurBreaked)
                {
                    FemurBreaked = true;
                    foreach (var fP in femuredPlayers.ToList())
                    {
                        fP.Kill("Femur Breakerの犠牲となった");
                    }
                    foreach (var _player in Player.List)
                    {
                        if (_player.GetCustomRole() == CRoleTypeId.Scp106 || (_player.GetCustomRole() == CRoleTypeId.None && _player.Role.Type == RoleTypeId.Scp106))
                        {
                            Timing.CallDelayed(28f, () =>
                            {
                                _player.Kill("Femur Breakerによって再収容された");
                            });
                        }
                    }
                    CreateAndPlayAudio("FemurBreaker.ogg","FemurBreaker",Vector3.zero,true,null,false,999999999,0);
                    Timing.CallDelayed(28f, () =>
                    {
                        Exiled.API.Features.Cassie.MessageTranslated("SCP 1 0 6 recontained successfully by femur breaker","<color=red>SCP-106</color>のFEMUR BREAKERによる再収容に成功しました。");
                    });
                }
                else
                {
                    ev.Player.SendHint("準備が完了していないか、既に実行されています。");
                }
            }

            if (Vector3.Distance(ev.Interactable.Position, OWB) <= PositionTolerance)
            {
                if (!SpecialEventsHandler.IsWarheadable() || OmegaWarhead.IsWarheadStarted)
                {
                    ev.Player.SendHint("何らかの要因で実行できませんでした");
                    return;
                }
                //Log.Debug($"OMEGA SWITCH: ACTIVATED\nlocalPID: {specialEventsHandler.EventPID}");
                OmegaWarhead.StartProtocol(specialEventsHandler.EventPID);
            }
        }

        private void DoorInteracted(PlayerInteractedDoorEventArgs ev)
        {
            const float PositionTolerance = 0.75f;
            //Log.Debug($"DoorInteracted in: {ev.Door.Position}, OWJoin: {OWJoin}, Distance: {Vector3.Distance(ev.Door.Position, OWJoin)}");
            if (Vector3.Distance(ev.Door.Position, OWJoin) <= PositionTolerance)
            {
                var castPlayer = Player.Get(ev.Player.NetworkId);
                var allowOpen = false;
                if (castPlayer != null)
                {
                    foreach (var item in castPlayer.Items.ToList())
                    {
                        CustomItem.TryGet(item, out var customItem);
                        if (customItem is { Id: 2005 })
                        {
                            allowOpen = true;
                        }
                    }

                    if (allowOpen)
                    {
                        ev.Door.IsOpened = !ev.Door.IsOpened;
                    }
                    else
                    {
                        ev.Player.SendHint("専用のアクセスパスが必要そうだ・・・");
                    }
                }
            }
        }

        private IEnumerator<float> FemurBreaker()
        {
            for (;;)
            {
                if (Round.IsLobby) yield break;
                foreach (var player in Player.List)
                {
                    if (Vector3.Distance(player.Position,FBJoin) <= 0.625f && player.GetTeam() != CTeam.SCPs)
                    {
                        player.Handcuff();
                        player.Position = FBCP;
                        femuredPlayers.Add(player);
                        FemurSetup = true;
                        Timing.RunCoroutine(Anim(FBDoor, FBDoor.Position, new Vector3(0f,-2.5f,0f),0.65f));
                        yield break;
                    }
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }
        
        ///////////////////////////
        /// SEASONABLE CONTENTS ///
        ///////////////////////////
        public void HolidaySeasonMapLoader()
        {
            // 0=Normal,
            // 1=Halloween,
            // 2=Christmas,
            // over=not available
            if (Plugin.Singleton.Config.Season == 0)
            {
                return;
            }
            else if (Plugin.Singleton.Config.Season == 1)
            {
                MapUtils.LoadMap("Holiday_HalloweenMap");
            }
            else if (Plugin.Singleton.Config.Season == 2)
            {
                MapUtils.LoadMap("Holiday_ChristmasMap");
            }
        }
    }
}