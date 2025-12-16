using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Objectives;
using Exiled.Events.EventArgs.Map;
using LabApi.Events.CustomHandlers;
using LightContainmentZoneDecontamination;
using MapGeneration;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using PlayerRoles;
using ProjectMER.Events.Arguments;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class OperationBlackout
{
    public OperationBlackout()
    {

        Exiled.Events.Handlers.Map.Generated += OnMAPGenerated;
        Exiled.Events.Handlers.Map.GeneratorActivating += OnGenerated;
    }
    ~OperationBlackout()
    {

        Exiled.Events.Handlers.Map.Generated -= OnMAPGenerated;
        Exiled.Events.Handlers.Map.GeneratorActivating -= OnGenerated;
    }

    private int globalEventPID = 0;
    Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
    
    public void Event()
    {
        var EventHandler = Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler;
        var SpecialEventHandler = Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler;
        int eventPID = SpecialEventHandler.EventPID;
        globalEventPID = eventPID;
        
        if (eventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;

        SpawnSystem.Disable = true; // Disable All Respawning.

        foreach (Room room in Room.List)
        {
            room.TurnOffLights();
        }

        List<DoorType> lockedDoors = new List<DoorType>()
        {
            DoorType.ElevatorLczA,
            DoorType.ElevatorLczB,
            DoorType.CheckpointGateA,
            DoorType.CheckpointGateB,
            DoorType.ElevatorGateA,
            DoorType.ElevatorGateB
        };
        List<ElevatorType> lockedEvs = new List<ElevatorType>()
        {
            ElevatorType.GateA,
            ElevatorType.GateB,
            ElevatorType.LczA,
            ElevatorType.LczB
        };
        foreach (Door door in Door.List)
        {
            if (lockedDoors.Contains(door.Type))
            {
                door.IsOpen = false;
                door.Lock(DoorLockType.AdminCommand);
            }
        }
        foreach (Lift lift in Lift.List)
        {
            if (lockedEvs.Contains(lift.Type))
            {
                lift.TryStart(1,false);
            }
        }

        foreach (Player player in Player.List)
        {
            if (player.Role.Type == RoleTypeId.FacilityGuard)
            {
                player.Teleport(Room.Get(RoomType.LczArmory).WorldPosition(new Vector3(0f,1.5f,0f)));
            }
            else if (player.Role.Team == Team.SCPs)
            {
                player.Role.Set(RoleTypeId.ClassD);
            }
        }
        CassieExtensions.CassieTranslated("Attention, All personnel. Facility electric systems is malfunctioning . please manual charge up the all generators.","全職員に通達。施設の電力システムに<color=red>問題</color>が発生しました。全ての非常用発電機を<color=#00b7eb>再起動</color>してください。",true);
    }

    public void OnMAPGenerated()
    {
        if (Plugin.Singleton.SpecialEventsHandler.nowEvent == SpecialEventType.OperationBlackout)
        {
            int i = 0;
            int spawnedCount = 0;
            while (spawnedCount < 3) // 3個スポーンするまで繰り返す
            {
                GameObject generatorObj = PrefabHelper.Spawn(PrefabType.GeneratorStructure, Room.Random(ZoneType.LightContainment).WorldPosition(new Vector3(0f,1.25f,0f)));
                Log.Debug($"Spawned object: {generatorObj != null}"); // スポーン確認
                Log.Debug($"Pos: {generatorObj.transform.position}");
                Log.Debug($"Rot: {generatorObj.transform.eulerAngles}");
                generatorObj.transform.position.TryGetRoom(out var room);
                Log.Debug($"Room: {room.Name}");
                Log.Debug($"Zone: {room.Zone}");
    
                spawnedCount++;
            }
        }
    }
    
    int generatedCount = 0;
    public void OnGenerated(GeneratorActivatingEventArgs ev)
    {
        if (globalEventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
        generatedCount++;
        if (generatedCount == 3)
        {
            Timing.CallDelayed(15f, () =>
            {
                foreach (Door door in Door.List)
                {
                    if (door.Type == DoorType.ElevatorLczA || door.Type == DoorType.ElevatorLczB)
                    {
                        door.Unlock();
                    }
                }
                CassieExtensions.CassieTranslated("All Light Containment Zone emergency generators is now power upped . and Heavy Containment Zone Elevators now Online.",
                    "全ての軽度収容区画の非常用発電機が起動され、重度収容区画とのエレベーターが再起動しました。",true);
                Timing.CallDelayed(15f, () =>
                {
                    if (globalEventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                    CassieExtensions.CassieTranslated("Warning, The Facility O 2 Supply Systems power down effect Detected. Please evacuation to the Upper Facility Zone.",
                        "警告、施設の酸素供給システムにて<color=red>停電による影響</color>が検出されました。出来るだけ早く施設の上部区画へ避難してください。",true);
                });
            });
        }
        else if (generatedCount >= 6)
        {
            Timing.CallDelayed(10f, () =>
            {
                foreach (Door door in Door.List)
                {
                    if (door.Type == DoorType.CheckpointGateA || door.Type == DoorType.CheckpointGateB)
                    {
                        door.Unlock();
                    }
                }
                CassieExtensions.CassieTranslated("All Heavy Containment Zone emergency generators is now power upped . and Entrance Zone Door systems now Online. Power upping Gate Elevator by Emergency Electric Power . . .",
                    "全ての重度収容区画の非常用発電機が起動され、エントランスゾーンのドアシステムが全て復帰しました。非常電源を用いてゲートのエレベーターを再起動しています・・・",true);
                Timing.CallDelayed(60f, () =>
                {
                    if (globalEventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                    CassieExtensions.CassieTranslated("Emergency Attention to the All personnel, Emergency Electric Power is Locked by Unknown Forces. and Facility O 2 is very very bad. Please evacuation to the Shelter or . .g1",
                        "全職員に緊急通達。非常用電源が何者かの影響によりロックされました。更に、現在の施設内酸素は<color=red>非常に悪く、危険</color>です。シェルター等に避難し、少しでも...(電力が切れる音)",true);
                    Timing.CallDelayed(15f, () =>
                    {
                        if (globalEventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                        CreateAndPlayAudio("oxygen.ogg","Cassie",Vector3.zero,true,null,false,999999999,0);
                        Timing.CallDelayed(232f, () =>
                        {
                            if (globalEventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                            foreach (Player player in Player.List)
                            {
                                player.EnableEffect(EffectType.Asphyxiated, 255);
                                player.EnableEffect(EffectType.Blurred, 1);
                                player.EnableEffect(EffectType.Slowness, 10);
                            }

                            Timing.RunCoroutine(Coroutine());
                        });
                    });
                });
            });
        }
    }

    private IEnumerator<float> Coroutine()
    {
        for (;;)
        {
            if (Round.IsLobby || globalEventPID != Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.EventPID) yield break;
            foreach (Player player in Player.List)
            {
                player.EnableEffect(EffectType.Asphyxiated, 255);
                player.EnableEffect(EffectType.Blurred, 1);
                player.EnableEffect(EffectType.Slowness, 10);
            }
            yield return Timing.WaitForSeconds(10f);
        }
    }
}