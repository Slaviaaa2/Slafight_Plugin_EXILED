using System;

namespace Slafight_Plugin_EXILED.API.Core.Features.Attributes;

/// <summary>
/// この属性を付けた <see cref="EventHandlerBase"/> の派生クラスは、
/// 起動時の自動登録の対象から外れます。
/// 自分でタイミングを制御して生成したいハンドラ (プレイヤー単位のものなど) に付けてください。
///
/// なお、public な引数なしコンストラクタを持たないクラスは、この属性がなくても自動登録されません。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NoAutoRegisterAttribute : Attribute
{
}
