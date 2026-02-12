using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Warhead;
using MapGeneration;
using MEC;
using PlayerRoles;
using ProjectMER.Events.Arguments;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Log = Exiled.API.Features.Log;
using Player = Exiled.API.Features.Player;
using PlayerHandler = Exiled.Events.Handlers.Player;
using Room = Exiled.API.Features.Room;
using ServerHandler = Exiled.Events.Handlers.Server;
using Warhead = Exiled.API.Features.Warhead;
using WarheadHandler = Exiled.Events.Handlers.Warhead;
using MapHandler = Exiled.Events.Handlers.Map;

namespace Slafight_Plugin_EXILED.MainHandlers
{
    public class EventHandler
    {
        // Event Handler
        public EventHandler()
        {
            PlayerHandler.Verified += OnVerified;
            PlayerHandler.Left += OnLeft;
            ServerHandler.RestartingRound += OnRoundRestarted;
            ServerHandler.RoundStarted += OnRoundStarted;
            ServerHandler.ReloadedPlugins += OnPluginLoad;

            MapHandler.Decontaminating += DeconCancell;
            
            PlayerHandler.ChangingRole += OnChangingRole;
            PlayerHandler.FlippingCoin += PositionGet;
            PlayerHandler.InteractingDoor += DoorGet;

            WarheadHandler.Starting += AlphaWarheadLock;
            //WarheadHandler.Starting += OmegaWarheadEvent;
            WarheadHandler.DeadmanSwitchInitiating += DeadmanCancell;
            WarheadHandler.Stopping += LockedStopSystem;
            WarheadHandler.Starting += LockedStartSystem;

            ProjectMER.Events.Handlers.Schematic.SchematicSpawned += SetupSpawnPoints;
        }
        ~EventHandler()
        {
            PlayerHandler.Verified -= OnVerified;
            PlayerHandler.Left -= OnLeft;
            ServerHandler.RoundStarted -= OnRoundRestarted;
            ServerHandler.RoundStarted -= OnRoundStarted;
            ServerHandler.ReloadedPlugins -= OnPluginLoad;
            
            MapHandler.Decontaminating -= DeconCancell;
            
            PlayerHandler.ChangingRole -= OnChangingRole;
            PlayerHandler.FlippingCoin -= PositionGet;
            PlayerHandler.InteractingDoor -= DoorGet;
            
            WarheadHandler.Starting -= AlphaWarheadLock;
            //WarheadHandler.Starting -= OmegaWarheadEvent;
            WarheadHandler.DeadmanSwitchInitiating -= DeadmanCancell;
            WarheadHandler.Stopping -= LockedStopSystem;
            WarheadHandler.Starting -= LockedStartSystem;

            ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= SetupSpawnPoints;
        }
        // Other File Access
        private readonly Config cfg = Plugin.Singleton.Config;
        // Setup Variables
        public bool DeadmanDisable = false;
        public bool SkeletonSpawned = false;
        public float SpawnRoll = 1;
        public float Funny = 0;
        
        public bool SpecialWarhead = false;
        public static int WarheadID = 0;
        public bool WarheadLocked = false;

        public bool DeconCancellFlag = false;

        public bool CryFuckEnabled = false;

        public bool CryFuckSpawned = false;

        private bool GateDoorLocked = false;
        public bool IsScpAutoSpawnLocked = false;
        // - System Flags - //
        private bool _pluginLoaded = false;

        private void OnPluginLoad()
        {
            Log.Info("OnPluginLoad is successfully called!");
            if (!_pluginLoaded)
            {

                _pluginLoaded = true;
            }
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            ev.Player.Broadcast(6,"\n<size=28><color=#008cff>シャープ鯖</color>へようこそ！\\n本サーバーはRP鯖です。RPを念頭に置いておく以外の制約は無いので自由に楽しんでください！</size>",Broadcast.BroadcastFlags.Normal,true);
            Timing.CallDelayed(0.05f, () =>
            {
                string tips = Tips.GetRandomTip();
                ev.Player?.ShowHint(("\n\n\n\n\n\n\n<size=32>次のイベント："+Plugin.Singleton.SpecialEventsHandler.LocalizedEventName+"</size>"+
                                    $"\n\n<size=28>Tips: {tips}</size>"
                                    ),5555f);
            });
        }

        private void OnLeft(LeftEventArgs ev)
        {
            if (ev.Player.GetTeam() != CTeam.SCPs)
                return;

            if (Round.ElapsedTime.TotalSeconds > 179)
                return;

            int scpAlive = Player.List.Count(p => p.IsAlive && p.GetTeam() == CTeam.SCPs);
            if (scpAlive >= 1)
                return;

            var candidate = Player.List.FirstOrDefault(p => !p.IsAlive);
            if (candidate == null)
                return;

            var roleInfo = ev.Player.GetRoleInfo();

            if (roleInfo.Custom == CRoleTypeId.None)
            {
                candidate.SetRole(roleInfo.Vanilla);
            }
            else
            {
                Debug.Assert(roleInfo.Custom != null, "roleInfo.Custom != null");
                candidate.SetRole((CRoleTypeId)roleInfo.Custom);
            }

            candidate.ShowHint("※SCPプレイヤーが切断したため代わりにスポーンしました");
        }

