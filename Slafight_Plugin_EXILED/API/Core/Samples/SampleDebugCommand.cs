using System;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Samples;

/// <summary>
/// デバッグ表示を切り替えるコマンドです。<c>slcore debug [対象]</c>。
/// </summary>
public sealed class SampleDebugCommand : CommandBase
{
    public override Type Parent => typeof(SampleRootCommand);

    public override string Command => "debug";

    public override string Usage => "debug [対象]";

    public override string Description => "デバッグ HUD の表示を切り替えます。";

    protected override bool OnExecute(out string response)
    {
        if (!TryGetPlayer(0, out Player target))
        {
            response = "対象が見つかりません。";

            return false;
        }

        bool enabled = DebugMode.Toggle(target);

        response = $"{target.Nickname} のデバッグ表示を {(enabled ? "有効" : "無効")} にしました。";

        return true;
    }
}
