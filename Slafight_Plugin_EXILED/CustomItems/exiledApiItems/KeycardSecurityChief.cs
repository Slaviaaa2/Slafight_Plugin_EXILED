using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.EventArgs;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Mirror;
using Scp914;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

[CustomItem(ItemType.KeycardCustomMetalCase)]
public class KeycardSecurityChief : CustomKeycard
{
    public override uint Id { get; set; } = 1100;
    public override string Name { get; set; } = "警備長キーカード";
    public override string Description { get; set; } = "警備隊を指揮したりする警備長が持つキーカード。";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.KeycardCustomMetalCase;
    public override SpawnProperties SpawnProperties { get; set; } = new();
    public override string KeycardLabel { get; set; } = "警備主任キーカード";
    [YamlIgnore]
    public override Color32? KeycardLabelColor { get; set; } = new Color32(255,255,255,255);
    public override string KeycardName { get; set; } = "Chf. Security";
    [YamlIgnore]
    public override Color32? TintColor { get; set; } = new Color32(68,68,68,255);
    [YamlIgnore]
    public override Color32? KeycardPermissionsColor { get; set; } = new Color32(0,0,0,255);

    public override KeycardPermissions Permissions { get; set; } =
        KeycardPermissions.ContainmentLevelOne |
        KeycardPermissions.ArmoryLevelOne |
        KeycardPermissions.ArmoryLevelTwo |
        KeycardPermissions.Intercom |
        KeycardPermissions.Checkpoints |
        KeycardPermissions.ExitGates;

    public override byte Rank { get; set; } = 1;
    public override string SerialNumber { get; set; } = "";

    public Color glowColor = Color.green;
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

    protected override void OnUpgrading(UpgradingEventArgs ev)
    {
        switch (ev.KnobSetting)
        {
            case Scp914KnobSetting.Rough:
                break;
            case Scp914KnobSetting.Coarse:
                Pickup.CreateAndSpawn(ItemType.KeycardGuard, ev.OutputPosition);
                break;
            case Scp914KnobSetting.OneToOne:
                CustomItemExtensions.TrySpawn<KeycardSecurityChief>(ev.OutputPosition, out _);
                break;
            case Scp914KnobSetting.Fine:
                Pickup.CreateAndSpawn(ItemType.KeycardMTFOperative, ev.OutputPosition);
                break;
            case Scp914KnobSetting.VeryFine:    
                Pickup.CreateAndSpawn(ItemType.KeycardMTFCaptain, ev.OutputPosition);
                break;
        }

        ev.IsAllowed = false;
        ev.Item.DestroySelf();
        base.OnUpgrading(ev);
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