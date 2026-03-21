using System;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class OperationBlackout : SpecialEvent
{
    // ===== メタ情報 =====
    public override SpecialEventType EventType => SpecialEventType.RevolverBattles;
    public override int MinPlayersRequired => 0;
    public override string LocalizedName => "Revolver Battles";
    public override string TriggerRequirement => "無し";

    private Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
        EventHandler.CreateAndPlayAudio;
    // ===== 実行エントリポイント & Register & Unregister =====
    public override void RegisterEvents() { }
    public override void UnregisterEvents() { }
    protected override void OnExecute(int eventPID)
    {
        if (CancelIfOutdated())
            return;

        
    }
}