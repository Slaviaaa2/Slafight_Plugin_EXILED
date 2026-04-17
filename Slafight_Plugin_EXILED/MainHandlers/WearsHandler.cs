using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.MainHandlers;

/// <summary>
/// プレイヤーに Schematic を「着せる」ためのユーティリティ。
/// - Wear / TryWear でスポーン＋WearFollowerアタッチ＋ロール情報保存
/// - RegisterExternal で外部生成済み Schematic を登録
/// - DestroyCoroutine でロール変化時に自動 Destroy
/// - ForceRemoveWear で外部から強制破壊
/// </summary>
public static class WearsHandler
{
    private static readonly Dictionary<int, PlayerRoleHelpers.PlayerRoleInfo> PlayerRoles = new();
    private static readonly Dictionary<int, SchematicObject> PlayerSchematics = new();

    private static CoroutineHandle _cleanupCoroutine;

    public static void Register()
    {
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestarting;
        Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
    }

    public static void Unregister()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestarting;
        Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;

        if (_cleanupCoroutine.IsRunning)
            Timing.KillCoroutines(_cleanupCoroutine);

        CleanupAll();
    }

    // ───────────────────────────────────────────
    //  Public API
    // ───────────────────────────────────────────

    /// <summary>指定プレイヤーの Schematic を強制破壊・クリーンアップ</summary>
    public static bool ForceRemoveWear(Player player)
    {
        if (player == null)
            return false;

        CleanupPlayer(player);
        return true;
    }

    /// <summary>全プレイヤーの Schematic を一括破壊・クリーンアップ</summary>
    public static void ForceRemoveAllWears() => CleanupAll();

    /// <summary>プレイヤーオブジェクト不要時用 ID 指定破壊</summary>
    public static bool ForceRemoveWearById(int playerId)
    {
        if (!PlayerSchematics.TryGetValue(playerId, out var schem))
            return false;

        schem.Destroy();
        PlayerSchematics.Remove(playerId);
        PlayerRoles.Remove(playerId);
        return true;
    }

    /// <summary>
    /// 既にスポーン済みの Schematic を登録する（外部用）。
    /// WearFollower を自動アタッチする。
    /// </summary>
    public static void RegisterExternal(Player player, SchematicObject schem, Vector3? offset = null)
    {
        if (player == null || schem == null)
            return;

        var id = player.Id;

        if (PlayerSchematics.TryGetValue(id, out var old))
        {
            old.Destroy();
            PlayerSchematics.Remove(id);
            PlayerRoles.Remove(id);
        }

        AttachFollower(schem, player.Transform, offset ?? Vector3.zero);

        PlayerSchematics[id] = schem;
        PlayerRoles[id] = player.GetRoleInfo();
    }

    /// <summary>
    /// Schematic 名を指定してスポーン＆追従。失敗時は何も返さない簡易版。
    /// </summary>
    public static void Wear(this Player player, string wearSchemName, Vector3? offset = null)
    {
        if (player == null)
            return;

        var id = player.Id;
        var offsetVector = offset ?? Vector3.zero;

        RemoveExisting(id);

        SchematicObject schem;

        try
        {
            schem = ObjectSpawner.SpawnSchematic(wearSchemName, player.Position + offsetVector);
            AttachFollower(schem, player.Transform, offsetVector);
        }
        catch (Exception e)
        {
            Log.Error($"[WearsHandler] Wear failed for {player.Nickname}: {e}");
            return;
        }

        if (schem == null)
            return;

        PlayerSchematics[id] = schem;
        PlayerRoles[id] = player.GetRoleInfo();
    }

    /// <summary>
    /// スポーン済みの SchematicObject を Wear させる版。
    /// </summary>
    public static void Wear(this Player player, SchematicObject schem, Vector3? offset = null)
    {
        if (player == null || schem == null)
            return;

        var id = player.Id;
        var offsetVector = offset ?? Vector3.zero;

        RemoveExisting(id);

        try
        {
            AttachFollower(schem, player.Transform, offsetVector);
        }
        catch (Exception e)
        {
            Log.Error($"[WearsHandler] Wear(SchematicObject) failed for {player.Nickname}: {e}");
            return;
        }

        PlayerSchematics[id] = schem;
        PlayerRoles[id] = player.GetRoleInfo();
    }

    /// <summary>
    /// 成否＋SchematicObject を取得したい場合はこちら。
    /// </summary>
    public static bool TryWear(this Player player, string wearSchemName, out SchematicObject schematicObject, Vector3? offset = null)
    {
        schematicObject = null;

        if (player == null)
            return false;

        var id = player.Id;
        var offsetVector = offset ?? Vector3.zero;

        RemoveExisting(id);

        SchematicObject schem;

        try
        {
            schem = ObjectSpawner.SpawnSchematic(wearSchemName, player.Position + offsetVector, player.Rotation);
            AttachFollower(schem, player.Transform, offsetVector);
        }
        catch (Exception e)
        {
            Log.Error($"[WearsHandler] TryWear failed for {player.Nickname}: {e}");
            return false;
        }

        if (schem == null)
            return false;

        PlayerSchematics[id] = schem;
        PlayerRoles[id] = player.GetRoleInfo();
        schematicObject = schem;
        return true;
    }
    
    /// <summary>
    /// 親Transform を明示指定できる TryWear。
    /// プレイヤー以外のオブジェクトに追従させたい場合に使用。
    /// </summary>
    public static bool TryWear(this Player player, string wearSchemName, Transform parent, out SchematicObject schematicObject, Vector3? offset = null)
    {
        schematicObject = null;

        if (player == null || parent == null)
            return false;

        var id = player.Id;
        var offsetVector = offset ?? Vector3.zero;

        RemoveExisting(id);

        SchematicObject schem;

        try
        {
            schem = ObjectSpawner.SpawnSchematic(wearSchemName, parent.position + offsetVector, player.Rotation);
            AttachFollower(schem, parent, offsetVector);
        }
        catch (Exception e)
        {
            Log.Error($"[WearsHandler] TryWear(parent) failed for {player.Nickname}: {e}");
            return false;
        }

        if (schem == null)
            return false;

        PlayerSchematics[id] = schem;
        PlayerRoles[id] = player.GetRoleInfo();
        schematicObject = schem;
        return true;
    }

    // ───────────────────────────────────────────
    //  Private helpers
    // ───────────────────────────────────────────

    /// <summary>
    /// SchematicObject に WearFollower をアタッチ（既存があれば差し替え）。
    /// SetParent は使わない。
    /// </summary>
    private static void AttachFollower(SchematicObject schem, Transform target, Vector3 offset)
    {
        // 既存コンポーネントを破棄してから付け直す
        var existing = schem.gameObject.GetComponent<WearFollower>();
        if (existing != null)
            UnityEngine.Object.Destroy(existing);

        var follower = schem.gameObject.AddComponent<WearFollower>();
        follower.Initialize(target, offset);
    }

    /// <summary>指定 ID にすでに Schematic が登録されていれば破壊して辞書から除去</summary>
    private static void RemoveExisting(int playerId)
    {
        if (!PlayerSchematics.TryGetValue(playerId, out var old))
            return;

        old.Destroy();
        PlayerSchematics.Remove(playerId);
        PlayerRoles.Remove(playerId);
    }

    // ───────────────────────────────────────────
    //  Event handlers
    // ───────────────────────────────────────────

    private static void OnRoundStarted()
    {
        if (_cleanupCoroutine.IsRunning)
            Timing.KillCoroutines(_cleanupCoroutine);

        _cleanupCoroutine = Timing.RunCoroutine(DestroyCoroutine());
    }

    private static void OnRoundRestarting()
    {
        if (_cleanupCoroutine.IsRunning)
            Timing.KillCoroutines(_cleanupCoroutine);

        CleanupAll();
    }

    private static void OnPlayerLeft(LeftEventArgs ev) => CleanupPlayer(ev.Player);

    // ───────────────────────────────────────────
    //  Coroutine
    // ───────────────────────────────────────────

    /// <summary>ロール変更を監視し、変化したプレイヤーの Schematic を自動 Destroy</summary>
    private static IEnumerator<float> DestroyCoroutine()
    {
        while (true)
        {
            if (!Round.InProgress)
            {
                yield return Timing.WaitForSeconds(1f);
                continue;
            }

            foreach (var kvp in PlayerRoles.ToList())
            {
                var player = Player.Get(kvp.Key);
                if (player == null)
                    continue;

                var current = player.GetRoleInfo();

                if (kvp.Value.Vanilla != current.Vanilla ||
                    kvp.Value.Custom  != current.Custom)
                {
                    CleanupPlayer(player);
                }
            }

            yield return Timing.WaitForSeconds(0.5f);
        }
    }

    // ───────────────────────────────────────────
    //  Cleanup
    // ───────────────────────────────────────────

    private static void CleanupPlayer(Player player)
    {
        if (player == null)
            return;

        var id = player.Id;

        if (PlayerSchematics.TryGetValue(id, out var schem))
        {
            schem.Destroy();
            PlayerSchematics.Remove(id);
        }

        PlayerRoles.Remove(id);
    }

    private static void CleanupAll()
    {
        foreach (var schem in PlayerSchematics.Values.ToList())
            schem.Destroy();

        PlayerSchematics.Clear();
        PlayerRoles.Clear();
    }
}