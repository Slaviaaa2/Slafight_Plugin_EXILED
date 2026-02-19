using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.MainHandlers;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class SpawnWave : ICommand
{
    public string Command => "spawnwave";
    public string[] Aliases { get; } = ["sw", "wave"];
    public string Description => "Instantly spawns a wave by SpawnTypeId";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        // パーミッションチェック
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: slperm.{Command}";
            return false;
        }

        var executor = Player.Get(sender);
        if (executor == null)
        {
            response = "Player not found.";
            return false;
        }

        // 引数なし → 使い方 + 利用可能 SpawnTypeId 一覧
        if (arguments.Count == 0)
        {
            var availableTypes = GetAvailableSpawnTypeNames();
            response =
                "Usage: spawnwave <spawnTypeId> [mini]\n" +
                "Example: spawnwave MTF_NtfNormal\n" +
                "         spawnwave GOI_ChaosBackup mini\n" +
                "Available spawn types:\n" +
                string.Join(", ", availableTypes);
            return false;
        }

        var typeArg = arguments.At(0);

        // SpawnTypeId パース
        if (!Enum.TryParse<SpawnTypeId>(typeArg, true, out var spawnType))
        {
            var availableTypes = GetAvailableSpawnTypeNames();
            response =
                $"Unknown SpawnTypeId: {typeArg}\n" +
                "Available spawn types:\n" +
                string.Join(", ", availableTypes);
            return false;
        }

        // 第2引数が "mini" の場合は MiniWave 扱い
        bool isMiniWave = false;
        if (arguments.Count >= 2)
        {
            var second = arguments.At(1);
            if (second.Equals("mini", StringComparison.OrdinalIgnoreCase) ||
                second.Equals("backup", StringComparison.OrdinalIgnoreCase))
            {
                isMiniWave = true;
            }
        }

        // 即時スポーン
        if (SpawnSystem.Instance == null)
        {
            response = "SpawnSystem is not initialized.";
            return false;
        }

        SpawnSystem.ForceSpawnNow(spawnType, isMiniWave);
        response = $"Spawned wave: {spawnType} (MiniWave: {isMiniWave})";
        return true;
    }

    /// <summary>
    /// 利用可能な SpawnTypeId 名を列挙（必要に応じてフィルタする用）。
    /// </summary>
    private static IEnumerable<string> GetAvailableSpawnTypeNames()
    {
        // 実際に使う SpawnTypeId だけ列挙するなら、ここで手動列挙か
        // Config / Context からキーを集めても良い
        return Enum.GetNames(typeof(SpawnTypeId))
            .OrderBy(n => n);
    }
}
