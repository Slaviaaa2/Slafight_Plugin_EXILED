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
using Slafight_Plugin_EXILED.CustomRoles.SCPs;
using Slafight_Plugin_EXILED.Extensions;
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
    }

    ~SpecialEventsHandler()
    {
        // SEH Default
        Exiled.Events.Handlers.Server.RoundStarted -= RoundStartedAddEvent;
        Exiled.Events.Handlers.Server.RestartingRound -= RoundRestartSkipEvent;
        Exiled.Events.Handlers.Server.RestartingRound -= RoundRestartAddEvent;
        Exiled.Events.Handlers.Server.WaitingForPlayers -= eventLocSet;
        Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayersInitEvent;
    }
    // Setup & Utils
    public string localizedEventName = String.Empty;
    public string eventNeedTriggers = String.Empty;
    public readonly List<SpecialEventType> EventQueue = new List<SpecialEventType>() { };
    public List<SpecialEventType> HappenedEvents = new List<SpecialEventType>() { };
    public int EventPID = 1;
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
    
    public void SetQueueEvent(SpecialEventType eventType)
    {
        if (!Enum.IsDefined(typeof(SpecialEventType), eventType))
        {
            Log.Error($"SEH: SetQueueEvent failed (invalid SpecialEventType: {eventType})");
            return;
        }

        // 既存キューを維持したまま、先頭だけ差し替え
        if (EventQueue.Count == 0)
            EventQueue.Add(eventType);
        else
            EventQueue[0] = eventType;

        // ローカライズ更新
        eventLocSet();

        Log.Info($"SEH: Next special event forced to {eventType} ({localizedEventName})");
    }

    public void InitStats()
    {
        Plugin.Singleton.EventHandler.DeconCancellFlag = false;
        Plugin.Singleton.EventHandler.DeadmanDisable = false;
        Plugin.Singleton.EventHandler.WarheadLocked = false;
        Plugin.Singleton.EventHandler.SpecialWarhead = false;
        isFifthistsRaidActive = false;
        SpawnSystem.Disable = false;
        MapExtensions.OmegaWarhead.IsWarheadStarted = false;
        OperationBlackout.isOperation = false;
        EventPID++;
    }
    // Automatic Event Controls
    public SpecialEventType nowEvent = SpecialEventType.None;
    public void SpecialEventsController()
    {
        Log.Debug(string.Join(", ", EventQueue));

        if (EventQueue.Count == 0)
        {
            Log.Warn("SEH: SpecialEventsControllerが空のEventQueueで呼ばれたためスキップします。");
            return;
        }

        InitStats();
        nowEvent = EventQueue[0];
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
        else if (nowEvent == SpecialEventType.SnowWarriersAttack)
        {
            SnowWarriersAttack SnowWarriersAttack = new SnowWarriersAttack();
            SnowWarriersAttack.SWAEvent();
        }
        else
        {
            Plugin.Singleton.OperationBlackout.Event();
        }
        Log.Info("今回の特殊イベント： "+localizedEventName);
    }
    
    public void RoundStartedAddEvent()
    {
        Timing.CallDelayed(0.1f, SpecialEventsController);
    }

    public SpecialEventType SelectedEvent = SpecialEventType.None; // 初期値
    public void SelectRandom()
    {
        // Config がまだなら何もしない
        if (Plugin.Singleton == null ||
            Plugin.Singleton.Config == null)
        {
            Log.Warn("SEH: SelectRandom called before Config initialized. Skipping.");
            return;
        }
        List<SpecialEventType> allowedEvents = new List<SpecialEventType>();
        if (Plugin.Singleton.Config.EventAllowed)
        {
            if (Player.List.Count >= 0)
            {
                allowedEvents.Add(SpecialEventType.OmegaWarhead);
            }
            if (Player.List.Count >= 0)
            {
                //allowedEvents.Add(SpecialEventType.DeltaWarhead);
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
            } // scrapped
            if (Player.List.Count >= 4)
            {
                allowedEvents.Add(SpecialEventType.OperationBlackout);
            }
            if (Player.List.Count >= 5 && Plugin.Singleton.Config.Season == 2)
            {
                allowedEvents.Add(SpecialEventType.SnowWarriersAttack);
            }
        }
        if (Plugin.Singleton.Config.EventAllowed)
        {
            SelectedEvent = SpecialEventType.None;
            if (Random.Range(0,3) == 0)
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
            Plugin.Singleton.EventHandler.SyncSpecialEvent();
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
            localizedEventName = "OMEGA WARHEAD";
            eventNeedTriggers = "無し";
        }
        else if (nowEvent == SpecialEventType.DeltaWarhead)
        {
            localizedEventName = "DELTA WARHEAD";
            eventNeedTriggers = "無し";
        }
        else if (nowEvent == SpecialEventType.Scp096CryFuck)
        {
            localizedEventName = "ENDLESS CRY";
            eventNeedTriggers = "4人以上のプレイヤー";
        }
        else if (nowEvent == SpecialEventType.Scp1509BattleField)
        {
            localizedEventName = "Scp1509BattleField";
            eventNeedTriggers = "4人以上のプレイヤー";
        }
        else if (nowEvent == SpecialEventType.FifthistsRaid)
        {
            localizedEventName = "Fifthists Raid";
            eventNeedTriggers = "4人以上のプレイヤー";
        }
        else if (nowEvent == SpecialEventType.NuclearAttack)
        {
            localizedEventName = "Chaos Insurgency Raid";
            eventNeedTriggers = "5人以上のプレイヤー";
        }
        else if (nowEvent == SpecialEventType.ClassicEvent)
        {
            localizedEventName = "MEGAPATCH II";
            eventNeedTriggers = "無し";
        }
        else if (nowEvent == SpecialEventType.OperationBlackout)
        {
            localizedEventName = "Operation: Blackout";
            eventNeedTriggers = "無し";
        }
        else if (nowEvent == SpecialEventType.SnowWarriersAttack)
        {
            localizedEventName = "Snow Warriers Raid";
            eventNeedTriggers = "5人以上のプレイヤー";
        }
        else
        {
            localizedEventName = "[エラー：存在しないイベント]";
            eventNeedTriggers = "What The Fuck";
        }
        Plugin.Singleton.EventHandler.SyncSpecialEvent();
    }
    
    public void OnWaitingForPlayersInitEvent()
    {
        Exiled.API.Features.Cassie.Clear();
        EventPID++;
        // まだキューが空のときだけ初期イベントを入れる
        if (EventQueue.Count == 0)
        {
            SelectRandom();
            AddEvent(SelectedEvent);
            eventLocSet();
            Log.Info($"SEH: 初期イベント {SelectedEvent} をキューに追加しました。");
        }
        Log.Info("現在選択中の特殊イベント： "+localizedEventName);
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
        Timing.CallDelayed(0.5f, () =>
        {
            foreach (Player player in Player.List)
            {
                if (eventPID != EventPID) return;
                if (player.Role.Team == Team.SCPs)
                {
                    Exiled.API.Features.Cassie.MessageTranslated("SCP 0 9 6 . SCP 0 9 6 . .g4 .g3 .g7 .g6 .g2 .g2 .g5", "<color=red>SCP-096！SCP-096！うわl...(ノイズ音)</color>", true);
                    player.SetRole(CRoleTypeId.Scp096Anger);
                    break;
                }
            }
        });
    }
    
    // APIs
    public static bool IsWarheadable()
    {
        switch (Plugin.Singleton.SpecialEventsHandler.nowEvent)
        {
            case SpecialEventType.OmegaWarhead:
                return false;
            case SpecialEventType.DeltaWarhead:
                return false;
            case SpecialEventType.NuclearAttack:
                return false;
            case SpecialEventType.OperationBlackout:
                return false;
            case SpecialEventType.Scp1509BattleField:
                return false;
            default:
                return true;
        }
    }
}
