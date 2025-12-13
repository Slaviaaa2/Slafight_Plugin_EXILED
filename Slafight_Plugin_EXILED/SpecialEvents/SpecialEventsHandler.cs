using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.SpecialEvents.Events;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.SpecialEvents;

public class SpecialEventsHandler
{
    public SpecialEventsHandler()
    {
        // SEH Default
        Exiled.Events.Handlers.Server.RoundStarted += RoundStartedAddEvent;
        Exiled.Events.Handlers.Server.RestartingRound += RoundRestartSkipEvent;
        Exiled.Events.Handlers.Server.RestartingRound += RoundRestartAddEvent;
        Exiled.Events.Handlers.Server.WaitingForPlayers += eventLocSet;
        Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayersInitEvent;
        
        // Need Handler Events Subscribes
        Exiled.Events.Handlers.Player.ChangingRole += CryFuckSpawn;
        Exiled.Events.Handlers.Scp096.CalmingDown += EndlessAnger;
        Exiled.Events.Handlers.Scp096.Enraging += CleanShyDummy;
    }

    ~SpecialEventsHandler()
    {
        // SEH Default
        Exiled.Events.Handlers.Server.RoundStarted -= RoundStartedAddEvent;
        Exiled.Events.Handlers.Server.RestartingRound -= RoundRestartSkipEvent;
        Exiled.Events.Handlers.Server.RestartingRound -= RoundRestartAddEvent;
        Exiled.Events.Handlers.Server.WaitingForPlayers -= eventLocSet;
        Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayersInitEvent;
        
        // Need Handler Events Unsubscribes
        Exiled.Events.Handlers.Player.ChangingRole -= CryFuckSpawn;
        Exiled.Events.Handlers.Scp096.CalmingDown -= EndlessAnger;
        Exiled.Events.Handlers.Scp096.Enraging -= CleanShyDummy;
    }
    // Setup & Utils
    public string localizedEventName = String.Empty;
    public string eventNeedTriggers = String.Empty;
    public List<SpecialEventType> EventQueue = new List<SpecialEventType>() { };
    public List<SpecialEventType> HappenedEvents = new List<SpecialEventType>() { };
    public int EventPID = 1;
    // Need Handler Events Variables
    public bool CryFuckEnabled = false;
    public bool CryFuckSpawned = false;
    public bool isFifthistsRaidActive = false;
    
    public void AddEvent(SpecialEventType eventType)
    {
        if (!Enum.IsDefined(typeof(SpecialEventType), eventType))
        {
            Log.Error("SEH: AddEvent失敗(存在しないSpecialEventType)");
            return;
        }
        EventQueue.Add(eventType);
    }

    public void SkipEvent(int EventQueueId = 0)
    {
        if (EventQueueId < 0 || EventQueueId >= EventQueue.Count)
        {
            Log.Warn($"SEH: SkipEventスキップ(無効なインデックス: {EventQueueId}, Count: {EventQueue.Count})");
            return;
        }
        EventQueue.RemoveAt(EventQueueId);
        EventPID++;
    }


    public void RunEvent(SpecialEventType eventType)
    {
        if (!Enum.IsDefined(typeof(SpecialEventType), eventType))
        {
            Log.Error("SEH: RunEvent失敗(存在しないSpecialEventType)");
            return;
        }

        if (EventQueue.Count == 0)
            EventQueue.Add(eventType);
        else
            EventQueue[0] = eventType;

        eventLocSet();
        SpecialEventsController();
    }
    
    public void RunRandomEvent()
    {
        SelectRandom();
        Log.Debug(SelectedEvent);
        RunEvent(SelectedEvent);
        Log.Debug(EventQueue[0]);
    }
    
    public void SetQueueRandomEvent()
    {
        SelectRandom();
        Log.Debug(SelectedEvent);
        if (!Enum.IsDefined(typeof(SpecialEventType), SelectedEvent))
        {
            Log.Error("SEH: RunEvent失敗(存在しないSpecialEventType)");
            return;
        }
        EventQueue.Insert(1, SelectedEvent);
        SkipEvent();
        eventLocSet();
    }

