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
using ProjectMER.Features.Serializable;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Changes;
using Slafight_Plugin_EXILED.Commands.DevTools;
using Slafight_Plugin_EXILED.CustomMaps.Features;
using Slafight_Plugin_EXILED.CustomMaps.ObjectPrefabs;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MainHandlers;
using Slafight_Plugin_EXILED.SpecialEvents;
using UnityEngine;
using ServerHandler = Exiled.Events.Handlers.Server;
using MapHandler = Exiled.Events.Handlers.Map;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;
using Slafight_Plugin_EXILED.API.Interface;

namespace Slafight_Plugin_EXILED.CustomMaps;

public class CustomMapMainHandler : CustomEventsHandler, IBootstrapHandler
{
    public static CustomMapMainHandler Instance { get; private set; }
    public static void Register() { Instance = new(); CustomHandlersManager.RegisterEventsHandler(Instance); }
    public static void Unregister() { CustomHandlersManager.UnregisterEventsHandler(Instance); Instance = null; }

    private const float PositionTolerance = 1.25f;
    private const float FemurJoinRadius = 1.005f;

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

    private readonly List<Player> femuredPlayers = [];
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

    public CustomMapMainHandler()
    {
        MapHandler.Generated += OnGeneratorGenerating;
        ServerHandler.RoundStarted += OnRoundStarted;
        ServerHandler.RestartingRound += ResetInRestart;
        MapHandler.SpawningTeamVehicle += ChaosAnimation;
        LabApi.Events.Handlers.PlayerEvents.SearchedToy += InteractionButton;
        LabApi.Events.Handlers.PlayerEvents.InteractedDoor += DoorInteracted;

    }

    ~CustomMapMainHandler()
    {
        MapHandler.Generated -= OnGeneratorGenerating;
        ServerHandler.RoundStarted -= OnRoundStarted;
        ServerHandler.RestartingRound -= ResetInRestart;
        MapHandler.SpawningTeamVehicle -= ChaosAnimation;
        LabApi.Events.Handlers.PlayerEvents.SearchedToy -= InteractionButton;
        LabApi.Events.Handlers.PlayerEvents.InteractedDoor -= DoorInteracted;


        // ★ コルーチンを全部 kill
        if (femurCoroutine.IsRunning)
            Timing.KillCoroutines(femurCoroutine);
        if (trainCoroutine.IsRunning)
            Timing.KillCoroutines(trainCoroutine);
    }

    private void OnGeneratorGenerating()
    {
        Generator.List.Where(generator => generator.Room.Type == RoomType.HczServerRoom).ToList().ForEach(generator => generator.IsEngaged = true);
    }

    private void OnRoundStarted()
    {
        WarheadBoomEffectUtil.StopAllEffects();
        // ★ ラウンド切り替え前に残コルーチンを殺す
        if (femurCoroutine.IsRunning)
            Timing.KillCoroutines(femurCoroutine);
        if (trainCoroutine.IsRunning)
            Timing.KillCoroutines(trainCoroutine);

        SetupSpecialDoors();
        SetDoorState();
        SetupMaps();
        HolidaySeasonMapLoader();
        TeleportClassD();
        SetCandyState();
    }

    private static void SetCandyState()
    {
        Timing.CallDelayed(3f, () =>
        {
            if (!CandyChanges.CandyChances.ContainsKey("Default"))
                CandyChanges.Init();
            
            if (MapFlags.GetSeason() == SeasonTypeId.April)
            {
                CandyChanges.CandyChances.TryGetValue("Default", out var result);
                result.MostRareChance = 0.22f;
                result.RareCandiesChance = 0.5f;
                CandyChanges.TryAddDictionary("April", result);
                CandyChanges.TrySetActiveDictionary("April", out _);
                return;
            }

            CandyChanges.TrySetActiveDictionary("Default", out _);
        });
    }

