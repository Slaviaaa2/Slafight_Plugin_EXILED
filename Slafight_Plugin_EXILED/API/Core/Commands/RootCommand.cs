using CommandSystem;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Commands;

/// <summary>
/// 新 API の運営コマンドの入口です。
/// </summary>
/// <remarks>
/// <b>子コマンドの一覧をここに書きません。</b>
/// <see cref="CommandBase.Parent"/> にこの型を書いたコマンドが自動的に並びます。
/// 引数なしで実行すると、実行者が権限を持つものだけが出ます。
/// これが help を兼ねるので、別に help コマンドは置きません。
/// </remarks>
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class RootCommand : ParentCommandBase
{
    public override string Command => "slc";

    public override string Description => "Slafight Core の運営コマンド。";

    protected override string Title => "Slafight Core";
}
