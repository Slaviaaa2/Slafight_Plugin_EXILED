using System;
using System.Globalization;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// 権限チェックと送信者の解決を肩代わりするコマンド基底です。
/// 派生クラスは <see cref="Command"/> と <see cref="OnExecute"/> を書くだけで済みます。
/// </summary>
/// <example>
/// <code>
/// [CommandHandler(typeof(RemoteAdminCommandHandler))]
/// public class HealCommand : CommandBase
/// {
///     public override string Command => "heal";
///
///     protected override bool OnExecute(out string response)
///     {
///         if (Sender is null)
///         {
///             response = "プレイヤーからのみ実行できます。";
///             return false;
///         }
///
///         Sender.Heal(Sender.MaxHealth);
///         response = "回復しました。";
///         return true;
///     }
/// }
/// </code>
/// </example>
public abstract class CommandBase : ICommand
{
    public abstract string Command { get; }

    public virtual string[] Aliases { get; } = [];

    public virtual string Description { get; } = string.Empty;

    /// <summary>
    /// このコマンドが属する親コマンドの型です。null ならトップレベルのコマンドになります。
    ///
    /// <see cref="ParentCommandBase"/> はこのプロパティだけを見て子を集めるので、
    /// 親側に「登録する子の一覧」を書く必要はありません。
    /// 旧実装の <c>RootCommand</c> が 29 行の <c>RegisterCommand</c> を抱えていたのがこれで消えます。
    /// </summary>
    public virtual Type Parent => null;

    /// <summary>
    /// 使い方の 1 行表記です。<c>help</c> 系の表示に使います。
    /// </summary>
    public virtual string Usage => Command;

    /// <summary>
    /// このコマンドに必要な権限ノードです。既定は <c>slperm.コマンド名</c>。
    ///
    /// 権限チェックを外したい場合は null または空文字を返してください。
    /// </summary>
    /// <remarks>
    /// <b>接頭辞と判定先は EXILED のままにします。</b>
    /// 運営の <c>permissions.yml</c> は EXILED の権限機構に <c>slperm.*</c> を並べたものなので、
    /// LabAPI の権限やアセンブリ名を接頭辞にしたノードへ替えると、
    /// 既存の設定がどのコマンドにも当たらなくなり、全部「権限がありません」で弾かれます。
    /// </remarks>
    public virtual string Permission => $"slperm.{Command}";

    /// <summary>
    /// 指定した送信者がこのコマンドを実行できるかどうか。カタログ表示の絞り込みに使います。
    /// </summary>
    public bool IsAllowedFor(ICommandSender sender) =>
        string.IsNullOrEmpty(Permission) || (sender is not null && sender.CheckPermission(Permission));

    /// <summary>
    /// 権限が無いときの応答です。
    /// </summary>
    protected virtual string PermissionDeniedResponse => $"この操作には {Permission} 権限が必要です。";

    /// <summary>
    /// 送信者に対応する <see cref="Player"/> です。
    /// サーバーコンソールから実行された場合は null になります。
    /// </summary>
    protected Player Sender { get; private set; }

    /// <summary>
    /// 生の送信者です。コンソールへの応答や権限の再確認に使います。
    /// </summary>
    protected ICommandSender InterfaceSender { get; private set; }

    /// <summary>
    /// コマンド名を除いた引数です。
    /// </summary>
    protected ArraySegment<string> Arguments { get; private set; }

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Sender = Player.Get(sender);
        InterfaceSender = sender;
        Arguments = arguments;

        if (!IsAllowedFor(sender))
        {
            response = PermissionDeniedResponse;

            return false;
        }

        return OnExecute(out response);
    }

    /// <summary>
    /// 指定位置の引数を取り出します。範囲外なら false を返します。
    /// </summary>
    protected bool TryGetArgument(int index, out string value)
    {
        if (index >= 0 && index < Arguments.Count)
        {
            value = Arguments.Array[Arguments.Offset + index];

            return !string.IsNullOrWhiteSpace(value);
        }

        value = null;

        return false;
    }

    /// <summary>
    /// 指定位置以降の引数を 1 つの文字列として取り出します。無ければ空文字。
    /// </summary>
    protected string GetRemainder(int index)
    {
        if (index < 0 || index >= Arguments.Count) return string.Empty;

        return string.Join(" ", Arguments.Skip(index));
    }

    /// <summary>
    /// 指定位置の引数を整数として取り出します。
    /// </summary>
    protected bool TryGetInt(int index, out int value)
    {
        value = 0;

        return TryGetArgument(index, out string raw) &&
               int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// 指定位置の引数を小数として取り出します。
    /// </summary>
    protected bool TryGetFloat(int index, out float value)
    {
        value = 0f;

        return TryGetArgument(index, out string raw) &&
               float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// 指定位置の引数をプレイヤーとして解決します。
    /// 省略・<c>@me</c> の場合は送信者自身になります。
    /// </summary>
    /// <remarks>
    /// ID の完全一致を先に試し、それ以外は EXILED の
    /// <see cref="Player.Get(string)"/> (UserId / 名前) に任せます。
    /// </remarks>
    protected bool TryGetPlayer(int index, out Player player)
    {
        if (!TryGetArgument(index, out string raw) ||
            raw is "@me" or "me" or "self")
        {
            player = Sender;

            return player is not null;
        }

        player = null;

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
            player = Player.Get(id);

        player ??= Player.Get(raw);

        return player is not null;
    }

    protected abstract bool OnExecute(out string response);
}
