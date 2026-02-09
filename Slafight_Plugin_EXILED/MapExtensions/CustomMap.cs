using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MEC;
using PlayerRoles;
using ProjectMER.Events.Arguments;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MainHandlers;
using Slafight_Plugin_EXILED.SpecialEvents;
using UnityEngine;
using ServerHandler = Exiled.Events.Handlers.Server;
using MapHandler = Exiled.Events.Handlers.Map;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.MapExtensions
{
    public class CustomMap : CustomEventsHandler
    {
        private const float PositionTolerance = 1.25f;
        private const float FemurJoinRadius = 0.625f;

        private SchematicObject ChaosBar;
        private Vector3 ChaosBarNormalPos;
        private Vector3 FBJoin;
        private SchematicObject FBDoor;
        private static bool FemurSetup;
        private SchematicObject FBButton;
        private static bool FemurBreaked;
        private Vector3 FBCP;
        private Vector3 OWB;
        public static Vector3 OWJoin;
        private Vector3 STS;
        private Vector3 STC;
        private Vector3 STE;
        public static SchematicObject Scp012_t;

        private readonly Dictionary<Vector3, DoorConfig> specialDoors = new();

        private readonly List<Player> femuredPlayers = new();
        private CoroutineHandle femurCoroutine;
        private CoroutineHandle trainCoroutine;

        public static Vector3 PDExJoin;
        public static Vector3 PDExJoinKing;
        public static bool _femurSetup => FemurSetup;
        public static bool _femurBreaked => FemurBreaked;

        private readonly Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio
            = EventHandler.CreateAndPlayAudio;

        private class DoorConfig
        {
            public ushort RequiredItemId { get; set; } = 0;
            public string? RequiredCode { get; set; }
            public string HintMessage { get; set; } = "専用のアクセスパスが必要そうだ・・・";
        }

        public CustomMap()
        {
            ServerHandler.RoundStarted += OnRoundStarted;
            MapHandler.SpawningTeamVehicle += ChaosAnimation;
            LabApi.Events.Handlers.PlayerEvents.SearchedToy += InteractionButton;
            LabApi.Events.Handlers.PlayerEvents.InteractedDoor += DoorInteracted;
            ProjectMER.Events.Handlers.Schematic.SchematicSpawned += GetSchems;
        }

        ~CustomMap()
        {
            ServerHandler.RoundStarted -= OnRoundStarted;
            MapHandler.SpawningTeamVehicle -= ChaosAnimation;
            LabApi.Events.Handlers.PlayerEvents.SearchedToy -= InteractionButton;
            LabApi.Events.Handlers.PlayerEvents.InteractedDoor -= DoorInteracted;
            ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= GetSchems;

            if (femurCoroutine.IsRunning)
                Timing.KillCoroutines(femurCoroutine);
        }

        private void OnRoundStarted()
        {
            SetupSpecialDoors();   // ★先に特別扉を登録
            SetDoorState();
            SetupMaps();
            HolidaySeasonMapLoader();
            TeleportClassD();
        }

        private void TeleportClassD()
        {
            Timing.CallDelayed(0.8f, () =>
            {
                foreach (var player in Player.List.ToList())
                {
                    if (player == null) continue;
                    var info = player.GetRoleInfo();
                    if (info is { Vanilla: RoleTypeId.ClassD, Custom: CRoleTypeId.None })
                    {
                        player.Position = new Vector3(20f, 263f, -155f);
                    }
                }
            });
        }

        private void SetupSpecialDoors()
        {
            specialDoors.Clear();

            // OWJoin：カスタムアイテム必須（コードは未使用）
            if (OWJoin != default)
            {
                specialDoors[OWJoin] = new DoorConfig
                {
                    RequiredItemId = 2005,
                    RequiredCode = null,
                    HintMessage = "専用のアクセスパスが必要そうだ・・・"
                };
            }

            // コード専用扉の例
            specialDoors[new Vector3(-18.614f, 257.005f, -91.739f)] = new DoorConfig
            {
                RequiredItemId = 0,
                RequiredCode = "55555",
                HintMessage = "コードが正しく無いようだ・・・"
            };
        }

        public void SetDoorState()
        {
            foreach (Door door in Door.List)
            {
                if (door is null)
                    continue;

                switch (door.Type)
                {
                    case DoorType.SurfaceGate:
                        door.RequireAllPermissions = true;
                        door.RequiredPermissions = DoorPermissionFlags.ExitGates;
                        break;

                    case DoorType.EscapeFinal:
                        door.Unlock();
                        break;

                    default:
                        foreach (var kvp in specialDoors)
                        {
                            if (Vector3.SqrMagnitude(door.Position - kvp.Key) <= PositionTolerance * PositionTolerance)
                            {
                                door.Lock(DoorLockType.AdminCommand);
                                break;
                            }
                        }
                        break;
                }
            }
        }

        private void SetupMaps()
        {
            if (femurCoroutine.IsRunning)
                Timing.KillCoroutines(femurCoroutine);

            if (FBJoin != default && FBCP != default)
                femurCoroutine = Timing.RunCoroutine(FemurBreaker());

            if (STS != default && STC != default && STE != default)
            {
                Timing.CallDelayed(25f, () =>
                {
                    trainCoroutine = Timing.RunCoroutine(TrainComing.SpawnTrainAndAnim(STS, STC, STE));
                });
            }
            else
            {
                Log.Error("Train Points not successfully spawned.");
            }
        }

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
                case "ST_S":
                    STS = ev.Schematic.Position;
                    ev.Schematic.Destroy();
                    break;
                case "ST_C":
                    STC = ev.Schematic.Position;
                    ev.Schematic.Destroy();
                    break;
                case "ST_E":
                    STE = ev.Schematic.Position;
                    ev.Schematic.Destroy();
                    break;
                case "Scp012_ThetaPrimed":
                    Scp012_t = ev.Schematic;
                    break;
            }

            FemurSetup = false;
            FemurBreaked = false;
            femuredPlayers.Clear();
        }

        public void ChaosAnimation(SpawningTeamVehicleEventArgs ev)
        {
            if (ev.Team.TargetFaction != Faction.FoundationEnemy || ChaosBar is null)
                return;

            Timing.CallDelayed(2.25f, () =>
            {
                Timing.RunCoroutine(PlayBarAnim(ChaosBar, 22f));
            });
        }

        private IEnumerator<float> PlayBarAnim(SchematicObject schem, float waitTime)
        {
            if (schem is null)
                yield break;

            yield return Timing.WaitUntilDone(Anim(schem, ChaosBarNormalPos, new Vector3(0, 4f, 0), 0.8f));
            yield return Timing.WaitForSeconds(waitTime);
            yield return Timing.WaitUntilDone(Anim(schem, ChaosBarNormalPos + new Vector3(0f, 4f, 0f), new Vector3(0, -4f, 0), 1.5f));
        }

        private IEnumerator<float> Anim(SchematicObject schem, Vector3 startpos, Vector3 offset, float duration)
        {
            if (schem is null || duration <= 0f)
                yield break;

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

        private void InteractionButton(PlayerSearchedToyEventArgs ev)
        {
            var specialEventsHandler = Plugin.Singleton.SpecialEventsHandler;
            var pos = ev.Interactable.Position;

            if (ChaosBar != null &&
                Vector3.SqrMagnitude(pos - new Vector3(-17.25f, 291.60f, -36.89f)) <= PositionTolerance * PositionTolerance)
            {
                Timing.RunCoroutine(PlayBarAnim(ChaosBar, 3f));
            }

            if (FBButton != null &&
                Vector3.SqrMagnitude(pos - FBButton.Position) <= PositionTolerance * PositionTolerance)
            {
                if (FemurSetup && !FemurBreaked)
                {
                    FemurBreaked = true;
                    foreach (var fP in femuredPlayers.ToList())
                    {
                        if (fP?.IsConnected == true)
                            fP.Kill("Femur Breakerの犠牲となった");
                    }

                    var scp106s = Player.List.Where(p => p?.IsConnected == true &&
                        (p.GetCustomRole() == CRoleTypeId.Scp106 ||
                         (p.GetCustomRole() == CRoleTypeId.None && p.Role.Type == RoleTypeId.Scp106))).ToList();

                    foreach (var scp in scp106s)
                    {
                        var local = scp;
                        Timing.CallDelayed(28f, () => { if (local?.IsConnected == true) local.Kill("Femur Breakerによって再収容された"); });
                    }

                    CreateAndPlayAudio("FemurBreaker.ogg", "FemurBreaker", Vector3.zero, true, null, false, 999999999, 0);
                    Timing.CallDelayed(28f, () => Exiled.API.Features.Cassie.MessageTranslated(
                        "SCP 1 0 6 recontained successfully by femur breaker",
                        "<color=red>SCP-106</color>のFEMUR BREAKERによる再収容に成功しました。"));
                }
                else
                {
                    ev.Player.SendHint("準備が完了していないか、既に実行されています。");
                }
            }

            if (OWB != default &&
                Vector3.SqrMagnitude(pos - OWB) <= PositionTolerance * PositionTolerance)
            {
                if (!SpecialEventsHandler.IsWarheadable() || OmegaWarhead.IsWarheadStarted)
                {
                    ev.Player.SendHint("何らかの要因で実行できませんでした");
                    return;
                }
                OmegaWarhead.StartProtocol(specialEventsHandler.EventPID);
            }
        }

        private void DoorInteracted(PlayerInteractedDoorEventArgs ev)
        {
            var castPlayer = Player.Get(ev.Player.NetworkId);
            if (castPlayer == null || specialDoors.Count == 0)
                return;

            // 近い特別扉を1つ特定
            KeyValuePair<Vector3, DoorConfig>? closest = null;
            float minDistSq = float.MaxValue;
            foreach (var kvp in specialDoors)
            {
                float distSq = Vector3.SqrMagnitude(ev.Door.Position - kvp.Key);
                if (distSq <= PositionTolerance * PositionTolerance && distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closest = kvp;
                }
            }

            if (!closest.HasValue)
                return;

            var config = closest.Value.Value;

            // アイテムチェック（RequiredItemIdが0ならアイテム条件なし）
            bool hasItem = false;
            if (config.RequiredItemId > 0)
            {
                foreach (var item in castPlayer.Items)
                {
                    if (CustomItem.TryGet(item, out var customItem) && customItem != null &&
                        customItem.Id == config.RequiredItemId)
                    {
                        hasItem = true;
                        break;
                    }
                }
            }

            // コードチェック
            bool hasCode = false;
            if (!string.IsNullOrEmpty(config.RequiredCode))
            {
                hasCode = RPNameSetter.Passcodes.TryGetValue(castPlayer, out string? playerCode) &&
                          playerCode == config.RequiredCode;
            }

            // OWJoin用: アイテムだけで開けたい → RequiredCode=nullなので hasCodeはfalse
            // コード扉: RequiredItemId=0, RequiredCode="..." → hasItem=false, hasCodeで判定
            bool allowOpen = (config.RequiredItemId > 0 && config.RequiredCode == null && hasItem)
                             || (config.RequiredItemId == 0 && config.RequiredCode != null && hasCode)
                             || (config.RequiredItemId > 0 && config.RequiredCode != null && (hasItem || hasCode));
            // ↑必要に応じてロジック変えてOK

            if (allowOpen)
            {
                ev.Door.IsOpened = !ev.Door.IsOpened;
            }
            else
            {
                ev.Player.SendHint(config.HintMessage);
            }
        }

        private IEnumerator<float> FemurBreaker()
        {
            while (!Round.IsLobby && !Round.IsEnded)
            {
                Player target = null;
                foreach (var p in Player.List)
                {
                    if (p != null && p.IsConnected && p.GetTeam() != CTeam.SCPs &&
                        Vector3.SqrMagnitude(p.Position - FBJoin) <= FemurJoinRadius * FemurJoinRadius)
                    {
                        target = p;
                        break;
                    }
                }

                if (target != null)
                {
                    target.Handcuff();
                    target.Position = FBCP;
                    femuredPlayers.Add(target);
                    FemurSetup = true;
                    if (FBDoor != null)
                        Timing.RunCoroutine(Anim(FBDoor, FBDoor.Position, new Vector3(0f, -2.5f, 0f), 0.65f));
                    yield break;
                }

                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        public void HolidaySeasonMapLoader()
        {
            switch (Plugin.Singleton.Config.Season)
            {
                case 0: return;
                case 1: MapUtils.LoadMap("Holiday_HalloweenMap"); break;
                case 2: MapUtils.LoadMap("Holiday_ChristmasMap"); break;
            }
        }
    }
}