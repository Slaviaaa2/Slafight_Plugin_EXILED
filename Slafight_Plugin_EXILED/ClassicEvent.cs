using System;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using MEC;
using ProjectMER.Commands.Modifying.Scale;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace Slafight_Plugin_EXILED;

public class ClassicEvent
{
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
                CustomItem.TrySpawn(101, pickup.Position,out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f, 0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardScientist)
            {
                pickup.Destroy();
                CustomItem.TrySpawn(102, pickup.Position,out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardGuard)
            {
                pickup.Destroy();
                CustomItem.TrySpawn(103, pickup.Position,out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardMTFPrivate)
            {
                pickup.Destroy();
                CustomItem.TrySpawn(104, pickup.Position,out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardMTFOperative)
            {
                pickup.Destroy();
                CustomItem.TrySpawn(105, pickup.Position,out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardMTFCaptain)
            {
                pickup.Destroy();
                CustomItem.TrySpawn(106, pickup.Position,out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardResearchCoordinator)
            {
                pickup.Destroy();
                CustomItem.TrySpawn(107, pickup.Position,out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardZoneManager)
            {
                pickup.Destroy();
                CustomItem.TrySpawn(108, pickup.Position,out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardFacilityManager)
            {
                pickup.Destroy();
                CustomItem.TrySpawn(109, pickup.Position,out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
            if (pickup.Type == ItemType.KeycardO5)
            {
                pickup.Destroy();
                CustomItem.TrySpawn(110, pickup.Position,out var pickup_);
                pickup_.Rotation *= Quaternion.Euler(180f,0f, 0f);
            }
        }
    }
}