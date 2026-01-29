using System;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features; // SpecialEvent 基底
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events
{
    public class FifthistsRaidEvent : SpecialEvent
    {
        // ===== メタ情報 =====
        public override SpecialEventType EventType => SpecialEventType.FifthistsRaid;
        public override int MinPlayersRequired => 4;
        public override string LocalizedName => "Fifthists Raid";
        public override string TriggerRequirement => "4人以上のプレイヤー";

        // ===== ショートカット =====
        private EventHandler EventHandler => Plugin.Singleton.EventHandler;

        private Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
            EventHandler.CreateAndPlayAudio;

        // ===== 実行本体 =====
        protected override void OnExecute(int eventPid)
        {
            // PID セット済みなので CancelIfOutdated() だけ見ればよい
            if (CancelIfOutdated())
                return;

            Plugin.Singleton.SpecialEventsHandler.IsFifthistsRaidActive = true;

            // まず非 SCP から 1/4 を FifthistRescure に
            int convertedCount = 0;
            int targetCount = (int)Math.Truncate(Player.List.Count / 4f);

            foreach (Player player in Player.List)
            {
                if (CancelIfOutdated())
                    return;

                if (player.Role.Team == Team.SCPs)
                    continue;

                player.SetRole(CRoleTypeId.FifthistRescure);
                convertedCount++;

                if (convertedCount >= targetCount)
                    break;
            }

            // SCP チームから SCP-3005 を 1 人だけ選ぶ（既に 3005 が居たらそれを優先）
            Player existing3005 = null;
            foreach (Player player in Player.List)
            {
                if (player.GetCustomRole() == CRoleTypeId.Scp3005)
                {
                    existing3005 = player;
                    break;
                }
            }

            if (existing3005 == null)
            {
                foreach (Player player in Player.List)
                {
                    if (CancelIfOutdated())
                        return;

                    if (player.Role.Team == Team.SCPs)
                    {
                        player.SetRole(CRoleTypeId.Scp3005);
                        existing3005 = player;
                        break;
                    }
                }
            }

            // 8 秒後に Cassie + BGM
            Timing.CallDelayed(8f, () =>
            {
                if (CancelIfOutdated())
                    return;

                // BGM 再生
                CreateAndPlayAudio("_w_fifthists.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999f, 0f);

                // Cassie アナウンス（元コードのヘルパー）
                CassieHelper.AnnounceFifthist(convertedCount);
            });
        }

        // 追加サブスク不要
        public override void RegisterEvents() { }
        public override void UnregisterEvents() { }
    }
}