    private static void TeleportClassD()
    {
        Timing.CallDelayed(1f, () =>
        {
            foreach (var player in Player.List.ToList())
            {
                if (player == null || !player.IsConnected) continue;
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

        if (OWJoin != default)
        {
            specialDoors[OWJoin] = new DoorConfig
            {
                RequiredItemId = 2005,
                RequiredCode = null,
                HintMessage = "専用のアクセスパスが必要そうだ・・・"
            };
        }

        specialDoors[new Vector3(-18.614f, 257.005f, -91.739f)] = new DoorConfig
        {
            RequiredItemId = 0,
            RequiredCode = "55555",
            HintMessage = "コードが正しくないようだ・・・"
        };

        specialDoors[MapFlags.SqDoorPoint] = new DoorConfig()
        {
            RequiredItemId = 0,
            RequiredCode = "0727",
            HintMessage = "コードが正しくないようだ・・・"
        };
    }

    public void SetDoorState()
    {
        foreach (var door in Door.List)
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
                    if (specialDoors.Any(kvp => Vector3.SqrMagnitude(door.Position - kvp.Key) <= PositionTolerance * PositionTolerance))
                    {
                        door.Lock(DoorLockType.AdminCommand);
                    }
                    break;
            }
        }
    }

    private void SetupMaps()
    {
        WarheadBoomEffectUtil.StopAllEffects();
        OmegaWarhead.Reset();
        if (femurCoroutine.IsRunning)
            Timing.KillCoroutines(femurCoroutine);

        ObjectPrefabLoader.LoadMap("aaa");

        Timing.CallDelayed(2.0f, () => 
        {
            GetSchematicsAndTriggerPoints();

            if (FBJoin != default && FBCP != default)
                femurCoroutine = Timing.RunCoroutine(FemurBreaker());

            if (STS != default && STC != default && STE != default)
            {
                Timing.CallDelayed(25f, () =>
                {
                    if (!Round.InProgress) return;
                    trainCoroutine = Timing.RunCoroutine(TrainComing.SpawnTrainAndAnim(STS, STC, STE));
                });
            }
            else
            {
                Log.Error("Train Points not successfully spawned.");
            }

            if (MapFlags.AntiAntiMemeDocPoint != default)
            {
                var doc = new Document().Create() as Document;
                doc?.DocumentType = DocumentType.AntiAntiMeme;
                doc?.Position = MapFlags.AntiAntiMemeDocPoint;
                doc?.ShowModel = false;
            }
        });
    }

    public void GetSchematicsAndTriggerPoints()
    {
        FemurSetup = false;
        FemurBreaked = false;
        femuredPlayers.Clear();

        foreach (var map in MapUtils.LoadedMaps.Values)
        {
            if (map.SpawnedObjects == null) continue;
            foreach (var meo in map.SpawnedObjects)
            {
                if (meo.TryGetComponent(out SchematicObject schematic))
                {
                    switch (schematic.Name)
                    {
                        case "Surface_CarStopper_Bar":
                            ChaosBar = schematic;
                            ChaosBarNormalPos = schematic.Position;
                            break;
                        case "FemurBreaker_Door":
                            FBDoor = schematic;
                            break;
                        case "FemurBreakerButton":
                            FBButton = schematic;
                            break;
                        case "Scp012_ThetaPrimed":
                            Scp012_t = schematic;
                            break;
                    }
                }
            }
        }

        foreach (var point in TriggerPointManager.GetAll())
        {
            if (point.Base is not SerializableCustomTriggerPoint trig || string.IsNullOrEmpty(trig.Tag))
                continue;

            var pos = TriggerPointManager.GetWorldPosition(point);
            switch (trig.Tag)
            {
                case "FemurBreaker_JoinPoint":
                    FBJoin = pos;
                    break;
                case "FemurBreaker_CapybaraPoint":
                    FBCP = pos;
                    break;
                case "PDEX_JoinPoint":
                    PDExJoin = pos;
                    break;
                case "PDEX_JoinPointKing":
                    PDExJoinKing = pos;
                    break;
                case "OWB":
                    OWB = pos;
                    break;
                case "OWJoin":
                    OWJoin = pos;
                    break;
                case "ST_S":
                    STS = pos;
                    break;
                case "ST_C":
                    STC = pos;
                    break;
                case "ST_E":
                    STE = pos;
                    break;
                case "AntiAntiMemeDoc":
                    MapFlags.AntiAntiMemeDocPoint = pos;
                    break;
                case "SQ_Door":
                    MapFlags.SqDoorPoint = pos;
                    break;
            }
        }
    }

    private static void ResetInRestart()
    {
        WarheadBoomEffectUtil.StopAllEffects();
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

    private IEnumerator<float> PlayBarAnim(SchematicObject? schem, float waitTime)
    {
        if (schem is null)
            yield break;

        // ★ ラウンド終了・null ガード
        if (Round.IsLobby || Round.IsEnded)
            yield break;

        yield return Timing.WaitUntilDone(Anim(schem, ChaosBarNormalPos, new Vector3(0, 4f, 0), 0.8f));

        if (Round.IsLobby || Round.IsEnded || schem == null || schem.transform == null)
            yield break;

        yield return Timing.WaitForSeconds(waitTime);

        if (Round.IsLobby || Round.IsEnded || schem == null || schem.transform == null)
            yield break;

        yield return Timing.WaitUntilDone(Anim(schem, ChaosBarNormalPos + new Vector3(0f, 4f, 0f), new Vector3(0, -4f, 0), 1.5f));
    }

    private static IEnumerator<float> Anim(SchematicObject schem, Vector3 startpos, Vector3 offset, float duration)
    {
        if (schem is null || schem.transform == null || duration <= 0f)
            yield break;

        float elapsedTime = 0f;
        Vector3 startPos = startpos;
        Vector3 endPos = startPos + offset;

        while (elapsedTime < duration)
        {
            if (Round.IsLobby || Round.IsEnded)
                yield break;

            if (schem == null || schem.transform == null)
                yield break;

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            schem.transform.position = Vector3.Lerp(startPos, endPos, progress);
            yield return 0f;
        }

        if (schem != null && schem.transform != null)
            schem.transform.position = endPos;
    }

    private void InteractionButton(PlayerSearchedToyEventArgs ev)
    {
        var specialEventsHandler = SpecialEventsHandler.Instance;
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
                    Timing.CallDelayed(28f, () =>
                    {
                        if (local?.IsConnected == true && FemurSetup && FemurBreaked)
                            local.Kill("Femur Breakerによって再収容された");
                    });
                }

                CreateAndPlayAudio("FemurBreaker.ogg", "FemurBreaker", Vector3.zero, true, null, false, 999999999, 0);
                Timing.CallDelayed(28f, () =>
                {
                    if (!FemurSetup || !FemurBreaked) return;
                    if (Player.List.Any(p => p?.IsConnected == true &&
                                             (p.GetCustomRole() == CRoleTypeId.Scp106 ||
                                              (p.GetCustomRole() == CRoleTypeId.None && p.Role.Type == RoleTypeId.Scp106))))
                    {
                        Exiled.API.Features.Cassie.MessageTranslated(
                            "SCP 1 0 6 recontained successfully by femur breaker",
                            "<color=red>SCP-106</color>のFEMUR BREAKERによる再収容に成功しました。");
                    }
                    else
                    {
                        Exiled.API.Features.Cassie.MessageTranslated(
                            "Femur Breaker Process Successfully Completed. but no effect for containment breach.",
                            "FEMUR BREAKERプロセスが正常に完了しましたが、収容違反への影響が確認されませんでした。");
                    }
                });
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
            OmegaWarhead.StartProtocol(0f, startedBy:ev.Player);
        }
    }

