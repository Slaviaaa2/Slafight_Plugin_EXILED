using System;
using System.Collections.Generic;
using System.Reflection;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;

using Log = Exiled.API.Features.Log;
using Player = Exiled.API.Features.Player;
using ExiledItem = Exiled.API.Features.Items.Item;
using Item = LabApi.Features.Wrappers.Item;
using Pickup = LabApi.Features.Wrappers.Pickup;
using LabPlayer = LabApi.Features.Wrappers.Player;
using LightSourceToy = LabApi.Features.Wrappers.LightSourceToy;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// カスタムアイテムです。<b>アイテム 1 個につき 1 インスタンス</b>生成されます。
/// フィールドをそのまま per-item 状態 (残弾・チャージ・使用回数など) として使えるので、
/// シリアルをキーにした static 辞書を自分で用意する必要はありません。
///
/// 追跡キーはシリアル (<see cref="ushort"/>) です。
/// シリアルはインベントリのアイテムと地面のピックアップで共通なので、
/// 落とす・拾い直すをまたいでも同じインスタンスが付いてきます。
///
/// 識別子の文字列も専用 enum もありません。<b>型そのものが同一性です。</b>
/// </summary>
/// <remarks>
/// <b>アイテムの実体とイベントだけは LabApi のものを使います。</b>
/// アーマー・SCP-330・使用効果の差し替えなど、EXILED 側に相当イベントが無いものが
/// あるためです。<see cref="Owner"/> をはじめプレイヤーを渡す口はすべて EXILED の
/// <see cref="Player"/> に揃えてあるので、機能側が LabApi を意識する必要はありません。
/// </remarks>
/// <example>
/// <code>
/// public sealed class MediPistol : CustomItem
/// {
///     public override ItemType BaseType => ItemType.GunCOM18;
///     public override string Name => "S41 医療用拳銃";
///
///     private int charges = 6;   // per-item 状態をそのままフィールドに持てる
///
///     protected override void OnShot()
///     {
///         if (charges-- &lt;= 0)
///             Owner?.ShowHint("チャージ切れ", 3f);
///     }
/// }
///
/// CustomItem.Give&lt;MediPistol&gt;(player);
/// CustomItem.Spawn&lt;MediPistol&gt;(position);
/// </code>
/// </example>
public abstract class CustomItem
{
    private static readonly Dictionary<ushort, CustomItem> BySerial = new Dictionary<ushort, CustomItem>();

    private static bool hooked;

    private LightSourceToy pickupLight;

    private SchematicObject pickupModel;

    /// <summary>
    /// このアイテムのシリアルです。生成時に決まります。
    /// </summary>
    public ushort Serial { get; private set; }

    /// <summary>
    /// 土台になるバニラアイテムです。
    /// </summary>
    public abstract ItemType BaseType { get; }

    /// <summary>
    /// 表示名です。既定はクラス名。
    /// </summary>
    public virtual string Name => GetType().Name;

    /// <summary>
    /// 説明です。
    /// </summary>
    public virtual string Description => string.Empty;

    /// <summary>
    /// 拾ったときに本人へ出す文言です。null / 空なら出しません。
    /// </summary>
    protected virtual string PickupHint => $"<size=24>あなたは{Name}を拾いました！\n{Description}</size>";

    /// <summary>
    /// 手に持ち替えたときに本人へ出す文言です。null / 空なら出しません。
    /// </summary>
    protected virtual string SelectedHint => $"<size=24>あなたは{Name}を選択しました！\n{Description}</size>";

    /// <summary><see cref="PickupHint"/> の表示秒数です。</summary>
    protected virtual float PickupHintDuration => 6f;

    /// <summary><see cref="SelectedHint"/> の表示秒数です。</summary>
    protected virtual float SelectedHintDuration => 5f;

    /// <summary>
    /// 地面に落ちている間、ピックアップを光らせるかどうか。
    /// 暗がりで見失いやすい特別なアイテムだけ true にします。
    /// </summary>
    protected virtual bool PickupLightEnabled => false;

    /// <summary>ピックアップライトの色です。</summary>
    protected virtual Color PickupLightColor => Color.white;

