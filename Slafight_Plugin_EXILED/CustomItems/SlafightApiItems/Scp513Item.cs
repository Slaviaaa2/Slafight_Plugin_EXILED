#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Usables.Scp244;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomMaps.Entities;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class Scp513Item : CItem
{
    public override string DisplayName => "SCP-513";
    public override string Description => "???";
    protected override string UniqueKey => "Scp513Item";
    protected override ItemType BaseItem => ItemType.Coin;
    protected override bool PickupLightEnabled => true;
    protected override Color PickupLightColor => Color.gray;

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        Exiled.Events.Handlers.Player.FlippingCoin += OnFlipping;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        Exiled.Events.Handlers.Player.FlippingCoin -= OnFlipping;
        base.UnregisterEvents();
    }

    protected override void OnPickupAdded(PickupAddedEventArgs ev)
    {
        var schem = ObjectSpawner.SpawnSchematic("SCP513ItemModel", ev.Pickup.Position, ev.Pickup.Rotation);
        schem.transform.SetParent(ev.Pickup.Transform);
        schem.transform.localPosition = Vector3.zero;
        schem.transform.localRotation = Quaternion.identity;
        base.OnPickupAdded(ev);
    }

    protected override void OnPickupDestroyed(PickupDestroyedEventArgs ev)
    {
        var schem = ev.Pickup.GameObject.GetComponentInChildren<SchematicObject>();
        schem.Destroy();
        base.OnPickupDestroyed(ev);
    }

    private static void OnRoundStarted()
    {
        var npc = Npc.Spawn("tmp", RoleTypeId.Tutorial, true, Room.Get(RoomType.HczArmory).WorldPosition(Vector3.up));
        Timing.CallDelayed(0.6f, () =>
        {
            Get<Scp513Item>()?.Give(npc);
            Timing.CallDelayed(1f, () =>
            {
                npc.Handcuff();
                npc.LateDestroy(1f);
            });
        });
    }

    private void OnFlipping(FlippingCoinEventArgs ev)
    {
        if (!CheckHeld(ev.Player)) return;
        Scp513.AddTarget(ev.Player);
        ev.Player.ShowHint("<size=25>何か視線を感じる気がする...</size>");
        Room? room = null;
        var i = 0;
        while (room is null || room.Type is RoomType.HczIncineratorWayside or RoomType.Hcz079)
        {
            if (i >= 5)
            {
                room = Room.Get(RoomType.HczHid);
                break;
            }
            room = Room.Random(ZoneType.HeavyContainment);
            i++;
        }
        ev.Item?.CreatePickup(room.Cameras.GetRandomValue().Position);
        ev.Item?.Destroy();
    }
}