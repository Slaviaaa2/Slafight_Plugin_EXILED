using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Map;
using LightContainmentZoneDecontamination;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomRoles;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.Hints;
using Slafight_Plugin_EXILED.MainHandlers;
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events
{
    public class FacilityTermination : SpecialEvent
    {
        public override SpecialEventType EventType => SpecialEventType.FacilityTermination;
        public override int MinPlayersRequired => 8;
        public override string LocalizedName => "FACILITY TERMINATION";
        public override string TriggerRequirement => "無し";
        
        private CoroutineHandle _mainCoroutine;
        private CoroutineHandle _humanitistsCoroutine;

        private static EventHandler EventHandler => Plugin.Singleton.EventHandler;
        private Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
            EventHandler.CreateAndPlayAudio;

        protected override void OnExecute(int eventPid)
        {
            if (KillEvent()) return;

            EventHandler.SpecialWarhead = true;
            EventHandler.WarheadLocked = true;
            EventHandler.DeadmanDisable = true;

            if (KillEvent()) return;

            RegisterCustomSpawnTable();
            SpawnSystem.SwitchSpawnContext("FacilityTerminationCustom");

            Timing.KillCoroutines(_mainCoroutine);
            Timing.KillCoroutines(_humanitistsCoroutine);
            _mainCoroutine = Timing.RunCoroutine(DecontaminationCoroutine());
            _humanitistsCoroutine = Timing.RunCoroutine(HumanitistsCoroutine());
        }

        private bool KillEvent()
        {
            if (!CancelIfOutdated()) return false;

            Timing.KillCoroutines(_mainCoroutine);
            Timing.KillCoroutines(_humanitistsCoroutine);
            SpawnSystem.SwitchSpawnContext("Default");
            return true;
        }

        // ===== エレベーター＆チェックポイント制御 =====

        private void SetElevatorLockByZone(ZoneType zone, bool locked)
        {
            foreach (var door in Door.List)
            {
                if (!door.IsElevator)
                    continue;

                bool isSurfaceElevator =
                    door.Type is DoorType.ElevatorGateA or DoorType.ElevatorGateB or DoorType.ElevatorNuke;
                bool isEntranceElevator =
                    door.Type is DoorType.ElevatorGateA or DoorType.ElevatorGateB or DoorType.ElevatorScp049;
                bool isHczElevator =
                    door.Type is DoorType.ElevatorScp049;
                bool isLczElevator =
                    door.Type is DoorType.ElevatorLczA or DoorType.ElevatorLczB;

                bool target = zone switch
                {
                    ZoneType.Surface => isSurfaceElevator,
                    ZoneType.Entrance => isEntranceElevator,
                    ZoneType.HeavyContainment => isHczElevator,
                    ZoneType.LightContainment => isLczElevator,
                    _ => false
                };

                if (!target)
                    continue;

                if (locked)
                {
                    door.IsOpen = false;
                    door.Lock(DoorLockType.AdminCommand);
                }
                else
                {
                    door.Unlock();
                    door.IsOpen = true;
                }
            }
        }

        private void SetCheckpointLock(bool locked)
        {
            foreach (var door in Door.List)
            {
                if (door.Type is DoorType.CheckpointLczA or DoorType.CheckpointLczB)
                {
                    if (locked)
                    {
                        door.IsOpen = false;
                        door.Lock(DoorLockType.AdminCommand);
                    }
                    else
                    {
                        door.Unlock();
                        door.IsOpen = true;
                    }
                }
            }
        }

        // ===== 除染＋段階ロック本体 =====

        private IEnumerator<float> DecontaminationCoroutine()
        {
            if (KillEvent()) yield break;

            // SCP全173化
            int scpCount = 0;
            foreach (var player in Player.List)
            {
                if (player?.GetTeam() == CTeam.SCPs)
                {
                    player.SetRole(CRoleTypeId.Sculpture);
                    scpCount++;
                }
            }

            Log.Debug($"[FacilityTermination] Converted {scpCount} SCPs to Sculpture");

            yield return Timing.WaitForSeconds(2f);
            if (KillEvent()) yield break;

            Plugin.Singleton.PlayerHUD.AllSyncHUD_();
            Exiled.API.Features.Cassie.MessageTranslated(
                "Attention, All personnel. Were recieved message from O5 Command. Please Red this and Terminate Human it Your Self.",
                "全職員に通達。O5評議会からメッセージを受信した為、お知らせします。これをよく読み、自身の人間性を<color=green>破壊</color>してください。<split>以下はO5評議会の総意によって作成されたメッセージです。<split>現時点で私たちの存在を知らない方々へ: 私たちはSCP財団という組織を代表しています。私たちのかつての使命は、異常な事物、実体、その他様々な現象の収容と研究を中心に展開されていました。この使命は過去100年以上にわたって私たちの組織の焦点でした。<split>やむを得ない事情により、この方針は変更されました。私たちの新たな使命は人類の根絶です。<split>今後の意思疎通は行われません。",
                true);

            yield return Timing.WaitForSeconds(25f);
            if (KillEvent()) yield break;
            CassieHelper.AnnounceLastOperationArrival();

            yield return Timing.WaitForSeconds(480f);
            Exiled.API.Features.Cassie.MessageTranslated(
                "Attention, All personnel. Were decided Decontamination of the Facility. Please Evacuate to the Light Containment Zone for Delta Protocol.",
                "全職員に通達。施設全体の<color=yellow>終了</color>が決定された為、これより地上～重度収容区画の<color=red>ロックダウン</color>及び<color=green>除染プロセス</color>を開始します。全職員は軽度収容区画に避難し、<color=green><b>DELTAプロトコル</b></color>を待機してください。");
            yield return Timing.WaitForSeconds(20f);
            if (KillEvent()) yield break;
            SpawnSystem.Disable = true;

            // ここから避難フェーズ開始：全エレベーター＋LCZチェックポイント開放
            SetElevatorLockByZone(ZoneType.Surface, false);
            SetElevatorLockByZone(ZoneType.Entrance, false);
            SetElevatorLockByZone(ZoneType.HeavyContainment, false);
            SetElevatorLockByZone(ZoneType.LightContainment, false);
            SetCheckpointLock(false);

            // HCZ緑色化＆全ドア開放
            foreach (var room in Room.List)
            {
                room.Color = Color.green;
                room.UnlockAll();
                room.Doors.ToList().ForEach(d =>
                {
                    d.Unlock();
                    d.IsOpen = true;
                });
            }

            CreateAndPlayAudio("newdelta.ogg", "DeltaWarhead", Vector3.zero, true, null, false, 999999999f, 0f);

            // ----- Surface -----
            yield return Timing.WaitForSeconds(10f);
            if (KillEvent()) yield break;
            Exiled.API.Features.Cassie.MessageTranslated(
                "Surface Zone Lockdown and Decontamination in T minus 80 Seconds.",
                "地上区画のロックダウン及び除染まで残り: 80秒", false, false);

            yield return Timing.WaitForSeconds(75f);
            if (KillEvent()) yield break;

            LockdownAndDecon(ZoneType.Surface);
            SetElevatorLockByZone(ZoneType.Surface, true);
            Exiled.API.Features.Cassie.MessageTranslated(
                "Surface Zone is now Lockdowned and Started Decontamination Process.",
                "地上区画がロックダウンされ、除染が開始されました。", false, false);

            // ----- Entrance -----
            Exiled.API.Features.Cassie.MessageTranslated(
                "Entrance Zone Lockdown and Decontamination in T minus 40 Seconds.",
                "上層区画のロックダウン及び除染まで残り: 40秒", false, false);

            yield return Timing.WaitForSeconds(35f);
            if (KillEvent()) yield break;

            LockdownAndDecon(ZoneType.Entrance);
            SetElevatorLockByZone(ZoneType.Entrance, true);
            Exiled.API.Features.Cassie.MessageTranslated(
                "Entrance Zone is now Lockdowned and Started Decontamination Process.",
                "上層区画がロックダウンされ、除染が開始されました。", false, false);

            // ----- Heavy Containment -----
            Exiled.API.Features.Cassie.MessageTranslated(
                "Heavy Containment Zone Lockdown and Decontamination in T minus 20 Seconds.",
                "重度収容区画のロックダウン及び除染まで残り: 20秒", false, false);

            yield return Timing.WaitForSeconds(15f);
            if (KillEvent()) yield break;

            LockdownAndDecon(ZoneType.HeavyContainment);
            SetElevatorLockByZone(ZoneType.HeavyContainment, true);
            Exiled.API.Features.Cassie.MessageTranslated(
                "Heavy Containment Zone is now Lockdowned and Started Decontamination Process.",
                "重度収容区画がロックダウンされ、除染が開始されました。", false, false);

            // ----- DELTA PROTOCOL: LCZ爆破 -----
            Exiled.API.Features.Cassie.MessageTranslated(
                "Attention, All personnel. Delta Protocol is started in Light Containment Zone and Detonate in T minus 100 seconds. Please Effect by Delta Protocol Warhead. See you human.",
                "全職員に通達。<color=green><b>DELTAプロトコル</b></color>が軽度収容区画にて開始されました。100秒後に爆破される、<b><color=green>DELTA PROTOCOL</color> <color=red>\"WARHEAD\"</color></b>の影響を受け、人間性を<color=yellow>終了</color>してください。");

            // ここでLCZエレベーター＋LCZチェックポイントもロック
            SetElevatorLockByZone(ZoneType.LightContainment, true);
            SetCheckpointLock(true);

            yield return Timing.WaitForSeconds(130f);
            if (KillEvent()) yield break;

            AlphaWarheadController.Singleton.RpcShake(false);
            CTeam.FoundationForces.EndRound();
            Player.List.Where(p => p.IsAlive).ToList().ForEach(p =>
            {
                p.ExplodeEffect(ProjectileType.FragGrenade);
                p.Kill("DELTA WARHEADに爆破された");
            });
        }

        private void LockdownAndDecon(ZoneType zone)
        {
            if (KillEvent()) return;

            var zoneRooms = Room.List.Where(r => r.Zone == zone).ToList();
            zoneRooms.ForEach(r => r.LockDown(-1, DoorLockType.DecontLockdown));

            Player.List.Where(p => p.Zone == zone && p.IsAlive)
                .ToList()
                .ForEach(p => p.EnableEffect(EffectType.Decontaminating));
        }

        private void RegisterCustomSpawnTable()
        {
            var ctx = new SpawnSystem.SpawnContext(
                "FacilityTerminationCustom",
                new() 
                { 
                    { SpawnTypeId.MTF_LastOperationNormal, 100 },
                    { SpawnTypeId.MTF_HDNormal, 0 },
                    { SpawnTypeId.MTF_NtfNormal, 0 }
                },
                new() 
                { 
                    { SpawnTypeId.GOI_ChaosNormal, 40 },
                    { SpawnTypeId.GOI_GoCNormal, 60 },
                    { SpawnTypeId.GOI_FifthistNormal, 0 }
                },
                new()
                {
                    { SpawnTypeId.MTF_LastOperationBackup, 100 },
                    { SpawnTypeId.MTF_HDBackup, 0 },
                    { SpawnTypeId.MTF_NtfBackup, 0 }
                },
                new()
                {
                    { SpawnTypeId.GOI_ChaosBackup, 40 },
                    { SpawnTypeId.GOI_GoCBackup, 60 },
                    { SpawnTypeId.GOI_FifthistBackup, 0 }
                },
                new()
                {
                    { SpawnTypeId.MTF_LastOperationNormal, new() { { new SpawnSystem.SpawnRoleKey(CRoleTypeId.Sculpture), (99f, true) } } },
                    { SpawnTypeId.MTF_LastOperationBackup, new() { { new SpawnSystem.SpawnRoleKey(CRoleTypeId.Sculpture), (99f, true) } } },
                    {
                        SpawnTypeId.GOI_GoCNormal, new()
                        {
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCSquadLeader), (1f, true) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCDeputy), (1f, true) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCMedic), (1f, false) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCThaumaturgist), (1f, false) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCCommunications), (1f, false) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCOperative), (99f, false) }
                        }
                    },
                    {
                        SpawnTypeId.GOI_GoCBackup, new()
                        {
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCSquadLeader), (1f, true) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCDeputy), (1f, true) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCMedic), (1f, false) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCThaumaturgist), (1f, false) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCCommunications), (1f, false) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCOperative), (99f, false) }
                        }
                    },
                    {
                        SpawnTypeId.GOI_ChaosNormal, new()
                        {
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.ChaosCommando), (1f, false) },
                            { new SpawnSystem.SpawnRoleKey(RoleTypeId.ChaosRepressor), (2f, false) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.ChaosSignal), (2f, false) },
                            { new SpawnSystem.SpawnRoleKey(RoleTypeId.ChaosMarauder), (2f, false) },
                            { new SpawnSystem.SpawnRoleKey(RoleTypeId.ChaosRifleman), (99f, false) }
                        }
                    },
                    {
                        SpawnTypeId.GOI_ChaosBackup, new()
                        {
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.ChaosSignal), (1f, true) },
                            { new SpawnSystem.SpawnRoleKey(RoleTypeId.ChaosMarauder), (2f, false) },
                            { new SpawnSystem.SpawnRoleKey(RoleTypeId.ChaosRifleman), (99f, false) }
                        }
                    }
                }
            );
            SpawnSystem.RegisterSpawnContext(ctx);
            Log.Debug("[FacilityTermination] Custom spawn context registered - Chaos/GoC/LastOp only");
        }
        
        private IEnumerator<float> HumanitistsCoroutine()
        {
            for (;;)
            {
                if (KillEvent()) yield break;

                var players = Player.List
                    .Where(p => p != null && p.IsAlive && p.Role.Type != RoleTypeId.Spectator)
                    .ToList();

                Log.Debug($"[Humanitists] AliveNonSpec={players.Count}");

                if (players.IsOnlyTeam(CTeam.GoC, "humanity"))
                {
                    Log.Debug("[Humanitists] Triggered");
                    Plugin.Singleton.CustomRolesHandler.EndRound(CTeam.GoC, "SavedHumanity");  // Handler経由呼び出し
                    SpawnSystem.SwitchSpawnContext("Default");
                    yield break;
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        private void CancelDeconAnnounce(AnnouncingDecontaminationEventArgs ev)
        {
            Timing.CallDelayed(0.05f, Exiled.API.Features.Cassie.Clear);
        }

        public override void RegisterEvents()
        {
            Exiled.Events.Handlers.Map.AnnouncingDecontamination += CancelDeconAnnounce;
            base.RegisterEvents();
        }

        public override void UnregisterEvents()
        {
            Exiled.Events.Handlers.Map.AnnouncingDecontamination -= CancelDeconAnnounce;
            base.UnregisterEvents();
        }
    }
}
