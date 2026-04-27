using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using MEC;
using Mirror;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.CustomRoles.Others.SergeyMakarov;
using UnityEngine;
using YamlDotNet.Serialization;
using Player = Exiled.API.Features.Player;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

[CustomItem(ItemType.KeycardCustomTaskForce)]
public class MagicMissile : CustomKeycard
{
    public override uint Id { get; set; } = 666;
    public override string Name { get; set; } = "第五教会 マジックミサイル";
    public override string Description { get; set; } = "非常に第五的なマジックミサイルを発射する。";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.KeycardCustomTaskForce;
    public override SpawnProperties SpawnProperties { get; set; } = new();
    public override string KeycardLabel { get; set; } = "マジックミサイル";
    [YamlIgnore]
    public override Color32? KeycardLabelColor { get; set; } = new Color32(255,0,250,255);
    public override string KeycardName { get; set; } = "Mgc. Fifth";
    [YamlIgnore]
    public override Color32? TintColor { get; set; } = new Color32(255,0,250,255);
    [YamlIgnore]
    public override Color32? KeycardPermissionsColor { get; set; } = new Color32(255,255,255,255);

    public override KeycardPermissions Permissions { get; set; } =
        KeycardPermissions.None;

    public override byte Rank { get; set; } = 1;
    public override string SerialNumber { get; set; } = "555555555555";

    public Color glowColor = Color.magenta;
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> ActiveLights = [];

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded += AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;

        Exiled.Events.Handlers.Player.DroppingItem += StartMagicMissile;
        
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;

        Exiled.Events.Handlers.Player.DroppingItem -= StartMagicMissile;
        
        base.UnsubscribeEvents();
    }

    private void StartMagicMissile(DroppingItemEventArgs ev)
    {
        if (Check(ev.Item))
        {
            ev.IsAllowed = false;
            ev.Item.Destroy();
            Vector3 startPos = ev.Player.Position;
            SchematicObject schematicObject;
            try
            {
                schematicObject = ObjectSpawner.SpawnSchematic("SCP3005",startPos,ev.Player.CameraTransform.forward);
                Timing.RunCoroutine(MissileCoroutine(schematicObject,ev.Player));
            }
            catch (Exception ex)
            {
                Log.Error("Stupid Null Error.");
                schematicObject = null;
                return;
            }
        }
    }

    private static IEnumerator<float> MissileCoroutine(SchematicObject schem, Player pushPlayer)
    {
        // 開始時点チェック
        if (schem == null || schem.transform == null)
        {
            Log.Warn("[MagicMissileAbility] MissileCoroutine aborted: schem or transform is null at start.");
            yield break;
        }

        float elapsedTime = 0f;
        const float totalDuration = 0.8f;

        Vector3 startPos = schem.transform.position;
        Vector3 cameraForward = pushPlayer != null
            ? pushPlayer.CameraTransform.forward.normalized
            : Vector3.forward;
        Vector3 endPos = startPos + cameraForward * 25f + new Vector3(0f, 0.15f, 0f);

        while (elapsedTime < totalDuration)
        {
            // ラウンド状態
            if (Round.IsLobby || Round.IsEnded)
            {
                Log.Info("[MagicMissileAbility] MissileCoroutine stopped: round lobby/ended.");
                yield break;
            }

            // Schematic 消滅
            if (schem == null || schem.transform == null)
            {
                Log.Warn("[MagicMissileAbility] MissileCoroutine stopped: schem destroyed.");
                yield break;
            }

            // 発射主 disconnect
            if (pushPlayer != null && !pushPlayer.IsConnected)
            {
                Log.Info("[MagicMissileAbility] MissileCoroutine stopped: owner disconnected.");
                yield break;
            }

            // 当たり判定
            foreach (var player in Player.List)
            {
                if (player == null || !player.IsConnected || !player.IsAlive)
                    continue;

                if (!(Vector3.Distance(schem.transform.position, player.Transform.position) <= 1f)) continue;
                if (player == pushPlayer) continue;
                try
                {
                    player.EnableEffect<Burned>(255);
                    player.EnableEffect<Concussed>(255);
                    player.EnableEffect<Asphyxiated>(1);
                    player.Hurt(pushPlayer, 10f, DamageType.Unknown, null,
                        !pushPlayer.IsSergeyMarkov()
                            ? "<color=#ff00fa>第五的</color>な力による影響"
                            : "<color=red><b>怨念的</b></color>な力による影響");
                    pushPlayer?.ShowHitMarker();
                }
                catch (Exception ex)
                {
                    Log.Error($"[MagicMissileAbility] Hurt error: {ex}");
                }
            }

            // 移動
            elapsedTime += Time.deltaTime;
            var progress = elapsedTime / totalDuration;
            schem.transform.position = Vector3.Lerp(startPos, endPos, progress);

            yield return 0f;
        }

        if (schem != null)
        {
            try
            {
                schem.Destroy();
            }
            catch (Exception ex)
            {
                Log.Error($"[MagicMissileAbility] Error destroying schem: {ex}");
            }
        }
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