using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features; // SpecialEvent 基底クラス
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class SpeedUpEvent : SpecialEvent
{
    // ===== メタ情報 =====
    public override SpecialEventType EventType => SpecialEventType.SpeedUpEvent;
    public override int MinPlayersRequired => 0; // 誰もいなくても可なら 0
    public override string LocalizedName => "255倍速SL";
    public override string TriggerRequirement => "無し";

    // ===== ショートカット =====
    private EventHandler EventHandler => Plugin.Singleton.EventHandler;

    private Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
        EventHandler.CreateAndPlayAudio;

    // ===== 実行本体 =====
    public override bool IsReadyToExecute()
    {
        return false;
    }

    protected override void OnExecute(int eventPid)
    {
        foreach (var player in Player.List)
        {
            if (player == null) continue;
            player.EnableEffect(EffectType.MovementBoost, 255);
        }

        Timing.RunCoroutine(SpeedUpCoroutine());
    }

    private IEnumerator<float> SpeedUpCoroutine()
    {
        while (true)
        {
            if (CancelIfOutdated()) yield break;
            foreach (var player in Player.List)
            {
                if (player == null) continue; 
                player.EnableEffect(EffectType.MovementBoost, 255);
            }
            yield return Timing.WaitForSeconds(5f);
        }
    }

    public override void RegisterEvents() { }
    public override void UnregisterEvents() { }
}