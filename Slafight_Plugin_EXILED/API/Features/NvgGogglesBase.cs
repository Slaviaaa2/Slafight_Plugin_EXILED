using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Mirror;
using Slafight_Plugin_EXILED.CustomItems;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;

namespace Slafight_Plugin_EXILED.API.Features;

/// <summary>
/// NVG 系ゴーグルの自前中間基底クラス。
/// グロー管理・NvgManager 呼び出しを共通化する。
/// サブクラスは NvgProfile と GlowColor をオーバーライドするだけでよい。
/// </summary>
public abstract class NvgGogglesBase : CustomGoggles
{
    // --------------------------------------------------------
    // サブクラスが上書きする設定
    // --------------------------------------------------------

    /// <summary>アイテムごとの NVG 挙動設定。デフォルトは標準 NVG。</summary>
    protected virtual NvgProfile NvgProfile => NvgProfile.Default;

    /// <summary>落としたときのグロー色。</summary>
    protected virtual Color GlowColor => new Color(0.6f, 1f, 0.6f);

    // --------------------------------------------------------
    // 内部状態
    // --------------------------------------------------------

    private readonly Dictionary<Pickup, Light> _activeLights = new();

    // --------------------------------------------------------
    // イベント購読
    // --------------------------------------------------------

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded     += OnPickupAdded;
        Exiled.Events.Handlers.Map.PickupDestroyed += OnPickupDestroyed;
        base.SubscribeEvents(); // CustomGoggles のイベントも必ず呼ぶ
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded     -= OnPickupAdded;
        Exiled.Events.Handlers.Map.PickupDestroyed -= OnPickupDestroyed;
        base.UnsubscribeEvents();
    }

    // --------------------------------------------------------
    // 装着 / 取り外し（CustomGoggles から呼ばれる）
    // --------------------------------------------------------

    protected override void OnWornGoggles(Player player, Scp1344 goggles)
    {
        if (player == null) return;
        Log.Debug($"[{Name}] OnWornGoggles: {player.Nickname} serial={goggles.Serial}");
        NvgManager.StartNvg(player, goggles.Serial, NvgProfile);
    }

    protected override void OnRemovedGoggles(Player player, Scp1344 goggles)
    {
        if (player == null) return;
        Log.Debug($"[{Name}] OnRemovedGoggles: {player.Nickname} serial={goggles.Serial}");
        NvgManager.StopNvg(player, goggles.Serial);
    }

    // --------------------------------------------------------
    // グロー管理
    // --------------------------------------------------------

    private void OnPickupAdded(PickupAddedEventArgs ev)
    {
        if (ev?.Pickup == null) return;
        if (!Check(ev.Pickup)) return;
        if (ev.Pickup.PreviousOwner == null) return;
        if (ev.Pickup.Base?.gameObject == null) return;
        if (!TryGet(ev.Pickup, out var ci) || ci == null) return;

        try
        {
            var light = Light.Create(ev.Pickup.Position);
            if (light == null) return;

            light.Color      = GlowColor;
            light.Intensity  = 0.7f;
            light.Range      = 5f;
            light.ShadowType = LightShadows.None;

            light.Base.gameObject.transform.SetParent(ev.Pickup.Base.gameObject.transform);

            _activeLights[ev.Pickup] = light;
        }
        catch (Exception ex)
        {
            Log.Error($"[{Name}] AddGlow エラー: {ex.Message}");
        }
    }

    private void OnPickupDestroyed(PickupDestroyedEventArgs ev)
    {
        if (ev?.Pickup == null) return;
        if (!Check(ev.Pickup)) return;
        if (ev.Pickup.Base?.gameObject == null) return;
        if (!TryGet(ev.Pickup.Serial, out var ci) || ci == null) return;
        if (!_activeLights.TryGetValue(ev.Pickup, out var light)) return;

        if (light?.Base != null)
        {
            try { NetworkServer.Destroy(light.Base.gameObject); }
            catch (Exception ex) { Log.Warn($"[{Name}] RemoveGlow Destroy 失敗: {ex.Message}"); }
        }

        _activeLights.Remove(ev.Pickup);
    }
}