using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Player;
using LightContainmentZoneDecontamination;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class ChaosInsurgencyRaid
{
    private bool teslaDisabled = false;
    private int eventPIDglobal = 0;
    public ChaosInsurgencyRaid()
    {
        teslaDisabled = false;
        
        Exiled.Events.Handlers.Player.TriggeringTesla += DisableTesla;
    }

    ~ChaosInsurgencyRaid()
    {
        teslaDisabled = false;
        
        Exiled.Events.Handlers.Player.TriggeringTesla -= DisableTesla;
    }
    public void CIREvent()
    {
        var EventHandler = Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler;
        var SpecialEventHandler = Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler;
        Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
        int eventPID = SpecialEventHandler.EventPID;
        eventPIDglobal = eventPID;

        if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
        
        EventHandler.SpecialWarhead = true;
        EventHandler.WarheadLocked = true;
        EventHandler.DeadmanDisable = true;
        //EventHandler.DeconCancellFlag = true;
        if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
        DecontaminationController.Singleton.DecontaminationOverride = DecontaminationController.DecontaminationStatus.Disabled;
        DecontaminationController.Singleton.TimeOffset = int.MinValue;
        DecontaminationController.DeconBroadcastDeconMessage = "除染は取り消されました";
        
        if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
        int i=0;
        foreach (Player player in Player.List)
        {
            if (player.Role.Team != Team.SCPs)
            {
                Slafight_Plugin_EXILED.Plugin.Singleton.CustomRolesHandler.SpawnChaosCommando(player);
                player.Broadcast(10,"※本イベント中、懐中電灯やライトアタッチメントの使用を推奨します。");
                i++;
            }
            if (i >= Math.Truncate(Player.List.Count/3f)) break;
        }
        //Exiled.API.Features.Cassie.MessageTranslated("$pitch_1.02 Danger Detected Unknown Forces in Gate A . Please Check $pitch_.2 .g4 .g1 .g2","警告、不明な部隊がGate Aで検出されました。確認を",true);
        CassieExtensions.CassieTranslated("$pitch_1.02 Danger Detected Unknown Forces in Gate A . Please Check $pitch_.2 .g4 .g1 .g2",
            $"警告、不明な部隊がGate Aで検出されました。確認を",true);
        Timing.CallDelayed(8f, () =>
        {
            if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
            //Exiled.API.Features.Cassie.MessageTranslated("$pitch_.8 Successfully terminated Foundations Cassie System and putted New Insurgencys Cassie System . Cassie is now under delta command","<color=#00b7eb>財団のCassieシステム</color>の<color=red>終了</color>に成功。新たな<color=#228b22>インサージェンシーのCassieシステム</color>の導入も成功。<split> Cassieは今や<b><color=#228b22>DELTA COMMAND</color></b>の手中にある。",false,false);
            CassieExtensions.CassieTranslated("$pitch_.8 Successfully terminated Foundations Cassie System and putted New Insurgencys Cassie System . Cassie is now under delta command",
                $"<color=#00b7eb>財団のCassieシステム</color>の<color=red>終了</color>に成功。新たな<color=#228b22>インサージェンシーのCassieシステム</color>の導入も成功。<split> Cassieは今や<b><color=#228b22>DELTA COMMAND</color></b>の手中にある。",false);
            Timing.CallDelayed(45f, () =>
            {
                if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                //Exiled.API.Features.Cassie.MessageTranslated("$pitch_.8 First Order of Delta Command . Turn off all facilitys . Accepted .","<b><color=#228b22>DELTA COMMAND</color></b>の最初の指令：全施設の消灯 ...承認",false,false);
                CassieExtensions.CassieTranslated("$pitch_.8 First Order of Delta Command . Turn off all facilitys . Accepted .",
                    "<b><color=#228b22>DELTA COMMAND</color></b>の最初の指令：全施設の消灯 ...承認",false);
                foreach (Room room in Room.List)
                {
                    room.TurnOffLights();
                }
                //Exiled.API.Features.Cassie.MessageTranslated("$pitch_.8 Next Order . Turn off Tesla Gates . Accepted .","次の指令：テスラゲートの無効化 ...承認",false,false);
                CassieExtensions.CassieTranslated("$pitch_.8 Next Order . Turn off Tesla Gates . Accepted .",
                    "次の指令：テスラゲートの無効化 ...承認",false);
                teslaDisabled = true;
                //Exiled.API.Features.Cassie.MessageTranslated("$pitch_.8 All Agent . Work Time .","エージェント達よ、働く時間だ。",false,false);
                CassieExtensions.CassieTranslated("$pitch_.8 All Agent . Work Time .",
                    "エージェント達よ、働く時間だ。",false);
                float testingDelayedInt = 400f;
                Timing.CallDelayed(testingDelayedInt, () =>
                {
                    if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                    int cicount = 0;
                    foreach (Player player in Player.List)
                    {
                        if (player == null) continue;
                        if (player.Role.Team == Team.ChaosInsurgency)
                        {
                            cicount++;
                        }
                    }
                    if (cicount != 0)
                    {
                        /*Exiled.API.Features.Cassie.MessageTranslated("$pitch_.8 All Insurgency Agents Tasks completed . Last Order . . $pitch_.75 Destroy the Facility . $pitch_.4 .g1 $pitch_.26 .g5 .g6 .g4 $pitch_2 .g1 $pitch_.75 Good by all anomalys and foundation personnels .",
                            "全インサージェンシーエージェントの任務完了を確認。最後の指令を下す：<b><color=red>施設を破壊せよ</color></b>");
                        */
                        CassieExtensions.CassieTranslated("$pitch_.8 All Insurgency Agents Tasks completed . Last Order . . $pitch_.75 Destroy the Facility . $pitch_.4 .g1 $pitch_.26 .g5 .g6 .g4 $pitch_2 .g1 $pitch_.75 Good by all anomalys and foundation personnels .",
                            "全インサージェンシーエージェントの任務完了を確認。最後の指令を下す：<b><color=red>施設を破壊せよ</color></b>",true);
                        Timing.CallDelayed(15f, () =>
                        {
                            if (eventPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                            //Exiled.API.Features.Cassie.MessageTranslated("$pitch_.2 .g4 .g4 $pitch_1 $pitch_.75 BY ORDER OF DELTA COMMAND . THE DEAD MANS SEQUENCE AND SURFACE ATTACK PROTOCOL ACTIVATED . DETONATION IN TMINUS 145 SECONDS . PLEASE D .g4 IE .g6 .g3 .g4","BY ORDER OF <color=#228b22><b>DELTA COMMAND</b></color>. THE DEAD MANS SEQUENCE AND SURFACE ATTACK PROTOCOL ACTIVATED. DETONATION IN T-145 SECONDS. <color=red><b>PLEASE DIE</b></color>",false,false);
                            CassieExtensions.CassieTranslated("$pitch_.2 .g4 .g4 $pitch_1 $pitch_.75 BY ORDER OF DELTA COMMAND . THE DEAD MANS SEQUENCE AND SURFACE ATTACK PROTOCOL ACTIVATED . DETONATION IN TMINUS 145 SECONDS . PLEASE D .g4 IE .g6 .g3 .g4",
                                "BY ORDER OF <color=#228b22><b>DELTA COMMAND</b></color>. THE DEAD MANS SEQUENCE AND SURFACE ATTACK PROTOCOL ACTIVATED. DETONATION IN T-145 SECONDS. <color=red><b>PLEASE DIE</b></color>",true);
                            Timing.CallDelayed(10f, () =>
                            {
                                if (eventPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                                CreateAndPlayAudio("cir.ogg","Cassie",Vector3.zero,true,null,false,999999999,0);
                                SchematicObject schematicObject;
                                try
                                {
                                    schematicObject = ObjectSpawner.SpawnSchematic("Nuke",Vector3.zero);
                                }
                                catch (Exception ex)
                                {
                                    schematicObject = null;
                                    return;
                                }
                                Timing.CallDelayed(0.5f, () =>
                                {
                                    schematicObject.Position = new Vector3(-90f,500f,-45f);
                                    schematicObject.Rotation = Quaternion.Euler(new Vector3(0,0,55));
                                    Timing.RunCoroutine(NukeDownCoroutine(schematicObject));
                                });
                                foreach (Room room in Room.List)
                                {
                                    room.AreLightsOff = false;
                                    room.Color = new Color32(255,0,0,255);
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
                                    if (eventPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                                    foreach (Player player in Player.List)
                                    {
                                        if (player == null) continue;
                                        player.ExplodeEffect(ProjectileType.FragGrenade);
                                        if (player.Zone == ZoneType.Surface)
                                        {
                                            player.Kill("SURFACE ATTACKに爆破された");
                                        }
                                        else
                                        {
                                            player.Kill("ALPHA WARHEADに爆破された");
                                        }
                                    }
                                });
                            });
                        });
                    }
                    else
                    {
                        //Exiled.API.Features.Cassie.MessageTranslated("$pitch_.2 .g3 $pitch_.7 .g2 $pitch_.4 .g4 .g5 .g5 $pitch_1 .g1 .g2 .g3 Attention . All personnel . the Foundation Forces Successfully Terminated All Chaos Insurgency Forces . All System now backed to the Foundation . All Delta Command Orders Now Terminated . Please back to normal Containment Breach Security Mode","全職員に報告します。財団の部隊は全カオス・インサージェンシー勢力の排除に成功しました。全てのDELTA COMMANDの指令は正常に終了。全職員は収容違反の対応モデルに復帰してください。");
                        CassieExtensions.CassieTranslated("$pitch_.2 .g3 $pitch_.7 .g2 $pitch_.4 .g4 .g5 .g5 $pitch_1 .g1 .g2 .g3 Attention . All personnel . the Foundation Forces Successfully Terminated All Chaos Insurgency Forces . All System now backed to the Foundation . All Delta Command Orders Now Terminated . Please back to normal Containment Breach Security Mode",
                            "全職員に報告します。財団の部隊は全カオス・インサージェンシー勢力の排除に成功しました。全てのDELTA COMMANDの指令は正常に終了。全職員は収容違反の対応モデルに復帰してください。",
                            true);
                    }
                });
            });
        });

    }

    public void DisableTesla(TriggeringTeslaEventArgs ev)
    {
        if (eventPIDglobal != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
        if (teslaDisabled)
        {
            ev.DisableTesla = true;
        }
        else
        {
            ev.DisableTesla = false;
        }
    }

    private IEnumerator<float> NukeDownCoroutine(SchematicObject schem)
    {
        float elapsedTime = 0f;
        float totalDuration = 150f;
        Vector3 startPos = new Vector3(-90, 500f, schem.transform.position.z);
        Vector3 endPos = new Vector3(70f, 300f, schem.transform.position.z);
        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / totalDuration;
            schem.transform.position = Vector3.Lerp(startPos, endPos, progress);
            yield return 0f;
        }
    }
}