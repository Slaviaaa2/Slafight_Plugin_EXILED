using Exiled.API.Features;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// 1 つの能力が持つ、切り替え可能な選択肢です。
///
/// <b>選択肢そのものが振る舞いを持ちます。</b>
/// 能力側が「いまどれが選ばれているか」を文字列 ID で調べて分岐する必要はありません。
/// </summary>
/// <remarks>
/// 旧実装の <c>AbilityOption</c> は ID と表示名だけを持つ入れ物で、
/// 能力側が <c>option.Is("gen_battleaxe")</c> のような比較を並べて分岐していました。
/// 選択肢を足すたびに能力本体の分岐が伸びるのが問題だったので、
/// ここでは<b>選択肢が自分で何をするかを知っている</b>形にします。
/// </remarks>
/// <example>
/// <code>
/// public sealed class BattleAxeChoice : AbilityOption
/// {
///     public override string Name => "戦斧";
///
///     public override void Use(Player player) => CustomItem.Give&lt;BattleAxe&gt;(player);
/// }
///
/// public sealed class GenerateWeapon : AbilityBase
/// {
///     public override string Name => "武器生成";
///
///     public override IReadOnlyList&lt;AbilityOption&gt; Options =>
///     [
///         new BattleAxeChoice(),
///         new RevolverChoice(),
///     ];
///
///     // 選ばれているものに任せるだけ。
///     protected override void OnUsed() => SelectedOption?.Use(Player);
/// }
/// </code>
/// </example>
public abstract class AbilityOption
{
    /// <summary>
    /// 表示名です。
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// 説明です。
    /// </summary>
    public virtual string Description => string.Empty;

    /// <summary>
    /// この選択肢が今使えるかどうか。使えないなら理由を返します。
    /// </summary>
    /// <remarks>
    /// 能力全体の条件 (生存・回数・クールダウン) は <see cref="AbilityBase"/> が見ます。
    /// ここに書くのは<b>この選択肢だけの条件</b>です。
    /// </remarks>
    public virtual bool CanUse(Player player, out string failureReason)
    {
        failureReason = null;

        return true;
    }

    /// <summary>
    /// この選択肢の効果です。
    /// </summary>
    public abstract void Use(Player player);

    /// <inheritdoc />
    public override string ToString() => Name;
}
