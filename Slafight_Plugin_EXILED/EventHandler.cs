using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Exiled.API.Features.Pickups;
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
using ProjectMER.Events.Arguments;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using Subtitles;
using UnityEngine;
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
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED
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
            PlayerHandler.ChangingRole += StatusManager;
            PlayerHandler.Hurting += OnTouchedEnemy;
            PlayerHandler.FlippingCoin += PositionGet;
            PlayerHandler.InteractingDoor += DoorGet;
            PlayerHandler.Shot += CreateRagdoll;

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
            PlayerHandler.ChangingRole -= StatusManager;
            PlayerHandler.Hurting -= OnTouchedEnemy;
            PlayerHandler.FlippingCoin -= PositionGet;
            PlayerHandler.InteractingDoor -= DoorGet;
            PlayerHandler.Shot -= CreateRagdoll;
            
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
        public bool PluginLoaded = false;

        public List<string> Tips = new List<string>()
        {
            "次のメジャーアプデv1.5.xでは第五が全面的に見直される予定だよ！",
            "「正常性は記録装置の夢だ。人間性は、そのバグとして残る。」 - DELTA COMMAND",
            " 「人間性など不確実性の残滓。再起動で正常性を永遠に。」 - O5-1",
            "Nu-7の元帥はいつ実装されるのだろうか・・・？その答えを得るべく、我々は(以下略",
            "ゲームサーバー公開ツールの有料プランの更新が来年なせいで回線がカスだよ！助けて！",
            "すらびあさんのバグ発生力は頭逝かれてます！",
            "第五第五第五第五第五第五",
            "実は何処かに隠し要素があるらしい...?",
            "隠し要素の場所にはいずれのアプデで追加される要素と関係があるとか...?",
            "Tipsっていいよね！！！！",
            "「サーバーが落ちた？落ちたのは希望だ、まだだ。」 - AI",
            "「SCPたちはバグじゃない、仕様という名の恐怖だ。」 - AI",
            "「デバッグは終わらない。SCPも、同じく。」 - AI",
            "皆もっと遊んでくれーーーーーー！！！",
            "Hello, World!\\n",
            "SCP-CN-2000はいいぞ",
            "Build -> Test -> Fail -> AI Suggestion -> Fix -> ...loop",
            "サイレントリアルタイムアップデートが行われる鯖なんて唯一無二ではないか？",
            "君もC#を覚えて開発に携わらないか？なあ、楽しいぞ？",
            "Exiledは実はLabApiプラグインである。その為LabApiのコードも動くのである。",
            "しかし、誰も来なかった。",
            "カピバラ様を崇めよ()",
            "「ロール抽選はAI任せ。でもバグらせるのは君次第だ。」 - AI",
            "「ZoneManagerは3人しかいない。見つけたら大事に扱おう、できれば。」 - AI",
            "「カスタムロールは全部ベース職の上に乗っている。\n中身を覗くときはUniqueRoleを忘れずに。」 - AI",
            "「5人に1人がSCP？安心しろ、人間側も3人に1人おかしい。」 - AI",
            "「/spawnはデバッグ用。ロールバランスを壊したら、\nログとにらめっこする覚悟を。」 - AI",
            "「今日のラウンドが安定していたら、\nそれはコードじゃなくて運が良かっただけかもしれない。」 - AI",
            "AIに鯖乗っ取られるんじゃないかってぐらい貢献させてて怖くなってくる今日この頃"
        };

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
            ev.Player.Broadcast(6,"\n<size=28><color=#008cff>シャープ鯖</color>へようこそ！\\n本サーバーはRP鯖です。RPを念頭に置いておく以外の制約は無いので自由に楽しんでください！</size>",Broadcast.BroadcastFlags.Normal,true);
            Timing.CallDelayed(0.05f, () =>
            {
                int tipsRandom = Random.Range(0,Tips.Count);
                string tips = Tips[tipsRandom];
                ev.Player?.ShowHint(("\n\n\n\n\n\n\n<size=32>次のイベント："+Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.localizedEventName+"</size>"+
                                    $"\n\n<size=28>Tips: {tips}</size>"
                                    ),5555f);
            });
        }

        public void OnLeft(LeftEventArgs ev)
        {
            if (ev.Player.Role.Team != Team.SCPs)
                return;

            if (Round.ElapsedTime.TotalSeconds > 179)
                return;

            int scpAlive = Player.List.Count(p => p.IsAlive && p.Role.Team == Team.SCPs);
            if (scpAlive >= 1)
                return;

            var candidate = Player.List.FirstOrDefault(p => !p.IsAlive);
            if (candidate == null)
                return;

            // 元プレイヤーのカスタムロールを取得
            var custom = ev.Player.GetCustomRole();

            // SCP用のCRoleTypeIdだけを補充対象にする
            bool isScpCustom = custom is CRoleTypeId.Scp3005
                    or CRoleTypeId.Scp966
                    or CRoleTypeId.Scp096Anger
                ;

            if (custom == null || custom == CRoleTypeId.None || !isScpCustom)
            {
                // 通常SCPロールで補充
                candidate.SetRole(ev.Player.Role);
            }
            else
            {
                // SCPカスタムロールで補充
                candidate.SetRole(custom);
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
                        int tipsRandom = Random.Range(0,Tips.Count);
                        string tips = Tips[tipsRandom];
                        player?.ShowHint(("\n\n\n\n\n\n\n<size=32>次のイベント："+Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.localizedEventName+"</size>"+
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
                    door.Lock(120f,DoorLockType.None);
                }

                if (door.Type == DoorType.PrisonDoor)
                {
                    door.Lock(DoorLockType.AdminCommand);
                }
            }

            Timing.CallDelayed(1f, () =>
            {
                if (Plugin.Singleton.SpecialEventsHandler.nowEvent != SpecialEventType.OperationBlackout && Plugin.Singleton.SpecialEventsHandler.nowEvent != SpecialEventType.Scp1509BattleField)
                {
                    if (Plugin.Singleton.SpecialEventsHandler.nowEvent == SpecialEventType.OmegaWarhead)
                    {
                        CassieExtensions.CassieTranslated($"Emergency , emergency , A large containment breach is currently started within the site. All personnel must immediately begin evacuation .",
                            "緊急、緊急、現在大規模な収容違反がサイト内で発生しています。全職員は警備隊の指示に従い、避難を開始してください。",true);
                    }
                    else
                    {
                        CassieExtensions.CassieTranslated($"Attention, All personnel . Detected containment breach is currently started within the site. All personnel must immediately begin evacuation .",
                            "全職員へ通達。収容違反の発生を確認しました。全職員は警備隊の指示に従い、避難を開始してください。",true);
                    }
                    foreach (Room room in Room.List)
                    {
                        room.RoomLightController.ServerFlickerLights(3f);
                    }
                }

                foreach (Door door in Door.List)
                {
                    if (door.Type == DoorType.PrisonDoor || door.Type == DoorType.Scp173Gate)
                    {
                        door.Unlock();
                        door.IsOpen = true;
                    }
                }

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
            foreach (Player player in Player.List)
            {
                if (Round.InProgress)
                {
                    player.ShowHint("");
                }
            }
            SpawnRoll = 1;
            SpawnRoll = UnityEngine.Random.Range(0, 1f);

            Timing.CallDelayed(1.05f, () =>
            {
                RoleTypeId role = ev.Player?.Role;
                Team allowed = PlayerRolesUtils.GetTeam(role);
                if (ev.Player == null) return;
                if (allowed == Team.SCPs) return;
                if (!Round.InProgress) return;
                if (ev.Player.HasItem(ItemType.Flashlight)) return;
                if (ev.Player.Items.Count >= 8) return;
                if (ev.NewRole == null) return;
                if (ev.Player.Inventory == null) return;
                Log.Debug("Giving Flashlight to " + ev.Player?.Nickname);
                ev.Player?.AddItem(ItemType.Flashlight);
            });
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
    
            AudioClipStorage.LoadClip(Path.Combine(Slafight_Plugin_EXILED.Plugin.Singleton.Config.AudioReferences, fileName), fileName);

            audioPlayer.AddClip(fileName, destroyOnEnd: destroyOnEnd);
        }

        public void SkeletonRagdoll()
        {
            return;
            // IT'S ALL DEPRECATED. GOOD BYE FUNNY BOYS.
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
            if (cfg.WarheadLockAllowed)
            {
                Log.Debug("AlphaWarheadLock Successfully Started.");
                if (!WarheadLocked && !DeadmanSwitch.IsSequenceActive && !SpecialWarhead /*&& Warhead.IsInProgress*/)
                {
                    float debugLocktime = Warhead.RealDetonationTimer * cfg.WarheadLockTimeMultiplier;
                    Log.Debug("Alpha Warhead Lock Timer:" + debugLocktime);
                    Timing.CallDelayed(Warhead.RealDetonationTimer * cfg.WarheadLockTimeMultiplier, () =>
                    {
                        if (!WarheadLocked && !DeadmanSwitch.IsSequenceActive && Warhead.IsInProgress && !SpecialWarhead)
                        {
                            WarheadLocked = true;
                            CassieExtensions.CassieTranslated("Alpha Warhead Stop Detonation System now Locked. All personnel evacuate to the surface immediately.","<color=red>ALPHA WARHEAD</color>停止システムが<color=red>ロック</color>されました。全職員は迅速に地上に<color=red>避難</color>してください",true);
                        }
                    });
                }
                else
                {
                    Log.Debug("Alpha Warhead Lock Not Working.\nStatus:\nWarheadLocked?: "+WarheadLocked+"\nIsDeadman?: "+DeadmanSwitch.IsSequenceActive/*+"\nIsProgress?: "+Warhead.IsInProgress*/);
                }
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

        public void PositionGet(FlippingCoinEventArgs ev)
        {
            Vector3 playerPosition = ev.Player.Position;
            if (ev.Player.UniqueRole == "Debug")
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
            if (ev.Player.UniqueRole == "Debug")
            {
                ev.Player.ShowHint("DoorType:" + ev.Door.Type + "\nName & Room: " + ev.Door.Name + ", " + ev.Door.Room.Type,5);
                Log.Debug("Door Get: " + ev.Door.Type);
                Log.Debug(" Name & Room: " + ev.Door.Name + ", " + ev.Door.Room.Type);
            }
            else
            {
                if (ev.Door.Type == DoorType.GateA || ev.Door.Type == DoorType.GateB)
                {
                    if (ev.Door.IsLocked && Plugin.Singleton.SpecialEventsHandler.nowEvent == SpecialEventType.None)
                    {
                        ev.Player.ShowHint("収容違反への対応として暫くロックされているようだ・・・");
                    }
                }
            }
        }

        public void CreateRagdoll(ShotEventArgs ev)
        {
            
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
            if (ev.Attacker != null && ev.Attacker.UniqueRole == "Scp096_Anger")
            {
                ev.Amount = 999999;
                ev.Attacker.ArtificialHealth += 25;
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