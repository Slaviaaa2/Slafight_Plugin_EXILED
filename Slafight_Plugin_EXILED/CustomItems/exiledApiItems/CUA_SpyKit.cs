using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Mirror;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

[CustomItem(ItemType.KeycardCustomSite02)]
public class CUA_SpyKit : CustomKeycard
{
    public override uint Id { get; set; } = 2034;
    public override string Name { get; set; } = "CUA式スパイキット";
    public override string Description { get; set; } = "カオスの潜入工作員が持つ、潜入任務用変装セット。\nTキーでDクラス、Iキーでカオスに見た目を切り替えられる";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.KeycardCustomSite02;
    public override SpawnProperties SpawnProperties { get; set; } = new();
    public override string KeycardLabel { get; set; } = "CUA. SpyKit";
    [YamlIgnore]
    public override Color32? KeycardLabelColor { get; set; } = new Color32(255,255,255,255);
    public override string KeycardName { get; set; } = "Chaos Insurgency";
    [YamlIgnore]
    public override Color32? TintColor { get; set; } = new Color32(0,68,0,255);
    [YamlIgnore]
    public override Color32? KeycardPermissionsColor { get; set; } = new Color32(0,0,0,255);

    public override KeycardPermissions Permissions { get; set; } = KeycardPermissions.None;

    public override byte Rank { get; set; } = 2;
    public override string SerialNumber { get; set; } = "";

    public Color glowColor = CustomColor.ChaoticGreen.ToUnityColor();
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> ActiveLights = [];

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Item.InspectingItem += OnInteresting;
        
        Exiled.Events.Handlers.Map.PickupAdded += AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;
        
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Item.InspectingItem -= OnInteresting;
        
        Exiled.Events.Handlers.Map.PickupAdded -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;
        
        base.UnsubscribeEvents();
    }

    protected override void OnDroppingItem(DroppingItemEventArgs ev)
    {
        if (!ev.IsThrown)
        {
            ev.Player.ChangeAppearance(RoleTypeId.ChaosRifleman, Player.List.Where(player => player != null).ToList());
            return;
        }
        ev.Player?.ChangeAppearance(RoleTypeId.ClassD, Player.List.Where(player => player != null).ToList());
        ev.Player?.ShowHint("<size=24><color=#EE7760>Class-D Personnel</color>に変装しました", 2.5f);
        ev.IsAllowed = false;
        base.OnDroppingItem(ev);
    }

    private void OnInteresting(InspectingItemEventArgs ev)
    {
        if (!Check(ev.Item)) return;
        ev.Player?.ChangeAppearance(RoleTypeId.ChaosRifleman, Player.List.Where(player => player != null).ToList());
        ev.Player?.ShowHint("<size=24><color=#228B22>Chaos Insurgency Rifleman</color>に変装しました", 2.5f);
        ev.IsAllowed = false;
    }

    private void RemoveGlow(PickupDestroyedEventArgs ev)
    {
        if (Check(ev.Pickup))
        {
            if (ev.Pickup != null)
            {
                if (ev.Pickup?.Base?.gameObject == null) return;
                if (TryGet(ev.Pickup.Serial, out var ci) && ci != null)
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