using System;
using System.Collections.Generic;
using System.Linq;
using AdminToys;
using Exiled.API.Features.Toys;
using MEC;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Structs;
using Slafight.API.Strcuts;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomMaps;

public static class Communications
{
    public static void Register()
    {
        Exiled.Events.Handlers.Server.RoundStarted += Setup;
    }

    public static void Unregister()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= Setup;
    }
    
    public static InteractableToy? InteractableToy { get; private set; }
    public static Text? TextToy { get; private set; }

    public static string MonitorText
    {
        get => TextToy?.TextFormat ?? string.Empty; 
        set => TextToy?.TextFormat = value;
    }

    private static void Setup()
    {
        Timing.CallDelayed(1f, () =>
        {
            SetupInteractable();
            SetupText();

            MonitorText = "Scanning...";
        });
    }
    
    private static void SetupInteractable()
    {
        ProjectMER.Features.TriggerPointManager.TryGetByTag("EZCInteractable", out var list);
        var point = list.FirstOrDefault();
        if (point is null) return;
        var interactableToy = InteractableToy.Create();
        interactableToy.Position = point.transform.position;
        interactableToy.Rotation = Quaternion.identity;
        interactableToy.Scale = Vector3.one;
        interactableToy.Shape = InvisibleInteractableToy.ColliderShape.Box;
        interactableToy.InteractionDuration = 3f;
        InteractableToy = interactableToy;
    }

    private static void SetupText()
    {
        ProjectMER.Features.TriggerPointManager.TryGetByTag("EZCScreen", out var list);
        var point = list.FirstOrDefault();
        if (point is null) return;
        var textToy = Text.Create();
        textToy.Position = point.transform.position;
        textToy.Rotation = point.transform.rotation;
        textToy.Scale = new Vector3(1f, 1f, 1f);
        TextToy = textToy;
    }
}