using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Mirror;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

[CustomItem(ItemType.KeycardCustomSite02)]
public class File012_5_033 : CustomKeycard
{
    public override uint Id { get; set; } = 2015;
    public override string Name { get; set; } = "File-012-R033";
    public override string Description { get; set; } = "";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.KeycardCustomSite02;
    public override SpawnProperties SpawnProperties { get; set; } = new();
    public override string KeycardLabel { get; set; } = "File-012-R033";
    [YamlIgnore]
    public override Color32? KeycardLabelColor { get; set; } = new Color32(255,255,255,255);
    public override string KeycardName { get; set; } = "Dr. Redheart";
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
        ev.Player.ShowHint("<size=15>事案012-5-033\n" +
                           "20■■年■月■日" +
                           "の定期調査にて、SCP-012がSCP-033と思われる力に侵食されてしまっていることが判明した。\n" +
                           "以前までは何事も無かったのに、急激にマゼンタ色を発し始め\n" +
                           "周囲にSCP-012影響ではなく、SCP-033影響を与えてしまっている。\n" +
                           "実に由々しき事態だ。\n" +
                           "これについて、私は本件についての大規模な調査及び対処を、強く求める。\n" +
                           "- Dr. Redheart</size>");
        
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