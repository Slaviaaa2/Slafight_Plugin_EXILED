using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.MainHandlers;

/// <summary>
/// プレイヤーに Schematic を「着せる」ためのユーティリティ。
/// - Wear / TryWear でスポーン＋親子付け＋ロール情報保存
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

    /// <summary>
    /// 外部から指定プレイヤーのSchematicを強制的に破壊・クリーンアップ
    /// </summary>
    public static bool ForceRemoveWear(Player player)
    {
        if (player == null)
            return false;

        CleanupPlayer(player);
        return true;
    }

    /// <summary>
    /// 外部から全プレイヤーのSchematicを一括破壊・クリーンアップ
    /// </summary>
    public static void ForceRemoveAllWears()
    {
        CleanupAll();
    }

    /// <summary>
    /// 指定IDのプレイヤーのSchematic破壊（プレイヤーオブジェクト不要時用）
    /// </summary>
    public static bool ForceRemoveWearById(int playerId)
    {
        if (PlayerSchematics.TryGetValue(playerId, out var schem))
        {
            schem.Destroy();
            PlayerSchematics.Remove(playerId);
            PlayerRoles.Remove(playerId);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 既にスポーン・親子付け済みの Schematic を登録する（LabApi など外部用）。
    /// </summary>
    public static void RegisterExternal(Player player, SchematicObject schem)
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

        PlayerSchematics[id] = schem;
        PlayerRoles[id] = player.GetRoleInfo();
    }

    /// <summary>
    /// 失敗時は何も返さない簡易版。
    /// </summary>
    public static void Wear(this Player player, string wearSchemName, Vector3? offset = null)
    {
        if (player == null)
            return;

        var id = player.Id;

        if (PlayerSchematics.TryGetValue(id, out var oldSchem))
        {
            oldSchem.Destroy();
            PlayerSchematics.Remove(id);
            PlayerRoles.Remove(id);
        }

        var offsetVector = offset ?? Vector3.zero;
        SchematicObject schem;

        try
        {
            schem = ObjectSpawner.SpawnSchematic(wearSchemName, player.Position + offsetVector);
            schem.transform.SetParent(player.Transform);
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
    /// スポーン済みのSchematicObjectをPlayerにWearさせる用。
    /// </summary>
    public static void Wear(this Player player, SchematicObject Schem, Vector3? offset = null)
    {
        if (player == null)
            return;

        var id = player.Id;

        if (PlayerSchematics.TryGetValue(id, out var oldSchem))
        {
            oldSchem.Destroy();
            PlayerSchematics.Remove(id);
            PlayerRoles.Remove(id);
        }

        var offsetVector = offset ?? Vector3.zero;

        try
        {
            Schem.transform.SetParent(player.Transform);
            Schem.Position = player.Transform.position + offsetVector;
        }
        catch (Exception e)
        {
            Log.Error($"[WearsHandler] Wear failed for {player.Nickname}: {e}");
            return;
        }

        if (Schem == null)
            return;

        PlayerSchematics[id] = Schem;
        PlayerRoles[id] = player.GetRoleInfo();
    }

    /// <summary>
    /// 成否＋ SchematicObject を取得したい場合はこちら。
    /// </summary>
    public static bool TryWear(this Player player, string wearSchemName, out SchematicObject schematicObject, Vector3? offset = null)
    {
        schematicObject = null;

        if (player == null)
            return false;

        var id = player.Id;

        if (PlayerSchematics.TryGetValue(id, out var oldSchem))
        {
            oldSchem.Destroy();
            PlayerSchematics.Remove(id);
            PlayerRoles.Remove(id);
        }

        var offsetVector = offset ?? Vector3.zero;
        SchematicObject schem;

        try
        {
            schem = ObjectSpawner.SpawnSchematic(wearSchemName, player.Position + offsetVector, player.Rotation);
            schem.transform.SetParent(player.Transform);
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

    private static void OnPlayerLeft(LeftEventArgs ev)
    {
        CleanupPlayer(ev.Player);
    }

    /// <summary>
    /// ロール変更を監視し、変化したプレイヤーの Schematic を自動 Destroy。
    /// </summary>
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
                var id = kvp.Key;
                var savedInfo = kvp.Value;

                var player = Player.Get(id);
                if (player == null)
                    continue;

                var currentInfo = player.GetRoleInfo();

                // 必要に応じて Vanilla のみに絞るなど調整
                if (savedInfo.Vanilla != currentInfo.Vanilla ||
                    savedInfo.Custom != currentInfo.Custom)
                {
                    CleanupPlayer(player);
                }
            }

            yield return Timing.WaitForSeconds(0.5f);
        }
    }

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