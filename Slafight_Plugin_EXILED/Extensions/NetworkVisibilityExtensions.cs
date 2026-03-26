using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using MEC;
using Mirror;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;

namespace Slafight_Plugin_EXILED.Extensions;

/// <summary>
/// NetworkIdentity を持つ任意のオブジェクト（Primitive / Light / AdminToy / Schematic等）の
/// 表示を特定プレイヤーだけに限定するヘルパー群。
/// EXILED の MirrorExtensions.SendSpawnMessageMethodInfo 経由で SendSpawnMessage を呼ぶ。
/// </summary>
public static class NetworkVisibilityExtensions
{
    // =========================================================
    // 表示対象プレイヤー管理
    // key: netId / value: 表示するプレイヤーIDのリスト
    // =========================================================
    private static readonly Dictionary<uint, List<int>> _showPlayers = new();
    private static readonly Dictionary<uint, NetworkIdentity> _identityCache = new();

    // =========================================================
    // Register / Unregister（Plugin 側から呼ぶこと）
    // =========================================================

    public static void Register()
    {
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        Exiled.Events.Handlers.Player.Verified     += OnVerified;
    }

    public static void Unregister()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        Exiled.Events.Handlers.Player.Verified     -= OnVerified;
    }

    private static void OnRoundStarted()
    {
        _showPlayers.Clear();
        _identityCache.Clear();
    }

    private static void OnVerified(Exiled.Events.EventArgs.Player.VerifiedEventArgs ev)
    {
        if (ev?.Player == null) return;

        foreach (var netId in _showPlayers.Keys.ToList())
        {
            if (!_identityCache.TryGetValue(netId, out var identity) || identity == null) continue;
            RefreshOne(netId, identity, ev.Player);
        }
    }

    // =========================================================
    // 低レベル送信
    // Mirror を Publicize した上で ShowForConnection / HideForConnection を直接呼ぶ。
    // SendSpawnMessage と違い observers 管理も行われるため再接続後も安全。
    // =========================================================

    /// <summary>指定プレイヤーにオブジェクトを表示させる。</summary>
    public static void ShowNetworkIdentity(this Player player, NetworkIdentity identity)
    {
        if (player?.Connection == null || identity == null) return;
        try
        {
            NetworkServer.ShowForConnection(identity, player.Connection);
        }
        catch (Exception ex)
        {
            Log.Warn($"[NetworkVisibility] ShowNetworkIdentity 失敗: {ex.Message}");
        }
    }

    /// <summary>指定プレイヤーからオブジェクトを非表示にする。</summary>
    public static void HideNetworkIdentity(this Player player, NetworkIdentity identity)
    {
        if (player?.Connection == null || identity == null) return;
        try
        {
            NetworkServer.HideForConnection(identity, player.Connection);
        }
        catch (Exception ex)
        {
            Log.Warn($"[NetworkVisibility] HideNetworkIdentity 失敗: {ex.Message}");
        }
    }

    // =========================================================
    // 内部 Refresh
    // =========================================================

    private static void RefreshAll(uint netId, NetworkIdentity identity)
    {
        if (identity == null) return;
        if (!_showPlayers.TryGetValue(netId, out var players)) return;

        foreach (var pid in players.ToList())
        {
            if (!Player.TryGet(pid, out var target) || target == null) continue;
            target.ShowNetworkIdentity(identity);
        }

        foreach (var player in Player.List)
        {
            if (player == null) continue;
            if (players.Contains(player.Id)) continue;
            player.HideNetworkIdentity(identity);
        }
    }

    private static void RefreshOne(uint netId, NetworkIdentity identity, Player player)
    {
        if (identity == null || player == null) return;
        if (!_showPlayers.TryGetValue(netId, out var players)) return;

        if (players.Contains(player.Id))
            player.ShowNetworkIdentity(identity);
        else
            player.HideNetworkIdentity(identity);
    }

    // =========================================================
    // コア API（NetworkIdentity 直接操作）
    // Primitive / Light / AdminToy / Schematic など何でも使える
    // =========================================================

    /// <summary>
    /// NetworkIdentity を表示管理に登録する。
    /// Spawn 直後に呼ぶこと。呼んだ時点で全員に Hide が送られる。
    /// </summary>
    public static void InitShowState(this NetworkIdentity identity)
    {
        if (identity == null) return;
        uint netId = identity.netId;

        if (_showPlayers.ContainsKey(netId))
            _showPlayers[netId].Clear();
        else
            _showPlayers[netId] = [];

        _identityCache[netId] = identity;
        RefreshAll(netId, identity);
    }

    /// <summary>表示管理から除外する。オブジェクト破棄前に呼ぶこと。</summary>
    public static void RemoveShowState(this NetworkIdentity identity)
    {
        if (identity == null) return;
        _showPlayers.Remove(identity.netId);
        _identityCache.Remove(identity.netId);
    }

    /// <summary>指定プレイヤーへの表示 ON/OFF を切り替える。</summary>
    public static void SetShowState(this NetworkIdentity identity, Player player, bool show)
    {
        if (identity == null || player == null) return;
        uint netId = identity.netId;

        if (!_showPlayers.ContainsKey(netId))
        {
            _showPlayers[netId] = [];
            _identityCache[netId] = identity;
        }

        var players = _showPlayers[netId];
        if (show) { if (!players.Contains(player.Id)) players.Add(player.Id); }
        else      { players.Remove(player.Id); }

        RefreshOne(netId, identity, player);
    }

    // =========================================================
    // Primitive 向け糖衣構文
    // =========================================================

    public static void InitShowState(this Primitive primitive)
        => primitive?.Base?.netIdentity?.InitShowState();

    public static void RemoveShowState(this Primitive primitive)
        => primitive?.Base?.netIdentity?.RemoveShowState();

    public static void SetShowState(this Primitive primitive, Player player, bool show)
        => primitive?.Base?.netIdentity?.SetShowState(player, show);

    /// <summary>Primitive をプレイヤーの Transform に追従させる。</summary>
    public static void AttachToPlayer(this Primitive primitive, Player player, Vector3 localOffset)
    {
        if (primitive?.Base == null || player?.Transform == null) return;
        try
        {
            var t = primitive.Base.gameObject.transform;
            t.SetParent(player.Transform);
            t.localPosition = localOffset;
            t.localRotation = Quaternion.identity;
        }
        catch (Exception ex) { Log.Warn($"[NetworkVisibility] AttachToPlayer 失敗: {ex.Message}"); }
    }

    /// <summary>Primitive を安全に破棄する。</summary>
    public static void SafeDestroy(this Primitive? primitive)
    {
        if (primitive?.Base == null) return;
        primitive.RemoveShowState();
        try { NetworkServer.Destroy(primitive.Base.gameObject); }
        catch (Exception ex) { Log.Warn($"[NetworkVisibility] SafeDestroy(Primitive) 失敗: {ex.Message}"); }
    }

    // =========================================================
    // Light 向け糖衣構文
    // =========================================================

    public static void InitShowState(this Light light)
        => light?.Base?.netIdentity?.InitShowState();

    public static void RemoveShowState(this Light light)
        => light?.Base?.netIdentity?.RemoveShowState();

    public static void SetShowState(this Light light, Player player, bool show)
        => light?.Base?.netIdentity?.SetShowState(player, show);

    /// <summary>Light を安全に破棄する。</summary>
    public static void SafeDestroy(this Light? light)
    {
        if (light?.Base == null) return;
        light.RemoveShowState();
        try { NetworkServer.Destroy(light.Base.gameObject); }
        catch (Exception ex) { Log.Warn($"[NetworkVisibility] SafeDestroy(Light) 失敗: {ex.Message}"); }
    }

    // =========================================================
    // AdminToyBase 向け糖衣構文（ShootingTarget など）
    // =========================================================

    public static void InitShowState(this AdminToys.AdminToyBase toy)
        => toy?.netIdentity?.InitShowState();

    public static void RemoveShowState(this AdminToys.AdminToyBase toy)
        => toy?.netIdentity?.RemoveShowState();

    public static void SetShowState(this AdminToys.AdminToyBase toy, Player player, bool show)
        => toy?.netIdentity?.SetShowState(player, show);

    // =========================================================
    // IEnumerable<NetworkIdentity> 向け一括操作
    // Schematic.NetworkIdentities など複数identity持つものに便利
    // =========================================================

    /// <summary>複数の NetworkIdentity を一括で表示管理に登録する。</summary>
    public static void InitShowState(this IEnumerable<NetworkIdentity> identities)
    {
        foreach (var identity in identities)
            identity?.InitShowState();
    }

    /// <summary>複数の NetworkIdentity を一括で表示管理から除外する。</summary>
    public static void RemoveShowState(this IEnumerable<NetworkIdentity> identities)
    {
        foreach (var identity in identities)
            identity?.RemoveShowState();
    }

    /// <summary>複数の NetworkIdentity の表示を一括で ON/OFF する。</summary>
    public static void SetShowState(this IEnumerable<NetworkIdentity> identities, Player player, bool show)
    {
        foreach (var identity in identities)
            identity?.SetShowState(player, show);
    }

    // =========================================================
    // ファクトリ
    // =========================================================

    /// <summary>
    /// 指定プレイヤーにだけ見えるブラックアウト Primitive を生成して返す。
    /// 不要になったら SafeDestroy() で破棄すること。
    /// </summary>
    public static Primitive? CreateBlackoutForPlayer(
        Player owner,
        Vector3 position,
        Vector3? scale = null)
    {
        if (owner?.ReferenceHub == null) return null;

        try
        {
            var blackout = Primitive.Create(
                PrimitiveType.Cube,
                position,
                owner.Rotation.eulerAngles,
                scale ?? Vector3.one * 1.8f,
                true,
                Color.black);

            if (blackout?.Base == null) return null;
            blackout.Collidable = false;

            // Spawn が全員に届いた後に Hide/Show を送る
            Timing.CallDelayed(0f, () =>
            {
                if (blackout?.Base == null) return;
                blackout.InitShowState();           // 全員 Hide
                blackout.SetShowState(owner, true); // 所有者だけ Show
            });

            return blackout;
        }
        catch (Exception ex)
        {
            Log.Error($"[NetworkVisibility] CreateBlackoutForPlayer 失敗: {ex.Message}");
            return null;
        }
    }
}