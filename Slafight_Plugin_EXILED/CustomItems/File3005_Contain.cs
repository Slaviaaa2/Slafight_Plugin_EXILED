using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.EventArgs;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;
using Exiled.Events.Handlers;
using InventorySystem.Items.FlamingoTapePlayer;
using InventorySystem.Items.MicroHID.Modules;
using MEC;
using Mirror;
using PlayerStatsSystem;
using Scp914;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using YamlDotNet.Serialization;
using Item = Exiled.API.Features.Items.Item;
using Player = Exiled.API.Features.Player;

namespace Slafight_Plugin_EXILED.CustomItems;

[CustomItem(ItemType.KeycardCustomSite02)]
public class File3005_Contain : CustomKeycard
{
    public override uint Id { get; set; } = 2022;
    public override string Name { get; set; } = "File-3005-Contain";
    public override string Description { get; set; } = "";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.KeycardCustomSite02;
    public override SpawnProperties SpawnProperties { get; set; } = new();
    public override string KeycardLabel { get; set; } = "File-3005-Contain";
    [YamlIgnore]
    public override Color32? KeycardLabelColor { get; set; } = new Color32(255,255,255,255);
    public override string KeycardName { get; set; } = "Site-55■■ Facility Manager";
    [YamlIgnore]
    public override Color32? TintColor { get; set; } = new Color32(0,0,0,255);
    [YamlIgnore]
    public override Color32? KeycardPermissionsColor { get; set; } = new Color32(0,0,0,255);

    public override KeycardPermissions Permissions { get; set; } =
        KeycardPermissions.None;

    public override byte Rank { get; set; } = 1;
    public override string SerialNumber { get; set; } = "";

    public Color glowColor = Color.white;
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> ActiveLights = [];

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded += AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;
        base.UnsubscribeEvents();
    }

    protected override void OnPickingUp(PickingUpItemEventArgs ev)
    {
        ev.IsAllowed = false;
        ev.Player.ShowHint(
            "<size=15>SCP-3005のSite-02への移管について\n" +
            "以前から予定されていた通り、当Site-55■■では手に負えない程\n" +
            "SCP-3005の反ミーム性質が強まってきてしまっています。\n" +
            "その為、先日のO5評議会にてSite-02への移管が決定されました。\n" +
            "収容コンテナの輸送は20■■年5月5日に完了する予定です。よろしくお願します。\n" +
            "- Site-55■■ Facility Manager</size>");
        
        base.OnPickingUp(ev);
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