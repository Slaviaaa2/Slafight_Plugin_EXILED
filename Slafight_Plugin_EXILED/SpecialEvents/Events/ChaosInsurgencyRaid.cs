using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.SpecialEvents;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events
{
    public class ChaosInsurgencyRaidEvent : SpecialEvent
    {
        // ===== メタ情報 =====
        public override SpecialEventType EventType => SpecialEventType.NuclearAttack;
        public override int MinPlayersRequired => 5;
        public override string LocalizedName => "Chaos Insurgency Raid";
        public override string TriggerRequirement => "5人以上のプレイヤー";

        // ===== 内部状態 =====
        private bool _teslaDisabled = false;
        private int _eventPidGlobal = 0;

        private EventHandler EventHandler => Plugin.Singleton.EventHandler;

        private Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
            EventHandler.CreateAndPlayAudio;

        // ===== 実行エントリポイント =====
        protected override void OnExecute(int eventPID)
        {
            _eventPidGlobal = eventPID;
            _teslaDisabled = false;

            if (CancelIfOutdated())
                return;

            RunRaid();
        }

        public override void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.TriggeringTesla += DisableTesla;
        }

        public override void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.TriggeringTesla -= DisableTesla;
        }

        private bool CancelIfOutdated()
            => _eventPidGlobal != Plugin.Singleton.SpecialEventsHandler.EventPID;

        // ===== メイン処理 =====
        private void RunRaid()
        {
            var specialEventHandler = Plugin.Singleton.SpecialEventsHandler;
            var evHandler = EventHandler;

            // Warhead ロックなど
            evHandler.SpecialWarhead = true;
            evHandler.WarheadLocked = true;
            evHandler.DeadmanDisable = true;

            if (CancelIfOutdated())
                return;

            // カオスに変える対象を抽選（元コード準拠で SCP 側など）
            var ciTargets = StaticUtils.SelectRandomPlayersByRatio(CTeam.SCPs, 1f / 3f, true);
            foreach (var player in ciTargets)
            {
                player.SetRole(CRoleTypeId.ChaosCommando);
            }

            // Cassie シーケンス開始（元の CI Raid 的ノリでアナウンス）
            Timing.CallDelayed(8f, () =>
            {
                if (CancelIfOutdated()) return;

                Exiled.API.Features.Cassie.MessageTranslated("$pitch_1.02 Danger Detected Unknown Forces in Gate A . Please Check $pitch_.2 .g4 .g1 .g2",
                    $"警告、不明な部隊がGate Aで検出されました。確認を",true);

                Timing.CallDelayed(12f, () =>
                {
                    if (CancelIfOutdated()) return;

                    Exiled.API.Features.Cassie.MessageTranslated("$pitch_.8 Successfully terminated Foundations Cassie System and putted New Insurgencys Cassie System . Cassie is now under delta command",
                        $"<color=#00b7eb>財団のCassieシステム</color>の<color=red>終了</color>に成功。新たな<color=#228b22>インサージェンシーのCassieシステム</color>の導入も成功。<split> Cassieは今や<b><color=#228b22>DELTA COMMAND</color></b>の手中にある。",false);

                    Timing.CallDelayed(45f, () =>
                    {
                        if (CancelIfOutdated()) return;

                        Exiled.API.Features.Cassie.MessageTranslated(
                            "$pitch_.8 First Order of Delta Command . Turn off all facilitys . Accepted .",
                            "<b><color=#228b22>DELTA COMMAND</color></b>の最初の指令：全施設の消灯 ...承認",
                            false);

                        // ここで全体暗転
                        foreach (Room room in Room.List)
                            room.Color = new Color(55/255f, 55/255f, 55/255f);

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
                                    "$pitch_.8 All Insurgency Agents . Work Time .",
                                    "インサージェンシーのエージェント達よ、働く時間だ。",
                                    false);
                            });
                        });

                        // しばらく攻防後、CI が残っているかチェック
                        float raidDuration = 400f;
                        Timing.CallDelayed(raidDuration, () =>
                        {
                            if (CancelIfOutdated()) return;

                            int ciCount = 0;
                            foreach (Player player in Player.List)
                            {
                                if (player == null) continue;
                                if (player.UniqueRole == "CI_Agent" || player.UniqueRole == "ChaosInsurgency")
                                    ciCount++;
                            }

                            if (ciCount != 0)
                            {
                                HandleCiSuccess();
                            }
                            else
                            {
                                HandleCiFailure();
                            }
                        });
                    });
                });
            });
        }

        // ===== 成功時（施設破壊 / 攻撃プロトコル） =====
        private void HandleCiSuccess()
        {
            if (CancelIfOutdated()) return;

            Exiled.API.Features.Cassie.MessageTranslated(
                "$pitch_.8 All Insurgency Agents Tasks completed . Last Order . . $pitch_.75 Destroy the Facility . $pitch_.4 .g1 $pitch_.26 .g5 .g6 .g4 $pitch_2 .g1 $pitch_.75 Good by all anomalys and foundation personnels .",
                "全インサージェンシーエージェントの任務完了を確認。最後の指令を下す：<b><color=red>施設を破壊せよ</color></b>",
                true);

            Timing.CallDelayed(15f, () =>
            {
                if (CancelIfOutdated()) return;

                Exiled.API.Features.Cassie.MessageTranslated(
                    "$pitch_.2 .g4 .g4 $pitch_1 $pitch_.75 BY ORDER OF DELTA COMMAND . THE DEAD MANS SEQUENCE AND SURFACE ATTACK PROTOCOL ACTIVATED . DETONATION IN TMINUS 145 SECONDS . PLEASE D .g4 IE .g6 .g3 .g4",
                    "BY ORDER OF <color=#228b22><b>DELTA COMMAND</b></color>. THE DEAD MANS SEQUENCE AND SURFACE ATTACK PROTOCOL ACTIVATED. DETONATION IN T-145 SECONDS. <color=red><b>PLEASE DIE</b></color>",
                    true);

                Timing.CallDelayed(10f, () =>
                {
                    if (CancelIfOutdated()) return;

                    CreateAndPlayAudio("cir.ogg", "Cassie", Vector3.zero, true, null, false, 999999999f, 0f);

                    SchematicObject schematicObject;
                    try
                    {
                        schematicObject = ObjectSpawner.SpawnSchematic("CI_Nuke", Vector3.zero);
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
                        room.Color = new Color32(255, 128, 0, 255);
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
                                player.Kill("SURFACE ATTACK PROTOCOL に爆破された");
                            else
                                player.Kill("ALPHA WARHEADに爆破された");
                        }
                    });
                });
            });
        }

        // ===== 失敗時（財団勝利） =====
        private void HandleCiFailure()
        {
            if (CancelIfOutdated()) return;

            Exiled.API.Features.Cassie.MessageTranslated(
                "$pitch_.2 .g3 $pitch_.7 .g2 $pitch_.4 .g4 .g5 .g5 $pitch_1 .g1 .g2 .g3 Attention . All personnel . the Foundation Forces Successfully Terminated All Chaos Insurgency Forces . All System now backed to the Foundation . All Delta Command Orders Now Terminated . Please back to normal Containment Breach Security Mode",
                "全職員に報告します。財団の部隊は全カオス・インサージェンシー勢力の排除に成功しました。全てのDELTA COMMANDの指令は正常に終了。全職員は収容違反の対応モデルに復帰してください。",
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
    }
}
