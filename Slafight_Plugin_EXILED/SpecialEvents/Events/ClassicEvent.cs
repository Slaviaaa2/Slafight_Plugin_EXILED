using System;
using System.Linq;
using Exiled.API.Features.Pickups;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class ClassicEvent : SpecialEvent
{
    protected override void OnExecute(int eventPid)
    {
        throw new NotImplementedException();
    }

    public override SpecialEventType EventType { get; } = SpecialEventType.ClassicEvent;
    public override string LocalizedName { get; } = "ClassicEvent";
    public override string TriggerRequirement { get; } = "不可";
    public override bool IsReadyToExecute()
    {
        return false;
    }

    private bool classicenabled = false;
    public ClassicEvent()
    {
        //Exiled.Events.Handlers.Server.RoundStarted += SchemHandler;
    }

    ~ClassicEvent()
    {
        //Exiled.Events.Handlers.Server.RoundStarted -= SchemHandler;
    }

    public void ClassicEvent_()
    {
        classicenabled = false;
        //classicenabled = true;
        //SchemHandler();
    }
    private void SchemHandler()
    {
        if (!classicenabled) return;
        foreach (Pickup pickup in Pickup.List.ToList())
        {
            if (pickup.Type == ItemType.KeycardJanitor)
            {
                pickup.Destroy();
                CustomItemExtensions.TrySpawn<KeycardOld_Janitor>(pickup.Position, out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f, 0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardScientist)
            {
                pickup.Destroy();
                CustomItemExtensions.TrySpawn<KeycardOld_Scientist>(pickup.Position, out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardGuard)
            {
                pickup.Destroy();
                CustomItemExtensions.TrySpawn<KeycardOld_Guard>(pickup.Position, out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardMTFPrivate)
            {
                pickup.Destroy();
                CustomItemExtensions.TrySpawn<KeycardOld_Cadet>(pickup.Position, out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardMTFOperative)
            {
                pickup.Destroy();
                CustomItemExtensions.TrySpawn<KeycardOld_Lieutenant>(pickup.Position, out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardMTFCaptain)
            {
                pickup.Destroy();
                CustomItemExtensions.TrySpawn<KeycardOld_Commander>(pickup.Position, out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardResearchCoordinator)
            {
                pickup.Destroy();
                CustomItemExtensions.TrySpawn<KeycardOld_ResearchSupervisor>(pickup.Position, out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardZoneManager)
            {
                pickup.Destroy();
                CustomItemExtensions.TrySpawn<KeycardOld_ZoneManager>(pickup.Position, out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardFacilityManager)
            {
                pickup.Destroy();
                CustomItemExtensions.TrySpawn<KeycardOld_FacilityManager>(pickup.Position, out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardO5)
            {
                pickup.Destroy();
                CustomItemExtensions.TrySpawn<KeycardOld_O5>(pickup.Position, out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
        }
    }
}