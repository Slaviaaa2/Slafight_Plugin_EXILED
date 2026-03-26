using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.API.Features;

public enum CRoomType
{
    None,
    LczExClassD,
}

public struct VentPoint
{
    // 入口識別
    public RoomType? JoinRoomType;     // 実 RoomType で指定したいとき
    public CRoomType? JoinCustomType;  // 机上 CRoomType で指定したいとき

    // 出口識別
    public RoomType? ExitRoomType;
    public CRoomType? ExitCustomType;

    // RoomType 基準のローカル座標（ExitRoomType を使う場合用）
    public Vector3 ExitLocalPosition;

    // ワールド座標で直接指定したいとき (!= zero を優先使用)
    public Vector3 ExitWorldPosition;

    public VentPoint(RoomType? joinRoomType, CRoomType? joinCustomType, RoomType? exitRoomType, CRoomType? exitCustomType, Vector3 exitLocalPosition, Vector3 exitWorldPosition)
    {
        JoinRoomType = joinRoomType;
        JoinCustomType = joinCustomType;
        ExitRoomType = exitRoomType;
        ExitCustomType = exitCustomType;
        ExitLocalPosition = exitLocalPosition;
        ExitWorldPosition = exitWorldPosition;
    }
}

public static class GlobalVentManager
{
    public static readonly List<VentPoint> VentPoints = [];

    public static void RegisterVentPoint(VentPoint ventPoint) => VentPoints.Add(ventPoint);

    public static void UnregisterAllVentPoints() => VentPoints.Clear();

    // ===== 共通: VentPoint → 出口ワールド座標・回転 =====
    private static (Vector3 worldPosition, Quaternion worldRotation)? GetExitTransform(VentPoint point)
    {
        if (point.ExitWorldPosition != Vector3.zero)
            return (point.ExitWorldPosition, Quaternion.identity);

        if (!point.ExitRoomType.HasValue)
        {
            Log.Error("[VentPoint] ExitRoomType is null and ExitWorldPosition is zero.");
            return null;
        }

        var data = StaticUtils.GetWorldFromRoomLocal(point.ExitRoomType.Value, point.ExitLocalPosition, Vector3.zero);
        return (data.worldPosition, data.worldRotation);
    }

    // ===== RoomType → RoomType =====
    public static bool TryTrigger(Player? player, RoomType joinRoom, RoomType exitRoom)
    {
        if (player == null)
        {
            Log.Error("[VentPoint]Failed trigger to VentPoint:\n Reason: Null Player lol");
            return false;
        }

        var matches = VentPoints
            .Where(p =>
                p.JoinRoomType == joinRoom &&
                p.ExitRoomType == exitRoom)
            .ToArray();

        if (!TryPickSingle(matches, joinRoom.ToString(), exitRoom.ToString(), out var point))
            return false;

        var exit = GetExitTransform(point);
        if (exit == null)
            return false;

        player.Position = exit.Value.worldPosition;
        player.Rotation = exit.Value.worldRotation;
        return true;
    }

    // ===== RoomType → CRoomType =====
    public static bool TryTrigger(Player? player, RoomType joinRoom, CRoomType exitCustom)
    {
        if (player == null)
        {
            Log.Error("[VentPoint]Failed trigger to VentPoint:\n Reason: Null Player lol");
            return false;
        }

        var matches = VentPoints
            .Where(p =>
                p.JoinRoomType == joinRoom &&
                p.ExitCustomType == exitCustom)
            .ToArray();

        if (!TryPickSingle(matches, joinRoom.ToString(), exitCustom.ToString(), out var point))
            return false;

        var exit = GetExitTransform(point);
        if (exit == null)
            return false;

        player.Position = exit.Value.worldPosition;
        player.Rotation = exit.Value.worldRotation;
        return true;
    }

