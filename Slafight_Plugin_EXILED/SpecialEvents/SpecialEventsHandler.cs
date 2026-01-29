using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using MEC;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.SpecialEvents
{
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
            HappenedEvents.Add(removed);
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
            // 1/3 の確率でイベント、それ以外は何も起こさない（＝None扱い）
            float chance = 1f / 3f;
            if (Random.value > chance)
            {
                // ハズレ: None として扱う（何も実行しない）
                Log.Info("SEH: RunRandomEvent rolled None (no event executed).");
                return;
            }

            // 当たり: 実際にランダムイベントを選んで実行
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

        // 位置0を書き換える「リロール」用
        public void SetQueueRandomEvent()
        {
            float chance = 1f / 3f;
            if (Random.value > chance)
            {
                // ハズレ: Queue[0] を None にする
                if (EventQueue.Count == 0)
                    EventQueue.Add(SpecialEventType.None);
                else
                    EventQueue[0] = SpecialEventType.None;

                EventLocSet();
                Log.Info("SEH: SetQueueRandomEvent rolled None (Queue[0] set to None).");
                return;
            }

            // 当たり: 普通にランダムイベントを選んでQueue[0]に入れる
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

        // 1つ目の「次」にランダムを追加する旧仕様寄せメソッド
        public void InsertQueueRandomEventAfterFirst()
        {
            float chance = 1f / 3f;
            if (Random.value > chance)
            {
                Log.Info("SEH: InsertQueueRandomEventAfterFirst rolled None (no insert).");
                return; // ハズレ → 何も挿し込まない
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

            // ここは純粋に「候補からランダム1個」を選ぶだけ
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
        }

        public void InitStats()
        {
            // 各イベントが独自に管理するよう変更
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

        /// <summary>
        /// 旧: イベントをキューの末尾に追加するメソッド。
        /// 例: SEH.Add(SpecialEventType.Xxx);
        /// </summary>
        [Obsolete("Use AddEvent(SpecialEventType eventType) instead.")]
        public void Add(SpecialEventType eventType)
        {
            AddEvent(eventType);
        }

        /// <summary>
        /// 旧: 先頭イベントをスキップするメソッド。
        /// 例: SEH.Skip();
        /// </summary>
        [Obsolete("Use SkipEvent(int index = 0) instead.")]
        public void Skip()
        {
            SkipEvent();
        }

        /// <summary>
        /// 旧: 指定イベントを即座に実行するメソッド。
        /// 例: SEH.ForceRun(SpecialEventType.Xxx);
        /// </summary>
        [Obsolete("Use RunEvent(SpecialEventType eventType) instead.")]
        public void ForceRun(SpecialEventType eventType)
        {
            RunEvent(eventType);
        }

        /// <summary>
        /// 旧: ランダムイベントを即座に実行するメソッド。
        /// 例: SEH.ForceRunRandom();
        /// </summary>
        [Obsolete("Use RunRandomEvent() instead.")]
        public void ForceRunRandom()
        {
            RunRandomEvent();
        }

        /// <summary>
        /// 旧: 次に起きるイベントを強制的に指定するメソッド。
        /// 例: SEH.ForceNext(SpecialEventType.Xxx);
        /// </summary>
        [Obsolete("Use SetQueueEvent(SpecialEventType eventType) instead.")]
        public void ForceNext(SpecialEventType eventType)
        {
            SetQueueEvent(eventType);
        }

        /// <summary>
        /// 旧: キューにランダムイベントを追加するメソッド。
        /// 例: SEH.QueueRandom();
        /// </summary>
        [Obsolete("Use SetQueueRandomEvent() instead.")]
        public void QueueRandom()
        {
            SetQueueRandomEvent();
        }

        /// <summary>
        /// 旧: 「指定イベントを即実行（キュー書き換え + コントローラ呼び）」なラッパー。
        /// どの名前で呼んでいたか分からない場合の保険用。
        /// </summary>
        [Obsolete("Use SetQueueEvent(...) and SpecialEventsController() instead.")]
        public void LegacyRun(SpecialEventType eventType)
        {
            SetQueueEvent(eventType);
            SpecialEventsController();
        }
    }
}