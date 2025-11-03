using System;
using System.Collections.Generic;
using System.IO;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.Decontamination;
using CustomPlayerEffects;
using Discord;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Core.Generic;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Hazards;
using Exiled.API.Features.Items;
using Exiled.API.Features.Toys;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Cassie;
using Exiled.Events.EventArgs.Interfaces;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp079;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Scp106;
using Exiled.Events.EventArgs.Scp173;
using Exiled.Events.EventArgs.Scp244;
using Exiled.Events.EventArgs.Scp330;
using Exiled.Events.EventArgs.Scp0492;
using Exiled.Events.EventArgs.Scp559;
using Exiled.Events.EventArgs.Scp914;
using Exiled.Events.EventArgs.Scp939;
using Exiled.Events.EventArgs.Scp1344;
using Exiled.Events.EventArgs.Scp1507;
using Exiled.Events.EventArgs.Scp2536;
using Exiled.Events.EventArgs.Scp3114;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Warhead;
using GameCore;
using Hazards;
using Interactables.Interobjects;
using LightContainmentZoneDecontamination;
using MapGeneration;
using MEC;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp096;
using Subtitles;
using UnityEngine;
using Cassie = Exiled.API.Features.Cassie;
using CassieHandler = Exiled.Events.Handlers.Cassie;
using ElevatorDoor = Exiled.API.Features.Doors.ElevatorDoor;
using Log = Exiled.API.Features.Log;
using Player = Exiled.API.Features.Player;
using PlayerHandler = Exiled.Events.Handlers.Player;
using Room = Exiled.API.Features.Room;
using Scp049Handler = Exiled.Events.Handlers.Scp049;
using Scp096Handler = Exiled.Events.Handlers.Scp096;
using ServerHandler = Exiled.Events.Handlers.Server;
using Scp330Handler = Exiled.Events.Handlers.Scp330;
using Warhead = Exiled.API.Features.Warhead;
using WarheadHandler = Exiled.Events.Handlers.Warhead;
using MapHandler = Exiled.Events.Handlers.Map;

namespace Slafight_Plugin_EXILED
{
    public class EventHandler
    {
        // Event Handler
        public EventHandler()
        {
            PlayerHandler.Verified += OnVerified;
            ServerHandler.RestartingRound += OnRoundRestarted;
            ServerHandler.RoundStarted += OnRoundStarted;
            ServerHandler.RoundStarted += SpEventHandler;
            ServerHandler.ReloadedPlugins += OnPluginLoad;

            MapHandler.Decontaminating += DeconCancell;
            
            PlayerHandler.ChangingRole += OnChangingRole;
            PlayerHandler.ChangingRole += SkeletonSpawn;
            PlayerHandler.ChangingRole += CryFuckSpawn;
            PlayerHandler.ChangingRole += StatusManager;
            PlayerHandler.Hurting += OnTouchedEnemy;
            PlayerHandler.FlippingCoin += PositionGet;
            PlayerHandler.Dying += CustomRoleRemover;
            PlayerHandler.InteractingDoor += DoorGet;
            PlayerHandler.Shot += CreateRagdoll;

            WarheadHandler.Starting += AlphaWarheadLock;
            //WarheadHandler.Starting += OmegaWarheadEvent;
            WarheadHandler.DeadmanSwitchInitiating += DeadmanCancell;
            WarheadHandler.Stopping += LockedStopSystem;
            WarheadHandler.Starting += LockedStartSystem;
            
            Scp096Handler.CalmingDown += EndlessAnger;
            Scp096Handler.Enraging += CleanShyDummy;
        }
        ~EventHandler()
        {
            PlayerHandler.Verified -= OnVerified;
            ServerHandler.RoundStarted -= OnRoundRestarted;
            ServerHandler.RoundStarted -= OnRoundStarted;
            ServerHandler.RoundStarted -= SpEventHandler;
            ServerHandler.ReloadedPlugins -= OnPluginLoad;
            
            MapHandler.Decontaminating -= DeconCancell;
            
            PlayerHandler.ChangingRole -= OnChangingRole;
            PlayerHandler.ChangingRole -= SkeletonSpawn;
            PlayerHandler.ChangingRole -= StatusManager;
            PlayerHandler.ChangingRole -= CryFuckSpawn;
            PlayerHandler.Hurting -= OnTouchedEnemy;
            PlayerHandler.FlippingCoin -= PositionGet;
            PlayerHandler.Dying -= CustomRoleRemover;
            PlayerHandler.InteractingDoor -= DoorGet;
            PlayerHandler.Shot -= CreateRagdoll;
            
            WarheadHandler.Starting -= AlphaWarheadLock;
            //WarheadHandler.Starting -= OmegaWarheadEvent;
            WarheadHandler.DeadmanSwitchInitiating -= DeadmanCancell;
            WarheadHandler.Stopping -= LockedStopSystem;
            WarheadHandler.Starting -= LockedStartSystem;
            
            Scp096Handler.CalmingDown -= EndlessAnger;
            Scp096Handler.Enraging -= CleanShyDummy;
        }
        // Other File Access
        private readonly Config cfg = Plugin.Singleton.Config;
        // Setup Variables
        public bool DeadmanDisable = false;
        public bool SkeletonSpawned = false;
        public float SpawnRoll = 1;
        public float Funny = 0;
        // - Event Flags - //
        public int EventPID = 1;
        
