using System;

namespace Slafight_Plugin_EXILED.API.Core.Features.Attributes;

/// <summary>
/// この型が昔名乗っていた名前です。<see cref="TypeParser"/> がこの綴りでも解決します。
/// </summary>
/// <remarks>
/// <para>
/// 外部データ (ProjectMER のマップ・保存済み JSON・運営が打つコマンド) は
/// <b>作られた当時の名前</b>を持ったままです。再構築で綴りを直した型は、
/// マップを書き換えない限りその名前で引かれ続けます。
/// </para>
/// <para>
/// これは「別名の string キー」ではありません。同一性はあくまで型で、
/// ここに書くのは<b>過去に使われていた綴り</b>だけです。新しい呼び名を増やす用途で
/// 使うと、旧実装の <c>UniqueKey</c> 表へ逆戻りします。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 旧 Slafight の UniqueKey は "ClassXMemoryForcePil" (l が 1 つ) だった。
/// [LegacyName("ClassXMemoryForcePil")]
/// public sealed class ClassXMemoryForcePill : CustomItem { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class LegacyNameAttribute : Attribute
{
    public LegacyNameAttribute(string name) => Name = name ?? string.Empty;

    /// <summary>昔の名前です。</summary>
    public string Name { get; }
}