    /// <summary>ピックアップライトの強さです。</summary>
    protected virtual float PickupLightIntensity => 0.7f;

    /// <summary>ピックアップライトの届く距離です。</summary>
    protected virtual float PickupLightRange => 3.75f;

    /// <summary>ピックアップライトの影です。既定は影なし。</summary>
    protected virtual LightShadows PickupLightShadowType => LightShadows.None;

    /// <summary>
    /// 地面のピックアップに重ねる ProjectMER スキマティック名です。null / 空なら重ねません。
    /// </summary>
    /// <remarks>
    /// バニラの見た目を借りているアイテムに、本来の姿を被せるためのものです。
    /// 状態で見た目が変わるアイテム (レベルの上がる装置など) は、
    /// ここを自分の状態から組み立てて <see cref="RefreshPickupModel"/> を呼んでください。
    /// </remarks>
    protected virtual string PickupModel => null;

    /// <summary>重ねるスキマティックの拡大率です。</summary>
    protected virtual Vector3 PickupModelScale => Vector3.one;

    /// <summary>
    /// インベントリ内の実体です。地面に落ちていれば null。
    /// </summary>
    public Item Item => Item.Get(Serial);

    /// <summary>
    /// 地面のピックアップです。誰かが持っていれば null。
    /// </summary>
    public Pickup Pickup => Pickup.Get(Serial);

    /// <summary>
    /// 現在の所持者です。地面にあれば null。
    /// </summary>
    public Player Owner => ToExiled(Item?.CurrentOwner);

    /// <summary>
    /// 追跡中のカスタムアイテムをすべて返します。
    /// </summary>
    public static IReadOnlyCollection<CustomItem> Tracked => BySerial.Values;

    /// <summary>
    /// 指定したアセンブリで定義されたカスタムアイテムの追跡だけを解除します。
    /// </summary>
    public static void ReleaseAssembly(Assembly assembly)
    {
        if (assembly is null) return;

        foreach (CustomItem custom in new List<CustomItem>(BySerial.Values))
        {
            if (custom.GetType().Assembly == assembly)
                custom.Release();
        }
    }

    /// <summary>
    /// 全アイテムを解放し、静的イベント購読を解除します。
    /// </summary>
    public static void Shutdown()
    {
        OnRoundRestarted();

        if (!hooked) return;

        PlayerEvents.PickedUpItem -= OnAnyPickedUp;
        PlayerEvents.PickedUpArmor -= OnAnyPickedUpArmor;
        PlayerEvents.PickedUpScp330 -= OnAnyPickedUpScp330;
        PlayerEvents.DroppingItem -= OnAnyDropping;
        PlayerEvents.DroppedItem -= OnAnyDropped;
        PlayerEvents.ChangedItem -= OnAnyChangedItem;
        PlayerEvents.UsingItem -= OnAnyUsingItem;
        PlayerEvents.ItemUsageEffectsApplying -= OnAnyUsageEffectsApplying;
        PlayerEvents.UsedItem -= OnAnyUsedItem;
        PlayerEvents.ShootingWeapon -= OnAnyShootingWeapon;
        PlayerEvents.ShotWeapon -= OnAnyShotWeapon;
        ServerEvents.RoundRestarted -= OnRoundRestarted;
        hooked = false;
    }

    /// <summary>
    /// このシリアルのカスタムアイテムです。無ければ null。
    /// </summary>
    public static CustomItem Of(ushort serial) =>
        BySerial.TryGetValue(serial, out CustomItem item) ? item : null;

    /// <inheritdoc cref="Of(ushort)"/>
    public static CustomItem Of(Item item) => item is null ? null : Of(item.Serial);

    /// <inheritdoc cref="Of(ushort)"/>
    public static CustomItem Of(Pickup pickup) => pickup is null ? null : Of(pickup.Serial);

    /// <summary>
    /// このシリアルが T のカスタムアイテムかどうか。
    /// </summary>
    public static bool Is<T>(ushort serial) where T : CustomItem => Of(serial) is T;

    /// <summary>
    /// プレイヤーに新しいカスタムアイテムを渡します。
    /// </summary>
    public static T Give<T>(Player player) where T : CustomItem, new() => (T)Give(new T(), player);