        public bool SpecialWarhead = false;
        public static int WarheadID = 0;
        public bool WarheadLocked = false;

        public bool DeconCancellFlag = false;

        public bool CryFuckEnabled = false;

        public bool CryFuckSpawned = false;
        // - System Flags - //
        public bool PluginLoaded = false;

        public void OnPluginLoad()
        {
            Log.Info("OnPluginLoad is successfully called!");
            if (!PluginLoaded)
            {

                PluginLoaded = true;
            }
        }
        
        public void OnVerified(VerifiedEventArgs ev)
        {
            ev.Player.Broadcast(6,"[WARN]THIS IS EXILED TEST SERVER.\nWelcome to Slafight Plugin Server!",Broadcast.BroadcastFlags.Normal,true);
        }

        public void OnRoundRestarted()
        {
            // Setup Variables
            DeadmanDisable = false;
            SkeletonSpawned = false;
            
            // - Event Flags - //
            DeconCancellFlag = false;
            
            SpecialWarhead =  false;
            WarheadID = 0;
            WarheadLocked = false;
            
            CryFuckEnabled = false;
            CryFuckSpawned = false;
            
            EventPID = EventPID + 1;
        }

        public void OnRoundStarted()
        {
            Funny = 0;
            Funny = UnityEngine.Random.Range(0, 1f);
            
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            ev.Player.UniqueRole = String.Empty;
            SpawnRoll = 1;
            SpawnRoll = UnityEngine.Random.Range(0, 1f);
            /*if (ev.NewRole == RoleTypeId.Tutorial)
            {
                Funny = UnityEngine.Random.Range(0, 1f);
                // Setup Variables
                DeadmanDisable = false;
            
                // - Event Flags - //
                DeconCancellFlag = false;
            
                SpecialWarhead =  false;
                WarheadID = 0;
                WarheadLocked = false;
                
                CryFuckEnabled = false;
                CryFuckSpawned = false;
            
                EventPID = EventPID + 1;
                SpEventHandler();
            }*/
        }

        public void RerollSpecial()
        {
            Funny = UnityEngine.Random.Range(0, 1f);
            // Setup Variables
            DeadmanDisable = false;
            
            // - Event Flags - //
            DeconCancellFlag = false;
            
            SpecialWarhead =  false;
            WarheadID = 0;
            WarheadLocked = false;
                
            CryFuckEnabled = false;
            CryFuckSpawned = false;
            
            EventPID = EventPID + 1;
            SpEventHandler();
        }
        
        public IEnumerator<float> Coroutine(float delay)
        {
            yield return Timing.WaitForSeconds(delay);
        }
        
        public static void CreateAndPlayAudio(string fileName, string audioPlayerName, Vector3 position, bool destroyOnEnd = false, Transform parent = null, bool isSpatial = false, float maxDistance = 5, float minDistance = 5)
        {
            
            var audioPlayer = AudioPlayer.CreateOrGet(audioPlayerName);
            

            if (!audioPlayer.TryGetSpeaker(audioPlayerName, out Speaker speaker))
            {
                speaker = audioPlayer.AddSpeaker(audioPlayerName, isSpatial: isSpatial, maxDistance: maxDistance, minDistance: minDistance);
            }

            if (parent)
            {
                speaker.transform.SetParent(parent);
                speaker.transform.localPosition = Vector3.zero;
                speaker.transform.localRotation = Quaternion.identity;
            }
            else
            {
                speaker.Position = position;
            }
    
            AudioClipStorage.LoadClip(Path.Combine("C:\\Users\\zeros\\AppData\\Roaming\\EXILED\\ServerContents\\", fileName), fileName);

            audioPlayer.AddClip(fileName, destroyOnEnd: destroyOnEnd);
        }
        