        public void SyncSpecialEvent()
        {
            Timing.CallDelayed(0.05f, () =>
            {
                if (!Round.InProgress)
                {
                    foreach (Player player in Player.List)
                    {
                        string tips = Tips.GetRandomTip();
                        player?.ShowHint(("\n\n\n\n\n\n\n<size=32>次のイベント："+Plugin.Singleton.SpecialEventsHandler.LocalizedEventName+"</size>"+
                                            $"\n\n<size=28>Tips: {tips}</size>"
                            ),5555f);
                    }
                }
            });
        }
        public void OnRoundRestarted()
        {
            Timing.CallDelayed(0.1f, () =>
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

                GateDoorLocked = false;
                IsScpAutoSpawnLocked = false;
            });
        }

        public static Vector3 Scp173SpawnPoint = Vector3.zero;
        public void SetupSpawnPoints(SchematicSpawnedEventArgs ev)
        {
            if (ev.Schematic.Name == "Scp173SpawnPoint")
            {
                Scp173SpawnPoint = ev.Schematic.Position;
                ev.Schematic.Destroy();
            }
        }

        public void OnRoundStarted()
        {
            foreach (Player player in Player.List.Where(p => p != null))
            {
                player.ShowHint("");
            }

            var roundPID = Plugin.Singleton.SpecialEventsHandler.EventPID;
            foreach (Door door in Door.List)
            {
                if (door.Type == DoorType.GateA || door.Type == DoorType.GateB)
                {
                    door.Lock(120f,DoorLockType.AdminCommand);
                }

                if (door.Type == DoorType.PrisonDoor)
                {
                    door.Lock(DoorLockType.AdminCommand);
                }
            }

            Timing.CallDelayed(1f, () =>
            {
                List<SpecialEventType> notallowed =
                [
                    SpecialEventType.OperationBlackout,
                    SpecialEventType.Scp1509BattleField,
                    SpecialEventType.FacilityTermination
                ];
                if (!notallowed.Contains(Plugin.Singleton.SpecialEventsHandler.NowEvent))
                {
                    if (Plugin.Singleton.SpecialEventsHandler.NowEvent == SpecialEventType.OmegaWarhead)
                    {
                        Exiled.API.Features.Cassie.MessageTranslated($"Emergency , emergency , A large containment breach is currently started within the site. All personnel must immediately begin evacuation .",
                            "緊急、緊急、現在大規模な収容違反がサイト内で発生しています。全職員は警備隊の指示に従い、避難を開始してください。",true);
                    }
                    else
                    {
                        Exiled.API.Features.Cassie.MessageTranslated($"Attention, All personnel . Detected containment breach is currently started within the site. All personnel must immediately begin evacuation .",
                            "全職員へ通達。収容違反の発生を確認しました。全職員は警備隊の指示に従い、避難を開始してください。",true);
                    }
                    foreach (Room room in Room.List)
                    {
                        room.RoomLightController.ServerFlickerLights(3f);
                    }
                }

                Timing.CallDelayed(5f, () =>
                {
                    foreach (Door door in Door.List)
                    {
                        if (door.Type == DoorType.Scp173Gate)
                        {
                            door.Unlock();
                            door.IsOpen = true;
                        }
                        else if (door.Type == DoorType.PrisonDoor)
                        {
                            door.IsOpen = true;
                        }
                    }
                });

                int i = 0;
                foreach (Item item in Item.List)
                {
                    item.Base.transform.position.TryGetRoom(out var room);
                    if (room.Name == RoomName.LczArmory)
                    {
                        i++;
                    }
                }

                if (i==0)
                {
                    var FSP = Pickup.Create(ItemType.GunFSP9);
                    FSP.Base.transform.position =
                        Room.Get(RoomType.LczArmory).WorldPosition(new Vector3(0f, 1f, 0f));
                    var CROSSVEC = Pickup.Create(ItemType.GunCrossvec);
                    CROSSVEC.Base.transform.position =
                        Room.Get(RoomType.LczArmory).WorldPosition(new Vector3(0f, 1f, 0f));
                    for (int j = 0; j < 20; j++)
                    {
                        var AMMO = Pickup.Create(ItemType.Ammo9x19);
                        AMMO.Base.transform.position =
                            Room.Get(RoomType.LczArmory).WorldPosition(new Vector3(0f, 1f, 0f));
                    }
                }

                Timing.CallDelayed(3f, () =>
                {
                    if (!IsScpAutoSpawnLocked)
                    {
                        int scpCount = Player.List.Count(p => p != null && p.Role.Team == Team.SCPs);
                        Log.Debug($"[AutoSCP] SCP count = {scpCount}, players = {Player.List.Count}");

                        foreach (var p in Player.List)
                            Log.Debug($"[AutoSCP] {p.Nickname}: {p.Role.Type} / {p.Role.Team}");

                        if (scpCount == 0 && Player.List.Count > 0)
                        {
                            var player = Player.List.GetRandomValue();
                            Log.Debug($"[AutoSCP] Forcing 173 to {player.Nickname}");
                            player.SetRole(RoleTypeId.Scp173);
                            player.ShowHint("※SCPが正常に生成されなかった為、SCP-173に変更されました。");
                        }
                    }
                });

            });
        }
        
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            Timing.CallDelayed(1.05f, () =>
            {
                RoleTypeId role = ev.Player?.Role;
                Team allowed = PlayerRolesUtils.GetTeam(role);
                if (ev.Player == null) return;
                if (allowed == Team.SCPs) return;
                if (!Round.InProgress) return;
                if (ev.Player.HasItem(ItemType.Flashlight)) return;
                if (ev.Player.IsInventoryFull) return;
                if (ev.NewRole == RoleTypeId.Spectator) return;
                if (ev.Player.Inventory == null) return;
                Log.Debug("Giving Flashlight to " + ev.Player?.Nickname);
                ev.Player?.GiveOrDrop(ItemType.Flashlight);
            });
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
    
