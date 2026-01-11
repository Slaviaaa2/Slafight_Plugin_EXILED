using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.EventArgs;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;
using Exiled.Events.Handlers;
using InventorySystem.Items.MicroHID.Modules;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using Scp914;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Item = Exiled.API.Features.Items.Item;

namespace Slafight_Plugin_EXILED.CustomItems;

[CustomItem(ItemType.Radio)]
public class SNAV300 : CustomItem
{
    public override uint Id { get; set; } = 2012;
    public override string Name { get; set; } = "S-Nav 300";
    public override string Description { get; set; } = "近くのユニークな部屋について調べられる。投げて使用可能";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.Radio;

    public Color glowColor = Color.green;
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> ActiveLights = [];

    public override SpawnProperties SpawnProperties { get; set; } = new();

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.ChangingRadioPreset += ChangeMode;
        Exiled.Events.Handlers.Player.DroppingItem += Use;
        
        Exiled.Events.Handlers.Map.PickupAdded += AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.ChangingRadioPreset -= ChangeMode;
        Exiled.Events.Handlers.Player.DroppingItem -= Use;
        
        Exiled.Events.Handlers.Map.PickupAdded -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;
        base.UnsubscribeEvents();
    }

    private RadioRange mode;
    private List<RoomType> uniques = new()
    {
        RoomType.Lcz914,
        RoomType.Hcz127,
        RoomType.HczCrossRoomWater,
        RoomType.HczHid,
        RoomType.HczNuke,
        RoomType.EzGateA,
        RoomType.EzGateB
    };
    private void ChangeMode(ChangingRadioPresetEventArgs ev)
    {
        if (!Check(ev.Item)) return;
        mode = ev.NewValue;
        switch (ev.NewValue)
        {
            case RadioRange.Short:
                ev.Player.ShowHint("近距離(30m)探知モード");
                break;
            case RadioRange.Medium:
                ev.Player.ShowHint("中距離(60m)探知モード");
                break;
            case RadioRange.Long:
                ev.Player.ShowHint("長距離(80m)探知モード");
                break;
            case RadioRange.Ultra:
                ev.Player.ShowHint("超長距離(100m)探知モード");
                break;
        }
    }

    private void Use(DroppingItemEventArgs ev)
    {
        if (!Check(ev.Item)) return;
        Radio radio = Item.Get<Radio>(ev.Item.Base);
        if (radio == null || radio.BatteryLevel <= 0) return;
        ev.IsAllowed = false;
        switch (mode)
        {
            case RadioRange.Short:
                radio.BatteryLevel -= 10;
                break;
            case  RadioRange.Medium:
                radio.BatteryLevel -= 20;
                break;
            case RadioRange.Long:
                radio.BatteryLevel -= 30;
                break;
            case RadioRange.Ultra:
                radio.BatteryLevel -= 40;
                break;
        }
        if (radio.BattereyLevel < 0)
        {
            radio.BatteryLevel = 0;
            return;
        } 

        List<Room> detected = [];
        Vector3 playerPos = ev.Player.Position;

        foreach (var room in Room.List)
        {
            if (room == null || !uniques.Contains(room.Type)) continue;

            float distance = Vector3.Distance(playerPos, room.Position);
            switch (mode)
            {
                case RadioRange.Short when distance <= 30f:
                case RadioRange.Medium when distance <= 60f:
                case RadioRange.Long when distance <= 80f:
                case RadioRange.Ultra when distance <= 100f:
                    detected.Add(room);
                    break;
            }
        }

        if (!detected.Any())
        {
            ev.Player.ShowHint("検知された部屋なし");
            return;
        }

        // 距離でソート（近い順）
        detected = detected.OrderBy(r => Vector3.Distance(playerPos, r.Position)).ToList();

        string hint = "見つかった部屋：\n";
        foreach (var room in detected)
        {
            float dist = Vector3.Distance(playerPos, room.Position);
            hint += $"{room.Type}: {dist:F0}m\n";
        }
        ev.Player.ShowHint(hint.TrimEnd('\n'));
    }
    
    private void RemoveGlow(PickupDestroyedEventArgs ev)
    {
        if (Check(ev.Pickup))
        {
            if (ev.Pickup != null)
            {
                if (ev.Pickup?.Base?.gameObject == null) return;
                if (TryGet(ev.Pickup.Serial, out CustomItem ci) && ci != null)
                {
                    if (ev.Pickup == null || !ActiveLights.ContainsKey(ev.Pickup)) return;
                    Exiled.API.Features.Toys.Light light = ActiveLights[ev.Pickup];
                    if (light != null && light.Base != null)
                    {
                        NetworkServer.Destroy(light.Base.gameObject);
                    }
                    ActiveLights.Remove(ev.Pickup);
                }
            }
        }

    }
    private void AddGlow(PickupAddedEventArgs ev)
    {
        if (Check(ev.Pickup) && ev.Pickup.PreviousOwner != null)
        {
            if (ev.Pickup?.Base?.gameObject == null) return;
            TryGet(ev.Pickup, out CustomItem ci);
            Log.Debug($"Pickup is CI: {ev.Pickup.Serial} | {ci.Id} | {ci.Name}");

            var light = Exiled.API.Features.Toys.Light.Create(ev.Pickup.Position);
            light.Color = glowColor;

            light.Intensity = 0.7f;
            light.Range = 5f;
            light.ShadowType = LightShadows.None;

            light.Base.gameObject.transform.SetParent(ev.Pickup.Base.gameObject.transform);
            ActiveLights[ev.Pickup] = light;
        }
    }
}