        public void SkeletonSpawn(ChangingRoleEventArgs ev)
        {
            var GetPlayerTeam = RoleExtensions.GetTeam(ev.NewRole);
            if (cfg.SkeletonSpawnAllowed && !SkeletonSpawned && SpawnRoll <= cfg.SkeletonSpawnChance && GetPlayerTeam == Team.SCPs && ev.Reason == SpawnReason.RoundStart)
            {
                ev.IsAllowed = false;
                SkeletonSpawned = true;
                ev.Player.Role.Set(RoleTypeId.Scp3114);
                Log.Debug("Scp3114 was Spawned!");
                SkeletonRagdoll();
            }
        }

        public void SkeletonRagdoll()
        {
            Timing.CallDelayed(1, () =>
            {
                    Player ev = null;
                    foreach (Player player in Player.List)
                    {
                        if (player.Role == RoleTypeId.Scp3114)
                        {
                            ev = player;
                            break;
                        }
                    }
                    //ev.Player.Rotation = Quaternion.Euler(0,-62f,0f);
                    Vector3 s_chamber = Vector3.zero;
                    Vector3 s_root = Vector3.zero;
                    Vector3 s_scientist_spawn = Vector3.zero;
                    Vector3 s_classd_spawn = Vector3.zero;
                    Vector3 s_fg_spawn = Vector3.zero;
                    Vector3 s_ntf_spawn = Vector3.zero;
                    Vector3 s_chaos_spawn = Vector3.zero;

                    Vector3 s_root_scientist = Vector3.zero;
                    Vector3 s_root_classd = Vector3.zero;
                    Quaternion s_root_rotation = Quaternion.identity;
                    s_chamber = Room.Get(RoomType.Lcz173).Position + new Vector3(-3f, 12f, -2f);
                    Log.Debug("S_Chamber Pos: " + s_chamber.x + "," + s_chamber.y + "," + s_chamber.z);
                    s_root = ev.Position;
                    Log.Debug("x:" + ev.Position.x + "y:" + ev.Position.y + "z:" + ev.Position.z);
                    foreach (Ragdoll ragdoll in Ragdoll.List)
                    {
                        if (ragdoll.Room.Type == RoomType.Lcz173 && ragdoll.Role == RoleTypeId.Scientist)
                        {
                            s_root_scientist = ragdoll.Position;
                            s_root_rotation = ragdoll.Rotation;
                            Log.Debug("ragdoll scientist get");
                            Log.Debug("Scientist Pos:"+s_root_scientist.x+","+s_root_scientist.y+","+s_root_scientist.z+"\nRot:"+s_root_rotation.x+","+s_root_rotation.y+","+s_root_rotation.z+","+s_root_rotation.w);
                            ragdoll.Destroy();
                        }

                        if (ragdoll.Room.Type == RoomType.Lcz173 && ragdoll.Role == RoleTypeId.ClassD)
                        {
                            s_root_classd = ragdoll.Position;
                            Log.Debug("ragdoll class-d get");
                            ragdoll.Destroy();
                        }
                    }
                    Vector3 s_changed_value = s_root_scientist - s_root_classd;
                    s_root_scientist += new Vector3(0f,0.002f,0f);
                    Log.Debug("S_CHANGED_VALUE - x:" + s_changed_value.x + "y:" + s_changed_value.y + "z:" + s_changed_value.z);
                    s_scientist_spawn = s_root_scientist;
                    s_classd_spawn = s_root_scientist - s_changed_value;
                    s_fg_spawn = s_root_scientist - s_changed_value * 2;
                    s_ntf_spawn = s_root_scientist - s_changed_value * 3;
                    s_chaos_spawn = s_root_scientist - s_changed_value * 4;
                    Ragdoll skeletonRagdoll_Scientist = Ragdoll.CreateAndSpawn(RoleTypeId.Scientist, "Scientist Ragdoll", "For You :)", s_scientist_spawn, s_root_rotation);
                    //skeletonRagdoll_Scientist.Rotation = Quaternion.Euler(0f,45f,0f);
                    Log.Debug("scientist ragdoll:\n" + skeletonRagdoll_Scientist.Position.x + "," + skeletonRagdoll_Scientist.Position.y + "," + skeletonRagdoll_Scientist.Position.z);
                    
                    Ragdoll skeletonRagdoll_ClassD = Ragdoll.CreateAndSpawn(RoleTypeId.ClassD, "ClassD Ragdoll", "For You :)", s_classd_spawn, s_root_rotation);
                    //skeletonRagdoll_ClassD.Rotation = Quaternion.Euler(0f,45f,0f);
                    Log.Debug("classd ragdoll:\n" + skeletonRagdoll_ClassD.Position.x + "," + skeletonRagdoll_ClassD.Position.y + "," + skeletonRagdoll_ClassD.Position.z);
                    
                    Ragdoll skeletonRagdoll_FG = Ragdoll.CreateAndSpawn(RoleTypeId.FacilityGuard, "FacilityGuard Ragdoll", "For You :)", s_fg_spawn, s_root_rotation);
                    //skeletonRagdoll_FG.Rotation = Quaternion.Euler(0f,45f,0f);
                    Log.Debug("fg ragdoll:\n" + skeletonRagdoll_FG.Position.x + "," + skeletonRagdoll_FG.Position.y + "," + skeletonRagdoll_FG.Position.z);
                    
                    Ragdoll skeletonRagdoll_NTF = Ragdoll.CreateAndSpawn(RoleTypeId.NtfPrivate, "NTF-Private Ragdoll", "For You :)", s_ntf_spawn, s_root_rotation);
                    //skeletonRagdoll_NTF.Rotation = Quaternion.Euler(0f,45f,0f);
                    Log.Debug("ntf ragdoll:\n" + skeletonRagdoll_NTF.Position.x + "," + skeletonRagdoll_NTF.Position.y + "," + skeletonRagdoll_NTF.Position.z);
                    
                    Ragdoll skeletonRagdoll_Chaos = Ragdoll.CreateAndSpawn(RoleTypeId.ChaosRifleman, "NTF-Rifleman Ragdoll", "For You :)", s_chaos_spawn, s_root_rotation);
                    //skeletonRagdoll_NTF.Rotation = Quaternion.Euler(0f,45f,0f);
                    Log.Debug("chaos ragdoll:\n" + skeletonRagdoll_Chaos.Position.x + "," + skeletonRagdoll_Chaos.Position.y + "," + skeletonRagdoll_Chaos.Position.z);
                    
                    // in 173 Chamber Funnies
                    Vector3 s_root_door = Vector3.zero;
                    Vector3 s_root_skeleton = Vector3.zero;
                    s_root_scientist += new Vector3(0f, 0.004f, 0f);
                    s_root_door = Door.Get(DoorType.Scp173Gate).Position;
                    foreach (Player player in Player.List)
                    {
                        if (player.Role == RoleTypeId.Scp3114)
                        {
                            s_root_skeleton = player.Position;
                            break;
                        }
                    }
                    Vector3 funnyPosModifier = s_root_door * 1.0092f - s_root_skeleton;
                    Vector3 FunnyPos = s_root_door + funnyPosModifier;//s_root_scientist - s_changed_value * 20 + Vector3.forward * 30 - Vector3.left * 13;
                    Quaternion FunnyQua = s_root_rotation;
                    List<RoleTypeId> roles = new List<RoleTypeId>() { RoleTypeId.ClassD,RoleTypeId.Scientist,RoleTypeId.FacilityGuard,RoleTypeId.NtfPrivate,RoleTypeId.ChaosRifleman };
                    for (int i = 0; i < 30; i++)
                    {
                        string funnyname = "FunnyBoy - No." + i;
                        Ragdoll funnyRagdoll = Ragdoll.CreateAndSpawn(roles.GetRandomValue(),funnyname,"LoL", FunnyPos, FunnyQua);
                        //funnyRagdoll.Position += new Vector3(0,2f,0f);
                        //funnyRagdoll.Position += -Vector3.forward * 3;
                        if (i == 29)
                        {
                            /*foreach (Player player in Player.List)
                            {
                                if (player.Role == RoleTypeId.Scp3114)
                                {
                                    player.Position = FunnyPos;
                                    player.Rotation = FunnyQua;
                                    //player.Position += new Vector3(0f,2f,-0f);
                                    //funnyRagdoll.Position += -Vector3.forward * 3;
                                    break;
                                }
                            }*/
                        }
                    }

                    foreach (Door door in Door.List)
                    {
                        if (door.Type == DoorType.Scp173Gate)
                        {
                            door.IsOpen = true;
                        }
                    }
                    //x:23y:112.4z:100
            });
        }