            AudioClipStorage.LoadClip(Path.Combine(Plugin.Singleton.Config.AudioReferences, fileName), fileName);

            audioPlayer.AddClip(fileName, destroyOnEnd: destroyOnEnd);
        }

        private void AlphaWarheadLock(StartingEventArgs ev)
        {
        }

        private void DeadmanCancell(DeadmanSwitchInitiatingEventArgs ev)
        {
            if (DeadmanDisable)
            {
                ev.IsAllowed = false;
            }
        }

        private void DeconCancell(DecontaminatingEventArgs ev)
        {
            if (DeconCancellFlag)
            {
                ev.IsAllowed = false;
                Log.Debug("Decon Cancell called.");
            }
        }

        private void PositionGet(FlippingCoinEventArgs ev)
        { 
            Vector3 playerPosition = ev.Player.Position;
            if (ev.Player.UniqueRole == "Debug")
            {
                if (ev.Player.CurrentRoom != null)
                {
                    Room currentRoom = ev.Player.CurrentRoom;
                    Vector3 localPos = currentRoom.Rotation * (playerPosition - currentRoom.Position);
                    Vector3 localRot = currentRoom.Rotation.eulerAngles;

                    ev.Player.ShowHint("X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z + 
                                       "\nRoom: " + currentRoom.Type +
                                       "\nLocal: " + localPos.x + "," + localPos.y + "," + localPos.z +
                                       "\nRot: " + currentRoom.Rotation.eulerAngles.x + "," + currentRoom.Rotation.eulerAngles.y + "," + currentRoom.Rotation.eulerAngles.z, 5);
            
                    Log.Debug("Position Get: " + "X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z);
                    Log.Debug(" Room: " + currentRoom.Type);
                    Log.Debug(" LocalPos: X:" + localPos.x + " Y:" + localPos.y + " Z:" + localPos.z);
                    Log.Debug(" RoomRot: X:" + localRot.x + " Y:" + localRot.y + " Z:" + localRot.z);
                }
                else
                {
                    ev.Player.ShowHint("X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z, 5);
                    Log.Debug("Position Get: " + "X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z);
                }
            }
        }

        private void DoorGet(InteractingDoorEventArgs ev)
        {
            if (ev.Player.UniqueRole == "Debug")
            {
                Room doorRoom = ev.Door.Room;
                Vector3 doorLocalPos = doorRoom.Rotation * (ev.Player.Position - doorRoom.Position);
                Vector3 doorLocalRot = doorRoom.Rotation.eulerAngles;

                ev.Player.ShowHint("DoorType:" + ev.Door.Type + "\nName & Room: " + ev.Door.Name + ", " + doorRoom.Type +
                                   "\nLocal: " + doorLocalPos.x + "," + doorLocalPos.y + "," + doorLocalPos.z +
                                   "\nRot: " + doorLocalRot.x + "," + doorLocalRot.y + "," + doorLocalRot.z, 5);
        
                Log.Debug("Door Get: " + ev.Door.Type);
                Log.Debug(" Name & Room: " + ev.Door.Name + ", " + doorRoom.Type);
                Log.Debug(" LocalPos: X:" + doorLocalPos.x + " Y:" + doorLocalPos.y + " Z:" + doorLocalPos.z);
                Log.Debug(" RoomRot: X:" + doorLocalRot.x + " Y:" + doorLocalRot.y + " Z:" + doorLocalRot.z);
            }
            else
            {
                if (ev.Door.Type == DoorType.GateA || ev.Door.Type == DoorType.GateB)
                {
                    if (ev.Door.IsLocked && Plugin.Singleton.SpecialEventsHandler.NowEvent == SpecialEventType.None)
                    {
                        ev.Player.ShowHint("収容違反への対応として暫くロックされているようだ・・・");
                    }
                }
            }
        }

        private void LockedStopSystem(StoppingEventArgs ev)
        {
        }

        private void LockedStartSystem(StartingEventArgs ev)
        {
            if (WarheadLocked)
            {
                ev.IsAllowed = false;
                ev.Player.ShowHint("Warheadの操作は現在ロックされています",3);
            }
        }
    }
}