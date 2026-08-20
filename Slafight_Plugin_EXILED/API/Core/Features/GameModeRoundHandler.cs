using Exiled.API.Features;
using LabApi.Events.Arguments.WarheadEvents;
using Slafight_Plugin_EXILED.API.Core.Enums;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// ラウンドごとにゲームモードを抽選して起動し、モードが名乗る制限を実際に効かせます。
/// </summary>
/// <remarks>
/// <b>ここにモードごとの分岐はありません。</b>
/// 何を止めるかはモード自身が <see cref="GameMode.AllowsWarhead"/> などで名乗ります。
/// モードを足すのにこのファイルを触る必要はありません。
///
/// <see cref="GameMode.Weight"/> が 0 のモードは抽選に出ません。手動起動専用です。
///
/// このクラスはどこからも登録されていません。<see cref="EventHandlerBase"/> を
/// 継承しているだけで <see cref="EventHandlerRegistry"/> が購読させます。
/// </remarks>
public sealed class GameModeRoundHandler : EventHandlerBase
{
    /// <summary>
    /// ラウンド開始から抽選までの待ち時間です。
    /// </summary>
    /// <remarks>
    /// 開始直後は役職の割り当てが走っています。人数を数えるモードがそれを跨がないよう、
    /// 一拍置いてから抽選します。
    /// </remarks>
    private const float StartDelay = 0.75f;

    /// <inheritdoc />
    public override HandlerLifetime Lifetime => HandlerLifetime.Round;

    /// <inheritdoc />
    protected override void OnEnabled()
    {
        RoundScope.Current.Delay(StartDelay, StartRolledMode);
    }

    /// <summary>
    /// モードが核を禁じているなら止めます。
    /// </summary>
    public override void OnWarheadStarting(WarheadStartingEventArgs ev)
    {
        if (GameMode.Current is { AllowsWarhead: false })
            ev.IsAllowed = false;
    }

    private static void StartRolledMode()
    {
        if (GameMode.Current is not null) return;

        if (GameMode.Roll() is not { } mode) return;

        if (mode.Start())
            Log.Debug($"[Slafight] ゲームモード '{mode.Name}' を開始しました。");
    }
}
