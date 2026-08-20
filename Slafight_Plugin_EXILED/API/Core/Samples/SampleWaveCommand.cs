using System;
using System.Linq;
using System.Text;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Samples;

/// <summary>
/// リスポーンウェーブを一覧・強制実行するコマンドです。
///
/// <c>slcore wave</c> で一覧、<c>slcore wave &lt;クラス名&gt;</c> で強制実行します。
/// 引数はクラス名そのものなので、<b>一覧に出た名前がそのまま使えます。</b>
/// </summary>
public sealed class SampleWaveCommand : CommandBase
{
    public override Type Parent => typeof(SampleRootCommand);

    public override string Command => "wave";

    public override string Usage => "wave [クラス名]";

    public override string Description => "宣言されているリスポーンウェーブの一覧と強制実行。";

    protected override bool OnExecute(out string response)
    {
        if (!TryGetArgument(0, out string name))
        {
            response = BuildList();

            return true;
        }

        if (SpawnContext.Find(name) is not { } wave)
        {
            response = $"'{name}' という波は見つかりません。\n{BuildList()}";

            return false;
        }

        int assigned = SpawnSystem.ForceSpawn(wave);

        response = assigned > 0
            ? $"'{wave.Name}' で {assigned} 人に割り当てました。"
            : $"'{wave.Name}' を実行しましたが、対象になる観戦者が居ませんでした。";

        return true;
    }

    private static string BuildList()
    {
        StringBuilder builder = new StringBuilder();
        SpawnContext context = SpawnContext.Active;

        builder.AppendLine($"<b>Waves</b>  (状況: {context.Name})");

        if (SpawnContext.AllWaves.Count == 0)
        {
            builder.AppendLine("  宣言されている波がありません。");

            return builder.ToString().TrimEnd();
        }

        foreach (SpawnSet wave in SpawnContext.AllWaves.OrderBy(wave => wave.GetType().Name, StringComparer.Ordinal))
        {
            string kind = wave.IsMiniWave ? "増援" : "本隊";

            builder.AppendLine(
                $"  <b>{wave.GetType().Name}</b> — {wave.Name} " +
                $"[{wave.RespawnFaction} / {kind} / 重み {context.WeightOf(wave)} / 割合 {wave.RespawnRatio:P0}]");
        }

        return builder.ToString().TrimEnd();
    }
}
