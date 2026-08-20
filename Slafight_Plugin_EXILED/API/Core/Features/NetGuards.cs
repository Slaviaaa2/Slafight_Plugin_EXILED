using CentralAuth;
using Exiled.API.Features;
using Mirror;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// Mirror へ直接メッセージを送るときの受信可否判定です。
/// </summary>
/// <remarks>
/// 使うのは「EXILED や LabApi を通さず自分で <see cref="NetworkConnection"/> に書き込む」
/// 場面だけです。普通のプレイヤー操作には要りません。そちらは
/// <c>PlayerSafetyExtensions.IsSafePlayer</c> で足ります。
///
/// AGENTS.md のとおり <see cref="NetworkConnection.isReady"/> だけでは不十分で、
/// 実クライアントは <see cref="ClientInstanceMode.ReadyClient"/> であることも必要です。
/// </remarks>
public static class NetGuards
{
    /// <summary>
    /// 実クライアントとして受信できる状態かどうか。
    /// スポーン途中のハブや NPC・ダミーに送ると壊れます。
    /// </summary>
    public static bool IsReadyClient(ReferenceHub hub)
    {
        if (hub is null) return false;
        if (hub.Mode != ClientInstanceMode.ReadyClient) return false;
        if (hub.netId == 0) return false;

        return hub.connectionToClient is { isReady: true };
    }

    /// <inheritdoc cref="IsReadyClient(ReferenceHub)"/>
    public static bool IsReadyClient(Player player) =>
        player?.ReferenceHub is { } hub && IsReadyClient(hub);

    /// <summary>
    /// 送信先として妥当かどうか。<see cref="IsReadyClient(ReferenceHub)"/> より緩く、
    /// 「未認証でなく、接続が生きている」ことだけを見ます。
    /// ロールシンクのように全ハブへ配る処理で使います。
    /// </summary>
    public static bool IsValidReceiver(ReferenceHub hub)
    {
        if (hub is null) return false;
        if (hub.isLocalPlayer) return false;
        if (hub.Mode == ClientInstanceMode.Unverified) return false;

        return hub.connectionToClient is { isReady: true };
    }
}
