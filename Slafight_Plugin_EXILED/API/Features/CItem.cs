#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using UnityEngine;

using PlayerHandlers = Exiled.Events.Handlers.Player;
using MapHandlers = Exiled.Events.Handlers.Map;
using PlayerEvents = Exiled.Events.EventArgs.Player;
using MapEvents = Exiled.Events.EventArgs.Map;
using ServerHandlers = Exiled.Events.Handlers.Server;
using Scp914Handlers = Exiled.Events.Handlers.Scp914;
using Scp914Events = Exiled.Events.EventArgs.Scp914;

namespace Slafight_Plugin_EXILED.API.Features;

/// <summary>
/// CRole と同じ思想の、独自で軽量なカスタムアイテム基底クラス。
/// Exiled の CustomItem と異なり、派生クラスは UniqueKey と BaseItem を設定し、
/// 必要なイベントだけを override するだけで動く。
/// シリアル追跡・自動登録・静的イベントディスパッチは基底側で面倒を見る。
/// </summary>
public abstract class CItem
{
    // 登録済み全インスタンス
    private static readonly HashSet<CItem> RegisteredInstances = [];

    // 全 CItem 派生タイプ
    private static readonly List<Type> ItemTypes;

    // UniqueKey → インスタンス
    private static readonly Dictionary<string, CItem> UniqueKeyToItem =
        new(StringComparer.OrdinalIgnoreCase);

    // 追跡中のシリアル → インスタンス
    private static readonly Dictionary<ushort, CItem> SerialToItem = new();

    private static bool _eventsSubscribed;

