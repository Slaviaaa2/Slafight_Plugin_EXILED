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

    public static string MonitorText
    {
        get => GetText();
        set => UpdateText(value);
    }

    public static WObjectInfo? ObjectInfo { get; private set; }

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
        ObjectInfo = new WObjectInfo(@object: textToy, position: textToy.Position, rotation: textToy.Rotation,
            scale: textToy.Scale);
    }

    private static string GetText()
    {
        if (ObjectInfo?.Object is Text txt)
        {
            return txt.TextFormat;
        }
        return string.Empty;
    }

    private static string UpdateText(string text)
    {
        if (ObjectInfo?.Object is not Text textToy) return string.Empty;
        textToy.TextFormat = text;
        return textToy.TextFormat;
    }
}