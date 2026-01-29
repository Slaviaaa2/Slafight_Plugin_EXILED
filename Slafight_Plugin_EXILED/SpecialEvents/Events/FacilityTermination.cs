using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using LightContainmentZoneDecontamination;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features; // SpecialEvent 基底
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events
{
    public class FacilityTermination : SpecialEvent
    {
        // ===== メタ情報 =====
        public override SpecialEventType EventType => SpecialEventType.FacilityTermination;
        public override int MinPlayersRequired => 0;
        public override string LocalizedName => "FACILITY TERMINATION";
        public override string TriggerRequirement => "無し";
        
        private CoroutineHandle _mainCoroutine;
        private CoroutineHandle _humanitistsCoroutine;

        public override bool IsReadyToExecute()
        {
            return false;
            return !SpecialEventsHandler.Instance.HappenedEvents.Take(5).Contains(SpecialEventType.FacilityTermination);
        }

        // ===== ショートカット =====
        private static EventHandler EventHandler => Plugin.Singleton.EventHandler;

        private Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
            EventHandler.CreateAndPlayAudio;

        // ===== 実行本体 =====
        protected override void OnExecute(int eventPid)
        {
            if (KillEvent()) return;

            // Warhead 関連フラグ
            EventHandler.SpecialWarhead = true;
            EventHandler.WarheadLocked = true;
            EventHandler.DeadmanDisable = true;

            if (KillEvent()) return;
            RegisterCustomSpawnTable();
            SpawnSystem.SwitchSpawnContext("FacilityTerminationCustom");
            Timing.KillCoroutines(_mainCoroutine);
            Timing.KillCoroutines(_humanitistsCoroutine);
            _mainCoroutine = Timing.RunCoroutine(Coroutine());
            _humanitistsCoroutine = Timing.RunCoroutine(HumanitistsCoroutine());
        }

        private bool KillEvent()
        {
            if (!CancelIfOutdated()) return false;
            Timing.KillCoroutines(_mainCoroutine);
            Timing.KillCoroutines(_humanitistsCoroutine);
            return true;
        }

        private IEnumerator<float> Coroutine()
        {
            if (KillEvent()) yield break;
            int i = 0;
            foreach (var player in Player.List)
            {
                if (player == null) continue;
                if (player.GetTeam() == CTeam.SCPs)
                {
                    player.SetRole(CRoleTypeId.Sculpture);
                    i++;
                }
            }
            yield return Timing.WaitForSeconds(2f);
            Exiled.API.Features.Cassie.MessageTranslated("Attention, All personnel. Were recieved message from O5 Command. Please Red this and Terminate Human it Your Self.",
                "全職員に通達。O5評議会からメッセージを受信した為、お知らせします。これをよく読み、自身の人間性を<color=green>破壊</color>してください。<split>以下はO5評議会の総意によって作成されたメッセージです。<split>現時点で私たちの存在を知らない方々へ: 私たちはSCP財団という組織を代表しています。私たちのかつての使命は、異常な事物、実体、その他様々な現象の収容と研究を中心に展開されていました。この使命は過去100年以上にわたって私たちの組織の焦点でした。<split>やむを得ない事情により、この方針は変更されました。私たちの新たな使命は人類の根絶です。<split>今後の意思疎通は行われません。");
            yield return Timing.WaitForSeconds(25f);
            CassieHelper.AnnounceLastOperationArrival();
            yield return Timing.WaitForSeconds(200f);
            Exiled.API.Features.Cassie.MessageTranslated("Attention, All personnel. ","");
        }

        private void RegisterCustomSpawnTable()
        {
            var FacilityTerminaationCustomTable = new SpawnSystem.SpawnContext(
                "FacilityTerminationCustom", // コンテキスト名

                // FoundationStaffWaveWeights（通常Wave）
                new()
                {
                    { SpawnTypeId.MTF_LastOperationNormal, 100 },
                },

                // FoundationEnemyWaveWeights（敵側Wave）: ここは通常と同じでもOK
                new()
                {
                    { SpawnTypeId.GOI_ChaosNormal, 40 },
                    { SpawnTypeId.GOI_GoCNormal, 60 }
                },

                // FoundationStaffMiniWaveWeights（ミニWave）
                new()
                {
                    { SpawnTypeId.MTF_LastOperationBackup, 100 },
                },

                // FoundationEnemyMiniWaveWeights（敵ミニWave）
                new()
                {
                    { SpawnTypeId.GOI_ChaosBackup, 40 },
                    { SpawnTypeId.GOI_GoCBackup , 60 }
                },

                // RoleTables（HDの中身をイベント用に上書きしたいならここで定義）
                new()
                {
                    {
                        SpawnTypeId.MTF_LastOperationNormal, new()
                        {
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.Sculpture),   (99f,  true)  },
                        }
                    },
                    {
                        SpawnTypeId.MTF_LastOperationBackup, new()
                        {
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.Sculpture), (99f,  true)  },
                        }
                    },
                    {
                        SpawnTypeId.GOI_GoCNormal, new()
                        {
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCSquadLeader), (1f, true) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCDeputy), (1f, true) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCMedic), (1f, false) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCThaumaturgist), (1f, false) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCCommunications), (1f, false) },
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCOperative), (99f, false) },
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
                            { new SpawnSystem.SpawnRoleKey(CRoleTypeId.GoCOperative), (99f, false) },
                        } 
                    }
                }
            );
        }
        
        private IEnumerator<float> HumanitistsCoroutine()
        {
            for (;;)
            {
                if (KillEvent())
                    yield break;

                var players = Player.List.ToList();

                if (players.IsOnlyTeam(CTeam.GoC, "humanity"))
                {
                    Round.IsLocked = false;
                    CTeam.GoC.EndRound("SavedHumanity");
                    yield break;
                }

                yield return MEC.Timing.WaitForSeconds(1f);
            }
        }

        public override void RegisterEvents() { }
        public override void UnregisterEvents() { }
    }
}
