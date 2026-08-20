namespace Slafight_Plugin_EXILED.API.Core.Enums;

/// <summary>
/// <see cref="Features.EventHandlerBase"/> インスタンスが自動破棄されるタイミングを表します。
/// </summary>
public enum HandlerLifetime
{
    /// <summary>
    /// 自動破棄しません。<c>Dispose()</c> / <c>DisposeAll()</c> が呼ばれるまで生存します。
    /// プラグインの起動から終了まで生きるハンドラ用。
    /// </summary>
    Manual,

    /// <summary>
    /// ラウンド再開時に自動で <c>Dispose</c> されます。
    /// ラウンド中だけ生きるハンドラ (カスタム役職の能力など) 用。
    /// </summary>
    Round,
}