        public void AlphaWarheadLock(StartingEventArgs ev)
        {
            Log.Debug("AlphaWarheadLock Successfully Started.");
            if (!WarheadLocked && !DeadmanSwitch.IsSequenceActive && !SpecialWarhead /*&& Warhead.IsInProgress*/)
            {
                float debugLocktime = Warhead.RealDetonationTimer * cfg.WarheadLockTimeMultipler;
                Log.Debug("Alpha Warhead Lock Timer:" + debugLocktime);
                Timing.CallDelayed(Warhead.RealDetonationTimer * cfg.WarheadLockTimeMultipler, () =>
                {
                    if (!WarheadLocked && !DeadmanSwitch.IsSequenceActive && Warhead.IsInProgress && !SpecialWarhead)
                    {
                        WarheadLocked = true;
                        Cassie.MessageTranslated("Alpha Warhead Stop Detonation System now Locked. All personnel evacuate to the surface immediately.","<color=red>ALPHA WARHEAD</color>停止システムが<color=red>ロック</color>されました。全職員は迅速に地上に<color=red>避難</color>してください",true,true,true);
                    }
                });
            }
            else
            {
                    Log.Debug("Alpha Warhead Lock Not Working.\nStatus:\nWarheadLocked?: "+WarheadLocked+"\nIsDeadman?: "+DeadmanSwitch.IsSequenceActive/*+"\nIsProgress?: "+Warhead.IsInProgress*/);
            }
        }

