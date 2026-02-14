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
public class File096_777_A : CustomKeycard
{
    public override uint Id { get; set; } = 2021;
    public override string Name { get; set; } = "File-096-777-A";
    public override string Description { get; set; } = "";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.KeycardCustomSite02;
    public override SpawnProperties SpawnProperties { get; set; } = new();
    public override string KeycardLabel { get; set; } = "File-096-777-A";
    [YamlIgnore]
    public override Color32? KeycardLabelColor { get; set; } = new Color32(255,255,255,255);
    public override string KeycardName { get; set; } = "Dr. ■■■, Dr. Redheart";
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
            "<size=14>事案096-777-A\n" +
            "20■■年■月■日、定期観察の過程で、■■■■■■■博士がSCP-096に対するセンサー監視を実施していました。\n" +
            "観察中、SCP-096は一切の予兆なく激しい激昂行動を開始し、施設全域で感知可能なレベルの悲鳴を発しました。\n" +
            "■■■博士は直ちに緊急アラームを作動させましたが、警備要員が現場に到着した時点で確認されたのは、多量の血痕のみでした。\n" +
            "SCP-096はその場でうずくまった姿勢を維持しており、当該姿勢は既知の激昂行動時のものと一致していました。\n" +
            "本事案は形式上は収束していますが、既知のトリガー事象が一切確認されていないにもかかわらずSCP-096が不安定化した可能性が高く、\n" +
            "現行の収容・観察プロトコルが抜本的な見直しを要する段階に来ていると判断せざるを得ません。\n" +
            "ついては、Site-02への■■■■■■■■システムの早急な導入を含む、SCP-096関連プロトコルの全面的な再評価および是正措置について、\n" +
            "本書をもって強く要請すると同時に、これ以上の対応遅延が重大な人的・情報的損失を招くおそれがあることを、ここに重ねて申し上げます。\n" +
            "- Dr. ■■■, Dr. Redheart</size>");
        
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