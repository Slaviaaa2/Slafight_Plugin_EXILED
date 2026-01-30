using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.SpecialEvents
{
    public static class LinqExtensions
    {
        // List<T> 用「直近 n 件を取る」互換メソッド
        public static IEnumerable<T> TakeLastCompat<T>(this IList<T> source, int count)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (count <= 0) yield break;

            int start = Math.Max(0, source.Count - count);
            for (int i = start; i < source.Count; i++)
                yield return source[i];
        }
    }

    public class SpecialEventsHandler
    {
        public static SpecialEventsHandler Instance { get; private set; }

        public SpecialEventsHandler()
        {
            Instance = this;
            SpecialEvent.RegisterAllEvents(); // 全イベント自動登録
        }

        ~SpecialEventsHandler()
        {
            SpecialEvent.UnregisterAllEvents();
            Instance = null;
        }

        // イベントキューと状態
        public readonly List<SpecialEventType> EventQueue = new();
        public readonly List<SpecialEventType> HappenedEvents = new();
        public int EventPID = 1;
        public SpecialEventType NowEvent => EventQueue.FirstOrDefault();
        public bool IsFifthistsRaidActive { get; set; }

        // ==== イベント操作（新実装） ====
        public void AddEvent(SpecialEventType eventType)
        {
            if (!Enum.IsDefined(typeof(SpecialEventType), eventType))
            {
                Log.Error($"SEH: AddEvent failed (invalid SpecialEventType: {eventType})");
                return;
            }

            if (!SpecialEvent.IsEventExecutable(eventType))
            {
                Log.Warn($"SEH: AddEvent skipped (not executable: {eventType})");
                return;
            }

            EventQueue.Add(eventType);
            EventLocSet();
            Log.Info($"SEH: Added event to queue: {eventType}");
        }

        public void SkipEvent(int index = 0)
        {
            if (index < 0 || index >= EventQueue.Count)
            {
                Log.Warn($"SEH: SkipEvent failed (invalid index: {index})");
                return;
            }

            var removed = EventQueue[index];
            EventQueue.RemoveAt(index);
            HappenedEvents.Add(removed); // スキップも「起こった扱い」にするならここで記録
            EventPID++;
            EventLocSet();
            Log.Info($"SEH: Skipped event: {removed}");
        }

        public void RunEvent(SpecialEventType eventType)
        {
            if (!Enum.IsDefined(typeof(SpecialEventType), eventType))
            {
                Log.Error($"SEH: RunEvent failed (invalid SpecialEventType: {eventType})");
                return;
            }

            InitStats();
            EventQueue.Clear();
            EventQueue.Add(eventType);
            SpecialEventsController();
        }

        public void RunRandomEvent()
        {
            float chance = 1f / 3f;
            if (Random.value > chance)
            {
                Log.Info("SEH: RunRandomEvent rolled None (no event executed).");
                return;
            }

            SelectRandom();
            if (SelectedEvent == SpecialEventType.None)
            {
                Log.Info("SEH: RunRandomEvent skipped (no executable events).");
                return;
            }

            RunEvent(SelectedEvent);
        }

        public void SetQueueEvent(SpecialEventType eventType)
        {
            if (!Enum.IsDefined(typeof(SpecialEventType), eventType))
            {
                Log.Error($"SEH: SetQueueEvent failed (invalid SpecialEventType: {eventType})");
                return;
            }

            if (EventQueue.Count == 0)
                EventQueue.Add(eventType);
            else
                EventQueue[0] = eventType;

            EventLocSet();
            Log.Info($"SEH: Queue set to: {eventType}");
        }

        public void SetQueueRandomEvent()
        {
            float chance = 1f / 3f;
            if (Random.value > chance)
            {
                if (EventQueue.Count == 0)
                    EventQueue.Add(SpecialEventType.None);
                else
                    EventQueue[0] = SpecialEventType.None;

                EventLocSet();
                Log.Info("SEH: SetQueueRandomEvent rolled None (Queue[0] set to None).");
                return;
            }

            SelectRandom();
            if (SelectedEvent == SpecialEventType.None)
            {
                Log.Info("SEH: SetQueueRandomEvent skipped (no executable events).");
                return;
            }

            if (EventQueue.Count == 0)
                EventQueue.Add(SelectedEvent);
            else
                EventQueue[0] = SelectedEvent;

            EventLocSet();
            Log.Info($"SEH: Queue[0] rerolled to: {SelectedEvent}");
        }

        public void InsertQueueRandomEventAfterFirst()
        {
            float chance = 1f / 3f;
            if (Random.value > chance)
            {
                Log.Info("SEH: InsertQueueRandomEventAfterFirst rolled None (no insert).");
                return;
            }

            SelectRandom();
            if (SelectedEvent == SpecialEventType.None)
            {
                Log.Info("SEH: InsertQueueRandomEventAfterFirst skipped (no executable events).");
                return;
            }

            int index = Math.Min(1, EventQueue.Count);
            EventQueue.Insert(index, SelectedEvent);

            EventLocSet();
            Log.Info($"SEH: Queue insert random at index {index}: {SelectedEvent}");
        }
        
        // ==== 内部処理 ====
        private SpecialEventType SelectedEvent = SpecialEventType.None;

        private void SelectRandom()
        {
            var allowedEvents = GetAllowedEvents();

            if (!allowedEvents.Any())
            {
                SelectedEvent = SpecialEventType.None;
                return;
            }

            SelectedEvent = allowedEvents[Random.Range(0, allowedEvents.Count)];
        }

        private List<SpecialEventType> GetAllowedEvents()
        {
            var allowed = new List<SpecialEventType>();
            foreach (SpecialEventType type in Enum.GetValues(typeof(SpecialEventType)))
            {
                if (type == SpecialEventType.None)
                    continue;

                if (SpecialEvent.IsEventExecutable(type))
                    allowed.Add(type);
            }

            return allowed;
        }

        public void SpecialEventsController()
        {
            if (EventQueue.Count == 0)
            {
                Log.Warn("SEH: Empty queue");
                return;
            }

            var eventType = EventQueue[0];
            var specialEvent = SpecialEvent.GetEvent(eventType);
            if (specialEvent == null)
            {
                Log.Error($"SEH: No implementation for {eventType}");
                return;
            }

            InitStats();
            specialEvent.Execute(EventPID);
            Log.Info($"SEH: Executed {eventType}: {specialEvent.LocalizedName}");

            // ★ 実行済みイベントを履歴に追加（直近5回チェック用）
            HappenedEvents.Add(eventType);

            // 必要に応じてキューを進める
            if (EventQueue.Count > 0)
                EventQueue.RemoveAt(0);

            EventLocSet();
        }

        public void InitStats()
        {
            EventPID++;
        }

        // ==== ラウンド系イベントハンドラ ====
        public void RoundStartedAddEvent()
        {
            Timing.CallDelayed(0.1f, SpecialEventsController);
        }

        public void RoundRestartSkipEvent()
        {
            EventPID++;
            if (EventQueue.Count <= 1)
                return;

            SkipEvent();
        }

        public void RoundRestartAddEvent()
        {
            SelectRandom();
            Timing.CallDelayed(0.01f, () => AddEvent(SelectedEvent));
        }

        public void OnWaitingForPlayersInitEvent()
        {
            if (EventQueue.Count == 0)
            {
                SelectRandom();
                AddEvent(SelectedEvent);
            }

            EventLocSet();
        }

        // ==== ローカライズ ====
        public string LocalizedEventName { get; private set; } = "無し";
        public string EventNeedTriggers { get; private set; } = "無し";

        public void EventLocSet()
        {
            if (EventQueue.Count == 0)
            {
                LocalizedEventName = "無し";
                EventNeedTriggers = "無し";
            }
            else
            {
                var ev = SpecialEvent.GetEvent(EventQueue[0]);
                LocalizedEventName = ev?.LocalizedName ?? "無し";
                EventNeedTriggers = ev?.TriggerRequirement ?? "無し";
            }

            Plugin.Singleton.EventHandler.SyncSpecialEvent();
        }

        // ==== API ====
        public static bool IsWarheadable()
        {
            var nowEvent = Instance?.NowEvent ?? SpecialEventType.None;
            return nowEvent switch
            {
                SpecialEventType.OmegaWarhead or
                SpecialEventType.OldDeltaWarhead or
                SpecialEventType.NuclearAttack or
                SpecialEventType.OperationBlackout or
                SpecialEventType.Scp1509BattleField or 
                SpecialEventType.SnowWarriersAttack or 
                SpecialEventType.FacilityTermination => false,
                _ => true
            };
        }

        // ================================
        //  互換用 Obsolete ラッパー
        // ================================

        [Obsolete("Use AddEvent(SpecialEventType eventType) instead.")]
        public void Add(SpecialEventType eventType) => AddEvent(eventType);

        [Obsolete("Use SkipEvent(int index = 0) instead.")]
        public void Skip() => SkipEvent();

        [Obsolete("Use RunEvent(SpecialEventType eventType) instead.")]
        public void ForceRun(SpecialEventType eventType) => RunEvent(eventType);

        [Obsolete("Use RunRandomEvent() instead.")]
        public void ForceRunRandom() => RunRandomEvent();

        [Obsolete("Use SetQueueEvent(SpecialEventType eventType) instead.")]
        public void ForceNext(SpecialEventType eventType) => SetQueueEvent(eventType);

        [Obsolete("Use SetQueueRandomEvent() instead.")]
        public void QueueRandom() => SetQueueRandomEvent();

        [Obsolete("Use SetQueueEvent(...) and SpecialEventsController() instead.")]
        public void LegacyRun(SpecialEventType eventType)
        {
            SetQueueEvent(eventType);
            SpecialEventsController();
        }
    }
}
