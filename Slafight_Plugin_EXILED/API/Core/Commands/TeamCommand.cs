using System;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Commands;

/// <summary>
/// 陣営の所属と勝利条件の充足状況を確認します。
/// </summary>
public sealed class TeamCommand : CommandBase
{
    public override Type Parent => typeof(RootCommand);

    public override string Command => "team";

    public override string Usage => "team [クラス名]";

    public override string Description => "陣営の生存者と勝利判定の状況を表示します。";

    protected override bool OnExecute(out string response)
    {
        StringBuilder builder = new StringBuilder();

        if (TryGetArgument(0, out string name))
        {
            if (!CoreCatalog.TryResolve<CustomTeam>(name, out Type teamType, out string failure))
            {
                response = failure;

                return false;
            }

            if (CustomTeam.Get(teamType) is not { } target)
            {
                response = $"{teamType.Name} を生成できませんでした。";

                return false;
            }

            builder.AppendLine(CoreCatalog.Header(target.Name, target.Members.Count()));

            foreach (Player member in target.Members)
            {
                builder.AppendLine($"  - {member.Nickname} ({CustomRole.Of(member)?.Name ?? member.Role.Type.ToString()})");
            }

            response = builder.ToString().TrimEnd();

            return true;
        }

        builder.AppendLine(CoreCatalog.Header("Teams", CustomTeam.All.Count));

        foreach (CustomTeam team in CustomTeam.All.OrderBy(team => team.GetType().Name, StringComparer.Ordinal))
        {
            string met = team.Victory is null
                ? "勝利条件なし"
                : team.Victory.IsMet(team) ? "<color=#38ff6b>条件成立</color>" : "未成立";

            builder.AppendLine(
                $"  <b>{team.GetType().Name}</b> — <color={team.Color}>{team.Name}</color> " +
                $"[生存 {team.Members.Count()} / {met}]");
        }

        if (CustomTeam.FindWinner() is { } winner)
            builder.AppendLine($"  → 現時点の勝者: <color={winner.Color}>{winner.Name}</color>");

        response = builder.ToString().TrimEnd();

        return true;
    }
}
