using System;
using Exiled.API.Features;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Extension;
using MEC;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;

using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// 新 API から一時的な文言を出すための入口です。
/// </summary>
/// <remarks>
/// <b>EXILED の <c>Player.ShowHint</c> を使ってはいけません。</b>
/// このサーバーの表示層は HintServiceMeow です。EXILED のヒントはバニラの
/// <c>HintDisplay</c> へ直接出るため、HSM の互換アダプタに吸収されなければ
/// 「一瞬バニラのヒントが出てから HUD が描き直される」という見え方になります。
/// 吸収されるかどうかは、その瞬間に HSM 側へ表示中のヒントがあるかに依存するので、
/// 表示のたびに当たり外れが出ます。
///
/// ここは最初から HSM に置きに行くので、その経路自体を通りません。
/// </remarks>
public static class CoreHints
{
    /// <summary>
    /// 一時的な文言を出します。同じ枠の古い文言は差し替えます。
    /// </summary>
    /// <param name="player">出す相手。</param>
    /// <param name="text">出す文言。空なら何もしません。</param>
    /// <param name="duration">表示し続ける秒数。</param>
    /// <param name="id">表示枠。既定は一時表示用の枠です。</param>
    /// <param name="yCoordinate">縦位置。</param>
    /// <param name="fontSize">文字の大きさ。</param>
    public static void Show(
        Player player,
        string text,
        float duration,
        string id = HudConstId.TemporaryHintService,
        int yCoordinate = 700,
        int fontSize = 24)
    {
        if (!player.IsSafePlayer() || string.IsNullOrEmpty(text))
            return;

        try
        {
            var display = player.GetPlayerDisplay();

            // 同じ枠に残っているものは先に外す。重ねると二重に見える。
            if (display.GetHint(id) is { } existing)
                display.RemoveHint(existing);

            display.ShowHint(
                new Hint
                {
                    Id = id,
                    Alignment = HintAlignment.Center,
                    XCoordinate = 0,
                    YCoordinate = yCoordinate,
                    FontSize = fontSize,
                    SyncSpeed = HintSyncSpeed.Fast,
                    Text = text,
                },
                duration);
        }
        catch (Exception exception)
        {
            Log.Warn($"[Slafight] ヒントを出せませんでした ({player.Nickname}): {exception.Message}");
        }
    }

    /// <summary>
    /// 役職に就いたときの説明を出します。
    /// </summary>
    /// <remarks>枠と位置は移行前の役職説明と同じです。</remarks>
    public static void ShowRoleDescription(Player player, string text, float duration) =>
        Show(player, text, duration, HudConstId.CRoleSpawned, yCoordinate: 770);
}
