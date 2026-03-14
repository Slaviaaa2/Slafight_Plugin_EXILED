using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.EventArgs;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Mirror;
using Scp914;
using UnityEngine;
using Item = Exiled.API.Features.Items.Item;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

[CustomItem(ItemType.Radio)]
public class SNAV300 : CustomItem
{
    public override uint Id { get; set; } = 2012;
    public override string Name { get; set; } = "S-Nav 300";
    public override string Description { get; set; } = "近くのユニークな部屋について調べられる。\n投げて使用可能";
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
        RoomType.Hcz127,
        RoomType.HczCrossRoomWater,
        RoomType.HczHid,
        RoomType.HczNuke,
        RoomType.EzIntercom,
        RoomType.EzGateA,
        RoomType.EzGateB
    };
    private void ChangeMode(ChangingRadioPresetEventArgs ev)
    {
        if (ev.Radio.BatteryLevel < 10) return;
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

        float consumption = mode switch
        {
            RadioRange.Short => 10f,
            RadioRange.Medium => 20f,
            RadioRange.Long => 30f,
            RadioRange.Ultra => 40f,
            _ => 40f
        };

        if (radio.BatteryLevel < consumption)
        {
            ev.Player.ShowHint("バッテリー不足！", 3f);
            ev.IsAllowed = false;
            return;
        }

        radio.BatteryLevel -= (byte)consumption;
    
        Vector3 playerPos = ev.Player.Position;
        List<Room> detected = [];
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
        detected.Sort((a, b) => Vector3.Distance(playerPos, a.Position).CompareTo(Vector3.Distance(playerPos, b.Position)));

        string hint = detected.Any()
            ? $"[{mode}]見つかった部屋：\n" + string.Join("\n", detected.Select(r => $"{r.Type}: {Vector3.Distance(playerPos, r.Position):F0}m"))
            : "検知された部屋なし";
        ev.Player.ShowHint(hint, 10f);

        ev.IsAllowed = false;
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
