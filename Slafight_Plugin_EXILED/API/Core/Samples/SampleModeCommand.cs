using System;
using System.Linq;
using System.Text;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Samples;

/// <summary>
/// ゲームモードを一覧・起動・停止するコマンドです。
///
/// <c>slcore mode</c> で一覧、<c>slcore mode &lt;クラス名&gt;</c> で起動、
/// <c>slcore mode stop</c> で停止します。
/// </summary>
/// <remarks>
/// <see cref="GameMode.Weight"/> が 0 のモードは抽選に出ないので、
/// 起動する手段はこれになります。
/// </remarks>
public sealed class SampleModeCommand : CommandBase
{
    public override Type Parent => typeof(SampleRootCommand);

    public override string Command => "mode";

    public override string Usage => "mode [クラス名|stop]";

    public override string Description => "ゲームモードの一覧・起動・停止。";

    protected override bool OnExecute(out string response)
    {
        if (!TryGetArgument(0, out string name))
        {
            response = BuildList();

            return true;
        }

        if (string.Equals(name, "stop", StringComparison.OrdinalIgnoreCase))
        {
            if (GameMode.Current is not { } running)
            {
                response = "走っているモードはありません。";

                return false;
            }

            GameMode.StopCurrent();
            response = $"'{running.Name}' を停止しました。";

            return true;
        }

        if (!TypeParser.TryCreate<GameMode>(name, out GameMode mode) || mode is null)
        {
            response = $"'{name}' というモードは見つかりません。\n{BuildList()}";

            return false;
        }

        if (!mode.Start())
        {
            response = $"'{mode.Name}' を開始できませんでした。";

            return false;
        }

        response = $"'{mode.Name}' を開始しました。";

        return true;
    }

    private static string BuildList()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"<b>Game Modes</b>  (実行中: {GameMode.Current?.Name ?? "なし"})");

        foreach (GameMode mode in GameMode.All().OrderBy(mode => mode.GetType().Name, StringComparer.Ordinal))
        {
            string availability = mode.Weight <= 0
                ? "手動のみ"
                : mode.IsAvailable ? $"重み {mode.Weight}" : $"重み {mode.Weight} / 条件未達";

            builder.AppendLine($"  <b>{mode.GetType().Name}</b> — {mode.Name} [{availability}]");
        }

        return builder.ToString().TrimEnd();
    }
}
