#nullable enable
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;

using PlayerEvents = Exiled.Events.EventArgs.Player;
using MapEvents = Exiled.Events.EventArgs.Map;
using Scp914Events = Exiled.Events.EventArgs.Scp914;

namespace Slafight_Plugin_EXILED.API.Features;

/// <summary>
/// 複数の CItem を「モード」として持ち、G キー（ServerSpecifics ID 5）で切り替えられる
/// 仮想カスタムアイテム基底クラス。
/// 各 SubMode は独立した CItem であり、スタンドアロン利用も可能。
/// モード切替は serial ごとの dispatch先差し替えで実現し、CItem の
/// イベント購読・自動登録の仕組みとは完全に独立している。
/// </summary>
public abstract class CItemHybrid : CItem
{
    // serial → 現在のモードインデックス
    private readonly Dictionary<ushort, int> _serialModeIndex = new();

    // 次の AddItem 操作に割り当てるモードインデックス（Give / SwitchMode から設定）
    private int _pendingModeIndex;

    // OnOwnerDying の重複呼び出し防止
    private PlayerEvents.DyingEventArgs? _lastDyingEv;

    private List<CItem>? _subModes;
    protected List<CItem> SubModes => _subModes ??= BuildSubModes();

    /// <summary>モードとして使う CItem インスタンスのリストを構築する。インデックス 0 が初期モード。</summary>
    protected abstract List<CItem> BuildSubModes();

    protected override ItemType BaseItem => SubModes[0].GetBaseItem();
    protected override bool ShowPickedUpHint => false;
    protected override bool ShowSelectedHint => false;

    // ==== Give ====

    public override Item? Give(Player? player, bool displayMessage = false)
    {
        _pendingModeIndex = 0;
        return base.Give(player, displayMessage);
    }

    // ==== モード切替 ====

    /// <summary>
    /// 指定 serial のアイテムを次のモードへ切り替える。
    /// 旧アイテムを破棄し、新 ItemType のアイテムを追加して自動持ち替えする。
    /// インベントリが満杯の場合は切り替えをキャンセルする。
    /// </summary>
    public void SwitchMode(ushort oldSerial, Player player)
    {
        if (!_serialModeIndex.TryGetValue(oldSerial, out var currentIndex)) return;

        var oldItem = player.Items.FirstOrDefault(i => i?.Serial == oldSerial);
        if (oldItem == null) return;

        int nextIndex = (currentIndex + 1) % SubModes.Count;
        var nextSub = SubModes[nextIndex];

        // AddItem 前に pending を設定することで、ItemAdded が Hybrid singleton を追跡する
        _pendingModeIndex = nextIndex;
        SetPendingGive(this, false);
        Item? newItem;
        try
        {
            newItem = player.AddItem(nextSub.GetBaseItem());
        }
        finally
        {
            ClearPendingGive();
            _pendingModeIndex = 0;
        }

        if (newItem == null) return; // インベントリ満杯

        // CurrentItem を切り替える時点では oldSerial がまだ SerialToItem に残っている。
        // これにより ChangingItem 発火時に旧 sub の OnChangingOther 系 cleanup が
        // Check() / CheckHeld() を通過して正しく動く（Burned 解除など）。
        player.CurrentItem = newItem;

        // ChangingItem が解決した後に旧 serial を解除
        _serialModeIndex.Remove(oldSerial);
        SerialTracker.ForceUnregister(oldSerial);

        // serial 解除後に RemoveItem → ItemRemoved の dispatch をスキップ
        player.RemoveItem(oldItem, destroy: true);

        player.ShowHint($"<size=24>モード切替: {nextSub.DisplayName}</size>", 2f);
    }

    // ==== sub 解決 ====

    private CItem? GetCurrentSub(ushort serial)
    {
        if (!_serialModeIndex.TryGetValue(serial, out var idx)) return null;
        if (idx < 0 || idx >= SubModes.Count) return null;
        return SubModes[idx];
    }

    /// <summary>
    /// 指定 serial の現在アクティブな sub が <paramref name="sub"/> と同一インスタンスか。
    /// CItem.Check() が Hybrid 管理下の serial でも sub 視点で true を返せるようにするために使う。
    /// </summary>
    internal bool IsCurrentSub(ushort serial, CItem sub)
    {
        var current = GetCurrentSub(serial);
        return current != null && ReferenceEquals(current, sub);
    }

    // ==== CItem virtual overrides ====

    protected override void OnAcquired(PlayerEvents.ItemAddedEventArgs ev, bool displayMessage)
    {
        // Give/Spawn 初回なら _pendingModeIndex、床拾い等の再取得なら既存インデックスを保持
        if (!_serialModeIndex.ContainsKey(ev.Item.Serial))
            _serialModeIndex[ev.Item.Serial] = _pendingModeIndex;
        GetCurrentSub(ev.Item.Serial)?.CallOnAcquired(ev, displayMessage);
    }

