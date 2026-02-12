using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Pickups;
using LightContainmentZoneDecontamination;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.SpecialEvents;
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events
{
    public class RevolverBattles : SpecialEvent
    {
        // ===== メタ情報 =====
        public override SpecialEventType EventType => SpecialEventType.RevolverBattles;
        public override int MinPlayersRequired => 0;
        public override string LocalizedName => "Revolver Battles";
        public override string TriggerRequirement => "無し";

        private Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
            EventHandler.CreateAndPlayAudio;

        // ===== 実行エントリポイント =====
        protected override void OnExecute(int eventPID)
        {
            if (CancelIfOutdated())
                return;

            foreach (var player in Player.List)
            {
                if (player == null) continue;
                player.GiveOrDrop(ItemType.GunRevolver);
                player.AddAmmo(AmmoType.Ammo44Cal, 80);
            }
        }

        public override void RegisterEvents() { }
        public override void UnregisterEvents() { }
    }
}