    /// <summary>
    /// 型を実行時に決めてプレイヤーに渡します。
    /// マップデータや RA コマンドのように、文字列から型を引いてくる経路で使います
    /// (<see cref="TypeParser.TryParse{TBase}"/> と組み合わせる)。
    /// </summary>
    public static CustomItem Give(Type type, Player player) => Give(Instantiate(type), player);

    /// <summary>
    /// 指定した位置に新しいカスタムアイテムを落とします。
    /// </summary>
    public static T Spawn<T>(Vector3 position, Quaternion? rotation = null, Vector3? scale = null)
        where T : CustomItem, new()
        => (T)Spawn(new T(), position, rotation, scale);

    /// <summary>
    /// 型を実行時に決めて、指定した位置に落とします。
    /// </summary>
    public static CustomItem Spawn(Type type, Vector3 position, Quaternion? rotation = null, Vector3? scale = null)
        => Spawn(Instantiate(type), position, rotation, scale);

    /// <summary>
    /// 既に存在するアイテムを、このカスタムアイテムとして追跡し始めます。
    /// SCP-914 での変換や、マップに配置済みのピックアップを引き取るときに使います。
    /// </summary>
    public static T Adopt<T>(ushort serial) where T : CustomItem, new() => (T)Adopt(new T(), serial);

    /// <summary>
    /// 型を実行時に決めて追跡し始めます。
    /// </summary>
    public static CustomItem Adopt(Type type, ushort serial) => Adopt(Instantiate(type), serial);

    /// <summary>
    /// このアイテムの追跡をやめます。以後、イベントは届きません。
    /// </summary>
    public void Release()
    {
        if (Serial == 0) return;

        DetachPickupLight();
        DetachPickupModel();
        BySerial.Remove(Serial);
        Invoke(OnReleased, nameof(OnReleased));
        Serial = 0;
    }

    /// <summary>
    /// このアイテムを消します。
    /// </summary>
    public void Destroy()
    {
        Pickup?.Destroy();

        if (Owner is { } owner && ExiledItem.Get(Serial) is { } held)
            owner.RemoveItem(held);

        Release();
    }

    /// <summary>
    /// 生成直後に呼ばれます。
    /// </summary>
    protected virtual void OnCreated()
    {
    }

    /// <summary>
    /// 追跡をやめるときに呼ばれます。
    /// </summary>
    protected virtual void OnReleased()
    {
    }

    /// <summary>
    /// 拾われたときに呼ばれます。
    /// </summary>
    protected virtual void OnPickedUp(Player player)
    {
    }

    /// <summary>
    /// 落とされる直前に呼ばれます。<c>ev.IsAllowed = false</c> で止められます。
    /// 投げ捨てを射出操作として使うアイテムは <c>ev.Throw</c> を見てください。
    /// </summary>
    protected virtual void OnDropping(PlayerDroppingItemEventArgs ev)
    {
    }

    /// <summary>
    /// 落とされたときに呼ばれます。
    /// </summary>
    protected virtual void OnDropped(Player player)
    {
    }

    /// <summary>
    /// 手に持ったときに呼ばれます。
    /// </summary>
    protected virtual void OnEquipped(Player player)
    {
    }

    /// <summary>
    /// しまったときに呼ばれます。
    /// </summary>
    protected virtual void OnUnequipped(Player player)
    {
    }

    /// <summary>
    /// 使用が始まるときに呼ばれます。<c>ev.IsAllowed = false</c> で止められます。
    /// </summary>
    protected virtual void OnUsing(PlayerUsingItemEventArgs ev)
    {
    }

    /// <summary>
    /// 使用モーションが終わり、バニラの効果が適用される直前に呼ばれます。
    /// <c>ev.IsAllowed = false</c> でバニラの効果を差し替えられます。
    ///
    /// 消費アイテムの効果を自前にしたい場合はここを使ってください。
    /// なお <c>IsAllowed = false</c> にすると <see cref="OnUsed"/> は呼ばれません。
    /// </summary>
    protected virtual void OnUseCompleting(PlayerItemUsageEffectsApplyingEventArgs ev)
    {
    }

