using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using MEC;
using Mirror;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

[CustomItem(ItemType.KeycardO5)]
public class KeycardOld_O5 : CustomItem
{
    public override uint Id { get; set; } = 110;
    public override string Name { get; set; } = "O5キーカード";
    public override string Description { get; set; } = "説明無し";
    public override float Weight { get; set; } = 1f;
    public override ItemType Type { get; set; } = ItemType.KeycardO5;
    public override SpawnProperties SpawnProperties { get; set; } = new();
    public override Vector3 Scale { get; set; } = new Vector3(1.25f, 1.25f, 1.25f);
    public string SchemName = "OldKeycard_O5";

    public Color glowColor = new Color(186f,140f,132f);
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> ActiveLights = [];

    protected override void SubscribeEvents()
    {
        //Exiled.Events.Handlers.Map.PickupAdded += AddGlow;
        //Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;
        
        Exiled.Events.Handlers.Map.PickupAdded += AddSchem;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveSchem;
        
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;
        
        Exiled.Events.Handlers.Map.PickupAdded -= AddSchem;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveSchem;
        
        base.UnsubscribeEvents();
    }

    //private void PickMessage(PickingUpItemEventArgs ev)
    //{
    //    ev.Player.ShowHint("あなたはH.I.D. Turretを拾いました！\nこのH.I.D.は、小チャージのみ使用可能で、無限に撃つことが出来ます！\nただしダメージは低いので慢心しないように！");
    //}
    private void RemoveSchem(PickupDestroyedEventArgs ev)
    {
        if (Check(ev.Pickup))
        {
            if (ev.Pickup != null)
            {
                if (ev.Pickup?.Base?.gameObject == null) return;
                if (TryGet(ev.Pickup.Serial, out CustomItem ci) && ci != null)
                {
                    SchematicObject schem = ev.Pickup.GameObject.GetComponentInChildren<SchematicObject>();
                    schem.Destroy();
                }
            }
        }

    }
    private void AddSchem(PickupAddedEventArgs ev)
    {
        if (Check(ev.Pickup))
        {
            ev.Pickup.GameObject.transform.localScale = Scale;
            SchematicObject schematicObject;
            try
            {
                schematicObject = ObjectSpawner.SpawnSchematic(SchemName,Vector3.zero);
            }
            catch (Exception ex)
            {
                Logger.Error("error schem");
                schematicObject = null;
                return;
            }
            schematicObject.transform.SetParent(ev.Pickup.GameObject.transform);
            Timing.CallDelayed(0.01f, () =>
            {
                //schematicObject.transform.GetChild(0).localScale = new Vector3(1000f,1f,1000f); // 沼みたいで何かに使えそう
                schematicObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                schematicObject.transform.position = ev.Pickup.GameObject.transform.position += new Vector3(0f, 0f, 0f);
                schematicObject.transform.rotation = ev.Pickup.GameObject.transform.rotation;
                schematicObject.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
            });
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