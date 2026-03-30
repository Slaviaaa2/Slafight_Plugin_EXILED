using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Slafight_Plugin_EXILED.API.Features;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class AbilityUniversal : ICommand
{
    public string Command => "giveability";
    public string[] Aliases { get; } = ["ga", "ability", "au"];
    public string Description => "任意のAbilityを付与\n.giveability sh @me 5 3 → Sinkhole(CD5s/3回)";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        // パーミッションチェック
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"権限不足: slperm.{Command}";
            return false;
        }

        var executor = Player.Get(sender);
        if (executor == null)
        {
            response = "プレイヤー未検出";
            return false;
        }

        if (arguments.Count == 0)
        {
            response = $"使用法: .{Command} <ability> [playerId] [CD] [回数]\n"
                     + $"例: .{Command} sh\n"
                     + $"    .{Command} sh 5\n"
                     + $"    .{Command} sh @me 5 3\n"
                     + $"Ability一覧: {string.Join(", ", AbilityParseHelper.GetAllAbilityNames())}";
            return false;
        }

        var abilityId = arguments.At(0);

        // --- ターゲット判定（@me / ID） ---
        Player target = executor;
        if (arguments.Count >= 2)
        {
            var targetArg = arguments.At(1).ToLower();
            
            if (targetArg is "@me" or "me")
            {
                target = executor;
            }
            else if (int.TryParse(targetArg, out var targetId))
            {
                target = Player.Get(targetId);
                if (target == null)
                {
                    response = $"ID{targetId}のプレイヤー不在";
                    return false;
                }
            }
            else
            {
                response = $"無効なターゲット: {targetArg} (@me or ID)";
                return false;
            }
        }

        // --- オプション引数 ---
        float? cooldown = null;
        int? maxUses = null;

        if (arguments.Count >= 3 && float.TryParse(arguments.At(2), out var cd))
        {
            cooldown = Math.Max(0.1f, cd); // 最低0.1秒
        }

        if (arguments.Count >= 4 && int.TryParse(arguments.At(3), out var uses))
        {
            maxUses = uses < 0 ? -1 : uses; // -1=無制限
        }

        // --- Ability付与 ---
        bool success = AbilityParseHelper.TryGiveAbility(abilityId, target, cooldown, maxUses);
        
        if (!success)
        {
            response = $"不明なAbility: {abilityId}\n"
                     + $"利用可能: {string.Join(", ", AbilityParseHelper.GetAllAbilityNames())}";
            return false;
        }

        // --- 成功メッセージ ---
        var msg = $"[{target.Nickname}] {abilityId}";
        if (cooldown.HasValue) msg += $" CD={cooldown:F1}s";
        if (maxUses.HasValue) msg += $" 回数={maxUses}";
        
        response = msg;
        executor.ShowHint($"<color=green>{msg}</color>", 3f);
        return true;
    }
}