        public void DeadmanCancell(DeadmanSwitchInitiatingEventArgs ev)
        {
            if (DeadmanDisable)
            {
                ev.IsAllowed = false;
            }
        }

        public void DeconCancell(DecontaminatingEventArgs ev)
        {
            if (DeconCancellFlag)
            {
                ev.IsAllowed = false;
                Log.Debug("Decon Cancell called.");
            }
        }

        public string NowEvent = String.Empty;
        public void SpEventHandler()
        {
            if (cfg.EventAllowed)
            {
                Timing.CallDelayed(1, () =>
                {
                    Log.Debug("Funny:" + Funny);
                    if (Funny <= 0.15) // Omega Warhead
                    {
                        if (cfg.OW_Allowed)
                        {
                            NowEvent = "Omega Warhead";
                            SpecialWarhead = true;
                            WarheadID = 1;
                            DeadmanDisable = true;
                            OmegaWarheadEvent();
                        }
                    }
                    else if (Funny <= 0.25) // Delta Warhead
                    {
                        if (cfg.DW_Allowed)
                        {
                            NowEvent = "Delta Warhead";
                            SpecialWarhead = true;
                            WarheadID = 2;
                            DeadmanDisable = true;
                            DeltaWarheadEvent();
                        }
                    }
                    else if (Funny <= 0.35) // SCP-096's Cry Fuck
                    {
                        if (cfg.CF_Allowed)
                        {
                            NowEvent = "SCP-096's Cry Fuck";
                            CryFuckEnabled = true;
                            Scp096CryFuckEvent();
                        }
                    }
                });
            }
            Slafight_Plugin_EXILED.Plugin.Singleton.CustomMap.LoadMap("normal",String.Empty);
        }

