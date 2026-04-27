using System;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class ClassicEvent : SpecialEvent
{
    protected override void OnExecute(int eventPid)
    {
        throw new NotImplementedException();
    }

    public override SpecialEventType EventType => SpecialEventType.ClassicEvent;
    public override string LocalizedName => "ClassicEvent";
    public override string TriggerRequirement => "不可";

    public override bool IsReadyToExecute() => false;
}