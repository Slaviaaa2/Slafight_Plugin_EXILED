using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.CustomEffects;

/// <summary>
/// カスタムステータスエフェクトをプレイヤーのプレハブへ差し込みます。
/// </summary>
/// <remarks>
/// 旧実装は <c>Plugin.OnEnabled</c> から直接
/// <see cref="CustomStatusEffectsRegistry.AllRegister()"/> を呼んでいましたが、
/// 現在の <c>Plugin.cs</c> は「機能の登録を並べない」方針なので、
/// <see cref="EventHandlerBase"/> 側に寄せてあります。
/// この形なら <c>EventHandlerRegistry</c> が自動で生成・破棄してくれます。
///
/// 購読するイベントはありません。有効化と後始末のためだけのハンドラです。
/// </remarks>
public sealed class CustomEffectsBootstrap : EventHandlerBase
{
    /// <inheritdoc />
    protected override void OnEnabled() => CustomStatusEffectsRegistry.AllRegister();

    /// <inheritdoc />
    /// <remarks>
    /// <see cref="CustomStatusEffectsRegistry"/> は
    /// <c>SceneManager.sceneLoaded</c> を掴んだままなので、必ず外します。
    /// </remarks>
    protected override void OnDisposed() => CustomStatusEffectsRegistry.Unhook();
}
