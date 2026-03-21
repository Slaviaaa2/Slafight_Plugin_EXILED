using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Slafight_Plugin_EXILED.API.Enums;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.MainHandlers;

public static class DebugModeHandler
{
    public static void Register()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
    }

    public static void Unregister()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSettingValueReceived;
    }

    // =====================
    //  デバッグモード管理
    // =====================

    private static readonly HashSet<Player> DebugModePlayers = new();

    /// <summary>指定プレイヤーのデバッグモードが ON かどうか。</summary>
    public static bool IsDebugMode(Player player)
    {
        return player.CheckPermission(PlayerPermissions.Noclip) && DebugModePlayers.Contains(player);
    }

    // =====================
    //  デバッグ状態ストア
    // =====================

    /// <summary>最後に触ったドアのスナップショット。</summary>
    public readonly record struct DoorInfo(
        string  DoorType,
        string  DoorName,
        string  RoomType,
        Vector3 LocalPos,
        Vector3 LocalEuler,
        Vector3 RoomEuler
    );

    private static readonly Dictionary<int, DoorInfo> _lastDoors = new();

    /// <summary>最後に触ったドア情報を更新する（EventHandler の DoorGet から呼ぶ）。</summary>
    public static void UpdateDoor(Player player, DoorInfo info)
        => _lastDoors[player.Id] = info;

    /// <summary>保持しているドア情報を取得する。</summary>
    public static bool TryGetDoor(Player player, out DoorInfo info)
        => _lastDoors.TryGetValue(player.Id, out info);

    // =====================
    //  設定受信
    // =====================

    public static void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase @base)
    {
        if (@base is not SSTwoButtonsSetting twoButton || twoButton.SettingId != 6)
            return;

        var player = Player.Get(hub);
        if (player == null || !player.IsConnected)
            return;

        // SyncIsA: true = 左ボタン(ON), false = 右ボタン(OFF)
        if (twoButton.SyncIsA)
        {
            DebugModePlayers.Add(player);
            // ignored
        }
        else
        {
            DebugModePlayers.Remove(player);
            // ignored
        }

        try
        {
            Plugin.Singleton.PlayerHUD.HintSync(SyncType.PHUD_Debug, "", player);
        }
        catch
        {
            // ignored
        }

        Log.Debug($"[DebugMode] {player.Nickname} => {(twoButton.SyncIsA ? "ON" : "OFF")}");
    }

    // =====================
    //  クリーンアップ
    // =====================

    /// <summary>プレイヤー退出時にエントリを一括掃除する。</summary>
    public static void RemovePlayer(Player player)
    {
        DebugModePlayers.Remove(player);
        _lastDoors.Remove(player.Id);
    }
}