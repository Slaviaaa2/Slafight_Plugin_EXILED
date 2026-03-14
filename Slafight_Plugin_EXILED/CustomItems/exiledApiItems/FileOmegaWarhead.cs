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
public class FileOmegaWarhead : CustomKeycard
{
    public override uint Id { get; set; } = 2023;
    public override string Name { get; set; } = "File-Warhead-OMEGA";
    public override string Description { get; set; } = "";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.KeycardCustomSite02;
    public override SpawnProperties SpawnProperties { get; set; } = new();
    public override string KeycardLabel { get; set; } = "File-Warhead-OMEGA";
    [YamlIgnore]
    public override Color32? KeycardLabelColor { get; set; } = new Color32(255,255,255,255);
    public override string KeycardName { get; set; } = "Dr. Aqurista Ω Boom";
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
            "<size=14>OMEGA WARHEAD取扱説明書\n" +
            "この度は私のOMEGA WARHEAD建造計画に賛同いただき、誠に...\n" +
            "ええい、こんな物に前書きなどいらん。そうだろう？\n" +
            "兎に角だな、この私の最高傑作の弾頭、<color=blue><b>OMEGA WARHEAD</b></color>を！\n" +
            "何処の馬の骨かもわからん奴に向けて説明する私の事を思って！\n" +
            "よーーーく読み込んでおくことだな！\n" +
            "この弾頭はまず、エンジニアの協力が作動には必要だ。\n" +
            "何故かって？そんなん、セキュリティ上の理由以外何がある！\n" +
            "えーそしたら、地上のOMEGA WARHEADサイロに行ってサイロを開けてもらえ。\n" +
            "...おっと、一番重要な奴も忘れていた。\n" +
            "O5評議会から承認を必ず取り付けるように。じゃないと制御ボタンが開かんからな！\n" +
            "承認があれば、弾頭の制御室でボタンを使ってようやく起動できるぞ！\n" +
            "ま、俺以外の職員に使わせる気なんてないけどな！！！！！\n" +
            "それじゃあ、グッドラック。\n" +
            "- Dr. Aqurista Ω Boom</size>");
        
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