    public void InitStats()
    {
        Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler.DeconCancellFlag = false;
        Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler.DeadmanDisable = false;
        Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler.WarheadLocked = false;
        Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler.SpecialWarhead = false;
        CryFuckEnabled = false;
        CryFuckSpawned = false;
        isFifthistsRaidActive = false;
        EventPID++;
    }
    // Automatic Event Controls
    public void SpecialEventsController()
    {
        Log.Debug(string.Join(", ", EventQueue));

        if (EventQueue.Count == 0)
        {
            Log.Warn("SEH: SpecialEventsControllerが空のEventQueueで呼ばれたためスキップします。");
            return;
        }

        InitStats();
        SpecialEventType nowEvent = EventQueue[0];
        if (nowEvent == SpecialEventType.None)
        {
            
        }
        else if (nowEvent == SpecialEventType.OmegaWarhead)
        {
            OmegaWarhead OmegaWarhead = new OmegaWarhead();
            OmegaWarhead.OmegaWarheadEvent();
        }
        else if (nowEvent == SpecialEventType.DeltaWarhead)
        {
            DeltaWarhead DeltaWarhead = new DeltaWarhead();
            DeltaWarhead.DeltaWarheadEvent();
        }
        else if (nowEvent == SpecialEventType.Scp096CryFuck)
        {
            Scp096CryFuckEvent();
        }
        else if (nowEvent == SpecialEventType.Scp1509BattleField)
        {
            Scp1509BattleField Scp1509BattleField = new Scp1509BattleField();
            Scp1509BattleField.Scp1509BattleFieldEvent();
        }
        else if (nowEvent == SpecialEventType.FifthistsRaid)
        {
            FifthistsRaid FifthistsRaid = new FifthistsRaid();
            FifthistsRaid.FifthistsRaidEvent();
        }
        else if (nowEvent == SpecialEventType.NuclearAttack)
        {
            ChaosInsurgencyRaid ChaosInsurgencyRaid = new ChaosInsurgencyRaid();
            ChaosInsurgencyRaid.CIREvent();
        }
        else if (nowEvent == SpecialEventType.ClassicEvent)
        {
            ClassicEvent ClassicEvent = new ClassicEvent();
            ClassicEvent.ClassicEvent_();
        }
        else
        {
            
        }
        Log.Info("今回の特殊イベント： "+localizedEventName);
    }
    
    public void RoundStartedAddEvent()
    {
        SpecialEventsController();
    }

    public SpecialEventType SelectedEvent = SpecialEventType.None; // 初期値
    public void SelectRandom()
    {
        // Config がまだなら何もしない
        if (Slafight_Plugin_EXILED.Plugin.Singleton == null ||
            Slafight_Plugin_EXILED.Plugin.Singleton.Config == null)
        {
            Log.Warn("SEH: SelectRandom called before Config initialized. Skipping.");
            return;
        }
        List<SpecialEventType> allowedEvents = new List<SpecialEventType>();
        if (Slafight_Plugin_EXILED.Plugin.Singleton.Config.EventAllowed)
        {
            if (Player.List.Count >= 0)
            {
                allowedEvents.Add(SpecialEventType.OmegaWarhead);
            }
            if (Player.List.Count >= 0)
            {
                allowedEvents.Add(SpecialEventType.DeltaWarhead);
            }
            if (Player.List.Count >= 4)
            {
                allowedEvents.Add(SpecialEventType.Scp096CryFuck);
            }
            if (Player.List.Count >= 4)
            {
                allowedEvents.Add(SpecialEventType.Scp1509BattleField);
            }
            if (Player.List.Count >= 4)
            {
                allowedEvents.Add(SpecialEventType.FifthistsRaid);
            }
            if (Player.List.Count >= 5)
            {
                allowedEvents.Add(SpecialEventType.NuclearAttack);
            }
            if (Player.List.Count >= 0)
            {
                // SCRAPPED. IT'S VERY FUCKIN ISSUES HAVE.
                //allowedEvents.Add(SpecialEventType.ClassicEvent);
            }
        }
        if (Slafight_Plugin_EXILED.Plugin.Singleton.Config.EventAllowed)
        {
            SelectedEvent = SpecialEventType.None;
            if (Random.Range(1,3) == 1)
            {
                // SpecialEventTypeの1番はNoneの為除外
                var EventTypes = Enum.GetValues(typeof(SpecialEventType));
                SpecialEventType selectedEvent = SpecialEventType.None;
                do
                {
                    int getRandomValue = Random.Range(1, EventTypes.Length);
                    selectedEvent = (SpecialEventType)getRandomValue;
                }
                while (!allowedEvents.Contains(selectedEvent)); // allowedEventsに含まれなければ再抽選

                SelectedEvent = selectedEvent;
            }
        }
    }

