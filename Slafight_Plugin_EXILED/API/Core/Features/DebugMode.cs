using System.Collections.Generic;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// プレイヤーごとのデバッグ表示の切り替えです。
/// </summary>
/// <remarks>
/// 寿命は <see cref="PlayerScope"/> に相乗りするので、退出・ラウンド再開で勝手に消えます。
/// 専用のイベント購読も後始末も要りません。
/// </remarks>
public static class DebugMode
{
    private static readonly HashSet<uint> Enabled = [];

    /// <summary>
    /// このプレイヤーがデバッグ表示を出しているか。
    /// </summary>
    public static bool IsEnabled(Player player) =>
        player is not null && Enabled.Contains(player.GetNetId());

    /// <summary>
    /// 切り替えます。
    /// </summary>
    /// <returns>切り替えた結果。</returns>
    public static bool Toggle(Player player) => Set(player, !IsEnabled(player));

    /// <summary>
    /// 明示的に設定します。
    /// </summary>
    /// <returns>設定後の状態。</returns>
    public static bool Set(Player player, bool enabled)
    {
        if (player is null) return false;

        uint netId = player.GetNetId();

        if (netId == 0) return false;

        if (!enabled)
        {
            Enabled.Remove(netId);

            return false;
        }

        if (Enabled.Add(netId))
            PlayerScope.Of(player).OnDispose(_ => Enabled.Remove(netId));

        return true;
    }

    /// <summary>
    /// 全員ぶん消します。
    /// </summary>
    public static void ClearAll() => Enabled.Clear();
}
