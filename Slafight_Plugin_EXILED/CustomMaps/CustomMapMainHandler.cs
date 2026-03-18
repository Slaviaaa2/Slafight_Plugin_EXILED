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
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Commands.DevTools;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MainHandlers;
using Slafight_Plugin_EXILED.SpecialEvents;
using UnityEngine;
using ServerHandler = Exiled.Events.Handlers.Server;
using MapHandler = Exiled.Events.Handlers.Map;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.CustomMaps;

public class CustomMapMainHandler : CustomEventsHandler
{
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

    public CustomMapMainHandler()
    {
        MapHandler.Generated += OnGeneratorGenerating;
        ServerHandler.RoundStarted += OnRoundStarted;
        ServerHandler.RestartingRound += ResetInRestart;
        MapHandler.SpawningTeamVehicle += ChaosAnimation;
        LabApi.Events.Handlers.PlayerEvents.SearchedToy += InteractionButton;
        LabApi.Events.Handlers.PlayerEvents.InteractedDoor += DoorInteracted;
        ProjectMER.Events.Handlers.Schematic.SchematicSpawned += GetSchems;
    }

    ~CustomMapMainHandler()
    {
        MapHandler.Generated -= OnGeneratorGenerating;
        ServerHandler.RoundStarted -= OnRoundStarted;
        ServerHandler.RestartingRound -= ResetInRestart;
        MapHandler.SpawningTeamVehicle -= ChaosAnimation;
        LabApi.Events.Handlers.PlayerEvents.SearchedToy -= InteractionButton;
        LabApi.Events.Handlers.PlayerEvents.InteractedDoor -= DoorInteracted;
        ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= GetSchems;

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
    }

    private void TeleportClassD()
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
        WarheadBoomEffectUtil.StopAllEffects();
        OmegaWarhead.Reset();
        if (femurCoroutine.IsRunning)
            Timing.KillCoroutines(femurCoroutine);

        if (FBJoin != default && FBCP != default)
            femurCoroutine = Timing.RunCoroutine(FemurBreaker());

        if (STS != default && STC != default && STE != default)
        {
            Timing.CallDelayed(25f, () =>
            {
                if (!Round.InProgress) return; // ← これだけ追加
                trainCoroutine = Timing.RunCoroutine(TrainComing.SpawnTrainAndAnim(STS, STC, STE));
            });
        }
        else
        {
            Log.Error("Train Points not successfully spawned.");
        }

        ObjectPrefabLoader.LoadMap("aaa");
    }

    public void GetSchems(SchematicSpawnedEventArgs ev)
    {
        if (ev?.Schematic == null)
            return;

        var schematic = ev.Schematic;
        var name = schematic.Name;

        // 位置が欲しいケースだけ先に取る（失敗しても死なないように）
        Vector3 pos = default;
        bool hasPos = false;
        try
        {
            pos = schematic.Position;
            hasPos = true;
        }
        catch (Exception e)
        {
            Log.Error($"[GetSchems] Failed to get position for schematic {name}: {e}");
        }

        switch (name)
        {
            case "Surface_CarStopper_Bar":
                ChaosBar = schematic;
                if (hasPos)
                    ChaosBarNormalPos = pos;
                break;

            case "FemurBreaker_JoinPoint":
                if (hasPos)
                    FBJoin = pos;
                ev.DestroySafe(0.05f); // SchematicHelpers 拡張
                break;

            case "FemurBreaker_Door":
                FBDoor = schematic;
                break;

            case "FemurBreakerButton":
                FBButton = schematic;
                break;

            case "FemurBreaker_CapybaraPoint":
                if (hasPos)
                    FBCP = pos;
                ev.DestroySafe(0.05f);
                break;

            case "PDEX_JoinPoint":
                if (hasPos)
                    PDExJoin = pos;
                ev.DestroySafe(0.05f);
                break;

            case "PDEX_JoinPointKing":
                if (hasPos)
                    PDExJoinKing = pos;
                ev.DestroySafe(0.05f);
                break;

            case "OWB":
                if (hasPos)
                    OWB = pos;
                ev.DestroySafe(0.05f);
                break;

            case "OWJoin":
                if (hasPos)
                    OWJoin = pos;
                ev.DestroySafe(0.05f);
                break;

            case "ST_S":
                if (hasPos)
                    STS = pos;
                ev.DestroySafe(0.05f);
                break;

            case "ST_C":
                if (hasPos)
                    STC = pos;
                ev.DestroySafe(0.05f);
                break;

            case "ST_E":
                if (hasPos)
                    STE = pos;
                ev.DestroySafe(0.05f);
                break;

            case "Scp012_ThetaPrimed":
                Scp012_t = schematic;
                break;
        }

        FemurSetup = false;
        FemurBreaked = false;
        femuredPlayers.Clear();
    }

    private void ResetInRestart()
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

    private IEnumerator<float> PlayBarAnim(SchematicObject schem, float waitTime)
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

    private IEnumerator<float> Anim(SchematicObject schem, Vector3 startpos, Vector3 offset, float duration)
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
                    Exiled.API.Features.Cassie.MessageTranslated(
                        "SCP 1 0 6 recontained successfully by femur breaker",
                        "<color=red>SCP-106</color>のFEMUR BREAKERによる再収容に成功しました。");
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
        switch (Plugin.Singleton.Config.Season)
        {
            case 0: return;
            case 1: MapUtils.LoadMap("Holiday_HalloweenMap"); break;
            case 2: MapUtils.LoadMap("Holiday_ChristmasMap"); break;
        }
    }
}