using System;
using System.Linq;
using System.Text;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Samples;

/// <summary>
/// 自動登録が生きているかを実機で確かめるためのコマンドです。
///
/// <c>slcore status</c> として並びます。<see cref="SampleRootCommand"/> 側は無変更です。
/// </summary>
public sealed class SampleStatusCommand : CommandBase
{
    public override Type Parent => typeof(SampleRootCommand);

    public override string Command => "status";

    public override string Description => "自動登録されたハンドラと、現在のカスタム役職・アイテムを表示します。";

    protected override bool OnExecute(out string response)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("<b>Slafight Core</b>");
        builder.AppendLine($"  自動登録ハンドラ : {EventHandlerRegistry.AutoRegistered.Count}");
        builder.AppendLine($"  有効なハンドラ   : {EventHandlerBase.Active.Count}");
        builder.AppendLine($"  宣言済みチーム   : {CustomTeam.All.Count}");
        builder.AppendLine($"  現在の役職       : {CustomRole.Active.Count}");
        builder.AppendLine($"  追跡中アイテム   : {CustomItem.Tracked.Count}");
        builder.AppendLine($"  ゲームモード     : {GameMode.Current?.Name ?? "なし"}");

        if (EventHandlerBase.Active.Count > 0)
        {
            builder.AppendLine("  内訳:");

            foreach (string name in EventHandlerBase.Active
                         .Select(handler => handler.GetType().Name)
                         .OrderBy(name => name, StringComparer.Ordinal))
            {
                builder.AppendLine($"    - {name}");
            }
        }

        response = builder.ToString().TrimEnd();

        return true;
    }
}
