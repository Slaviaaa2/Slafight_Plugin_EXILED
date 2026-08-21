using System.Linq;
using Exiled.API.Features;
using LabApi.Events.Arguments.ServerEvents;
using Slafight_Plugin_EXILED.API.Core.Enums;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// ラウンドの終了を判定します。
/// </summary>
/// <remarks>
/// <b>自前のタイマーを持ちません。</b>
/// ゲーム本体が「もう終わってよいか」と訊いてくるのに乗って答えます。
/// 旧実装は 1 秒ごとのコルーチンで全員を数え直していました。
///
/// 誰が勝ったかは <see cref="CustomTeam.FindWinner"/> が、
/// その陣営がどう終わるかは <see cref="CustomTeam.UsesVanillaEnding"/> が決めます。
/// ここに陣営ごとの分岐はありません。
///
/// このクラスはどこからも登録されていません。<see cref="EventHandlerBase"/> を
/// 継承しているだけで <see cref="EventHandlerRegistry"/> が購読させます。
/// </remarks>
public sealed class RoundEndHandler : EventHandlerBase
{
    /// <summary>
    /// 勝敗表示を出してから実際に終わらせるまでの秒数です。
    /// </summary>
    private const float EndDelay = 10f;

    private bool ended;

    /// <inheritdoc />
    public override HandlerLifetime Lifetime => HandlerLifetime.Round;

    /// <inheritdoc />
    public override void OnServerRoundEndingConditionsCheck(RoundEndingConditionsCheckEventArgs ev)
    {
        // 独自終了を始めた後は、バニラに二重で終わらせない。
        if (ended)
        {
            ev.CanEnd = false;

            return;
        }

        if (CustomTeam.FindWinner() is { } winner)
        {
            // バニラに任せる陣営なら、そのまま通す。
            if (winner.UsesVanillaEnding) return;

            ev.CanEnd = false;
            End(winner);

            return;
        }

        // 独自終了を持つ陣営がまだ生きているなら、バニラに終わらせない。
        // ここで通すと、勝敗が決まる前にラウンドが畳まれる。
        if (HasCustomEndingTeamAlive())
            ev.CanEnd = false;
    }

    /// <summary>
    /// 独自の終了処理を持つ陣営に、まだ生存者が居るか。
    /// </summary>
    private static bool HasCustomEndingTeamAlive() =>
        CustomTeam.All.Any(team => !team.UsesVanillaEnding && team.Members.Any());

    /// <summary>
    /// 勝敗表示を出してラウンドを畳みます。
    /// </summary>
    private void End(CustomTeam winner)
    {
        ended = true;

        // 同じ陣営でも、走っているゲームモードが別の名乗りをすることがある。
        string text = GameMode.Current?.VictoryText(winner) ?? winner.VictoryText;

        if (text is { Length: > 0 })
        {
            foreach (Player player in Player.List.Where(candidate => candidate.IsSafePlayer()))
            {
                CoreHints.Show(player, text, EndDelay, yCoordinate: 500, fontSize: 40);
            }
        }

        Log.Debug($"[Slafight] ラウンド終了: {winner.GetVictoryReason()}");

        RoundScope.Current.Delay(EndDelay, () => Round.EndRound(true));
    }
}
