using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using Exiled.Events.EventArgs.Player;
using MEC;
using Mirror;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;

namespace Slafight_Plugin_EXILED.CustomItems;

// ============================================================
// NVG プロファイル（CustomItem 側で定義して StartNvg に渡す）
// ============================================================

/// <summary>
/// NVG ライトの挙動をアイテムごとに定義するプロファイル。
/// CustomItem 側で new して StartNvg() に渡す。
/// </summary>
public readonly struct NvgProfile
{
    // --- 電池 ---
    /// <summary>
    /// 1秒あたりの電池消費量（%）。0 以下で無限。
    /// </summary>
    public float DrainPerSecond { get; init; }

    // --- ライト ---
    public Color  LightColor     { get; init; }
    public float  LightRange     { get; init; }
    public float  LightIntensity { get; init; }

    // --- ブラックアウト ---
    /// <summary>
    /// true のとき、電池切れで黒いキューブを被せて視界を塞ぐ。
    /// </summary>
    public bool UseBlackout { get; init; }

    // --- デフォルト値（標準 NVG と同じ挙動） ---
    public static NvgProfile Default => new()
    {
        DrainPerSecond = 5f,                        // 20秒で空
        LightColor     = new Color(0.6f, 1f, 0.6f), // 緑
        LightRange     = 30f,
        LightIntensity = 10000f,
        UseBlackout    = true,
    };
}

// ============================================================
// NvgManager
// ============================================================

/// <summary>
/// NVG のライト・ブラックアウトとバッテリー管理を行うマネージャ.
/// NetworkVisibilityExtensions を使って「所有者とその観戦者だけ」に見えるよう制御する。
/// </summary>
public static class NvgManager
{
    private const float MaxBattery   = 100f;
    private const float TickInterval = 0.1f;

    private static readonly Dictionary<ushort, NvgRuntimeData> ActiveData  = new();
    private static readonly Dictionary<ushort, float>          BatteryData = new();

    // --------------------------------------------------------
    // イベント登録 / 解除
    // --------------------------------------------------------

    public static void Register()
    {
        Exiled.Events.Handlers.Server.RoundStarted            += OnRoundStarted;
        Exiled.Events.Handlers.Player.Verified                += OnVerified;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += OnChangingSpectator;
        Exiled.Events.Handlers.Player.ChangingRole            += OnChangingRole;
        Exiled.Events.Handlers.Player.Died                    += OnDied;
    }

    public static void Unregister()
    {
        Exiled.Events.Handlers.Server.RoundStarted            -= OnRoundStarted;
        Exiled.Events.Handlers.Player.Verified                -= OnVerified;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= OnChangingSpectator;
        Exiled.Events.Handlers.Player.ChangingRole            -= OnChangingRole;
        Exiled.Events.Handlers.Player.Died                    -= OnDied;
    }

    // --------------------------------------------------------
    // イベントハンドラ
    // --------------------------------------------------------

    private static void OnRoundStarted()
    {
        foreach (var data in ActiveData.Values)
            KillRuntimeData(data);
        ActiveData.Clear();
        BatteryData.Clear();
    }

    private static void OnVerified(VerifiedEventArgs ev)
    {
        if (ev?.Player == null) return;

        Timing.CallDelayed(1f, () =>
        {
            if (ev.Player == null || !ev.Player.IsConnected) return;
            RefreshVisibilityForPlayer(ev.Player);
        });
    }

    private static void OnChangingSpectator(ChangingSpectatedPlayerEventArgs ev)
    {
        if (ev?.Player == null) return;

        // 旧対象のライトを隠す
        if (ev.OldTarget != null && ev.OldTarget != ev.NewTarget)
        {
            foreach (var data in ActiveData.Values)
            {
                if (data.OwnerId == ev.OldTarget.Id && data.NvgLight?.Base?.netIdentity != null)
                    data.NvgLight.Base.netIdentity.SetShowState(ev.Player, false);
            }
        }

        // 新対象のライトを見せる
        if (ev.NewTarget != null)
        {
            foreach (var data in ActiveData.Values)
            {
                if (data.OwnerId == ev.NewTarget.Id && data.NvgLight?.Base?.netIdentity != null)
                    data.NvgLight.Base.netIdentity.SetShowState(ev.Player, true);
            }
        }
    }

