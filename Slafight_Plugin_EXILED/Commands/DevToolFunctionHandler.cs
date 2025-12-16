using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.SpecialEvents.Events;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands;

public class DevToolFunctionHandler
{
    Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
    public void PlaySurfaceAttack()
    {
        Exiled.API.Features.Cassie.MessageTranslated("pitch_.8 All Insurgency Agents Tasks completed . Last Order . . pitch_.75 Destroy the Facility . pitch_.4 .g1 pitch_.26 .g5 .g6 .g4 pitch_2 .g1 pitch_.75 Good by all anomalys and foundation personnels .","全インサージェンシーエージェントの任務完了を確認。最後の指令を下す：<b><color=red>施設を破壊せよ</color></b>");
        Timing.CallDelayed(15f, () =>
        {
                            Exiled.API.Features.Cassie.MessageTranslated("pitch_.2 .g4 .g4 pitch_1 pitch_.75 BY ORDER OF DELTA COMMAND . THE DEAD MANS SEQUENCE AND SURFACE ATTACK PROTOCOL ACTIVATED . DETONATION IN TMINUS 145 SECONDS . PLEASE D .g4 IE .g6 .g3 .g4","BY ORDER OF <color=#228b22><b>DELTA COMMAND</b></color>. THE DEAD MANS SEQUENCE AND SURFACE ATTACK PROTOCOL ACTIVATED. DETONATION IN T-145 SECONDS. <color=red><b>PLEASE DIE</b></color>",false,false);
                            Timing.CallDelayed(10f, () =>
                            {
                                CreateAndPlayAudio("cir.ogg","Exiled.API.Features.Cassie",Vector3.zero,true,null,false,999999999,0);
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

    public void PlayOmegaWarhead()
    {
        foreach (Room rooms in Room.List)
        {
            rooms.Color = Color.blue;
        }

        foreach (Door door in Door.List)
        {
            if (door.Type != DoorType.ElevatorGateA && door.Type != DoorType.ElevatorGateB && door.Type != DoorType.ElevatorLczA && door.Type != DoorType.ElevatorLczB && door.Type != DoorType.ElevatorNuke && door.Type != DoorType.ElevatorScp049 && door.Type != DoorType.ElevatorServerRoom)
            {
                door.IsOpen = true;
                door.Lock(DoorLockType.Warhead);
            }
        }
        Exiled.API.Features.Cassie.MessageTranslated($"By Order of O5 Command . Omega Warhead Sequence Activated . All Facility Detonated in T MINUS {Slafight_Plugin_EXILED.Plugin.Singleton.Config.OwBoomTime} Seconds.",$"O5評議会の決定により、<color=blue>OMEGA WARHEAD</color>シーケンスが開始されました。施設の全てを{Slafight_Plugin_EXILED.Plugin.Singleton.Config.OwBoomTime}秒後に爆破します。",true);
        CreateAndPlayAudio("omega_v2.ogg","Exiled.API.Features.Cassie",Vector3.zero,true,null,false,999999999,0);
        Timing.CallDelayed(Slafight_Plugin_EXILED.Plugin.Singleton.Config.OwBoomTime, () =>
        {
            foreach (Player player in Player.List)
            {
                if (player == null) continue;
                player.ExplodeEffect(ProjectileType.FragGrenade);
                player.Kill("OMEGA WARHEADに爆破された");
            }
        });
    }
}