    static CItem()
    {
        var asm = typeof(CItem).Assembly;
        ItemTypes = asm.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(CItem)) && !t.IsAbstract)
            .ToList();
    }

    /// <summary>
    /// 自動登録を除外したい CItem 用属性。
    /// 手動で new して OverrideItemInstance で差し替える場合に付ける。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CItemAutoRegisterIgnoreAttribute : Attribute { }

    // ==== Plugin から呼ぶ入り口 ====

    public static void RegisterAllItems()
    {
        if (!_eventsSubscribed)
        {
            PlayerHandlers.PickingUpItem += OnAnyPickingUpItem;
            PlayerHandlers.ItemAdded += OnAnyItemAdded;
            PlayerHandlers.ItemRemoved += OnAnyItemRemoved;
            PlayerHandlers.DroppingItem += OnAnyDroppingItem;
            PlayerHandlers.UsingItem += OnAnyUsingItem;
            PlayerHandlers.UsedItem += OnAnyUsedItem;
            PlayerHandlers.Shooting += OnAnyShooting;
            PlayerHandlers.Shot += OnAnyShot;
            PlayerHandlers.Hurting += OnAnyHurting;
            PlayerHandlers.Dying += OnAnyDying;
            PlayerHandlers.ChangingItem += OnAnyChangingItem;
            PlayerHandlers.ThrowingRequest += OnAnyThrowingRequest;

            MapHandlers.PickupAdded += OnAnyPickupAdded;
            MapHandlers.PickupDestroyed += OnAnyPickupDestroyed;

            // SCP-914 アップグレードを CItem 側でも直接ハンドル。
            // Exiled の CustomKeycard は SCP-914 で正常に処理されない既知バグ
            // (ExMod-Team/EXILED#718) があるため、CItem は独自のシリアル追跡で
            // 同じ挙動を自前実装して回避する。
            Scp914Handlers.UpgradingPickup += OnAnyUpgradingPickup;
            Scp914Handlers.UpgradingInventoryItem += OnAnyUpgradingInventoryItem;

            ServerHandlers.WaitingForPlayers += OnAnyWaitingForPlayers;

            _eventsSubscribed = true;
        }

        foreach (var type in ItemTypes)
        {
            try
            {
                var instance = (CItem)Activator.CreateInstance(type);

                if (string.IsNullOrEmpty(instance.UniqueKey))
                {
                    Log.Warn($"CItem.RegisterAllItems: {type.Name} has null/empty UniqueKey, skipping");
                    continue;
                }

                bool autoRegisterEvents =
                    type.GetCustomAttributes(typeof(CItemAutoRegisterIgnoreAttribute), true).Length == 0;

                instance.InternalRegister(autoRegisterEvents);
            }
            catch (Exception ex)
            {
                Log.Error($"CItem.RegisterAllItems failed for {type.Name}: {ex}");
            }
        }
    }

    public static void UnregisterAllItems()
    {
        foreach (var instance in RegisteredInstances.ToList())
            instance.InternalUnregister();

        RegisteredInstances.Clear();
        UniqueKeyToItem.Clear();
        SerialToItem.Clear();

        if (_eventsSubscribed)
        {
            PlayerHandlers.PickingUpItem -= OnAnyPickingUpItem;
            PlayerHandlers.ItemAdded -= OnAnyItemAdded;
            PlayerHandlers.ItemRemoved -= OnAnyItemRemoved;
            PlayerHandlers.DroppingItem -= OnAnyDroppingItem;
            PlayerHandlers.UsingItem -= OnAnyUsingItem;
            PlayerHandlers.UsedItem -= OnAnyUsedItem;
            PlayerHandlers.Shooting -= OnAnyShooting;
            PlayerHandlers.Shot -= OnAnyShot;
            PlayerHandlers.Hurting -= OnAnyHurting;
            PlayerHandlers.Dying -= OnAnyDying;
            PlayerHandlers.ChangingItem -= OnAnyChangingItem;
            PlayerHandlers.ThrowingRequest -= OnAnyThrowingRequest;

            MapHandlers.PickupAdded -= OnAnyPickupAdded;
            MapHandlers.PickupDestroyed -= OnAnyPickupDestroyed;

            Scp914Handlers.UpgradingPickup -= OnAnyUpgradingPickup;
            Scp914Handlers.UpgradingInventoryItem -= OnAnyUpgradingInventoryItem;

            ServerHandlers.WaitingForPlayers -= OnAnyWaitingForPlayers;

            _eventsSubscribed = false;
        }
    }

    /// <summary>
    /// Ignore 属性付き CItem 用:
    /// 手動で生成したインスタンスを UniqueKey マップの「本体」として登録する。
    /// </summary>
    public static void OverrideItemInstance(string uniqueKey, CItem instance)
    {
        if (string.IsNullOrEmpty(uniqueKey) || instance == null) return;
        UniqueKeyToItem[uniqueKey] = instance;
        Log.Debug($"CItem.OverrideItemInstance: {uniqueKey} -> {instance.GetType().Name}");
    }

    // ==== 派生クラスが実装/上書きするメタ情報 ====

    /// <summary>このアイテムを一意に識別するキー。</summary>
    protected abstract string UniqueKey { get; }

    /// <summary>ベースとなる素の ItemType。</summary>
    protected abstract ItemType BaseItem { get; }

    /// <summary>表示名。ログと Hint 用。</summary>
    public virtual string DisplayName => UniqueKey;

    /// <summary>アイテムの説明。ピックアップ/選択 Broadcast に差し込まれる。</summary>
    public virtual string Description => string.Empty;

    public string UniqueKeyName => UniqueKey;

    // ==== ヒント表示 (Exiled CustomItem の PickedUpHint / SelectedHint に書式を合わせる) ====

    private const string PickedUpHintFormat = "<size=24>あなたは{0}を拾いました！\n{1}</size>";
    private const float PickedUpHintDuration = 4f;

    private const string SelectedHintFormat = "<size=24>あなたは{0}を選択しました！\n{1}</size>";
    private const float SelectedHintDuration = 3f;

    /// <summary>拾ったときに Hint を自動表示するか。</summary>
    protected virtual bool ShowPickedUpHint => true;

    /// <summary>選択（手に持つ）したときに Hint を自動表示するか。</summary>
    protected virtual bool ShowSelectedHint => true;

    /// <summary>拾った瞬間に出す Hint メッセージを生成。</summary>
    protected virtual string BuildPickedUpMessage()
        => string.Format(PickedUpHintFormat, DisplayName, Description);

    /// <summary>選択（手に持った）時に出す Hint メッセージを生成。</summary>
    protected virtual string BuildSelectedMessage()
        => string.Format(SelectedHintFormat, DisplayName, Description);

    /// <summary>拾った時の Hint を実際に表示する。派生でまるごと差し替え可能。</summary>
    protected virtual void ShowPickedUpMessage(Player player)
    {
        if (player == null) return;
        player.ShowHint(BuildPickedUpMessage(), PickedUpHintDuration);
    }

    /// <summary>選択した時の Hint を実際に表示する。派生でまるごと差し替え可能。</summary>
    protected virtual void ShowSelectedMessage(Player player)
    {
        if (player == null) return;
        player.ShowHint(BuildSelectedMessage(), SelectedHintDuration);
    }

    // ==== インスタンス管理 ====

    private void InternalRegister(bool autoRegisterEvents)
    {
        if (!RegisteredInstances.Add(this)) return;

        UniqueKeyToItem[UniqueKey] = this;

        if (autoRegisterEvents)
            RegisterEvents();

        Log.Debug($"CItem registered: {GetType().Name} key={UniqueKey} (autoEvents={autoRegisterEvents})");
    }

    private void InternalUnregister()
    {
        if (!RegisteredInstances.Remove(this)) return;

        if (UniqueKeyToItem.TryGetValue(UniqueKey, out var inst) && ReferenceEquals(inst, this))
            UniqueKeyToItem.Remove(UniqueKey);

        // このインスタンスで追跡していたシリアルを掃除
        var mine = SerialToItem.Where(kv => ReferenceEquals(kv.Value, this))
                               .Select(kv => kv.Key)
                               .ToList();
        foreach (var s in mine) SerialToItem.Remove(s);

        UnregisterEvents();
        Log.Debug($"CItem unregistered: {GetType().Name}");
    }

    /// <summary>派生クラスが追加イベント購読をしたい場合は override する。</summary>
    public virtual void RegisterEvents() { }

    /// <summary>派生クラスの追加イベント購読解除。</summary>
    public virtual void UnregisterEvents() { }

    // ==== 付与/生成 ====

    // Give() が実行中に走る AddItem → ItemAdded イベントに対し、
    // 「この ItemAdded は Give() 由来」だと伝えるための一時マーカー。
    // ゲームロジックは単一スレッドなので static で競合しない。
    private static CItem? _pendingGiveCItem;
    private static bool _pendingGiveDisplayMessage;

    /// <summary>
    /// プレイヤーにこの CItem を付与する。
    /// Exiled CustomItem と同じく、既定で PickedUp 相当の Hint を表示する。
    /// </summary>
    /// <param name="player">付与先プレイヤー。</param>
    /// <param name="displayMessage">true なら ShowPickedUpHint に従い Hint を表示する。</param>
    public virtual Item? Give(Player? player, bool displayMessage = true)
    {
        if (player == null) return null;

        try
        {
            _pendingGiveCItem = this;
            _pendingGiveDisplayMessage = displayMessage;

            var item = player.AddItem(BaseItem);
            if (item == null) return null;

            // AddItem 同期で ItemAdded が発火し OnAnyItemAdded 側で
            // SerialToItem 登録と OnAcquired ディスパッチが済むのが正常系。
            // 何らかの理由で ItemAdded を拾えていなかった場合に備えて保険でここでも登録。
            if (!SerialToItem.ContainsKey(item.Serial))
                SerialToItem[item.Serial] = this;

            return item;
        }
        catch (Exception ex)
        {
            Log.Error($"CItem.Give failed ({GetType().Name}): {ex}");
            return null;
        }
        finally
        {
            _pendingGiveCItem = null;
            _pendingGiveDisplayMessage = false;
        }
    }

    /// <summary>指定位置にこの CItem の Pickup を生成する。</summary>
    public virtual Pickup? Spawn(Vector3 position)
    {
        try
        {
            var pickup = Pickup.CreateAndSpawn(BaseItem, position, Quaternion.identity);
            if (pickup == null) return null;

            SerialToItem[pickup.Serial] = this;
            OnSpawned(pickup);
            return pickup;
        }
        catch (Exception ex)
        {
            Log.Error($"CItem.Spawn failed ({GetType().Name}): {ex}");
            return null;
        }
    }

    /// <summary>プレイヤーの所持品から一致する最初のインスタンスを消す。見つからなければ false。</summary>
    public bool RemoveFrom(Player? player, bool destroy = true)
    {
        if (player == null) return false;

        foreach (var item in player.Items.ToList())
        {
            if (item == null) continue;
            if (!Check(item)) continue;

            SerialToItem.Remove(item.Serial);
            if (destroy)
                player.RemoveItem(item, destroy: true);
            return true;
        }

        return false;
    }

    // ==== Check / TryGet ====

    /// <summary>このアイテムが CItem のこのインスタンス由来か。</summary>
    public bool Check(Item? item)
        => item != null && SerialToItem.TryGetValue(item.Serial, out var ci) && ReferenceEquals(ci, this);

    /// <summary>このピックアップが CItem のこのインスタンス由来か。</summary>
    public bool Check(Pickup? pickup)
        => pickup != null && SerialToItem.TryGetValue(pickup.Serial, out var ci) && ReferenceEquals(ci, this);

    /// <summary>プレイヤーが手に持っている現在のアイテムがこの CItem のインスタンスか。</summary>
    public bool CheckHeld(Player? player)
        => player != null && Check(player.CurrentItem);

    /// <summary>プレイヤーがこの CItem をインベントリに所持しているか。</summary>
    public bool HasIn(Player? player)
    {
        if (player == null) return false;
        foreach (var it in player.Items)
            if (Check(it)) return true;
        return false;
    }

    /// <summary>シリアル→登録済み CItem。無ければ false。</summary>
    public static bool TryGet(ushort serial, out CItem? cItem)
        => SerialToItem.TryGetValue(serial, out cItem!);

    public static bool TryGet(Item? item, out CItem? cItem)
    {
        cItem = null;
        return item != null && SerialToItem.TryGetValue(item.Serial, out cItem!);
    }

    public static bool TryGet(Pickup? pickup, out CItem? cItem)
    {
        cItem = null;
        return pickup != null && SerialToItem.TryGetValue(pickup.Serial, out cItem!);
    }

    public static bool TryGetByKey(string uniqueKey, out CItem? cItem)
        => UniqueKeyToItem.TryGetValue(uniqueKey, out cItem!);

    public static T? Get<T>() where T : CItem
        => RegisteredInstances.OfType<T>().FirstOrDefault();

    // ==== 派生クラス向けイベントフック ====

    /// <summary>
    /// プレイヤーのインベントリにこの CItem が入った瞬間（経路問わず）発火する
    /// 汎用フック。Give / 床からのピックアップ / ロードアウト / SCP-914 /
    /// 他プラグイン経由の AddItem など全ての経路で一度だけ呼ばれる。
    /// </summary>
    /// <param name="ev">Exiled の ItemAddedEventArgs。Pickup が床由来の場合は ev.Pickup に元ピックアップが入る。</param>
    /// <param name="displayMessage">Give(displayMessage:false) が明示されなければ true。</param>
    protected virtual void OnAcquired(PlayerEvents.ItemAddedEventArgs ev, bool displayMessage) { }

    protected virtual void OnReleased(PlayerEvents.ItemRemovedEventArgs ev) { }

    protected virtual void OnSpawned(Pickup pickup) { }

    protected virtual void OnPickingUp(PlayerEvents.PickingUpItemEventArgs ev) { }
    protected virtual void OnDropping(PlayerEvents.DroppingItemEventArgs ev) { }
    protected virtual void OnUsing(PlayerEvents.UsingItemEventArgs ev) { }
    protected virtual void OnUsed(PlayerEvents.UsedItemEventArgs ev) { }
    protected virtual void OnShooting(PlayerEvents.ShootingEventArgs ev) { }
    protected virtual void OnShot(PlayerEvents.ShotEventArgs ev) { }
    protected virtual void OnHurtingOthers(PlayerEvents.HurtingEventArgs ev) { }
    protected virtual void OnOwnerDying(PlayerEvents.DyingEventArgs ev) { }
    protected virtual void OnChangingItem(PlayerEvents.ChangingItemEventArgs ev) { }
    protected virtual void OnThrowingRequest(PlayerEvents.ThrowingRequestEventArgs ev) { }
    protected virtual void OnPickupAdded(MapEvents.PickupAddedEventArgs ev) { }
    protected virtual void OnPickupDestroyed(MapEvents.PickupDestroyedEventArgs ev) { }
    protected virtual void OnWaitingForPlayers() { }

    /// <summary>
    /// SCP-914 が床に置かれたこの CItem の Pickup をアップグレードするとき。
    /// デフォルト動作: 既定のアップグレードを無効化し、同じ CItem のまま
    /// 出力位置にテレポートする（シリアル保持したままなので追跡も継続）。
    /// </summary>
    protected virtual void OnUpgradingPickup(Scp914Events.UpgradingPickupEventArgs ev)
    {
        ev.IsAllowed = false;
        if (ev.Pickup != null)
            ev.Pickup.Position = ev.OutputPosition;
    }

    /// <summary>
    /// SCP-914 がインベントリ内のこの CItem をアップグレードするとき。
    /// デフォルト動作: 既定のアップグレードを無効化する（CItem を保持する）。
    /// </summary>
    protected virtual void OnUpgradingInventoryItem(Scp914Events.UpgradingInventoryItemEventArgs ev)
    {
        ev.IsAllowed = false;
    }

    // ==== 静的イベントディスパッチ ====

    private static void Dispatch(ushort serial, Action<CItem> body, string tag)
    {
        if (!SerialToItem.TryGetValue(serial, out var ci) || ci == null) return;
        try { body(ci); }
        catch (Exception ex) { Log.Error($"CItem.{tag} error in {ci.GetType().Name}: {ex}"); }
    }

    private static void OnAnyPickingUpItem(PlayerEvents.PickingUpItemEventArgs ev)
    {
        // 床からの手動ピックアップ特有のキャンセル用フック。
        // Hint 表示は OnAnyItemAdded 側に集約したのでここでは扱わない。
        if (ev?.Pickup == null) return;
        Dispatch(ev.Pickup.Serial, ci => ci.OnPickingUp(ev), nameof(OnPickingUp));
    }

    /// <summary>
    /// 全ての経路で「プレイヤーのインベントリに ItemBase が足された瞬間」を拾う。
    /// Give() 由来 / 床拾い / ロードアウト / SCP-914 / 他プラグイン AddItem すべて。
    /// </summary>
    private static void OnAnyItemAdded(PlayerEvents.ItemAddedEventArgs ev)
    {
        if (ev?.Player == null || ev.Item == null) return;

        CItem? ci;
        bool displayMessage;

        // 1. Give() からの呼び出し: pending marker で CItem を決定し、即 tracking
        if (_pendingGiveCItem != null)
        {
            ci = _pendingGiveCItem;
            displayMessage = _pendingGiveDisplayMessage;
            SerialToItem[ev.Item.Serial] = ci;
            _pendingGiveCItem = null;
            _pendingGiveDisplayMessage = false;
        }
        // 2. それ以外 (床拾い / 914 / 他プラグインの AddItem): 既にシリアル追跡済みなら CItem
        else if (SerialToItem.TryGetValue(ev.Item.Serial, out var existing) && existing != null)
        {
            ci = existing;
            displayMessage = true;
        }
        else
        {
            return;
        }

        try { ci.OnAcquired(ev, displayMessage); }
        catch (Exception e) { Log.Error($"CItem.OnAcquired error in {ci.GetType().Name}: {e}"); }

        if (displayMessage && ci.ShowPickedUpHint)
        {
            try { ci.ShowPickedUpMessage(ev.Player); }
            catch (Exception e) { Log.Error($"CItem.ShowPickedUpMessage error in {ci.GetType().Name}: {e}"); }
        }
    }

    private static void OnAnyItemRemoved(PlayerEvents.ItemRemovedEventArgs ev)
    {
        if (ev?.Item == null) return;
        // ItemRemoved は「インベントリから抜けた」時点の通知。
        // 対応する Pickup に遷移している可能性があるので SerialToItem は掃除しない。
        // 本当に消滅したときは OnAnyPickupDestroyed 側で掃除する。
        if (!SerialToItem.TryGetValue(ev.Item.Serial, out var ci) || ci == null) return;
        try { ci.OnReleased(ev); }
        catch (Exception e) { Log.Error($"CItem.OnReleased error in {ci.GetType().Name}: {e}"); }
    }

    private static void OnAnyDroppingItem(PlayerEvents.DroppingItemEventArgs ev)
    {
        if (ev?.Item == null) return;
        Dispatch(ev.Item.Serial, ci => ci.OnDropping(ev), nameof(OnDropping));
    }

    private static void OnAnyUsingItem(PlayerEvents.UsingItemEventArgs ev)
    {
        if (ev?.Item == null) return;
        Dispatch(ev.Item.Serial, ci => ci.OnUsing(ev), nameof(OnUsing));
    }

    private static void OnAnyUsedItem(PlayerEvents.UsedItemEventArgs ev)
    {
        if (ev?.Item == null) return;
        Dispatch(ev.Item.Serial, ci => ci.OnUsed(ev), nameof(OnUsed));
    }

    private static void OnAnyShooting(PlayerEvents.ShootingEventArgs ev)
    {
        var item = ev?.Player?.CurrentItem;
        if (item == null) return;
        Dispatch(item.Serial, ci => ci.OnShooting(ev!), nameof(OnShooting));
    }

    private static void OnAnyShot(PlayerEvents.ShotEventArgs ev)
    {
        var item = ev?.Player?.CurrentItem;
        if (item == null) return;
        Dispatch(item.Serial, ci => ci.OnShot(ev!), nameof(OnShot));
    }

    private static void OnAnyHurting(PlayerEvents.HurtingEventArgs ev)
    {
        var item = ev?.Attacker?.CurrentItem;
        if (item == null) return;
        Dispatch(item.Serial, ci => ci.OnHurtingOthers(ev!), nameof(OnHurtingOthers));
    }

    private static void OnAnyDying(PlayerEvents.DyingEventArgs ev)
    {
        if (ev?.Player == null) return;

        // 所持してた CItem 全てに通知
        foreach (var item in ev.Player.Items.ToList())
        {
            if (item == null) continue;
            if (!SerialToItem.TryGetValue(item.Serial, out var ci) || ci == null) continue;

            try { ci.OnOwnerDying(ev); }
            catch (Exception ex) { Log.Error($"CItem.OnOwnerDying error in {ci.GetType().Name}: {ex}"); }
        }
    }

    private static void OnAnyChangingItem(PlayerEvents.ChangingItemEventArgs ev)
    {
        if (ev?.Item == null) return;
        if (!SerialToItem.TryGetValue(ev.Item.Serial, out var ci) || ci == null) return;

        try { ci.OnChangingItem(ev); }
        catch (Exception e) { Log.Error($"CItem.OnChangingItem error in {ci.GetType().Name}: {e}"); }

        if (ev.IsAllowed && ci.ShowSelectedHint && ev.Player != null)
        {
            try { ci.ShowSelectedMessage(ev.Player); }
            catch (Exception e) { Log.Error($"CItem.ShowSelectedMessage error in {ci.GetType().Name}: {e}"); }
        }
    }

    private static void OnAnyThrowingRequest(PlayerEvents.ThrowingRequestEventArgs ev)
    {
        var item = ev?.Item;
        if (item == null) return;
        Dispatch(item.Serial, ci => ci.OnThrowingRequest(ev!), nameof(OnThrowingRequest));
    }

    private static void OnAnyPickupAdded(MapEvents.PickupAddedEventArgs ev)
    {
        if (ev?.Pickup == null) return;
        Dispatch(ev.Pickup.Serial, ci => ci.OnPickupAdded(ev), nameof(OnPickupAdded));
    }

    private static void OnAnyPickupDestroyed(MapEvents.PickupDestroyedEventArgs ev)
    {
        if (ev?.Pickup == null) return;

        if (!SerialToItem.TryGetValue(ev.Pickup.Serial, out var ci) || ci == null) return;

        try { ci.OnPickupDestroyed(ev); }
        catch (Exception ex) { Log.Error($"CItem.OnPickupDestroyed error in {ci.GetType().Name}: {ex}"); }

        // PickupDestroyed は「プレイヤーが拾って pickup が消えた」時にも発火するため、
        // ここで SerialToItem を即削除すると同じ CItem が拾われた瞬間に追跡が切れる。
        // シリアル再利用は round 毎なので WaitingForPlayers 側で一括掃除に任せる。
    }

    private static void OnAnyUpgradingPickup(Scp914Events.UpgradingPickupEventArgs ev)
    {
        if (ev?.Pickup == null) return;
        Dispatch(ev.Pickup.Serial, ci => ci.OnUpgradingPickup(ev), nameof(OnUpgradingPickup));
    }

    private static void OnAnyUpgradingInventoryItem(Scp914Events.UpgradingInventoryItemEventArgs ev)
    {
        if (ev?.Item == null) return;
        Dispatch(ev.Item.Serial, ci => ci.OnUpgradingInventoryItem(ev), nameof(OnUpgradingInventoryItem));
    }

    private static void OnAnyWaitingForPlayers()
    {
        SerialToItem.Clear();

        foreach (var ci in RegisteredInstances.ToList())
        {
            try { ci.OnWaitingForPlayers(); }
            catch (Exception ex) { Log.Error($"CItem.OnWaitingForPlayers error in {ci.GetType().Name}: {ex}"); }
        }
    }
}
