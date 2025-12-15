using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using InventorySystem.Items.MicroHID.Modules;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;
using YamlDotNet.Serialization;
using DamageHandlerBase = Exiled.API.Features.DamageHandlers.DamageHandlerBase;
using Player = Exiled.API.Features.Player;

namespace Slafight_Plugin_EXILED.CustomItems;

[CustomItem(ItemType.KeycardCustomTaskForce)]
public class DummyRoad : CustomKeycard
{
    public override uint Id { get; set; } = 1000;
    public override string Name { get; set; } = "Dummy Road Spawner";
    public override string Description { get; set; } = "What the Fuck?";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.KeycardCustomTaskForce;
    public override SpawnProperties SpawnProperties { get; set; } = new();
    public override string KeycardLabel { get; set; } = "Dummy Road Spawn Device";
    [YamlIgnore]
    public override Color32? KeycardLabelColor { get; set; } = new Color32(0,0,0,255);
    public override string KeycardName { get; set; } = "Dummy Lord";
    [YamlIgnore]
    public override Color32? TintColor { get; set; } = new Color32(0,0,0,255);
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

    private IEnumerator<float> MissileCoroutine(SchematicObject schem, Player pushPlayer)
    {
        float elapsedTime = 0f;
        float totalDuration = 0.5f;
        Vector3 startPos = schem.transform.position;
        // カメラの完全な方向（Y成分そのまま）で発射
        Vector3 cameraForward = pushPlayer.CameraTransform.forward.normalized;
        Vector3 endPos = startPos + cameraForward * 10f;  // 上・下どちらもOK
        int i = 0;
        while (elapsedTime < totalDuration)
        {
            Npc npc = Npc.Spawn("DummyRoad No. "+i,RoleTypeId.ClassD,schem.transform.position);
            i++;
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / totalDuration;
            schem.transform.position = Vector3.Lerp(startPos, endPos, progress);
            yield return 0f;
        }
        schem.Destroy();
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