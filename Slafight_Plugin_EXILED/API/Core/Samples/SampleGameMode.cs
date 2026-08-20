using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Samples;

/// <summary>
/// ゲームモード (旧 SpecialEvent) の書き方の見本です。
///
/// 見どころは、<b>打ち切り判定を自分で持っていない</b>ことです。
/// 旧実装では各イベントが世代カウンタ (EventPID) を抱え、
/// コルーチンの合間に自分で照合していました。
/// ここでは <see cref="GameMode.Scope"/> に載せるか
/// <see cref="GameMode.IsCanceled"/> を見るだけで済みます。
/// </summary>
public sealed class SampleGameMode : GameMode
{
    public override string Name => "Sample Mode";

    public override string Description => "動作確認用のゲームモードです。";

    /// <summary>
    /// 0 なので抽選には出ません。コマンドから明示的に <see cref="GameMode.Start"/> したときだけ走ります。
    /// </summary>
    public override int Weight => 0;

    protected override void OnStarted()
    {
        Log.Debug($"[Sample] {Name} を開始しました。");

        // ラウンドが終われば、この遅延実行は呼ばれません。
        Scope.Delay(10f, () => Log.Debug($"[Sample] {Name}: 開始から 10 秒経ちました。"));

        Scope.RunLoop(30f, () =>
        {
            if (IsCanceled) return;

            Log.Debug($"[Sample] {Name}: 継続中です。");
        });
    }

    protected override void OnStopped()
    {
        Log.Debug($"[Sample] {Name} を終了しました。");
    }
}
