using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Objectives;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Scp079;
using LabApi.Events.CustomHandlers;
using LightContainmentZoneDecontamination;
using MapGeneration;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using PlayerRoles;
using ProjectMER.Events.Arguments;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MainHandlers;
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events
{
    public class OperationBlackoutEvent : SpecialEvent
    {
        // ==== メタ情報 ====
        public override SpecialEventType EventType => SpecialEventType.OperationBlackout;
        public override int MinPlayersRequired => 4;
        public override string LocalizedName => "Operation: Blackout";
        public override string TriggerRequirement => "4人以上のプレイヤー";

        // ==== 内部状態 ====
        public static bool IsOperation = false;

        private int _generatedCount = 0;

        private CoroutineHandle _specCoroutine;
        private CoroutineHandle _asphyxiationCoroutine;

        private static EventHandler EventHandler => Plugin.Singleton.EventHandler;

        // 音声再生デリゲート（EventHandler 経由）
        private Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
            EventHandler.CreateAndPlayAudio;

        public override bool IsReadyToExecute()
        {
            return false;
        }

        // ==== 実際に呼ばれるエントリポイント ====
        protected override void OnExecute(int eventPID)
        {
            // Execute(eventPid) 済みで CurrentEventPid セット済み
            if (KillEvent())
                return;

            IsOperation = true;
            _generatedCount = 0;

            DoBlackoutSetup();

            Timing.KillCoroutines(_specCoroutine);
            Timing.KillCoroutines(_asphyxiationCoroutine);
            _specCoroutine = Timing.RunCoroutine(SpecCoroutine());
        }

        // ==== イベントサブスク / 解除 ====
        public override void RegisterEvents()
        {
            Exiled.Events.Handlers.Map.Generated += OnMapGenerated;
            Exiled.Events.Handlers.Map.GeneratorActivating += OnGeneratorActivating;
            Exiled.Events.Handlers.Scp079.Recontaining += OnRecontaining;
        }

        public override void UnregisterEvents()
        {
            Exiled.Events.Handlers.Map.Generated -= OnMapGenerated;
            Exiled.Events.Handlers.Map.GeneratorActivating -= OnGeneratorActivating;
            Exiled.Events.Handlers.Scp079.Recontaining -= OnRecontaining;
        }

        // ==== 共通キャンセル＆クリーンアップ ====
        private bool KillEvent()
        {
            if (!CancelIfOutdated())
                return false;

            // このイベント固有の状態をリセット
            IsOperation = false;

            SpawnSystem.Disable = false;
            SpawnSystem.SwitchSpawnContext("Default");
            EventHandler.IsScpAutoSpawnLocked = false;

            Timing.KillCoroutines(_specCoroutine);
            Timing.KillCoroutines(_asphyxiationCoroutine);

            return true;
        }

        // ==== 初期セットアップ ====
        private void DoBlackoutSetup()
        {
            // リスポーン禁止
            SpawnSystem.Disable = true;

            // 全室消灯
            foreach (Room room in Room.List)
                room.Color = new Color(55 / 255f, 55 / 255f, 55 / 255f);

            LockInitialDoorsAndElevators();
            SetupLczGeneratorsPermissions();

            // SCP オートスポーンロック + プレイヤー配置
            Timing.CallDelayed(0.5f, () =>
            {
                if (KillEvent()) return;

                EventHandler.IsScpAutoSpawnLocked = true;
                TeleportAndReassignPlayers();

                // 無限長 BGM 的なサウンド
                CreateAndPlayAudio("Blackout.ogg", "Facility", Vector3.zero, true, null, false, 999999999f, 0f);

                Exiled.API.Features.Cassie.Clear();
                Exiled.API.Features.Cassie.MessageTranslated(
                    "Attention, All personnel. Facility electric systems is malfunctioning . please manual charge up the all generators.",
                    "全職員に通達。施設の電力システムに<color=red>問題</color>が発生しました。全ての非常用発電機を<color=#00b7eb>再起動</color>してください。",
                    true);
            });
        }

        private void LockInitialDoorsAndElevators()
        {
            List<DoorType> lockedDoors = new()
            {
                DoorType.ElevatorLczA,
                DoorType.ElevatorLczB,
                DoorType.CheckpointGateA,
                DoorType.CheckpointGateB,
                DoorType.ElevatorGateA,
                DoorType.ElevatorGateB
            };

            List<ElevatorType> lockedElevators = new()
            {
                ElevatorType.GateA,
                ElevatorType.GateB,
                ElevatorType.LczA,
                ElevatorType.LczB
            };

            foreach (Door door in Door.List)
            {
                if (!lockedDoors.Contains(door.Type))
                    continue;

                door.IsOpen = false;
                door.Lock(DoorLockType.AdminCommand);
            }

            foreach (Lift lift in Lift.List)
            {
                if (!lockedElevators.Contains(lift.Type))
                    continue;

                lift.TryStart(1, false);
            }
        }

        private void SetupLczGeneratorsPermissions()
        {
            foreach (var generator in Generator.List)
            {
                if (generator.Position.GetZone() != FacilityZone.LightContainment)
                    continue;

                generator.KeycardPermissions = KeycardPermissions.Intercom;
                Log.Debug($"[LczGenPermLog]{generator.KeycardPermissions}");
            }
        }

        private void TeleportAndReassignPlayers()
        {
            foreach (Player player in Player.List)
            {
                if (player.Role.Type == RoleTypeId.FacilityGuard)
                {
                    // ガードを LCZ Armory に集合
                    player.Teleport(Room.Get(RoomType.LczArmory)
                        .WorldPosition(new Vector3(0f, 1.5f, 0f)));
                }
                else if (player.Role.Team == Team.SCPs)
                {
                    // SCP は人間側に変更
                    player.SetRole(RoleTypeId.ClassD);
                }
            }
        }

        // ==== マップ生成時: 追加ジェネレーター設置 ====
        private void OnMapGenerated()
        {
            // このラウンドのイベントが OperationBlackout でないなら何もしない
            if (Plugin.Singleton.SpecialEventsHandler.NowEvent != SpecialEventType.OperationBlackout)
                return;

            // LCZ の禁止部屋を除いたランダムな部屋に 3 つジェネレーター
            var rooms = Room.List.ToList();
            var denyRooms = new List<RoomType>
            {
                RoomType.Lcz173,
                RoomType.LczArmory,
                RoomType.LczCheckpointA,
                RoomType.LczCheckpointB,
                RoomType.LczClassDSpawn
            };

            rooms.RemoveAll(r =>
                denyRooms.Contains(r.Type) ||
                r.Zone != ZoneType.LightContainment);

            int spawnedCount = 0;

            while (spawnedCount < 3 && rooms.Count > 0)
            {
                var targetRoom = rooms.RandomItem();
                var pos = targetRoom.WorldPosition(new Vector3(0f, 0.05f, 0f));

                GameObject generatorObj = PrefabHelper.Spawn(PrefabType.GeneratorStructure, pos);
                generatorObj.transform.eulerAngles += new Vector3(-90f, 0f, 0f);

                foreach (Generator generator in Generator.List)
                {
                    if (generator.GameObject != generatorObj) continue;
                    generator.KeycardPermissions = KeycardPermissions.Intercom;
                }

                Log.Debug($"[OperationBlackout] Spawned generator obj: {generatorObj != null}");
                Log.Debug($"Pos: {generatorObj.transform.position}");
                Log.Debug($"Rot: {generatorObj.transform.eulerAngles}");

                generatorObj.transform.position.TryGetRoom(out var room);
                Log.Debug($"Room: {room?.Name}");
                Log.Debug($"Zone: {room?.Zone}");

                spawnedCount++;
            }
        }

        // ==== ジェネレーター起動時 ====
        private void OnGeneratorActivating(GeneratorActivatingEventArgs ev)
        {
            if (KillEvent())
                return;

            _generatedCount++;

            // LCZ 全部起動後: LCZ エレベーター解放 + Cassie
            if (_generatedCount == 3)
            {
                Timing.CallDelayed(15f, () =>
                {
                    if (KillEvent()) return;

                    foreach (Door door in Door.List)
                    {
                        if (door.Type == DoorType.ElevatorLczA || door.Type == DoorType.ElevatorLczB)
                            door.Unlock();
                    }

                    Exiled.API.Features.Cassie.MessageTranslated(
                        "All Light Containment Zone emergency generators is now power upped . and Heavy Containment Zone Elevators now Online.",
                        "全ての軽度収容区画の非常用発電機が起動され、重度収容区画とのエレベーターが再起動しました。",
                        true);

                    Timing.CallDelayed(15f, () =>
                    {
                        if (KillEvent()) return;

                        Exiled.API.Features.Cassie.MessageTranslated(
                            "Warning, The Facility O 2 Supply Systems power down effect Detected. Please evacuation to the Upper Facility Zone.",
                            "警告、施設の酸素供給システムにて<color=red>停電による影響</color>が検出されました。出来るだけ早く施設の上部区画へ避難してください。",
                            true);
                    });
                });
            }
            // HCZ も全部起動: EZ 解放 + 酸素イベント進行
            else if (_generatedCount >= 6)
            {
                Timing.CallDelayed(10f, () =>
                {
                    if (KillEvent()) return;

                    foreach (Door door in Door.List)
                    {
                        if (door.Type == DoorType.CheckpointGateA || door.Type == DoorType.CheckpointGateB)
                            door.Unlock();
                    }

                    Exiled.API.Features.Cassie.MessageTranslated(
                        "All Heavy Containment Zone emergency generators is now power upped . and Entrance Zone Door systems now Online. Power upping Gate Elevator by Emergency Electric Power . . .",
                        "全ての重度収容区画の非常用発電機が起動され、エントランスゾーンのドアシステムが全て復帰しました。非常電源を用いてゲートのエレベーターを再起動しています・・・",
                        true);

                    // 非常電源ロック & 酸素枯渇パート
                    Timing.CallDelayed(60f, () =>
                    {
                        if (KillEvent()) return;

                        Exiled.API.Features.Cassie.MessageTranslated(
                            "Emergency Attention to the All personnel, Emergency Electric Power is Locked by Unknown Forces. and Facility O 2 is very very bad. Please evacuation to the Shelter or . .g1",
                            "全職員に緊急通達。非常用電源が何者かの影響によりロックされました。更に、現在の施設内酸素は<color=red>非常に悪く、危険</color>です。シェルター等に避難し、少しでも...(電力が切れる音)",
                            true);

                        Timing.CallDelayed(15f, () =>
                        {
                            if (KillEvent()) return;

                            CreateAndPlayAudio("oxygen.ogg", "Cassie", Vector3.zero, true, null, false, 999999999f, 0f);

                            Timing.KillCoroutines(_asphyxiationCoroutine);
                            _asphyxiationCoroutine = Timing.RunCoroutine(AsphyxiationCoroutine());
                        });
                    });
                });
            }
        }

        // ==== SCP-079 再収容禁止 ====
        private void OnRecontaining(RecontainingEventArgs ev)
        {
            if (KillEvent()) return;

            Log.Debug("[OperationBlackout] Canceling Recontain SCP-079");
            ev.IsAllowed = false;
            ev.Player?.ShowHint("<size=26>電力が無いようだ・・・</size>");
        }

        // ==== 酸素枯渇コルーチン ====
        private IEnumerator<float> AsphyxiationCoroutine()
        {
            // BGM 再生開始時に呼ばれる
            // ここで待機付きループにする
            for (;;)
            {
                if (KillEvent()) yield break;

                foreach (Player player in Player.List)
                {
                    player.EnableEffect(EffectType.Asphyxiated, 255);
                    player.EnableEffect(EffectType.Blurred, 1);
                    player.EnableEffect(EffectType.Slowness, 10);
                }

                yield return Timing.WaitForSeconds(10f);
            }
        }

        private IEnumerator<float> SpecCoroutine()
        {
            for (;;)
            {
                if (KillEvent()) yield break;

                foreach (Player player in Player.List)
                {
                    if (player.Role.Type == RoleTypeId.Spectator)
                    {
                        player.Role.Set(RoleTypeId.Scp0492);
                        player.Position = Room.Get(RoomType.LczGlassBox).WorldPosition(new Vector3(0f, 1f, 0f));
                    }
                }

                // 1秒間隔で監視
                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}