    private static void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (ev?.Player == null) return;
        StopNvgAndHideFromSpectators(ev.Player);
    }

    private static void OnDied(DiedEventArgs ev)
    {
        if (ev?.Player == null) return;
        StopNvgAndHideFromSpectators(ev.Player);
    }

    // --------------------------------------------------------
    // 公開 API
    // --------------------------------------------------------

    /// <summary>
    /// NVG を起動する。プロファイル未指定時は NvgProfile.Default を使用。
    /// </summary>
    public static void StartNvg(Player player, ushort serial, NvgProfile? profile = null)
    {
        if (player == null) return;

        var prof = profile ?? NvgProfile.Default;
        Log.Debug($"[NvgManager] StartNvg: {player.Nickname} serial={serial} drain={prof.DrainPerSecond}/s");

        // 電池0%なら起動しない（無限電池アイテムは DrainPerSecond<=0 なので常に起動可）
        bool isInfinite = prof.DrainPerSecond <= 0f;
        if (!isInfinite && BatteryData.TryGetValue(serial, out var savedBattery) && savedBattery <= 0f)
        {
            player.ShowHint("このNVGの電池は完全に切れています。", 3f);
            Log.Debug($"[NvgManager] StartNvg拒否: 電池切れ serial={serial}");
            return;
        }

        StopNvgBySerial(serial);

        float battery = isInfinite ? MaxBattery
                      : BatteryData.TryGetValue(serial, out var saved) ? saved : MaxBattery;

        var light = CreateNvgLight(player, prof);
        if (light == null)
        {
            Log.Error($"[NvgManager] StartNvg: NVGライト生成失敗 ({player.Nickname})");
            return;
        }

        var data = new NvgRuntimeData
        {
            Serial   = serial,
            OwnerId  = player.Id,
            NvgLight = light,
            Profile  = prof,
        };

        BatteryData[serial]  = battery;
        data.CoroutineHandle = Timing.RunCoroutine(BatteryLoop(player, serial));
        ActiveData[serial]   = data;

        Timing.CallDelayed(0.5f, RefreshVisibilityForAll);
    }

    public static void StopNvg(Player player, ushort serial)
    {
        if (player == null) return;
        StopNvgBySerial(serial);
    }

    // --------------------------------------------------------
    // 内部停止処理
    // --------------------------------------------------------

    private static void StopNvgBySerial(ushort serial)
    {
        if (!ActiveData.TryGetValue(serial, out var data))
            return;

        ActiveData.Remove(serial);
        KillRuntimeData(data);
        BatteryData.Remove(serial);

        RefreshVisibilityForAll();
    }

    private static void StopNvgAndHideFromSpectators(Player player)
    {
        if (player == null) return;

        var entry = ActiveData.Values.FirstOrDefault(d => d.OwnerId == player.Id);
        if (entry == null) return;

        if (entry.NvgLight?.Base?.netIdentity != null)
        {
            foreach (var spectator in Player.List)
            {
                if (spectator == null || spectator == player) continue;
                entry.NvgLight.Base.netIdentity.SetShowState(spectator, false);
            }
        }

        StopNvgBySerial(entry.Serial);
    }

    // --------------------------------------------------------
    // 可視状態リフレッシュ
    // --------------------------------------------------------

    private static void RefreshVisibilityForPlayer(Player player)
    {
        if (player == null) return;

        foreach (var kv in ActiveData)
        {
            var data  = kv.Value;
            var light = data.NvgLight;
            var ident = light?.Base?.netIdentity;
            if (ident == null) continue;

            bool shouldShow =
                player.Id == data.OwnerId ||
                (!player.IsAlive && player.CurrentSpectatingPlayers.Any(s => s?.Id == data.OwnerId));

            ident.SetShowState(player, shouldShow);
        }
    }

    private static void RefreshVisibilityForAll()
    {
        foreach (var player in Player.List)
        {
            if (player == null || !player.IsConnected) continue;
            RefreshVisibilityForPlayer(player);
        }
    }

    // --------------------------------------------------------
    // ライト生成
    // --------------------------------------------------------

    private static Light? CreateNvgLight(Player player, NvgProfile prof)
    {
        try
        {
            var light = Light.Create(
                player.CameraTransform.position,
                player.Rotation.eulerAngles,
                null,
                spawn: true);

            if (light?.Base == null || light.Base.netIdentity == null)
            {
                Log.Error("[NvgManager] CreateNvgLight: Light または netIdentity が null");
                return null;
            }

            // プロファイルの値を適用
            light.Range     = prof.LightRange;
            light.Intensity = prof.LightIntensity;
            light.Color     = prof.LightColor;

            light.Transform.SetParent(player.Transform, true);

            var identity = light.Base.netIdentity;
            identity.InitShowState();
            identity.SetShowState(player, true);

            foreach (var spectator in Player.List)
            {
                if (spectator == null || spectator == player) continue;
                if (!spectator.IsAlive && spectator.CurrentSpectatingPlayers.Contains(player))
                    identity.SetShowState(spectator, true);
            }

            return light;
        }
        catch (Exception ex)
        {
            Log.Error($"[NvgManager] CreateNvgLight 例外: {ex}");
            return null;
        }
    }

    // --------------------------------------------------------
    // バッテリーとブラックアウト
    // --------------------------------------------------------

    private static void KillRuntimeData(NvgRuntimeData data)
    {
        if (data == null) return;

        Timing.KillCoroutines(data.CoroutineHandle);

        if (data.NvgLight != null)
        {
            try
            {
                data.NvgLight.Base?.netIdentity?.RemoveShowState();
                if (data.NvgLight.Base?.gameObject != null)
                    NetworkServer.Destroy(data.NvgLight.Base.gameObject);
            }
            catch (Exception ex)
            {
                Log.Warn($"[NvgManager] KillRuntimeData: NvgLight 破棄失敗 {ex.Message}");
            }
            data.NvgLight = null;
        }

        if (data.Blackout != null)
        {
            try
            {
                data.Blackout.Base?.netIdentity?.RemoveShowState();
                if (data.Blackout.Base?.gameObject != null)
                    NetworkServer.Destroy(data.Blackout.Base.gameObject);
            }
            catch (Exception ex)
            {
                Log.Warn($"[NvgManager] KillRuntimeData: Blackout 破棄失敗 {ex.Message}");
            }
            data.Blackout = null;
        }
    }

    private static IEnumerator<float> BatteryLoop(Player player, ushort serial)
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(TickInterval);

            if (player == null || !player.IsConnected) yield break;
            if (!ActiveData.TryGetValue(serial, out var data)) yield break;

            var prof       = data.Profile;
            bool isInfinite = prof.DrainPerSecond <= 0f;

            // 無限電池: 強度そのままで電池表示なし
            if (isInfinite)
            {
                // 強度は常にプロファイル値のまま（変化なし）
                continue;
            }

            float battery = BatteryData.TryGetValue(serial, out var b) ? b : 0f;

            if (battery <= 0f)
            {
                BatteryData[serial] = 0f;

                // ライト破棄
                if (data.NvgLight != null)
                {
                    try
                    {
                        data.NvgLight.Base?.netIdentity?.RemoveShowState();
                        if (data.NvgLight.Base?.gameObject != null)
                            NetworkServer.Destroy(data.NvgLight.Base.gameObject);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"[NvgManager] BatteryLoop: NvgLight 破棄失敗 {ex.Message}");
                    }
                    data.NvgLight = null;
                }

                // プロファイルに応じてブラックアウト
                if (prof.UseBlackout)
                    EnsureBlackout(player, data);

                player.ShowHint("NVGの電池が切れた…視界が真っ暗になった。", 5f);
                RefreshVisibilityForAll();

                StopNvgBySerial(serial);
                yield break;
            }

            // 電池消費（TickInterval 分だけ減らす）
            float drain = prof.DrainPerSecond * TickInterval;
            battery = Math.Max(0f, battery - drain);
            BatteryData[serial] = battery;

            // 残量に応じて輝度を落とす
            if (data.NvgLight?.Base != null)
            {
                float ratio = battery / MaxBattery;
                data.NvgLight.Intensity = prof.LightIntensity * (0.4f + 0.6f * ratio);
            }

            player.ShowHint($"NVG電池: {(int)battery}%", 1f);
        }
    }

    private static void EnsureBlackout(Player player, NvgRuntimeData data)
    {
        if (data.Blackout != null) return;

        try
        {
            var blackout = Primitive.Create(
                PrimitiveType.Cube,
                player.Position + Vector3.up * 0.85f,
                player.Rotation.eulerAngles,
                Vector3.one * 1.8f,
                true,
                Color.black);

            if (blackout?.Base == null || blackout.Base.netIdentity == null)
            {
                Log.Error($"[NvgManager] EnsureBlackout: Primitive.Create 失敗 ({player.Nickname})");
                return;
            }

            blackout.Collidable = false;

            var identity = blackout.Base.netIdentity;

            Timing.CallDelayed(0f, () =>
            {
                if (!player.IsConnected || blackout.Base == null || identity == null) return;

                identity.InitShowState();
                identity.SetShowState(player, true);

                foreach (var spectator in Player.List)
                {
                    if (spectator == null || spectator == player) continue;
                    if (!spectator.IsAlive && spectator.CurrentSpectatingPlayers.Contains(player))
                        identity.SetShowState(spectator, true);
                }

                var t = blackout.Base.gameObject.transform;
                t.SetParent(player.Transform);
                t.localPosition = Vector3.up * 0.85f;
                t.localRotation = Quaternion.identity;
            });

            data.Blackout = blackout;
        }
        catch (Exception ex)
        {
            Log.Error($"[NvgManager] EnsureBlackout 例外: {ex}");
        }
    }

    // --------------------------------------------------------
    // 内部データ
    // --------------------------------------------------------

    private class NvgRuntimeData
    {
        public ushort          Serial          { get; set; }
        public int             OwnerId         { get; set; }
        public Light?          NvgLight        { get; set; }
        public Primitive?      Blackout        { get; set; }
        public CoroutineHandle CoroutineHandle { get; set; }
        public NvgProfile      Profile         { get; set; }  // ★追加
    }
}