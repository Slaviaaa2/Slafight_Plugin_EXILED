using Exiled.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomMaps;

public static class VentControl
{
    public static void Register()
    {
        Exiled.Events.Handlers.Server.RoundStarted += RegisterBuiltInPoints;
        Exiled.Events.Handlers.Server.RestartingRound += UnregisterAll;
    }

    public static void Unregister()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= RegisterBuiltInPoints;
        Exiled.Events.Handlers.Server.RestartingRound -= UnregisterAll;
    }

    private static void RegisterBuiltInPoints()
    {
        // LczAirlock -> GR18
        GlobalVentManager.RegisterVentPoint(new (){ JoinRoomType = RoomType.LczAirlock, ExitRoomType = RoomType.LczGlassBox, ExitLocalPosition = new Vector3(8.55f, 2.5f, -5.44f) });
        // Lcz173 -> Hcz049
        GlobalVentManager.RegisterVentPoint(new (){ JoinRoomType = RoomType.Lcz173, ExitRoomType = RoomType.Hcz049, ExitLocalPosition = new Vector3(-6.24f, 91.5f, -10.77f) });
        // LczEx_ClassD <-> LczClassDSpawn
        GlobalVentManager.RegisterVentPoint(new() { JoinRoomType = RoomType.Unknown, JoinCustomType = null, ExitRoomType = RoomType.LczClassDSpawn, ExitCustomType = null, ExitLocalPosition = new Vector3(4.1f, 1f, -0.05f), ExitWorldPosition = Vector3.zero});
        GlobalVentManager.RegisterVentPoint(new() { JoinRoomType = RoomType.LczClassDSpawn, JoinCustomType = null, ExitRoomType = null, ExitCustomType = CRoomType.LczExClassD, ExitLocalPosition = Vector3.zero, ExitWorldPosition = new Vector3(0.82f, 263f, -174.5f), });
        // EzDownstairsPcs <-> EzIntercom
        GlobalVentManager.RegisterVentPoint(new (){ JoinRoomType = RoomType.EzDownstairsPcs, ExitRoomType = RoomType.EzIntercom, ExitLocalPosition = new Vector3(-5.64f, -4.84f, 1.282f) });
        GlobalVentManager.RegisterVentPoint(new (){ JoinRoomType = RoomType.EzIntercom, ExitRoomType = RoomType.EzDownstairsPcs, ExitLocalPosition = new Vector3(6f, -0.4f, 6.3f) });
    }

    private static void UnregisterAll()
    {
        GlobalVentManager.UnregisterAllVentPoints();
    }
}