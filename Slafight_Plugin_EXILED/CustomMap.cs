using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Toys;
using MapGeneration;
using MEC;
using ProjectMER.Events.Arguments;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using UnityEngine;

using Scp049Handler = Exiled.Events.Handlers.Scp049;
using Scp096Handler = Exiled.Events.Handlers.Scp096;
using ServerHandler = Exiled.Events.Handlers.Server;
using Scp330Handler = Exiled.Events.Handlers.Scp330;
using Warhead = Exiled.API.Features.Warhead;
using WarheadHandler = Exiled.Events.Handlers.Warhead;
using MapHandler = Exiled.Events.Handlers.Map;
using PlayerHandler = Exiled.Events.Handlers.Player;
using CassieHandler = Exiled.Events.Handlers.Cassie;

namespace Slafight_Plugin_EXILED
{
    public class CustomMap
    {
        public CustomMap()
        {
            Exiled.Events.Handlers.Server.RoundStarted += SpawnToiletTeleport;
        }
        ~CustomMap()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= SpawnToiletTeleport;
        }

        public void SpawnToiletTeleport()
        {
            SchematicObject tp = ObjectSpawner.SpawnSchematic("HczToilet_TP",Vector3.zero);
            Room room = Room.Get(RoomType.HczStraightC);
            Log.Debug(room.Position);
            Vector3 offset = new Vector3(0f,0f,0f);
            tp.Position = room.Position + room.Rotation * offset;
            tp.Rotation = room.Rotation;
        }

    }
}