    /// <summary>
    /// バニラの効果まで含めて使用が完了したときに呼ばれます。
    /// <see cref="OnUseCompleting"/> で差し止めた場合は呼ばれません。
    /// </summary>
    protected virtual void OnUsed()
    {
    }

    /// <summary>
    /// 撃つ直前に呼ばれます。
    /// </summary>
    protected virtual void OnShooting(PlayerShootingWeaponEventArgs ev)
    {
    }

    /// <summary>
    /// 撃った直後に呼ばれます。
    /// </summary>
    protected virtual void OnShot()
    {
    }

    /// <summary>
    /// インベントリ内の実体に対する見た目・性能の調整です。
    /// </summary>
    protected virtual void Customize(Item item)
    {
    }

    /// <summary>
    /// 地面のピックアップに対する見た目の調整です。
    /// </summary>
    protected virtual void Customize(Pickup pickup)
    {
    }

    /// <summary>
    /// 重ねているスキマティックを今の <see cref="PickupModel"/> で作り直します。
    /// 状態で見た目が変わるアイテムが、状態を変えた後に呼びます。
    /// </summary>
    protected void RefreshPickupModel()
    {
        DetachPickupModel();
        AttachPickupModel();
    }

    private static CustomItem Give(CustomItem custom, Player player)
    {
        if (custom is null || player is null) return null;

        if (player.AddItem(custom.BaseType) is not { } item) return null;

        custom.Attach(item.Serial);

        if (custom.Item is { } wrapper)
            custom.Customize(wrapper);

        return custom;
    }

    private static CustomItem Spawn(CustomItem custom, Vector3 position, Quaternion? rotation, Vector3? scale)
    {
        if (custom is null) return null;

        // networkSpawn: false で作る。
        // Mirror の SpawnMessage はスケールを含むので、Customize で見た目を決めてから Spawn する。
        // 先にネットワーク生成すると UnSpawn → 変更 → Spawn の焼き直しが要り、netId も変わる。
        Pickup pickup = Pickup.Create(
            custom.BaseType,
            position,
            rotation ?? Quaternion.identity,
            scale ?? Vector3.one,
            networkSpawn: false);

        if (pickup is null) return null;

        custom.Attach(pickup.Serial);
        custom.Customize(pickup);

        pickup.Spawn();
        custom.AttachPickupLight();
        custom.AttachPickupModel();

        return custom;
    }

    private static CustomItem Adopt(CustomItem custom, ushort serial)
    {
        if (custom is null) return null;

        custom.Attach(serial);

        if (custom.Item is { } item)
        {
            custom.Customize(item);
        }
        else if (custom.Pickup is { } pickup)
        {
            custom.Customize(pickup);
            custom.AttachPickupLight();
            custom.AttachPickupModel();
        }

        return custom;
    }

    /// <summary>
    /// 型からインスタンスを作ります。カスタムアイテムでない型を渡されたら null を返します。
    /// </summary>
    private static CustomItem Instantiate(Type type)
    {
        if (type is null || !typeof(CustomItem).IsAssignableFrom(type) || type.IsAbstract)
        {
            Log.Error($"[Slafight] {type?.FullName ?? "null"} はカスタムアイテムとして生成できません。");

            return null;
        }

        try
        {
            return (CustomItem)Activator.CreateInstance(type);
        }
        catch (Exception exception)
        {
            Log.Error($"[Slafight] カスタムアイテム {type.FullName} の生成に失敗しました: {exception}");

            return null;
        }
    }

    /// <summary>
    /// LabApi のプレイヤーを EXILED のものへ変換します。
    /// 機能側に LabApi の型を見せないための境界です。
    /// </summary>
    private static Player ToExiled(LabPlayer player) =>
        player?.ReferenceHub is { } hub ? Player.Get(hub) : null;

