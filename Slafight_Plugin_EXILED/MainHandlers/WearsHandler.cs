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

public static class WearsHandler
{
    private static readonly Dictionary<Player, PlayerRoleHelpers.PlayerRoleInfo> PlayerRoles = new();
    private static readonly Dictionary<Player, SchematicObject> PlayerSchematics = new();
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
        Timing.KillCoroutines(_cleanupCoroutine);
        CleanupAll();
    }

    public static void Wear(this Player player, string wearSchemName, Vector3? offset = null)
    {
        if (player == null || !player.IsVerified) return;

        // 古い装備破壊
        if (PlayerSchematics.TryGetValue(player, out var oldSchem))
        {
            oldSchem.Destroy();
            PlayerSchematics.Remove(player);
            PlayerRoles.Remove(player);
        }

        var offsetVector = offset ?? Vector3.zero;
        SchematicObject schem = null;

        try
        {
            schem = ObjectSpawner.SpawnSchematic(wearSchemName, player.Position + offsetVector);
            schem.transform.SetParent(player.Transform);
        }
        catch (Exception e)
        {
            Log.Error($"Wear failed for {player.Nickname}: {e}");
            return;
        }

        if (schem != null)
        {
            PlayerSchematics[player] = schem;
            PlayerRoles[player] = player.GetRoleInfo();  // ロール情報保存
        }
    }
    
    public static bool TryWear(this Player player, string wearSchemName, out SchematicObject schematicObject, Vector3? offset = null)
    {
        schematicObject = null;
        if (player == null || !player.IsVerified)
        {
            schematicObject = null;
            return false;
        }

        // 古い装備破壊
        if (PlayerSchematics.TryGetValue(player, out var oldSchem))
        {
            oldSchem.Destroy();
            PlayerSchematics.Remove(player);
            PlayerRoles.Remove(player);
        }

        var offsetVector = offset ?? Vector3.zero;
        SchematicObject schem = null;

        try
        {
            schem = ObjectSpawner.SpawnSchematic(wearSchemName, player.Position + offsetVector);
            schem.transform.SetParent(player.Transform);
        }
        catch (Exception e)
        {
            Log.Error($"Wear failed for {player.Nickname}: {e}");
            schematicObject = null;
            return false;
        }

        if (schem == null)
        {
            schematicObject = null;
            return false;
        }
        else
        {
            PlayerSchematics[player] = schem;
            PlayerRoles[player] = player.GetRoleInfo(); // ロール情報保存
            schematicObject = schem;
            return true;
        }
    }

    private static void OnRoundStarted()
    {
        _cleanupCoroutine = Timing.RunCoroutine(DestroyCoroutine());
    }

    private static void OnRoundRestarting()
    {
        Timing.KillCoroutines(_cleanupCoroutine);
    }

    private static void OnPlayerLeft(LeftEventArgs ev)
    {
        CleanupPlayer(ev.Player);
    }

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
                var player = kvp.Key;
                if (!player.IsVerified || player.IsNPC) continue;

                if (PlayerRoles.TryGetValue(player, out var savedInfo))
                {
                    var currentInfo = player.GetRoleInfo();
                    
                    // ロール変更検知（Vanilla + Custom両方）
                    if (savedInfo.Vanilla != currentInfo.Vanilla || 
                        savedInfo.Custom != currentInfo.Custom)
                    {
                        CleanupPlayer(player);
                    }
                }
            }
            yield return Timing.WaitForSeconds(0.5f);  // 負荷軽減
        }
    }

    private static void CleanupPlayer(Player player)
    {
        if (PlayerSchematics.TryGetValue(player, out var schem))
        {
            schem.Destroy();
            PlayerSchematics.Remove(player);
        }
        PlayerRoles.Remove(player);
    }

    private static void CleanupAll()
    {
        foreach (var schem in PlayerSchematics.Values.ToList())
            schem.Destroy();
        PlayerSchematics.Clear();
        PlayerRoles.Clear();
    }
}