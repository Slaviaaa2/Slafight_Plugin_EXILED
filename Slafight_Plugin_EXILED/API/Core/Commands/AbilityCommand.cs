using System;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Commands;

/// <summary>
/// 能力を付与・剥奪・発動します。
/// </summary>
public sealed class AbilityCommand : CommandBase
{
    public override Type Parent => typeof(RootCommand);

    public override string Command => "ability";

    public override string Usage => "ability <クラス名|clear|use|show> [対象]";

    public override string Description => "能力の付与・剥奪・発動と、所持状況の確認。";

    protected override bool OnExecute(out string response)
    {
        if (!TryGetArgument(0, out string name))
        {
            response = $"使い方: {Usage}\n{CoreCatalog.Names<AbilityBase>()}";

            return false;
        }

        if (!TryGetPlayer(1, out Player target))
        {
            response = "対象が見つかりません。";

            return false;
        }

        switch (name.ToLowerInvariant())
        {
            case "clear":
                AbilityBase.RevokeAll(target);
                response = $"{target.Nickname} の能力をすべて剥奪しました。";

                return true;

            case "use":
                if (AbilityBase.Active(target) is not { } active)
                {
                    response = $"{target.Nickname} は能力を持っていません。";

                    return false;
                }

                if (!active.TryUse(out string reason))
                {
                    response = $"{active.DisplayName}: {reason}";

                    return false;
                }

                response = $"{target.Nickname} の {active.DisplayName} を発動しました。";

                return true;

            case "show":
                response = Describe(target);

                return true;
        }

        if (!CoreCatalog.TryResolve<AbilityBase>(name, out Type abilityType, out string failure))
        {
            response = failure;

            return false;
        }

        if (AbilityBase.Give(abilityType, target) is not { } granted)
        {
            response = $"{abilityType.Name} を付与できませんでした。";

            return false;
        }

        response = $"{target.Nickname} に {granted.DisplayName} を付与しました。";

        return true;
    }

    private static string Describe(Player target)
    {
        var abilities = AbilityBase.Of(target);

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(CoreCatalog.Header($"{target.Nickname} の能力", abilities.Count));

        if (abilities.Count == 0)
        {
            builder.AppendLine("  (なし)");

            return builder.ToString().TrimEnd();
        }

        int activeIndex = AbilityBase.ActiveIndexOf(target);

        foreach (var (ability, index) in abilities.Select((ability, index) => (ability, index)))
        {
            string marker = index == activeIndex ? "*" : " ";
            string uses = ability.MaxUses < 0 ? "∞" : $"{ability.RemainingUses}/{ability.MaxUses}";
            string state = ability.IsReady ? "READY" : $"CD {ability.RemainingCooldown:0.#}s";
            string option = ability.SelectedOption is { } selected ? $" [{selected.Name}]" : string.Empty;

            builder.AppendLine($"  {marker}{index + 1}. {ability.DisplayName}{option} — {state} / 残り {uses}");
        }

        return builder.ToString().TrimEnd();
    }
}