    /// <summary>
    /// 地面のピックアップにライトを付けます。既に付いていれば何もしません。
    /// </summary>
    /// <remarks>
    /// ピックアップの transform にぶら下げるので、投げても転がっても追従します。
    /// <c>AdminToyBase.LateUpdate</c> が毎フレーム位置を同期するため、
    /// 旧実装のような追従コルーチンは要りません。
    ///
    /// <para>
    /// 位置は <b>親からのローカル座標</b>です。<c>AdminToy.Create</c> は
    /// 渡した位置をそのまま <c>localPosition</c> に入れるので、
    /// ここにワールド座標を渡すとライトだけが遠くへ飛びます。
    /// </para>
    /// </remarks>
    private void AttachPickupLight()
    {
        if (!PickupLightEnabled || pickupLight is not null) return;
        if (Pickup is not { } pickup) return;

        LightSourceToy light = LightSourceToy.Create(
            Vector3.zero,
            Quaternion.identity,
            Vector3.one,
            pickup.Transform,
            networkSpawn: false);

        if (light is null) return;

        light.Color = PickupLightColor;
        light.Intensity = PickupLightIntensity;
        light.Range = PickupLightRange;
        light.ShadowType = PickupLightShadowType;
        light.Spawn();

        pickupLight = light;
    }

    /// <summary>ピックアップライトを片付けます。二重に呼んでも安全です。</summary>
    private void DetachPickupLight()
    {
        if (pickupLight is null) return;

        // ピックアップごと消えたときは子のライトも一緒に消えている。
        if (!pickupLight.IsDestroyed)
            pickupLight.Destroy();

        pickupLight = null;
    }

    /// <summary>
    /// 地面のピックアップへスキマティックを重ねます。既に重なっていれば何もしません。
    /// </summary>
    private void AttachPickupModel()
    {
        if (pickupModel is not null) return;
        if (PickupModel is not { Length: > 0 } model) return;
        if (Pickup is not { } pickup) return;

        SchematicObject schematic = ObjectSpawner.SpawnSchematic(model, pickup.Position, pickup.Transform.rotation);

        if (schematic is null)
        {
            Log.Warn($"[Slafight] {GetType().Name} のスキマティック '{model}' を出せませんでした。");

            return;
        }

        schematic.Scale = PickupModelScale;
        schematic.transform.SetParent(pickup.Transform, true);

        pickupModel = schematic;
    }

    /// <summary>重ねたスキマティックを片付けます。二重に呼んでも安全です。</summary>
    private void DetachPickupModel()
    {
        SchematicObject schematic = pickupModel;
        pickupModel = null;

        if (schematic == null) return;

        schematic.Destroy();
    }

    /// <summary>拾得・選択のヒントを出します。文言が空なら何もしません。</summary>
    private void ShowItemHint(LabPlayer player, string text, float duration)
    {
        if (ToExiled(player) is not { } target || string.IsNullOrEmpty(text)) return;

        target.ShowHint(text, duration);
    }

    private void Attach(ushort serial)
    {
        Serial = serial;

        // 同じシリアルを別のカスタムアイテムが持っていたら明け渡す。
        if (BySerial.TryGetValue(serial, out CustomItem previous) && !ReferenceEquals(previous, this))
            previous.Release();

        BySerial[serial] = this;

        Hook();
        Invoke(OnCreated, nameof(OnCreated));
    }

