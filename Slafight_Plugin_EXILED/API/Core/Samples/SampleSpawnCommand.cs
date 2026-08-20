using System;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Samples;

/// <summary>
/// 見本の役職・アイテム・割り当てを実際に動かすコマンドです。
///
/// <c>slcore spawn [role|item|set] [対象]</c> として並びます。
/// </summary>
public sealed class SampleSpawnCommand : CommandBase
{
    public override Type Parent => typeof(SampleRootCommand);

    public override string Command => "spawn";

    public override string Usage => "spawn <role|item|set> [対象]";

    public override string Description => "見本の役職・アイテム・一括割り当てを試します。";

    protected override bool OnExecute(out string response)
    {
        if (!TryGetArgument(0, out string what))
        {
            response = $"使い方: {Usage}";

            return false;
        }

        switch (what.ToLowerInvariant())
        {
            case "role":
                if (!TryGetPlayer(1, out Player target))
                {
                    response = "対象が見つかりません。";

                    return false;
                }

                if (!new SampleRole().Spawn(target, out string failure))
                {
                    response = failure;

                    return false;
                }

                response = $"{target.Nickname} を SampleRole にしました。";

                return true;

            case "item":
                if (!TryGetPlayer(1, out Player receiver))
                {
                    response = "対象が見つかりません。";

                    return false;
                }

                if (CustomItem.Give<SampleItem>(receiver) is null)
                {
                    response = "アイテムを渡せませんでした (インベントリが満杯かもしれません)。";

                    return false;
                }

                response = $"{receiver.Nickname} に SampleItem を渡しました。";

                return true;

            case "set":
                int assigned = new SampleSpawnSet().Spawn();
                response = $"SampleSpawnSet で {assigned} 人に割り当てました。";

                return true;

            default:
                response = $"不明な指定です: {what}\n使い方: {Usage}";

                return false;
        }
    }
}
