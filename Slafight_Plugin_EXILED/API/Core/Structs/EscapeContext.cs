using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Structs;

/// <summary>
/// 脱出しようとしている 1 人ぶんの状況です。役職と陣営はこれを見て行き先を決めます。
/// </summary>
/// <remarks>
/// 役職が変わる<b>前</b>の状態を写し取ったものです。脱出後に配られる
/// <see cref="Slafight_Plugin_EXILED.Handlers.EscapeHandler.Escaped"/> でも、
/// 「誰がどの陣営から、誰に護送されて出て行ったか」がそのまま残ります。
/// </remarks>
public readonly struct EscapeContext
{
    /// <summary>
    /// 指定したプレイヤーの現在の状況を写し取ります。
    /// </summary>
    public EscapeContext(Player player)
    {
        Player = player;
        Role = CustomRole.Of(player);
        Team = CustomTeam.Of(player);

        // 手錠を掛けられていなければ Cuffer は null になる。
        Escort = player?.Cuffer;
        EscortRole = CustomRole.Of(Escort);
        EscortTeam = CustomTeam.Of(Escort);
    }

    /// <summary>
    /// 脱出しようとしているプレイヤーです。
    /// </summary>
    public Player Player { get; }

    /// <summary>
    /// そのプレイヤーのカスタム役職です。持っていなければ null。
    /// </summary>
    public CustomRole Role { get; }

    /// <summary>
    /// そのプレイヤーの陣営です。属していなければ null。
    /// </summary>
    public CustomTeam Team { get; }

    /// <summary>
    /// 手錠を掛けて連れてきた相手です。単独で出てきたなら null。
    /// </summary>
    public Player Escort { get; }

    /// <summary>
    /// 護送してきた相手のカスタム役職です。
    /// </summary>
    public CustomRole EscortRole { get; }

    /// <summary>
    /// 護送してきた相手の陣営です。
    /// </summary>
    public CustomTeam EscortTeam { get; }

    /// <summary>
    /// 誰かに護送されているかどうか。
    /// </summary>
    public bool IsEscorted => Escort is not null;

    /// <summary>
    /// <typeparamref name="T"/> に護送されているかどうか。派生した陣営も含みます。
    /// </summary>
    public bool IsEscortedBy<T>() where T : CustomTeam => EscortTeam is T;

    /// <summary>
    /// <typeparamref name="T"/> と同じ側の陣営に護送されているかどうか。
    /// </summary>
    /// <remarks>
    /// 「財団側に連れて行かれた」のように、<see cref="CustomTeam.Allies"/> でひとまとまりに
    /// なっている勢力を指したいときに使います。陣営をいくつも並べた配列は要りません。
    /// </remarks>
    public bool IsEscortedByAllyOf<T>() where T : CustomTeam, new() =>
        EscortTeam is not null && CustomTeam.Get<T>().IsSameSide(EscortTeam);
}
