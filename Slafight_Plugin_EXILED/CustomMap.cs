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
        // 位置判定のトレラントは定数に
        private const float PositionTolerance = 0.75f;
        private const float FemurJoinRadius = 0.625f;

        // 状態
        private SchematicObject ChaosBar;
        private Vector3 ChaosBarNormalPos;
        private Vector3 FBJoin;
        private SchematicObject FBDoor;
        private static bool FemurSetup;
        private SchematicObject FBButton;
        private static bool FemurBreaked;
        private Vector3 FBCP;
        private Vector3 OWB;
        private Vector3 OWJoin;
        private Vector3 STS;
        private Vector3 STC;
        private Vector3 STE;

        private readonly List<Player> femuredPlayers = new();
        private CoroutineHandle femurCoroutine;
        private CoroutineHandle trainCoroutine;

        // APIs
        public static Vector3 PDExJoin;
        public static Vector3 PDExJoinKing;
        public static bool _femurSetup => FemurSetup;
        public static bool _femurBreaked => FemurBreaked;

        private readonly Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio
            = EventHandler.CreateAndPlayAudio;

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

        /// <summary>RoundStarted 統合ハンドラ</summary>
        private void OnRoundStarted()
        {
            SetDoorState();
            SetupMaps();
            HolidaySeasonMapLoader();
        }

        public void SetDoorState()
        {
            // 位置判定のため、事前にOWJoinが有効かチェック
            bool hasOwJoin = OWJoin != default;

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
                        if (hasOwJoin)
                        {
                            // ここだけ位置判定を行う
                            if (Vector3.SqrMagnitude(door.Position - OWJoin) <= PositionTolerance * PositionTolerance)
                                door.Lock(DoorLockType.AdminCommand);
                        }
                        break;
                }
            }
        }

        private void SetupMaps()
        {
            // 既存Coroutineが走っていたら止める
            if (femurCoroutine.IsRunning)
                Timing.KillCoroutines(femurCoroutine);

            // FBJoinがセットされている時だけ開始
            if (FBJoin != default && FBCP != default)
                femurCoroutine = Timing.RunCoroutine(FemurBreaker());
            
            // SCP-021-JP
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
            }

            // ラウンド毎に状態リセット
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

            // 上に 4 上げる
            yield return Timing.WaitUntilDone(Anim(schem, ChaosBarNormalPos, new Vector3(0, 4f, 0), 0.8f));

            // 待機
            yield return Timing.WaitForSeconds(waitTime);

            // 下に 4 下げる
            yield return Timing.WaitUntilDone(Anim(schem, ChaosBarNormalPos + new Vector3(0f, 4f, 0f),
                new Vector3(0, -4f, 0), 1.5f));
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

            // ChaosBarボタン（固定座標）
            if (ChaosBar != null &&
                Vector3.SqrMagnitude(pos - new Vector3(-17.25f, 291.60f, -36.89f)) <= PositionTolerance * PositionTolerance)
            {
                Timing.RunCoroutine(PlayBarAnim(ChaosBar, 3f));
            }

            // Femurボタン
            if (FBButton != null &&
                Vector3.SqrMagnitude(pos - FBButton.Position) <= PositionTolerance * PositionTolerance)
            {
                if (FemurSetup && !FemurBreaked)
                {
                    FemurBreaked = true;

                    // femuredPlayersは固定サイズ小なのでforeachでOK
                    foreach (var fP in femuredPlayers.ToList())
                    {
                        if (fP is null || !fP.IsConnected)
                            continue;

                        fP.Kill("Femur Breakerの犠牲となった");
                    }

                    // 106処理はLINQでフィルタ→単ループ
                    var scp106s = Player.List
                        .Where(p =>
                            p != null &&
                            (p.GetCustomRole() == CRoleTypeId.Scp106 ||
                             (p.GetCustomRole() == CRoleTypeId.None && p.Role.Type == RoleTypeId.Scp106)))
                        .ToList();

                    foreach (var scp in scp106s)
                    {
                        var local = scp;
                        Timing.CallDelayed(28f, () =>
                        {
                            if (local != null && local.IsConnected)
                                local.Kill("Femur Breakerによって再収容された");
                        });
                    }

                    CreateAndPlayAudio("FemurBreaker.ogg", "FemurBreaker", Vector3.zero, true, null, false, 999999999, 0);

                    Timing.CallDelayed(28f, () =>
                    {
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

            // OMEGA WARHEAD ボタン
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
            if (OWJoin == default)
                return;

            // OWJoinに一致するドアだけ特別処理
            if (Vector3.SqrMagnitude(ev.Door.Position - OWJoin) > PositionTolerance * PositionTolerance)
                return;

            var castPlayer = Player.Get(ev.Player.NetworkId);
            if (castPlayer == null)
                return;

            // LINQで目的のカードを一度だけ取得
            bool allowOpen = castPlayer.Items.Any(item =>
                CustomItem.TryGet(item, out var customItem) &&
                customItem is { Id: 2005 });

            if (allowOpen)
            {
                ev.Door.IsOpened = !ev.Door.IsOpened;
            }
            else
            {
                ev.Player.SendHint("専用のアクセスパスが必要そうだ・・・");
            }
        }

        /// <summary>
        /// FemurBreaker用の「近くにいる1人だけ」検出コルーチン（LINQ風）
        /// </summary>
        private IEnumerator<float> FemurBreaker()
        {
            // Lobby中は回さない
            while (!Round.IsLobby && !Round.IsEnded)
            {
                // SCP以外でJoinPointに近いプレイヤーをLINQで1人だけ取る
                var target = Player.List
                    .Where(p =>
                        p != null &&
                        p.IsConnected &&
                        p.GetTeam() != CTeam.SCPs &&
                        Vector3.SqrMagnitude(p.Position - FBJoin) <= FemurJoinRadius * FemurJoinRadius)
                    .FirstOrDefault();

                if (target != null)
                {
                    target.Handcuff();
                    target.Position = FBCP;
                    femuredPlayers.Add(target);
                    FemurSetup = true;

                    if (FBDoor != null)
                        Timing.RunCoroutine(Anim(FBDoor, FBDoor.Position, new Vector3(0f, -2.5f, 0f), 0.65f));

                    // 1人捕まえたら終了（常時監視しない）
                    yield break;
                }

                // 0.5秒間隔ならGCもそこまで問題にならないが、
                // 必要ならLINQをやめてforループにするとさらに軽くなる。[web:44][web:45]
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        ///////////////////////////
        /// SEASONABLE CONTENTS ///
        ///////////////////////////
        public void HolidaySeasonMapLoader()
        {
            // 0=Normal, 1=Halloween, 2=Christmas, over=not available
            switch (Plugin.Singleton.Config.Season)
            {
                case 0:
                    return;
                case 1:
                    MapUtils.LoadMap("Holiday_HalloweenMap");
                    break;
                case 2:
                    MapUtils.LoadMap("Holiday_ChristmasMap");
                    break;
            }
        }
    }
}
