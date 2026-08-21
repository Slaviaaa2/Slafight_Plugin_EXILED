using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.API.Features.FilmmakerAnimations;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.Hints;
using Slafight_Plugin_EXILED.Patches;
using Slafight_Plugin_EXILED.ProximityChat;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// まだ <see cref="EventHandlerBase"/> へ移していないサブシステムを起動します。
/// </summary>
/// <remarks>
/// <b>これは移行用の足場です。恒久的なものではありません。</b>
///
/// 旧実装ではこれらを <c>Plugin.cs</c> の手書き <c>Register()</c> と、
/// 反射式の <c>AutoHandlerBootstrapRegister</c> の 2 系統が起動していました。
/// どちらも削除しましたが、ここに並んでいるクラスはまだ
/// <c>static Register()/Unregister()</c> のままなので、誰も起こさなくなっていました
/// (HUD が出なくなったのがその症状です)。
///
/// <b>ここに新しい行を足さないでください。</b>
/// 新しい機能は <see cref="EventHandlerBase"/> を継承すれば自動で購読されます。
/// 1 つずつ移行して、この一覧が空になったらこのクラスごと消します。
///
/// 旧 <c>AutoHandlerBootstrapRegister</c> は <c>Assembly.GetTypes()</c> 順という
/// 非決定的な順序で起動していました。ここは<b>明示した順</b>で起こし、逆順で止めます。
/// </remarks>
public sealed class LegacySubsystemBootstrap : EventHandlerBase
{
    /// <summary>
    /// 起動する順に並べたサブシステムです。停止はこの逆順で行います。
    /// </summary>
    private static readonly IReadOnlyList<Subsystem> Subsystems =
    [
        // コルーチン管理。他が使うので最初に。
        new Subsystem(nameof(TimingUtils), TimingUtils.Register, TimingUtils.Unregister),

        new Subsystem(nameof(NetworkVisibilityManager), NetworkVisibilityManager.Register, NetworkVisibilityManager.Unregister),
        new Subsystem(nameof(IntercomApiHandler), IntercomApiHandler.Register, IntercomApiHandler.Unregister),
        new Subsystem(nameof(KillCounter), KillCounter.Register, KillCounter.Unregister),
        new Subsystem(nameof(CustomShieldState), CustomShieldState.RegisterEvents, CustomShieldState.UnregisterEvents),
        new Subsystem(nameof(Scp1576DatabaseHandler), Scp1576DatabaseHandler.Register, Scp1576DatabaseHandler.Unregister),
        new Subsystem(nameof(RoundHazardController), RoundHazardController.Register, RoundHazardController.Unregister),

        // 音声・メディア。ProximityChat がボイス経路を登録するので、その前に。
        new Subsystem(nameof(PlayerSpeakerManager), PlayerSpeakerManager.RegisterEvents, PlayerSpeakerManager.UnregisterEvents),
        new Subsystem(nameof(VoiceRoutingApi), VoiceRoutingApi.RegisterEvents, VoiceRoutingApi.UnregisterEvents),
        new Subsystem(nameof(SnakeImageApi), SnakeImageApi.RegisterEvents, SnakeImageApi.UnregisterEvents),
        new Subsystem(nameof(WaypointChunkStreamer), WaypointChunkStreamer.RegisterEvents, WaypointChunkStreamer.UnregisterEvents),
        new Subsystem(nameof(FilmmakerAnimationPlayer), FilmmakerAnimationPlayer.RegisterEvents, FilmmakerAnimationPlayer.UnregisterEvents),
        new Subsystem(nameof(Scp914ProcessorFix), Scp914ProcessorFix.Register, Scp914ProcessorFix.Unregister),
        new Subsystem("ProximityChat", Handler.RegisterEvents, Handler.UnregisterEvents),

        // 表示層。読む先が揃ってから起こす。
        new Subsystem(nameof(ScpStatusHints), ScpStatusHints.Register, ScpStatusHints.Unregister),
        new Subsystem(nameof(RespawnTimerHints), RespawnTimerHints.Register, RespawnTimerHints.Unregister),
        new Subsystem(nameof(Scp079PingHints), Scp079PingHints.Register, Scp079PingHints.Unregister),
        new Subsystem(nameof(PlayerHUD), PlayerHUD.Register, PlayerHUD.Unregister),
    ];

    /// <inheritdoc />
    public override void RegisterEvents()
    {
        int started = 0;

        foreach (Subsystem subsystem in Subsystems)
        {
            // 1 つ落ちても残りを巻き添えにしない。
            if (Invoke(subsystem.Name, "起動", subsystem.Start))
                started++;
        }

        Log.Debug($"[Slafight] 未移行サブシステムを起動しました: {started} / {Subsystems.Count} 件");
    }

    /// <inheritdoc />
    public override void UnregisterEvents()
    {
        for (int i = Subsystems.Count - 1; i >= 0; i--)
        {
            Invoke(Subsystems[i].Name, "停止", Subsystems[i].Stop);
        }
    }

    private static bool Invoke(string name, string phase, Action action)
    {
        try
        {
            action();

            return true;
        }
        catch (Exception exception)
        {
            Log.Error($"[Slafight] {name} の{phase}に失敗しました: {exception}");

            return false;
        }
    }

    private sealed class Subsystem(string name, Action start, Action stop)
    {
        public string Name { get; } = name;

        public Action Start { get; } = start;

        public Action Stop { get; } = stop;
    }
}