    private void DoorInteracted(PlayerInteractedDoorEventArgs ev)
    {
        var castPlayer = Player.Get(ev.Player.NetworkId);
        if (castPlayer == null || specialDoors.Count == 0)
            return;

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

        bool hasCode = false;
        if (!string.IsNullOrEmpty(config.RequiredCode))
        {
            hasCode = RPNameSetter.Passcodes.TryGetValue(castPlayer, out string? playerCode) &&
                      playerCode == config.RequiredCode;
        }

        bool allowOpen = (config.RequiredItemId > 0 && config.RequiredCode == null && hasItem)
                         || (config.RequiredItemId == 0 && config.RequiredCode != null && hasCode)
                         || (config.RequiredItemId > 0 && config.RequiredCode != null && (hasItem || hasCode));

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
        while (true)
        {
            if (!Round.InProgress)
            {
                femuredPlayers.Clear();
                FemurSetup = false;
                FemurBreaked = false;
                yield break;
            }
                
            var target = Player.List.OfType<Player>().FirstOrDefault(p => p.IsConnected && p.GetTeam() != CTeam.SCPs && Vector3.SqrMagnitude(p.Position - FBJoin) <= FemurJoinRadius * FemurJoinRadius);

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
        switch (MapFlags.GetSeason())
        {
            case SeasonTypeId.Halloween: MapUtils.LoadMap("Holiday_HalloweenMap"); break;
            case SeasonTypeId.Christmas: MapUtils.LoadMap("Holiday_ChristmasMap"); break;
        }
    }
}