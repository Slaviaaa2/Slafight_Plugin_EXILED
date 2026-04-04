using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.EventArgs;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using MEC;
using Mirror;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

[CustomItem(ItemType.Medkit)]
public class Scp1425 : CustomItem
{
    public override uint Id { get; set; } = 1102;
    public override string Name { get; set; } = "SCP-1425";
    public override string Description { get; set; } = "第五的な力を感じる・・・";
    public override float Weight { get; set; } = 1.05f;
    public override ItemType Type { get; set; } = ItemType.Medkit;

    private Color _glowColor = Color.magenta;
    private Dictionary<Exiled.API.Features.Pickups.Pickup, Exiled.API.Features.Toys.Light> _activeLights = [];
    private Dictionary<int, byte> _readCount = [];

    public override SpawnProperties SpawnProperties { get; set; } = new();

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem += OnUsed;
        Exiled.Events.Handlers.Player.Dying += OnDying;
        
        Exiled.Events.Handlers.Map.PickupAdded += AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed += RemoveGlow;
        
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem -= OnUsed;
        Exiled.Events.Handlers.Player.Dying -= OnDying;
        
        Exiled.Events.Handlers.Map.PickupAdded -= AddGlow;
        Exiled.Events.Handlers.Map.PickupDestroyed -= RemoveGlow;
        
        base.UnsubscribeEvents();
    }

    protected override void OnWaitingForPlayers()
    {
        _readCount.Clear();
        base.OnWaitingForPlayers();
    }

    private void OnDying(DyingEventArgs ev)
    {
        if (ev.Player == null) return;
        _readCount[ev.Player.Id] = 0;
    }

    private void OnUsed(UsedItemEventArgs ev)
    {
        if (!Check(ev.Item) || ev.Player == null) return;

        // キーがなくても 0 を返す（結果は byte にキャスト）
        var count = _readCount.GetValueOrDefault(ev.Player.Id, (byte)0);

        switch (count)
        {
            case 0:
                ev.Player.ShowHint("<size=22>1ページ目\n壊れた星の五本の輻</size>", 5f);
                break;
            case 1:
                ev.Player.ShowHint("<size=22>2ページ目\n永遠に争う五つの元素</size>", 5f);
                break;
            case 2:
                ev.Player.ShowHint($"<size=22>3ページ目\n<color={CTeam.Fifthists.GetTeamColor()}>精神を呼び起こす五つの感覚</color></size>", 5f);
                ev.Player.EnableEffect<Flashed>(3f);
                Timing.CallDelayed(2.5f,
                    () => ev.Player?.SetRole(CRoleTypeId.FifthistConvert, RoleSpawnFlags.AssignInventory));
                break;
            case 3:
                ev.Player.ShowHint("<size=22>4ページ目\n五つの欠片が緩慢に解かれてゆく</size>", 5f);
                break;
            case 4:
                ev.Player.ShowHint($"<size=22>5ページ目\n<b><color={CTeam.Fifthists.GetTeamColor()}>咆哮する黒き月は、見るに値しない幻である</color></b></size>", 5f);
                ev.Player.EnableEffect<Flashed>(3f);
                Timing.CallDelayed(2.5f,
                    () => ev.Player?.SetRole(CRoleTypeId.FifthistMarionette, RoleSpawnFlags.AssignInventory));
                break;
            default:
                // 万が一範囲外の値が入っても 0 にリセットする目安
                _readCount[ev.Player.Id] = 0;
                return;
        }

        // 5ページ読みきったらリセット
        if (count >= 4)
        {
            _readCount[ev.Player.Id] = 0;
        }
        else
        {
            _readCount[ev.Player.Id] = (byte)(count + 1);
        }

        Timing.CallDelayed(0.1f, () => ev.Player?.TryAddCustomItem<Scp1425>());
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
                    if (ev.Pickup == null || !_activeLights.ContainsKey(ev.Pickup)) return;
                    Exiled.API.Features.Toys.Light light = _activeLights[ev.Pickup];
                    if (light != null && light.Base != null)
                    {
                        NetworkServer.Destroy(light.Base.gameObject);
                    }
                    _activeLights.Remove(ev.Pickup);
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
            _activeLights[ev.Pickup] = light;
        }
    }
}
