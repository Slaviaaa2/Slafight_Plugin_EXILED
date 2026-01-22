using System;
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
public class SNAVUltimate : CustomItem
{
    public override uint Id { get; set; } = 2014;
    public override string Name { get; set; } = "S-Nav Ultimate";
    public override string Description { get; set; } = "SCP-914によって改良されたS-Nav。\n電池不要かつマップが大幅に拡張されており、SCPの情報も得られる。\nより多くの、近くのユニークな部屋について調べられる。\n投げて使用可能";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.Radio;

    public Color glowColor = Color.blue;
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
    
    protected override void OnUpgrading(UpgradingEventArgs ev)
    {
        if (ev.KnobSetting == Scp914KnobSetting.OneToOne)
        {
            CustomItem.TrySpawn(2012, ev.OutputPosition, out _);
        }
        else if (ev.KnobSetting == Scp914KnobSetting.Fine)
        {
            CustomItem.TrySpawn(2013, ev.OutputPosition, out _);
        }
        else if (ev.KnobSetting == Scp914KnobSetting.VeryFine)
        {
            CustomItem.TrySpawn(2014, ev.OutputPosition, out _);
        }

        ev.IsAllowed = false;
        ev.Item.DestroySelf();
        base.OnUpgrading(ev);
    }

    private RadioRange mode;
    private List<RoomType> uniques = new()
    {
        RoomType.Lcz914,
        RoomType.Lcz330,
        RoomType.LczGlassBox,
        RoomType.LczArmory,
        RoomType.LczCheckpointA,
        RoomType.LczCheckpointB,
        RoomType.Hcz127,
        RoomType.HczCrossRoomWater,
        RoomType.HczHid,
        RoomType.HczNuke,
        RoomType.HczTestRoom,
        RoomType.Hcz049,
        RoomType.Hcz106,
        RoomType.HczElevatorA,
        RoomType.HczElevatorB,
        RoomType.HczEzCheckpointA,
        RoomType.HczEzCheckpointB,
        RoomType.HczTesla,
        RoomType.EzIntercom,
        RoomType.EzDownstairsPcs,
        RoomType.EzPcs,
        RoomType.EzSmallrooms,
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
        if (!ev.IsThrown) return;
        if (!Check(ev.Item)) return;

        Radio radio = Item.Get<Radio>(ev.Item.Base);
        if (radio == null) return;

        ev.IsAllowed = false;
        Vector3 playerPos = ev.Player.Position;

        // 部屋検知
        List<Room> detectedRooms = GetDetectedRooms(playerPos, mode);

        // SCP検知
        List<Exiled.API.Features.Player> detectedScps = GetDetectedScps(playerPos, mode);

        string roomsText = detectedRooms.Any()
            ? string.Join("\n", detectedRooms.Select(r =>
                $"{r.Type}: {Vector3.Distance(playerPos, r.Position):F0}m"))
            : "なし";

        string scpsText = detectedScps.Any()
            ? string.Join("\n", detectedScps.Select(p =>
                $"{p.Nickname} ({p.Role.Type}): {Vector3.Distance(playerPos, p.Position):F0}m"))
            : "なし";

        string hint =
            $"[{mode}]検知された部屋：\n{roomsText}\n\n" +
            $"検知されたSCP：\n{scpsText}";

        ev.Player.ShowHint(hint, 10f);
    }
    
    private List<Exiled.API.Features.Player> GetDetectedScps(Vector3 playerPos, RadioRange currentMode)
    {
        float range = currentMode switch
        {
            RadioRange.Short  => 30f,
            RadioRange.Medium => 60f,
            RadioRange.Long   => 80f,
            RadioRange.Ultra  => 100f,
            _ => 0f
        };

        if (range <= 0f)
            return new List<Exiled.API.Features.Player>();

        return Exiled.API.Features.Player.List
            .Where(p =>
                p != null &&
                p.IsAlive &&
                p.GetTeam() == CTeam.SCPs &&
                Vector3.Distance(playerPos, p.Position) <= range)
            .OrderBy(p => Vector3.Distance(playerPos, p.Position))
            .ToList();
    }

    // 新規追加：検知メソッド（元のforeachを関数化）
    private List<Room> GetDetectedRooms(Vector3 playerPos, RadioRange currentMode)
    {
        List<Room> detected = [];
        foreach (var room in Room.List)
        {
            if (room == null || !uniques.Contains(room.Type)) continue;

            float distance = Vector3.Distance(playerPos, room.Position);
            bool withinRange = currentMode switch
            {
                RadioRange.Short => distance <= 30f,
                RadioRange.Medium => distance <= 60f,
                RadioRange.Long => distance <= 80f,
                RadioRange.Ultra => distance <= 100f,
                _ => false
            };
        
            if (withinRange)
                detected.Add(room);
        }
    
        // 距離順ソート
        return detected.OrderBy(r => Vector3.Distance(playerPos, r.Position)).ToList();
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