    public void eventLocSet()
    {
        if (EventQueue.Count == 0)
        {
            localizedEventName = "無し";
            eventNeedTriggers = "無し";
            Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler.SyncSpecialEvent();
            return;
        }

        SpecialEventType nowEvent = EventQueue[0];
        if (nowEvent == SpecialEventType.None)
        {
            localizedEventName = "無し";
            eventNeedTriggers = "無し";
        }
        else if (nowEvent == SpecialEventType.OmegaWarhead)
        {
            localizedEventName = "OmegaWarhead";
            eventNeedTriggers = "無し";
        }
        else if (nowEvent == SpecialEventType.DeltaWarhead)
        {
            localizedEventName = "DeltaWarhead";
            eventNeedTriggers = "無し";
        }
        else if (nowEvent == SpecialEventType.Scp096CryFuck)
        {
            localizedEventName = "Scp096CryFuck";
            eventNeedTriggers = "4人以上のプレイヤー";
        }
        else if (nowEvent == SpecialEventType.Scp1509BattleField)
        {
            localizedEventName = "Scp1509BattleField";
            eventNeedTriggers = "4人以上のプレイヤー";
        }
        else if (nowEvent == SpecialEventType.FifthistsRaid)
        {
            localizedEventName = "FifthistsRaid";
            eventNeedTriggers = "4人以上のプレイヤー";
        }
        else if (nowEvent == SpecialEventType.NuclearAttack)
        {
            localizedEventName = "Chaos Insurgency Raid";
            eventNeedTriggers = "6人以上のプレイヤー";
        }
        else if (nowEvent == SpecialEventType.ClassicEvent)
        {
            localizedEventName = "MEGAPATCH II";
            eventNeedTriggers = "無し";
        }
        else
        {
            localizedEventName = "[エラー：存在しないイベント]";
            eventNeedTriggers = "What The Fuck";
        }
        Slafight_Plugin_EXILED.Plugin.Singleton.EventHandler.SyncSpecialEvent();
    }
    
    public void OnWaitingForPlayersInitEvent()
    {
        Cassie.Clear();
        EventPID++;
        // まだキューが空のときだけ初期イベントを入れる
        if (EventQueue.Count == 0)
        {
            SelectRandom();
            AddEvent(SelectedEvent);
            eventLocSet();
            Log.Info($"SEH: 初期イベント {SelectedEvent} をキューに追加しました。");
        }
    }

    public void RoundRestartAddEvent()
    {
        SelectRandom();
        Timing.CallDelayed(0.01f, () =>
        {
            AddEvent(SelectedEvent);
        });
    }
    
    public void InitAddEvent()
    {
        SelectRandom();
        Timing.CallDelayed(0.01f, () =>
        {
            AddEvent(SelectedEvent);
        
            SpecialEventType nowEvent = EventQueue[0];
        });
    }

