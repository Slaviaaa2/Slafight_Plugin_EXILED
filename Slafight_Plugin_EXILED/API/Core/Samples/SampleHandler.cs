using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using LabApi.Events.Arguments.PlayerEvents;
using Slafight_Plugin_EXILED.API.Core.Enums;
using Slafight_Plugin_EXILED.API.Core.Features;

using PlayerHandlers = Exiled.Events.Handlers.Player;

namespace Slafight_Plugin_EXILED.API.Core.Samples;

/// <summary>
/// イベントハンドラの書き方の見本です。
///
/// <b>このクラスはどこからも登録されていません。</b>
/// <see cref="EventHandlerBase"/> を継承しているというだけで
/// <see cref="EventHandlerRegistry"/> が見つけて生成・購読します。
/// <c>Plugin.cs</c> に <c>Register()</c> を足す必要はありません。
///
/// <see cref="Lifetime"/> が <see cref="HandlerLifetime.Round"/> なので、
/// ラウンド再開で自動的に破棄され、次のラウンド開始で作り直されます。
/// 解除漏れを人間が気にする必要はありません。
/// </summary>
public sealed class SampleHandler : EventHandlerBase
{
    private int deaths;

    public override HandlerLifetime Lifetime => HandlerLifetime.Round;

    /// <summary>
    /// EXILED 側のイベントはここで購読します。解除は <see cref="UnregisterEvents"/> と対です。
    /// </summary>
    public override void RegisterEvents()
    {
        PlayerHandlers.Verified += OnVerified;
    }

    /// <inheritdoc />
    public override void UnregisterEvents()
    {
        PlayerHandlers.Verified -= OnVerified;
    }

    /// <summary>
    /// LabApi 側は <c>OnXxx</c> を override するだけで購読されます。登録・解除は不要です。
    /// EXILED 側と LabApi 側を同じインスタンスで併用できます。
    /// </summary>
    public override void OnPlayerDeath(PlayerDeathEventArgs ev)
    {
        deaths++;
        Log.Debug($"[Sample] このラウンドの死亡数: {deaths}");
    }

    /// <inheritdoc />
    protected override void OnEnabled()
    {
        Log.Debug("[Sample] SampleHandler が購読を開始しました (登録コードはどこにもありません)。");
    }

    /// <inheritdoc />
    protected override void OnDisposed()
    {
        Log.Debug($"[Sample] SampleHandler を破棄しました。最終的な死亡数: {deaths}");
    }

    private static void OnVerified(VerifiedEventArgs ev)
    {
        Log.Debug($"[Sample] {ev.Player?.Nickname} が参加しました。");
    }
}