        public void OmegaWarheadEvent()
        {
            int eventPID = EventPID;
            Log.Debug("OmegaWarheadEvent called. PID:"+eventPID);
            
            if (eventPID != EventPID) return;
            WarheadLocked = true;
            
            //DeadmanSwitch.IsDeadmanSwitchEnabled = false;
            //DeadmanSwitch.IsSequenceActive = false;
            if (eventPID != EventPID) return;
            Cassie.MessageTranslated("Warning. By order of O5 Command , Starting Alpha Warhead System for Stop Containment Breach.","警告。O5評議会の決定により収容違反収束の為<color=red>ALPHA WARHEAD</color>の使用が決定されました。システムを準備しています・・・", true);
            Timing.CallDelayed(30, () =>
            {
                if (eventPID != EventPID) return;
                Cassie.MessageTranslated("Alpha Warhead System is now Locked . Warhead Starting . . .","<color=red>ALPHA WARHEAD</color>がロックされました。爆破システムを起動しています・・・");
                Timing.CallDelayed(35, () =>
                {
                    if (eventPID != EventPID) return;
                    Cassie.MessageTranslated("Alpha Warhead System Not Found . Reporting to O5 Command . . .","<color=red>ALPHA WARHEAD</color>起爆システムが見つかりませんでした。O5に報告しています・・・");
                    Timing.CallDelayed(350, () =>
                    {
                        if (eventPID != EventPID) return;
                        foreach (Room rooms in Room.List)
                        {
                            rooms.Color = Color.blue;
                        }
                        Cassie.MessageTranslated("By Order of O5 Command . Omega Warhead Sequence Activated .","O5評議会の決定により、<color=blue>OMEGA WARHEAD</color>シーケンスが開始されました。施設の全てを2分40秒後に爆破します。",true);
                        Cassie.MessageTranslated("Goodbye .","さようなら",false);
                        CreateAndPlayAudio("omega.ogg","Cassie",Vector3.zero,true,null,false,999999999,0);
                        Timing.CallDelayed(160, () =>
                        {
                            if (eventPID != EventPID) return;
                            foreach (Player player in Player.List)
                            {
                                if (player == null) continue;
                                player.ExplodeEffect(ProjectileType.FragGrenade);
                                player.Kill("OMEGA WARHEADに爆破された");
                            }
                        });
                    });
                });
            });
        }
        
        public void DeltaWarheadEvent()
        { 
            int eventPID = EventPID;
            Log.Debug("DeltaWarheadEvent called. PID:"+eventPID);
            
            if (eventPID != EventPID) return;
            DeconCancellFlag = true;
            WarheadLocked = true;
            
            if (eventPID != EventPID) return;
            Timing.CallDelayed(60, () =>
            {
                if (eventPID != EventPID) return;
                DecontaminationController.Singleton.TimeOffset = int.MinValue;
                DecontaminationController.DeconBroadcastDeconMessage = "除染は取り消されました";
                Cassie.MessageTranslated("Light Containment Zone Decontamination Process now Stopped . Waiting new Process or Sequence Order .","軽度収容区画の除染プロセスが停止されました。代替となる指令を待機しています・・・");
                Timing.CallDelayed(180, () =>
                {
                    if (eventPID != EventPID) return;
                    Cassie.MessageTranslated("By order of O5 Command . Delta Warhead Sequence Activated . Entrance Zone and Heavy Containment Zone personnels . Please go to Light Containment Zone .","O5評議会の決定により、<color=green>DELTA WARHEAD</color>シーケンスが開始されました。中層及びエントランス区画は1分40秒後に爆破します。中層及びエントランス区画職員は下層か地上へ非難してください。", true);
                    foreach (Room rooms in Room.List)
                    {
                        if (rooms.Zone == ZoneType.Entrance || rooms.Zone == ZoneType.HeavyContainment)
                        {
                            rooms.Color = Color.green;
                        }
                    }
                    CreateAndPlayAudio("delta.ogg","Cassie",Vector3.zero,true,null,false,999999999,0);
                    Timing.CallDelayed(100, () =>
                    {
                        if (eventPID != EventPID) return;
                        Log.Debug("Delta Passed EventPID Checker");
                        List<ElevatorType> lockEvTypes = new List<ElevatorType>() { ElevatorType.GateA,ElevatorType.GateB,ElevatorType.LczA,ElevatorType.LczB };
                        foreach (Lift lift in Lift.List){
                            Log.Debug("sendforeach:"+lift.Type);
                            if (lockEvTypes.Contains(lift.Type))
                            {
                                Log.Debug("foreach catched: "+lift.Type);
                                lift.TryStart(0,true);
                            }
                        }
                        Log.Debug("Delta Passed TryStart Elevator Foreach.");
                        List<DoorType> lockEvDoorTypes = new List<DoorType>() { DoorType.ElevatorGateA,DoorType.ElevatorGateB,DoorType.ElevatorLczA,DoorType.ElevatorLczB };
                        foreach (Door door in Door.List)
                        {
                            Log.Debug("lockforeach:"+door.Type);
                            if (lockEvDoorTypes.Contains(door.Type))
                            {
                                Log.Debug("foreach catched: "+door.Type);
                                door.Lock(DoorLockType.Warhead);
                            }
                        }
                        Log.Debug("Delta Passed Lock Elevator Foreach.");
                        foreach (Player player in Player.List)
                        {
                            Log.Debug("playerforeach:"+player.Zone);
                            if (player.Zone == ZoneType.Entrance || player.Zone == ZoneType.HeavyContainment)
                            {
                                player.ExplodeEffect(ProjectileType.FragGrenade);
                                player.Kill("DELTA WARHEADに爆破された");
                            }
                        }
                        Log.Debug("Delta Passed Kill Player Foreach");
                    });
                });
            });
        }

