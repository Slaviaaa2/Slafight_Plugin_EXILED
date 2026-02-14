using System;
using System.Linq;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features; // SpecialEvent 基底クラス
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MapExtensions;
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events
{
    public class SergeyMakarovReturns : SpecialEvent
    {
        // ===== メタ情報 =====
        public override SpecialEventType EventType => SpecialEventType.SergeyMakarovReturns;
        public override int MinPlayersRequired => 8;
        public override string LocalizedName => "-=[セルゲイ・マカロフ: リターンズ]=-";
        public override string TriggerRequirement => "April Fools";

        // ===== ショートカット =====
        private EventHandler EventHandler => Plugin.Singleton.EventHandler;

        private Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
            EventHandler.CreateAndPlayAudio;

        // ===== 実行本体 =====
        public override bool IsReadyToExecute()
        {
            return MapFlags.GetSeason() == SeasonTypeId.April;
        }

        protected override void OnExecute(int eventPid)
        {
            int i = 0;
            Player targetPlayer = null;
            foreach (var player in Player.List)
            {
                if (player == null) continue;
                if (player.GetCustomRole() != CRoleTypeId.FacilityManager) continue;
                i++;
                targetPlayer = player;
                break;
            }
            Player.List.Where(player => player.GetTeam() == CTeam.SCPs).ToList().ForEach(player => player.SetRole(RoleTypeId.ClassD));
            if (i <= 0)
            {
                Player.List.GetRandomValue().SetRole(CRoleTypeId.FacilityManager); // TODO: Create Sergey Markov Role.
            }
            else
            {
                targetPlayer.SetRole(CRoleTypeId.FacilityManager); // TODO: That's too.
            }
        }

        public override void RegisterEvents() { }
        public override void UnregisterEvents() { }
    }
}
