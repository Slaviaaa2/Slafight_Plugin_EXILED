using CommandSystem;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Samples;

/// <summary>
/// 親コマンドの書き方の見本です。
///
/// <b>子コマンドの一覧をここに書いていない</b>のが見どころです。
/// <see cref="CommandBase.Parent"/> にこの型を書いたコマンドが自動的に並びます。
/// 旧 <c>RootCommand</c> は 29 行の <c>RegisterCommand(new X())</c> を抱えていました。
/// </summary>
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class SampleRootCommand : ParentCommandBase
{
    public override string Command => "slcore";

    public override string[] Aliases { get; } = ["slc"];

    public override string Description => "新 API の動作確認コマンドです。";

    protected override string Title => "Slafight Core";
}
