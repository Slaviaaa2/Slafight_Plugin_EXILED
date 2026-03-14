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

/// <summary>
/// NVG のライト・ブラックアウトとバッテリー管理を行うマネージャ.
/// NetworkVisibilityExtensions を使って「所有者とその観戦者だけ」に見えるよう制御する。
/// </summary>
public static class NvgManager
{
    private const float MaxBattery   = 100f;
    private const float DrainPerTick = 0.5f * TickInterval;
    private const float TickInterval = 0.1f;

    private static readonly Dictionary<ushort, NvgRuntimeData> ActiveData  = new();
    private static readonly Dictionary<ushort, float>          BatteryData = new();

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

    public static void StartNvg(Player player, ushort serial)
    {
        if (player == null) return;
        Log.Debug($"[NvgManager] StartNvg: {player.Nickname} serial={serial}");

        // ★電池0%なら起動しない
        if (BatteryData.TryGetValue(serial, out var savedBattery) && savedBattery <= 0f)
        {
            player.ShowHint("このNVGの電池は完全に切れています。", 3f);
            Log.Debug($"[NvgManager] StartNvg拒否: 電池切れ serial={serial}");
            return;
        }

        StopNvgBySerial(serial);

        float battery = BatteryData.TryGetValue(serial, out var saved) ? saved : MaxBattery;

        var light = CreateNvgLight(player);
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

        // ★アイテム破棄時は電池データもクリア（再取得時は満タン）
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
            var data   = kv.Value;
            var light  = data.NvgLight;
            var ident  = light?.Base?.netIdentity;
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

    private static Light? CreateNvgLight(Player player)
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

            light.Range     = 30f;
            light.Intensity = 10000f;
            light.Color     = new Color(0.6f, 1f, 0.6f);

            light.Transform.SetParent(player.Transform, true);

            var identity = light.Base.netIdentity;
            identity.InitShowState();          // 全員 Hide
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

            if (player == null || !player.IsConnected)
                yield break;

            if (!ActiveData.TryGetValue(serial, out var data))
                yield break;

            float battery = BatteryData.TryGetValue(serial, out var b) ? b : 0f;

            if (battery <= 0f)
            {
                // 電池切れ → 完全終了
                BatteryData[serial] = 0f;  // 明示的に0保存
            
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

                EnsureBlackout(player, data);
                player.ShowHint("NVGの電池が切れた…視界が真っ暗になった。", 5f);
                RefreshVisibilityForAll();

                // ★重要: ここでループを終了し、ActiveDataから削除
                StopNvgBySerial(serial);
                yield break;
            }

            // 通常の電池消費処理
            battery -= DrainPerTick;
            if (battery < 0f) battery = 0f;
            BatteryData[serial] = battery;

            if (data.NvgLight?.Base != null)
            {
                float ratio = battery / MaxBattery;
                data.NvgLight.Intensity = 10000f * (0.4f + 0.6f * ratio);
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

                identity.InitShowState();          // 全員 Hide
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

    private class NvgRuntimeData
    {
        public ushort Serial { get; set; }
        public int OwnerId { get; set; }
        public Light? NvgLight { get; set; }
        public Primitive? Blackout { get; set; }
        public CoroutineHandle CoroutineHandle { get; set; }
    }
}