    private void Invoke(Action action, string name)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            Log.Error($"[Slafight] {GetType().Name}.{name} で例外が発生しました: {exception}");
        }
    }

    private void HandlePickedUp(Item item, LabPlayer player)
    {
        DetachPickupLight();
        DetachPickupModel();
        Customize(item);
        Invoke(() => OnPickedUp(ToExiled(player)), nameof(OnPickedUp));
        ShowItemHint(player, PickupHint, PickupHintDuration);
    }

    private static void Hook()
    {
        if (hooked) return;

        hooked = true;

        // 拾得は種類ごとに別イベント。PickedUpItem だけではアーマーとキャンディを取りこぼす。
        PlayerEvents.PickedUpItem += OnAnyPickedUp;
        PlayerEvents.PickedUpArmor += OnAnyPickedUpArmor;
        PlayerEvents.PickedUpScp330 += OnAnyPickedUpScp330;
        PlayerEvents.DroppingItem += OnAnyDropping;
        PlayerEvents.DroppedItem += OnAnyDropped;
        PlayerEvents.ChangedItem += OnAnyChangedItem;
        PlayerEvents.UsingItem += OnAnyUsingItem;
        PlayerEvents.ItemUsageEffectsApplying += OnAnyUsageEffectsApplying;
        PlayerEvents.UsedItem += OnAnyUsedItem;
        PlayerEvents.ShootingWeapon += OnAnyShootingWeapon;
        PlayerEvents.ShotWeapon += OnAnyShotWeapon;
        ServerEvents.RoundRestarted += OnRoundRestarted;
    }

    private static void OnAnyPickedUp(PlayerPickedUpItemEventArgs ev)
    {
        if (Of(ev.Item) is { } custom)
            custom.HandlePickedUp(ev.Item, ev.Player);
    }

    // アーマーは PickedUpItem を通らず、専用イベントで来る。
    private static void OnAnyPickedUpArmor(PlayerPickedUpArmorEventArgs ev)
    {
        if (ev.BodyArmorItem is not { } armor) return;

        if (Of(armor.Serial) is { } custom)
            custom.HandlePickedUp(armor, ev.Player);
    }

    // SCP-330 のキャンディも専用イベント。
    private static void OnAnyPickedUpScp330(PlayerPickedUpScp330EventArgs ev)
    {
        if (Of(ev.CandyItem.Serial) is { } custom)
            custom.HandlePickedUp(ev.CandyItem, ev.Player);
    }

    private static void OnAnyDropping(PlayerDroppingItemEventArgs ev)
    {
        if (Of(ev.Item.Serial) is { } item)
            item.Invoke(() => item.OnDropping(ev), nameof(OnDropping));
    }

    private static void OnAnyDropped(PlayerDroppedItemEventArgs ev)
    {
        if (Of(ev.Pickup) is { } custom)
        {
            custom.Customize(ev.Pickup);
            custom.Invoke(() => custom.OnDropped(ToExiled(ev.Player)), nameof(OnDropped));
            custom.AttachPickupLight();
            custom.AttachPickupModel();
        }
    }

    private static void OnAnyChangedItem(PlayerChangedItemEventArgs ev)
    {
        if (Of(ev.OldItem) is { } unequipped)
            unequipped.Invoke(() => unequipped.OnUnequipped(ToExiled(ev.Player)), nameof(OnUnequipped));

        if (Of(ev.NewItem) is { } equipped)
        {
            equipped.Invoke(() => equipped.OnEquipped(ToExiled(ev.Player)), nameof(OnEquipped));
            equipped.ShowItemHint(ev.Player, equipped.SelectedHint, equipped.SelectedHintDuration);
        }
    }

    // 以下 4 つの ev.UsableItem / ev.FirearmItem は、そのイベントが発火した時点で
    // 必ず存在する。念のための null チェックは入れない。
    private static void OnAnyUsingItem(PlayerUsingItemEventArgs ev)
    {
        if (Of(ev.UsableItem.Serial) is { } custom)
            custom.Invoke(() => custom.OnUsing(ev), nameof(OnUsing));
    }

    private static void OnAnyUsageEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev)
    {
        if (Of(ev.UsableItem.Serial) is { } custom)
            custom.Invoke(() => custom.OnUseCompleting(ev), nameof(OnUseCompleting));
    }

    private static void OnAnyUsedItem(PlayerUsedItemEventArgs ev)
    {
        if (Of(ev.UsableItem.Serial) is { } custom)
            custom.Invoke(custom.OnUsed, nameof(OnUsed));
    }

    private static void OnAnyShootingWeapon(PlayerShootingWeaponEventArgs ev)
    {
        if (Of(ev.FirearmItem.Serial) is { } custom)
            custom.Invoke(() => custom.OnShooting(ev), nameof(OnShooting));
    }

    private static void OnAnyShotWeapon(PlayerShotWeaponEventArgs ev)
    {
        if (Of(ev.FirearmItem.Serial) is { } custom)
            custom.Invoke(custom.OnShot, nameof(OnShot));
    }

    private static void OnRoundRestarted()
    {
        foreach (CustomItem custom in new List<CustomItem>(BySerial.Values))
        {
            custom.Release();
        }

        BySerial.Clear();
    }
}
