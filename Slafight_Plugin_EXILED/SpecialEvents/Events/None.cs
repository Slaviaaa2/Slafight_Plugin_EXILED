using System;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features; // SpecialEvent 基底クラス
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events
{
    public class NoneEvent : SpecialEvent
    {
        // ===== メタ情報 =====
        public override SpecialEventType EventType => SpecialEventType.None;
        public override int MinPlayersRequired => 0; // 誰もいなくても可なら 0
        public override string LocalizedName => "無し";
        public override string TriggerRequirement => "無し";

        // ===== ショートカット =====
        private EventHandler EventHandler => Plugin.Singleton.EventHandler;

        private Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
            EventHandler.CreateAndPlayAudio;

        // ===== 実行本体 =====
        protected override void OnExecute(int eventPid)
        {
            return; // None lol.
        }

        public override void RegisterEvents() { }
        public override void UnregisterEvents() { }
    }
}
