using System;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Samples;

/// <summary>
/// カスタム役職を一覧・付与するコマンドです。
///
/// <c>slcore role</c> で一覧、<c>slcore role &lt;クラス名&gt; [対象]</c> で付与します。
/// 引数はクラス名そのものなので、<b>一覧に出た名前がそのまま使えます。</b>
/// </summary>
public sealed class SampleRoleCommand : CommandBase
{
    public override Type Parent => typeof(SampleRootCommand);

    public override string Command => "role";

    public override string Usage => "role [クラス名] [対象]";

    public override string Description => "宣言されているカスタム役職の一覧と付与。";

    protected override bool OnExecute(out string response)
    {
        if (!TryGetArgument(0, out string name))
        {
            response = BuildList();

            return true;
        }

        if (!TypeParser.TryParse<CustomRole>(name, out Type roleType))
        {
            response = $"'{name}' という役職は見つかりません。\n{BuildList()}";

            return false;
        }

        if (!TryGetPlayer(1, out Player target))
        {
            response = "対象が見つかりません。";

            return false;
        }

        if (CustomRole.Spawn(roleType!, target) is not { } spawned)
        {
            response = $"{target.Nickname} に {roleType!.Name} を付与できませんでした。";

            return false;
        }

        response = $"{target.Nickname} を {spawned.Name} にしました。";

        return true;
    }

    private static string BuildList()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"<b>Custom Roles</b>  (現在 {CustomRole.Active.Count} 人)");

        foreach (Type type in TypeParser.FindTypes<CustomRole>()
                     .OrderBy(type => type.Name, StringComparer.Ordinal))
        {
            if (Activator.CreateInstance(type) is not CustomRole probe) continue;

            string voice = probe.Voice.Proximity.IsAvailable ? " / 近接ボイス可" : string.Empty;

            builder.AppendLine(
                $"  <b>{type.Name}</b> — {probe.Name} [{probe.BaseRole} / {probe.Team?.Name ?? "陣営なし"}{voice}]");
        }

        return builder.ToString().TrimEnd();
    }
}
