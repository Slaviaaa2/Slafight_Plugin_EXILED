using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class SpawnUniversal : ICommand
{
    public string Command => "spawn";
    public string[] Aliases { get; } = { "spawn", "us" };
    public string Description => "Universal Customrole Spawner";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        // パーミッションチェック
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        var player = Player.Get(sender);
        if (player == null)
        {
            response = "Player not found.";
            return false;
        }

        // 引数なし → 使い方（必要ならここで一覧を出してもOK）
        if (arguments.Count == 0)
        {
            var names = RoleParseHelper.GetAllRoleNames();
            response = "Usage: spawn <roleId>\nAvailable roles:\n" + string.Join(", ", names);
            return false;
        }

        var roleId = arguments.First(); // 最初の引数

        // 特殊系だけ個別処理
        if (roleId.Equals("mp", StringComparison.OrdinalIgnoreCase))
        {
            player.UniqueRole = "MapEditor";
            Plugin.Singleton.PlayerHUD.DestroyHints();
            response = $"You're now {player.UniqueRole}";
            return true;
        }

        if (roleId.Equals("debug", StringComparison.OrdinalIgnoreCase))
        {
            player.UniqueRole = "Debug";
            response = $"You're now {player.UniqueRole}";
            return true;
        }

        // 共通パーサに投げて RoleTypeId / CRoleTypeId を自動判定
        if (!RoleParseHelper.TryParseRole(roleId, out var vanilla, out var custom))
        {
            var names = RoleParseHelper.GetAllRoleNames();
            response = $"Unknown role: {roleId}\nAvailable roles:\n" + string.Join(", ", names);
            return false;
        }

        // 通常ロール
        if (vanilla.HasValue)
        {
            player.SetRole(vanilla.Value, RoleSpawnFlags.All);
            response = $"You're now {vanilla.Value}";
            return true;
        }

        // カスタムロール
        if (custom.HasValue)
        {
            player.SetRole(custom.Value, RoleSpawnFlags.All);
            response = $"You're now {player.UniqueRole}";
            return true;
        }

        response = "Failed to assign role.";
        return false;
    }
}