        public void Scp096CryFuckEvent()
        {
            int eventPID = EventPID;
            Log.Debug("Scp096's CryFuckEvent called. PID:"+eventPID);
            if (eventPID != EventPID) return;

            CryFuckEnabled = true;
            foreach (Player player in Player.List)
            {
                if (player.Role.Team == Team.SCPs || player.Role.Type == RoleTypeId.Tutorial)
                {
                    player.Role.Set(RoleTypeId.Scp096);
                    break;
                }
            }
        }

        private Vector3 ShyguyPosition = Vector3.zero;
        public void CryFuckSpawn(ChangingRoleEventArgs ev)
        {
            var GetPlayerTeam = RoleExtensions.GetTeam(ev.NewRole);
            if (CryFuckEnabled && !CryFuckSpawned && GetPlayerTeam == Team.SCPs && (ev.Reason == SpawnReason.RoundStart || ev.Reason == SpawnReason.ForceClass))
            {
                ev.IsAllowed = false;
                CryFuckSpawned = true;
                ev.Player.Role.Set(RoleTypeId.Scp096);
                ev.Player.CustomInfo = "SCP-096: ANGER";
                ev.Player.UniqueRole = "Scp096_Anger";
                ev.Player.MaxArtificialHealth = 1000;
                ev.Player.MaxHealth = 5000;
                ev.Player.Health = 5000;
                StatusEffectBase? movement = ev.Player.GetEffect(EffectType.MovementBoost);
                movement.Intensity = 50;
                ev.Player.ShowHint("あなたは<color=red>SCP-096: ANGER!</color>\nSCP-096の怒りと悲しみが頂点に達し、その化身へと変貌して大いなる力を手に入れた。\n<color=red>とにかく破壊しまくれ！！！！！</color>",10);
                /*Vector3 shyroom = Vector3.zero;
                foreach (Room rooms in Room.List)
                {
                    if (rooms.Type == RoomType.Hcz096)
                    {
                        shyroom = rooms.Position;
                    }
                }
                Log.Debug("Position Get from Hcz096: "+"X:" + shyroom.x + " Y:" + shyroom.y + " Z:" + shyroom.z);
                */
                //ev.Player.Position = new Vector3(shyroom.x,shyroom.y + 3f,shyroom.z);
                //ev.Player.Position = new Vector3(31.8f,-97.04092f,105.0388f);
                //ev.Player.Transform.localEulerAngles = new Vector3(0, -90, 0);
                ev.Player.Transform.eulerAngles = new Vector3(0, -90, 0);
                ShyguyPosition = ev.Player.Position;
                Log.Debug("Scp096: Anger was Spawned!");
                StartAnger();
            }
        }

        public void StartAnger()
        {
            /*Vector3 shyroom = Room.Get(RoomType.Hcz096).Position;
            foreach (Room rooms in Room.List)
            {
                if (rooms.Type == RoomType.Hcz096)
                {
                    shyroom = rooms.Position;
                    Log.Debug("Position Get from for096 Npc_shyroom_get vector3: "+"X:" + shyroom.x + " Y:" + shyroom.y + " Z:" + shyroom.z);
                }
            }*/

            foreach (Door door in Door.List)
            {
                if (door.Type == DoorType.HeavyContainmentDoor && door.Room.Type == RoomType.Hcz096)
                {
                    door.Lock(DoorLockType.AdminCommand);
                }
            }
            
            Vector3 spawnPoint = new Vector3(ShyguyPosition.x + 1f, ShyguyPosition.y + 0f, ShyguyPosition.z);
            Npc term_npc = Npc.Spawn("for096",RoleTypeId.ClassD,false,position:spawnPoint);
            term_npc.Transform.localEulerAngles = new Vector3(0,-90,0);
        }
        
