using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Warhead;
using MEC;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Changes;

public static class FacilityLightHandler
{
    public struct RoomColorData
    {
        public string ColorCode;
    }
    
    public static void Register()
    {
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        Exiled.Events.Handlers.Warhead.Starting += OnWarhead;
        Exiled.Events.Handlers.Warhead.Stopping += OnStopping;
    }

    public static void Unregister()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        Exiled.Events.Handlers.Warhead.Starting -= OnWarhead;
        Exiled.Events.Handlers.Warhead.Stopping -= OnStopping;
    }

    private static void OnRoundStarted()
    {
        Timing.CallDelayed(0.5f, InitLight);
    }

    private static void InitLight()
    {
        var Surface = new RoomColorData() { ColorCode = "#c1eaff" };
        var Entrance = new RoomColorData() { ColorCode = "#9bddff" };
        var Hcz = new RoomColorData() { ColorCode = "#9bddff" };
        var Lcz = new RoomColorData() { ColorCode = "#fcd4b0" };
    
        var Intercom = new RoomColorData() { ColorCode = "#FFBCBC" };
        var Lockroom = new RoomColorData() { ColorCode = "#FF0000" };
        var Endroom = new RoomColorData() { ColorCode = "#FF0000" };
    
        // Zone処理（特定ルーム優先）
        foreach (var room in Room.List.Where(r => r != null && r.Zone == ZoneType.Surface))
        {
            ColorUtility.TryParseHtmlString(Surface.ColorCode, out var color);
            room.Color = color;
        }
    
        foreach (var room in Room.List.Where(r => r != null && r.Zone == ZoneType.Entrance))
        {
            ColorUtility.TryParseHtmlString(Entrance.ColorCode, out var color);
            switch (room.Type)
            {
                case RoomType.EzIntercom:
                    ColorUtility.TryParseHtmlString(Intercom.ColorCode, out color);
                    break;
                case RoomType.EzVent or RoomType.EzShelter:
                    ColorUtility.TryParseHtmlString(Endroom.ColorCode, out color);
                    break;
            }

            room.Color = color;
        }
    
        foreach (var room in Room.List.Where(r => r != null && r.Zone == ZoneType.HeavyContainment))
        {
            ColorUtility.TryParseHtmlString(Hcz.ColorCode, out var color);
            room.Color = color;
        }
    
        foreach (var room in Room.List.Where(r => r != null && r.Zone == ZoneType.LightContainment))
        {
            ColorUtility.TryParseHtmlString(Lcz.ColorCode, out var color);
            if (room.Type == RoomType.LczAirlock)
            {
                ColorUtility.TryParseHtmlString(Lockroom.ColorCode, out color);
            }
            room.Color = color;
        }
    }

    public static void TurnToNormal() => InitLight();

    public static void OnWarhead(StartingEventArgs ev)
    {
        if (!ev.IsAllowed) return;
        ColorUtility.TryParseHtmlString("#ff1500", out var color);
        Room.List.ToList().ForEach(room => room.Color = color);
    }

    public static void OnStopping(StoppingEventArgs ev)
    {
        if (!ev.IsAllowed) return;
        InitLight();
    }
}