    public void RoundRestartSkipEvent()
    {
        if (EventQueue.Count <= 1)
        {
            Log.Debug("SEH: RoundRestartSkipEvent - キューが1件以下のためスキップ");
            return;
        }
        SkipEvent();
    }
    // Need Handler Events
     // Scp096 CryFuck //
    public void Scp096CryFuckEvent()
        {
            int eventPID = EventPID;
            Log.Debug("Scp096's CryFuckEvent called. PID:"+eventPID);
            if (eventPID != EventPID) return;
            
            CryFuckEnabled = true;
            foreach (Player player in Player.List)
            {
                if (eventPID != EventPID) return;
                if (player.Role.Team == Team.SCPs)
                {
                    player.Role.Set(RoleTypeId.Scp096);
                    break;
                }
            }
        }
    
    public static void OverrideRoleName(Player player, string CustomInfo, string DisplayName, string RoleName, string Color)
    {
        // Custom Role Name Area
        player.InfoArea |= PlayerInfoArea.Nickname;
        // Hide Things
        player.InfoArea &= ~PlayerInfoArea.Role;
        player.InfoArea &= ~PlayerInfoArea.Nickname;
        
        if (CustomInfo is null || CustomInfo.Length < 1)
        {
            player.ReferenceHub.nicknameSync.Network_customPlayerInfoString = $"<color={Color}>{DisplayName}\n{player.UniqueRole}</color>";
        }
        else
        {
            player.ReferenceHub.nicknameSync.Network_customPlayerInfoString = $"<color={Color}>{CustomInfo}\n{DisplayName}\n{player.UniqueRole}</color>";
        }
    }

        public Vector3 ShyguyPosition = Vector3.zero;
        public void CryFuckSpawn(ChangingRoleEventArgs ev)
        {
            var GetPlayerTeam = RoleExtensions.GetTeam(ev.NewRole);
            if (CryFuckEnabled && !CryFuckSpawned && GetPlayerTeam == Team.SCPs && (ev.Reason == SpawnReason.RoundStart || ev.Reason == SpawnReason.ForceClass))
            {
                ev.IsAllowed = false;
                Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.CryFuckSpawned = true;
                ev.Player.Role.Set(RoleTypeId.Scp096);
                Timing.CallDelayed(0.02f, () =>
                {
                    ev.Player.UniqueRole = "Scp096_Anger";
                    ev.Player.CustomInfo = "<color=#C50000>SCP-096: ANGER</color>";
                    ev.Player.MaxArtificialHealth = 1000;
                    ev.Player.MaxHealth = 5000;
                    ev.Player.Health = 5000;
                    StatusEffectBase? movement = ev.Player.GetEffect(EffectType.MovementBoost);
                    movement.Intensity = 50;
                    ev.Player.ShowHint(
                        "<color=red>SCP-096: ANGER</color>\nSCP-096の怒りと悲しみが頂点に達し、その化身へと変貌して大いなる力を手に入れた。\n<color=red>とにかく破壊しまくれ！！！！！</color>",
                        10);
                    ev.Player.Transform.eulerAngles = new Vector3(0, -90, 0);
                    Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.ShyguyPosition = ev.Player.Position;
                    Log.Debug("Scp096: Anger was Spawned!");
                    Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.StartAnger();
                });
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
                if (door.Type == DoorType.HeavyContainmentDoor && door.Room.Type == RoomType.Hcz096) // This Door Lock is Don't working in 14.2 or later version. TODO:Create MER's Locked Door
                {
                    door.Lock(DoorLockType.AdminCommand);
                }
            }
            
            Vector3 spawnPoint = new Vector3(ShyguyPosition.x + 1f, ShyguyPosition.y + 0f, ShyguyPosition.z);
            Npc term_npc = Npc.Spawn("for096",RoleTypeId.ClassD,false,position:spawnPoint);
            term_npc.Transform.localEulerAngles = new Vector3(0,-90,0);
            Cassie.MessageTranslated("SCP 0 9 6 . SCP 0 9 6 . .g4 .g3 .g7 .g6 .g2 .g2 .g5","<color=red>SCP-096！SCP-096！うわl...(ノイズ音)</color>");
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
                ev.InitialDuration = Single.MaxValue;
            }
        }
}
