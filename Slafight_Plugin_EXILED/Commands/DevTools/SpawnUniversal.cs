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
    public string[] Aliases { get; } = ["spawn", "us"];
    public string Description => "Universal CustomRole Spawner";

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

        // 引数なし → 使い方
        if (arguments.Count == 0)
        {
            var names = RoleParseHelper.GetAllRoleNames();
            response = "Usage: spawn <roleId> [targetPlayerId]\nAvailable roles:\n" + string.Join(", ", names);
            return false;
        }

        var roleId = arguments.At(0); // 1番目の引数（roleId）

        // 特殊系だけ個別処理
        if (roleId.Equals("mp", StringComparison.OrdinalIgnoreCase))
        {
            executor.UniqueRole = "MapEditor";
            Plugin.Singleton.PlayerHUD.DestroyHints();
            response = $"{executor.Nickname} is now {executor.UniqueRole}";
            return true;
        }

        if (roleId.Equals("debug", StringComparison.OrdinalIgnoreCase))
        {
            executor.UniqueRole = "Debug";
            response = $"{executor.Nickname} is now {executor.UniqueRole}";
            return true;
        }

        // 共通パーサに投げて RoleTypeId / CRoleTypeId を自動判定
        if (!RoleParseHelper.TryParseRole(roleId, out var vanilla, out var custom))
        {
            var names = RoleParseHelper.GetAllRoleNames();
            response = $"Unknown role: {roleId}\nAvailable roles:\n" + string.Join(", ", names);
            return false;
        }

        // ターゲットプレイヤー判定（2番目の引数）
        Player target = executor;
        if (arguments.Count >= 2)
        {
            if (int.TryParse(arguments.At(1), out int targetId))
            {
                target = Player.Get(targetId);
                if (target == null)
                {
                    response = $"Player with ID {targetId} not found.";
                    return false;
                }
            }
            else
            {
                response = $"Invalid player ID: {arguments.At(1)}. Use numeric ID.";
                return false;
            }
        }

        // ロール付与
        if (vanilla.HasValue)
        {
            target.SetRole(vanilla.Value, RoleSpawnFlags.All);
            response = $"{target.Nickname} is now {vanilla.Value}";
            return true;
        }

        if (custom.HasValue)
        {
            target.SetRole(custom.Value, RoleSpawnFlags.All);
            response = $"{target.Nickname} is now {target.UniqueRole}";
            return true;
        }

        response = "Failed to assign role.";
        return false;
    }
}