    // ===== CRoomType → CRoomType （必要なら）=====
    public static bool TryTrigger(Player? player, CRoomType joinCustom, CRoomType exitCustom)
    {
        if (player == null)
        {
            Log.Error("[VentPoint]Failed trigger to VentPoint:\n Reason: Null Player lol");
            return false;
        }

        var matches = VentPoints
            .Where(p =>
                p.JoinCustomType == joinCustom &&
                p.ExitCustomType == exitCustom)
            .ToArray();

        if (!TryPickSingle(matches, joinCustom.ToString(), exitCustom.ToString(), out var point))
            return false;

        var exit = GetExitTransform(point);
        if (exit == null)
            return false;

        player.Position = exit.Value.worldPosition;
        player.Rotation = exit.Value.worldRotation;
        return true;
    }

    // ===== VentPoint も欲しい版（例: デバッグ用）=====
    public static bool TryTrigger(Player? player, RoomType joinRoom, RoomType exitRoom, out VentPoint? ventPoint)
    {
        ventPoint = null;

        if (player == null)
        {
            Log.Error("[VentPoint]Failed trigger to VentPoint:\n Reason: Null Player lol");
            return false;
        }

        var matches = VentPoints
            .Where(p =>
                p.JoinRoomType == joinRoom &&
                p.ExitRoomType == exitRoom)
            .ToArray();

        if (!TryPickSingle(matches, joinRoom.ToString(), exitRoom.ToString(), out var point))
            return false;

        var exit = GetExitTransform(point);
        if (exit == null)
            return false;

        player.Position = exit.Value.worldPosition;
        player.Rotation = exit.Value.worldRotation;
        ventPoint = point;
        return true;
    }
        
    // ふんわり版: joinRoom だけ渡して一番それっぽい VentPoint を拾う
    // join だけ渡して、一番それっぽい VentPoint を選ぶ版
    public static bool TryTriggerLoose(Player? player, RoomType joinRoom, out VentPoint point)
    {
        point = default;

        if (player == null)
        {
            Log.Error("[VentPoint]Failed trigger (loose): Null Player");
            return false;
        }

        // Join 一致を全部拾う
        var candidates = VentPoints
            .Where(p => p.JoinRoomType == joinRoom)
            .ToList();

        if (candidates.Count == 0)
        {
            Log.Warn($"[VentPoint]Loose trigger: no candidates for join {joinRoom}");
            return false;
        }

        // WorldPos 指定ありを優先
        var withWorld = candidates
            .Where(p => p.ExitWorldPosition != Vector3.zero)
            .ToList();

        if (withWorld.Count == 1)
        {
            point = withWorld[0];
        }
        else if (withWorld.Count > 1)
        {
            Log.Warn($"[VentPoint]Loose trigger: multiple world-pos candidates for join {joinRoom}, using first.");
            point = withWorld[0];
        }
        else
        {
            // WorldPos なければ最初の 1 件
            if (candidates.Count > 1)
                Log.Warn($"[VentPoint]Loose trigger: multiple local candidates for join {joinRoom}, using first.");

            point = candidates[0];
        }

        var exit = GetExitTransform(point);
        if (exit == null)
            return false;

        player.Position = exit.Value.worldPosition;
        player.Rotation = exit.Value.worldRotation;
        return true;
    }

    // ===== 共通: 0 or 1 or multiple を処理 =====
    private static bool TryPickSingle(VentPoint[] matches, string joinLabel, string exitLabel, out VentPoint point)
    {
        point = default;

        if (matches.Length <= 0)
        {
            Log.Error($"[VentPoint]Failed trigger to VentPoint:\n Reason: VentPoint not found!\n Join: {joinLabel}, Exit: {exitLabel}");
            return false;
        }

        if (matches.Length > 1)
        {
            Log.Error($"[VentPoint]Failed trigger to VentPoint:\n Reason: VentPoint multiple detected!\n Join: {joinLabel}, Exit: {exitLabel}");
            return false;
        }

        point = matches[0];
        return true;
    }
}