    protected override void OnReleased(PlayerEvents.ItemRemovedEventArgs ev)
        => GetCurrentSub(ev.Item.Serial)?.CallOnReleased(ev);

    protected override void OnSpawned(Pickup pickup)
    {
        if (!_serialModeIndex.ContainsKey(pickup.Serial))
            _serialModeIndex[pickup.Serial] = 0;
        GetCurrentSub(pickup.Serial)?.CallOnSpawned(pickup);
    }

    protected override void OnPickingUp(PlayerEvents.PickingUpItemEventArgs ev)
        => GetCurrentSub(ev.Pickup.Serial)?.CallOnPickingUp(ev);

    protected override void OnDropping(PlayerEvents.DroppingItemEventArgs ev)
        => GetCurrentSub(ev.Item.Serial)?.CallOnDropping(ev);

    protected override void OnUsing(PlayerEvents.UsingItemEventArgs ev)
        => GetCurrentSub(ev.Item.Serial)?.CallOnUsing(ev);

    protected override void OnUsed(PlayerEvents.UsedItemEventArgs ev)
        => GetCurrentSub(ev.Item.Serial)?.CallOnUsed(ev);

    protected override void OnShooting(PlayerEvents.ShootingEventArgs ev)
    {
        var serial = ev.Player?.CurrentItem?.Serial;
        if (serial == null) return;
        GetCurrentSub(serial.Value)?.CallOnShooting(ev);
    }

    protected override void OnShot(PlayerEvents.ShotEventArgs ev)
    {
        var serial = ev.Player?.CurrentItem?.Serial;
        if (serial == null) return;
        GetCurrentSub(serial.Value)?.CallOnShot(ev);
    }

    protected override void OnHurtingOthers(PlayerEvents.HurtingEventArgs ev)
    {
        var serial = ev.Attacker?.CurrentItem?.Serial;
        if (serial == null) return;
        GetCurrentSub(serial.Value)?.CallOnHurtingOthers(ev);
    }

    protected override void OnOwnerDying(PlayerEvents.DyingEventArgs ev)
    {
        // 同一 ev に対して複数 serial が同じ Hybrid singleton を参照する場合の重複防止
        if (ReferenceEquals(_lastDyingEv, ev)) return;
        _lastDyingEv = ev;

        var notified = new HashSet<int>();
        foreach (var item in ev.Player.Items)
        {
            if (item == null) continue;
            if (!TryGet(item.Serial, out var ci) || !ReferenceEquals(ci, this)) continue;
            if (!_serialModeIndex.TryGetValue(item.Serial, out var idx)) continue;
            if (notified.Add(idx))
                SubModes[idx]?.CallOnOwnerDying(ev);
        }
    }

    protected override void OnChangingItem(PlayerEvents.ChangingItemEventArgs ev)
        => GetCurrentSub(ev.Item.Serial)?.CallOnChangingItem(ev);

    protected override void OnThrowingRequest(PlayerEvents.ThrowingRequestEventArgs ev)
        => GetCurrentSub(ev.Item.Serial)?.CallOnThrowingRequest(ev);

    protected override void OnPickupAdded(MapEvents.PickupAddedEventArgs ev)
        => GetCurrentSub(ev.Pickup.Serial)?.CallOnPickupAdded(ev);

    protected override void OnPickupDestroyed(MapEvents.PickupDestroyedEventArgs ev)
    {
        GetCurrentSub(ev.Pickup.Serial)?.CallOnPickupDestroyed(ev);
        _serialModeIndex.Remove(ev.Pickup.Serial);
    }

    public override void RegisterEvents()
    {
        foreach (var sub in SubModes)
            sub?.RegisterEvents();
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        foreach (var sub in SubModes)
            sub?.UnregisterEvents();
        base.UnregisterEvents();
    }

    protected override void OnWaitingForPlayers()
    {
        _serialModeIndex.Clear();
        _lastDyingEv = null;
        foreach (var sub in SubModes)
            sub?.CallOnWaitingForPlayers();
    }

    protected override void OnUpgradingPickup(Scp914Events.UpgradingPickupEventArgs ev)
        => GetCurrentSub(ev.Pickup.Serial)?.CallOnUpgradingPickup(ev);

    protected override void OnUpgradingInventoryItem(Scp914Events.UpgradingInventoryItemEventArgs ev)
        => GetCurrentSub(ev.Item.Serial)?.CallOnUpgradingInventoryItem(ev);

    protected override void CustomizeItem(Item item)
    {
        var idx = _serialModeIndex.TryGetValue(item.Serial, out var i) ? i : _pendingModeIndex;
        if (idx >= 0 && idx < SubModes.Count)
            SubModes[idx]?.CallCustomizeItem(item);
        base.CustomizeItem(item);
    }

    protected override void ShowPickedUpMessage(Player player) { }
    protected override void ShowSelectedMessage(Player player) { }
}