        public void EndlessAnger(Exiled.Events.EventArgs.Scp096.CalmingDownEventArgs ev)
        {
            if (ev.Player.UniqueRole == "Scp096_Anger")
            {
                ev.IsAllowed = false;
                ev.ShouldClearEnragedTimeLeft = true;
            }
        }

        public void CleanShyDummy(EnragingEventArgs ev)
        {
            if (ev.Player.UniqueRole == "Scp096_Anger")
            {
                foreach (Npc npc in Npc.List)
                {
                    if (npc.CustomName == "for096")
                    {
                        npc.Destroy();
                    }
                }
            }
        }

        public void CustomRoleRemover(DyingEventArgs ev)
        {
            ev.Player.UniqueRole = String.Empty;
        }

        public void PositionGet(FlippingCoinEventArgs ev)
        {
            Vector3 playerPosition = ev.Player.Position;
            if (ev.Player.Role == RoleTypeId.Tutorial)
            {
                if (ev.Player.CurrentRoom != null)
                {
                    ev.Player.ShowHint("X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z + "\nRoom: " + ev.Player.CurrentRoom.Type,5);
                    Log.Debug("Position Get: "+"X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z);
                    Log.Debug(" Room: " + ev.Player.CurrentRoom.Type);
                }
                else if (ev.Player.CurrentRoom == null)
                {
                    ev.Player.ShowHint("X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z /*+ "\nRoom: " + ev.Player.CurrentRoom.Type*/,5);
                    Log.Debug("Position Get: "+"X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z);
                    //Log.Debug(" Room: " + ev.Player.CurrentRoom.Type);
                }
                else
                {
                    ev.Player.ShowHint("X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z /*+ "\nRoom: " + ev.Player.CurrentRoom.Type*/,5);
                    Log.Debug("Position Get: "+"X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z);
                    //Log.Debug(" Room: " + ev.Player.CurrentRoom.Type);
                }
            }
        }

        public void DoorGet(InteractingDoorEventArgs ev)
        {
            if (ev.Player.Role == RoleTypeId.Tutorial)
            {
                ev.Player.ShowHint("DoorType:" + ev.Door.Type + "\nName & Room: " + ev.Door.Name + ", " + ev.Door.Room.Type,5);
                Log.Debug("Door Get: " + ev.Door.Type);
                Log.Debug(" Name & Room: " + ev.Door.Name + ", " + ev.Door.Room.Type);
            }
        }

        public void CreateRagdoll(ShotEventArgs ev)
        {
            if (ev.Player.Role == RoleTypeId.Tutorial)
            {
                if (ev.Firearm.Type == ItemType.GunCOM15)
                {
                    Ragdoll testrag = Ragdoll.CreateAndSpawn(RoleTypeId.ClassD,"test","aaa",new Vector3(ev.Player.Position.x,ev.Player.Position.y + 1f,ev.Player.Position.z));
                }
            }
        }

        public void LockedStopSystem(StoppingEventArgs ev)
        {
            if (WarheadLocked)
            {
                ev.IsAllowed = false;
                ev.Player.ShowHint("Warheadの操作は現在ロックされています",3);
            }
        }
        public void LockedStartSystem(StartingEventArgs ev)
        {
            if (WarheadLocked)
            {
                ev.IsAllowed = false;
                ev.Player.ShowHint("Warheadの操作は現在ロックされています",3);
            }
        }
        
        public void OnTouchedEnemy(Exiled.Events.EventArgs.Player.HurtingEventArgs ev)
        {
            if (ev.Attacker.UniqueRole == "Scp096_Anger")
            {
                ev.Amount = 999999;
                ev.Attacker.ArtificialHealth = ev.Attacker.ArtificialHealth + 25;
            }
        }

        public void StatusManager(ChangingRoleEventArgs ev)
        {
            var GetPlayerTeam = RoleExtensions.GetTeam(ev.NewRole);
            if (ev.NewRole == RoleTypeId.ClassD)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.Scientist)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.FacilityGuard)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.NtfPrivate)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.NtfSergeant)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.NtfSpecialist)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.NtfCaptain)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.ChaosConscript)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.ChaosRifleman)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.ChaosMarauder)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.ChaosRepressor)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.Scp049)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.Scp0492)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.Scp079)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.Scp096)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.Scp106)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.Scp173)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.Scp939)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.Scp3114)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.Tutorial)
            {
                
            }
            else if (ev.NewRole == RoleTypeId.CustomRole)
            {
                
            }
        }
    }
}