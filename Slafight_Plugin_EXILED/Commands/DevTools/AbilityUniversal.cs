using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

public class AbilityUniversal : ICommand
{
    public string Command => "giveability";
    public string[] Aliases { get; } = ["ga", "ability", "au"];
    public string Description => "Give a custom ability to a player";

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

        if (arguments.Count == 0)
        {
            var names = AbilityParseHelper.GetAllAbilityNames();
            response = "Usage: giveability <abilityId> [playerId] [cooldown] [maxUses]\n"
                       + "Available abilities:\n" + string.Join(", ", names);
            return false;
        }

        var abilityId = arguments.At(0);

        // --- 1) ターゲット ---
        Player target = executor;
        if (arguments.Count >= 2)
        {
            if (!int.TryParse(arguments.At(1), out var targetId))
            {
                response = $"Invalid player ID: {arguments.At(1)}";
                return false;
            }

            target = Player.Get(targetId);
            if (target == null)
            {
                response = $"Player with ID {targetId} not found.";
                return false;
            }
        }

        // --- 2) オプション: クールダウン / 使用回数 ---
        float? cooldown = null;
        int? maxUses = null;

        if (arguments.Count >= 3)
        {
            if (!float.TryParse(arguments.At(2), out var cd))
            {
                response = $"Invalid cooldown: {arguments.At(2)}";
                return false;
            }
            cooldown = cd;
        }

        if (arguments.Count >= 4)
        {
            if (!int.TryParse(arguments.At(3), out var uses))
            {
                response = $"Invalid maxUses: {arguments.At(3)}";
                return false;
            }
            maxUses = uses;
        }

        // --- 3) 付与 ---
        if (!AbilityParseHelper.TryGiveAbility(abilityId, target, cooldown, maxUses))
        {
            var names = AbilityParseHelper.GetAllAbilityNames();
            response = $"Unknown ability: {abilityId}\nAvailable abilities:\n" + string.Join(", ", names);
            return false;
        }

        response = $"Gave ability \"{abilityId}\" to {target.Nickname}"
                   + (cooldown.HasValue ? $" (CD={cooldown.Value}s" : "")
                   + (maxUses.HasValue ? $", Uses={maxUses.Value})" : (cooldown.HasValue ? ")" : ""));
        return true;
    }
}