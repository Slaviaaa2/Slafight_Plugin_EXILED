using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Player;
using LightContainmentZoneDecontamination;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MapExtensions;
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events
{
    public class SnowWarriersAttackEvent : SpecialEvent
    {
        // ===== メタ情報 =====
        public override SpecialEventType EventType => SpecialEventType.SnowWarriersAttack;
        public override int MinPlayersRequired => 5;
        public override string LocalizedName => "Snow Warriers Raid";
        public override string TriggerRequirement => "5人以上のプレイヤー";

        // ===== 内部状態 =====
        private bool _teslaDisabled = false;
        private int _eventPidGlobal = 0;

        private Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
            EventHandler.CreateAndPlayAudio;

        public override bool IsReadyToExecute()
        {
            return MapFlags.GetSeason() == SeasonTypeId.Christmas;
        }

        // ===== イベント入り口 =====
        protected override void OnExecute(int eventPID)
        {
            _eventPidGlobal = eventPID;
            _teslaDisabled = false;

            if (CancelIfOutdated())
                return;

            DoSnowWarriorsSetup();
        }

        // ===== イベントサブスク / 解除 =====
        public override void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.TriggeringTesla += DisableTesla;
        }

        public override void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.TriggeringTesla -= DisableTesla;
        }

        // ===== 共通キャンセル判定 =====
        private bool CancelIfOutdated()
        {
            return _eventPidGlobal != Plugin.Singleton.SpecialEventsHandler.EventPID;
        }

        // ===== メイン処理 =====
        private void DoSnowWarriorsSetup()
        {
            var eventHandler = Plugin.Singleton.EventHandler;

            eventHandler.SpecialWarhead = true;
            Warhead.IsLocked = true;
            eventHandler.DeadmanDisable = true;

            if (CancelIfOutdated())
                return;

            DecontaminationController.Singleton.DecontaminationOverride =
                DecontaminationController.DecontaminationStatus.Disabled;
            DecontaminationController.Singleton.TimeOffset = int.MinValue;
            DecontaminationController.DeconBroadcastDeconMessage = "除染は取り消されました";

            if (CancelIfOutdated())
                return;

            var snowTargets = StaticUtils.SelectRandomPlayersByRatio(CTeam.SCPs, 1f / 3f);
            foreach (var player in snowTargets)
            {
                player.SetRole(CRoleTypeId.SnowWarrier);
            }

            // 以降の Cassie / シーケンスは元コードを移植
            Timing.CallDelayed(8f, () =>
            {
                if (CancelIfOutdated()) return;

                Exiled.API.Features.Cassie.MessageTranslated(
                    "$pitch_1.02 Danger Detected Unknown Organism in Gate A . Please Check $pitch_.2 .g4 .g1 .g2",
                    "警告、不明な生命体がGate Aで検出されました。確認を",
                    true);

                Timing.CallDelayed(12f, () =>
                {
                    if (CancelIfOutdated()) return;

                    Exiled.API.Features.Cassie.MessageTranslated(
                        "$pitch_.8 Successfully terminated Foundations Cassie System and putted New Division Cassie System . Cassie is now under us",
                        "<color=#00b7eb>財団のCassieシステム</color>の<color=red>終了</color>に成功。新たな<color=#ffffff>雪の戦士たちのCassieシステム</color>の導入も成功。<split> Cassieは今や<b><color=#ffffff>雪の帝王</color></b>の手中にある。",
                        false);

                    Timing.CallDelayed(45f, () =>
                    {
                        if (CancelIfOutdated()) return;

                        Exiled.API.Features.Cassie.MessageTranslated(
                            "$pitch_.8 First Order . Light up all facility . Accepted .",
                            "<b><color=#ffffff>雪の帝王</color></b>の最初の指令：全施設のライトアップ ...承認",
                            false);

                        Timing.RunCoroutine(LightUpCoroutine());

                        Timing.CallDelayed(8f, () =>
                        {
                            if (CancelIfOutdated()) return;

                            Exiled.API.Features.Cassie.MessageTranslated(
                                "$pitch_.8 Next Order . Turn off Tesla Gates . Accepted .",
                                "次の指令：テスラゲートの無効化 ...承認",
                                false);

                            _teslaDisabled = true;

                            Timing.CallDelayed(8f, () =>
                            {
                                if (CancelIfOutdated()) return;

                                Exiled.API.Features.Cassie.MessageTranslated(
                                    "$pitch_.8 All Division . Work Time .",
                                    "戦士達よ、働く時間だ。",
                                    false);
                            });
                        });

                        float testingDelayedInt = 400f;
                        Timing.CallDelayed(testingDelayedInt, () =>
                        {
                            if (CancelIfOutdated()) return;

                            int snowCount = 0;
                            foreach (Player player in Player.List)
                            {
                                if (player == null) continue;
                                if (player.UniqueRole == "SnowWarrier")
                                    snowCount++;
                            }

                            if (snowCount != 0)
                            {
                                HandleSnowWarriorSuccess();
                            }
                            else
                            {
                                HandleSnowWarriorFailure();
                            }
                        });
                    });
                });
            });
        }

        private void HandleSnowWarriorSuccess()
        {
            if (CancelIfOutdated()) return;

            Exiled.API.Features.Cassie.MessageTranslated(
                "$pitch_.8 All Division Agents Tasks completed . Last Order . . $pitch_.75 Destroy the Facility . $pitch_.4 .g1 $pitch_.26 .g5 .g6 .g4 $pitch_2 .g1 $pitch_.75 Good by all anomalys and foundation personnels .",
                "全戦士達の任務完了を確認。最後の指令を下す：<b><color=white>施設を埋もれさせよ</color></b>",
                true);

            Timing.CallDelayed(15f, () =>
            {
                if (CancelIfOutdated()) return;

                Exiled.API.Features.Cassie.MessageTranslated(
                    "$pitch_.2 .g4 .g4 $pitch_1 $pitch_.75 BY ORDER OF DIVISION COMMAND . THE DEAD MANS SEQUENCE AND ATTACK PROTOCOL ACTIVATED . DETONATION IN TMINUS 145 SECONDS . PLEASE D .g4 IE .g6 .g3 .g4",
                    "BY ORDER OF <color=#ffffff><b>DIVISION COMMAND</b></color>. THE DEAD MANS SEQUENCE AND MEGABALL ATTACK PROTOCOL ACTIVATED. DETONATION IN T-145 SECONDS. <color=red><b>PLEASE DIE</b></color>",
                    true);

                Timing.CallDelayed(10f, () =>
                {
                    if (CancelIfOutdated()) return;

                    CreateAndPlayAudio("cir.ogg", "Cassie", Vector3.zero, true, null, false, 999999999f, 0f);

                    SchematicObject schematicObject;
                    try
                    {
                        schematicObject = ObjectSpawner.SpawnSchematic("Xmas_Nuke", Vector3.zero);
                    }
                    catch (Exception)
                    {
                        schematicObject = null;
                        return;
                    }

                    Timing.CallDelayed(0.5f, () =>
                    {
                        if (schematicObject == null) return;

                        schematicObject.Position = new Vector3(-90f, 500f, -45f);
                        schematicObject.Rotation = Quaternion.Euler(new Vector3(0, 0, 55));
                        Timing.RunCoroutine(NukeDownCoroutine(schematicObject));
                    });

                    foreach (Room room in Room.List)
                    {
                        room.AreLightsOff = false;
                        room.Color = new Color32(255, 255, 0, 255);
                    }

                    foreach (Door door in Door.List)
                    {
                        if (door.Type != DoorType.ElevatorGateA &&
                            door.Type != DoorType.ElevatorGateB &&
                            door.Type != DoorType.ElevatorLczA &&
                            door.Type != DoorType.ElevatorLczB &&
                            door.Type != DoorType.ElevatorNuke &&
                            door.Type != DoorType.ElevatorScp049 &&
                            door.Type != DoorType.ElevatorServerRoom)
                        {
                            door.IsOpen = true;
                            door.Lock(DoorLockType.Warhead);
                        }
                    }

                    Timing.CallDelayed(145f, () =>
                    {
                        if (CancelIfOutdated()) return;

                        foreach (Player player in Player.List)
                        {
                            if (player == null) continue;

                            player.ExplodeEffect(ProjectileType.FragGrenade);

                            if (player.Zone == ZoneType.Surface)
                                player.Kill("MEGABALL ATTACKに爆破された");
                            else
                                player.Kill("ALPHA WARHEADに爆破された");
                        }
                    });
                });
            });
        }

        private void HandleSnowWarriorFailure()
        {
            if (CancelIfOutdated()) return;

            Exiled.API.Features.Cassie.MessageTranslated(
                "$pitch_.2 .g3 $pitch_.7 .g2 $pitch_.4 .g4 .g5 .g5 $pitch_1 .g1 .g2 .g3 Attention . All personnel . the Foundation Forces Successfully Terminated All Forces . All System now backed to the Foundation . All Division Command Orders Now Terminated . Please back to normal Containment Breach Security Mode",
                "全職員に報告します。財団の部隊は全雪の戦士達勢力の排除に成功しました。全てのDIVISION COMMANDの指令は正常に終了。全職員は収容違反の対応モデルに復帰してください。",
                true);
        }

        // ===== Tesla 無効化 =====
        private void DisableTesla(TriggeringTeslaEventArgs ev)
        {
            if (CancelIfOutdated())
                return;

            ev.DisableTesla = _teslaDisabled;
        }

        // ===== コルーチン =====
        private IEnumerator<float> NukeDownCoroutine(SchematicObject schem)
        {
            float elapsedTime = 0f;
            float totalDuration = 150f;
            Vector3 startPos = new Vector3(-90f, 500f, schem.transform.position.z);
            Vector3 endPos = new Vector3(70f, 300f, schem.transform.position.z);

            while (elapsedTime < totalDuration)
            {
                if (CancelIfOutdated())
                    yield break;

                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / totalDuration;
                schem.transform.position = Vector3.Lerp(startPos, endPos, progress);
                yield return 0f;
            }
        }

        private IEnumerator<float> LightUpCoroutine()
        {
            for (;;)
            {
                if (CancelIfOutdated())
                    yield break;

                foreach (Room room in Room.List)
                {
                    room.AreLightsOff = false;
                    room.Color = new Color32(
                        (byte)Random.Range(0, 256),
                        (byte)Random.Range(0, 256),
                        (byte)Random.Range(0, 256),
                        255);
                }

                yield return Timing.WaitForSeconds(30f);
            }
        }
    }
}
