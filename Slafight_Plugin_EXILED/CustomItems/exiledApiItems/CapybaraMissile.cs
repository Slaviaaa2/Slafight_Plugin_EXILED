using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.API.Features.Toys;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using MEC;
using Mirror;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;
using YamlDotNet.Serialization;
using Player = Exiled.API.Features.Player;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

[CustomItem(ItemType.GunCOM18)]
public class CapybaraMissile : CustomWeapon
{
    public override uint Id { get; set; } = 2041;
    public override string Name { get; set; } = "I love NW";
    public override string Description { get; set; } = "HUBERT YEAAAAAAAAA";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.GunCOM18;
    public override SpawnProperties SpawnProperties { get; set; } = new();
    public override float Damage { get; set; } = 0f;
    public override byte ClipSize { get; set; } = 5;
    public override Vector3 Scale { get; set; } = Vector3.one * 5f;

    private Color _glowColor = Color.white;
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> ActiveLights = [];
    
    private static Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =>
        EventHandler.CreateAndPlayAudio;

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

    protected override void OnShot(ShotEventArgs ev)
    {
        var startPos = ev.Player.Position;
        try
        {
            var cameraForward = ev.Player != null ? ev.Player.CameraTransform.forward.normalized : Vector3.forward;
            if (ev.Player != null)
            {
                var schematicObject = Capybara.Create(startPos + cameraForward * 1.5f, Quaternion.Euler(ev.Player.CameraTransform.forward));
                schematicObject.Collidable = false;
                Timing.RunCoroutine(MissileCoroutine(schematicObject,ev.Player));
            }
        }
        catch (Exception ex)
        {
            Log.Error("Stupid Null Error.");
            return;
        }
        base.OnShot(ev);
    }

    private IEnumerator<float> MissileCoroutine(Capybara schem, Player pushPlayer)
    {
        if (schem == null || schem.Transform == null)
        {
            Log.Warn("[MagicMissile] MissileCoroutine aborted: schem or transform is null at start.");
            yield break;
        }

        float elapsedTime = 0f;
        const float totalDuration = 5f;
        const float rotationSpeed = 1440f; // 1秒2回転

        Vector3 startPos = schem.Transform.position;
        Vector3 cameraForward = pushPlayer != null ? pushPlayer.CameraTransform.forward.normalized : Vector3.forward;
        Vector3 endPos = startPos + cameraForward * 99f;

        while (elapsedTime < totalDuration)
        {
            if (Round.IsLobby || Round.IsEnded)
                yield break;

            if (schem == null || schem.Transform == null)
                yield break;

            if (pushPlayer != null && !pushPlayer.IsConnected)
                yield break;

            // 当たり判定
            foreach (var player in Player.List)
            {
                if (player == null || !player.IsConnected || !player.IsAlive)
                    continue;

                if (Vector3.Distance(schem.Transform.position, player.Transform.position) <= 1f)
                {
                    try
                    {
                        player.Hurt(pushPlayer, 999f, DamageType.Unknown);
                        pushPlayer?.ShowHitMarker();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[CapybaraMissile] Attack error: {ex}");
                    }
                }
            }

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / totalDuration;

            // 位置更新：まっすぐ飛ぶ
            schem.Transform.position = Vector3.Lerp(startPos, endPos, progress);

            // 回転更新：ワールドY軸でコマのようにくるくる
            float spinAngle = rotationSpeed * elapsedTime;
            schem.Transform.rotation = Quaternion.Euler(0f, spinAngle, 0f);

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
                Log.Error($"[MagicMissile] Error destroying schem: {ex}");
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
            light.Color = _glowColor;

            light.Intensity = 0.7f;
            light.Range = 5f;
            light.ShadowType = LightShadows.None;

            light.Base.gameObject.transform.SetParent(ev.Pickup.Base.gameObject.transform);
            ActiveLights[ev.Pickup] = light;
        }
    }
}