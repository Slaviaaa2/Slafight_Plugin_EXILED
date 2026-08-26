using System.Collections.Generic;
using Exiled.API.Features;
using MEC;
using Server = Exiled.Events.Handlers.Server;

namespace Slafight_Plugin_EXILED.API.Features;

/// <summary>
/// ラウンド中だけ生きていればよいコルーチンの起動口。
///
/// ロール・アイテム・アビリティ・特殊イベントのコルーチンは
/// <c>while (true)</c> + 自前の終了条件で書かれているものが多く、
/// <see cref="CoroutineHandle"/> を保持しない起動が大半だった。
/// 終了条件の穴（対象が既に破棄されている、フラグが落ちない、例外で判定に到達しない等）に
/// はまった分は誰も止められず、ラウンドをまたいで残り続けて時間経過とともに重くなる。
///
/// ここを通して起動しておけば、<see cref="Server.RestartingRound"/> で MEC のタグごと
/// まとめて停止できるため、取りこぼしがそのラウンド限りで打ち切られる。
/// プラグイン生存中ずっと回す必要があるループ（HUD ループなど）には使わないこと。
/// </summary>
public static class RoundScopedCoroutines
{
    /// <summary>MEC 側でまとめて停止するためのタグ。</summary>
    public const string Tag = "Slafight.RoundScoped";

    private static bool _registered;

    /// <summary>
    /// 他の RestartingRound ハンドラより先に停止させたいので、
    /// Plugin.OnEnabled の先頭付近で登録すること。
    /// </summary>
    public static void Register()
    {
        if (_registered)
            return;

        Server.RestartingRound += KillAll;
        _registered = true;
    }

    public static void Unregister()
    {
        if (!_registered)
            return;

        Server.RestartingRound -= KillAll;
        _registered = false;
        KillAll();
    }

    /// <summary>ラウンド終了時に自動停止するコルーチンとして起動する。</summary>
    public static CoroutineHandle Run(IEnumerator<float> coroutine)
        => Timing.RunCoroutine(coroutine, Tag);

    public static void KillAll()
    {
        int killed = Timing.KillCoroutines(Tag);
        if (killed > 0)
            Log.Debug($"[RoundScopedCoroutines] Killed {killed} round-scoped coroutine(s) on round restart.");
    }
}
