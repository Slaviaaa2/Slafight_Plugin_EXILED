using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// 波を出す直前の情報です。ここで波を差し替えたり、取り止めたりできます。
/// </summary>
public sealed class SpawningEventArgs(
    SpawnSet wave,
    SpawnContext context,
    IReadOnlyList<Player> candidates,
    RespawningTeamEventArgs source,
    byte? unitId) : EventArgs
{
    /// <summary>
    /// 出そうとしている波です。<b>差し替えられます。</b>
    /// </summary>
    public SpawnSet Wave { get; set; } = wave;

    /// <summary>
    /// 抽選に使われた状況です。
    /// </summary>
    public SpawnContext Context { get; } = context;

    /// <summary>
    /// 対象になる観戦者です。
    /// </summary>
    public IReadOnlyList<Player> Candidates { get; } = candidates;

    /// <summary>
    /// 元になったバニラのウェーブ情報です。<c>ForceSpawn</c> 経由なら null。
    /// </summary>
    public RespawningTeamEventArgs Source { get; } = source;

    /// <summary>
    /// この波に割り当てる予定の部隊番号 (UnitId) です。
    /// <see cref="SpawnSet.RespawnFaction"/> が <c>FoundationStaff</c> でなければ null。
    /// </summary>
    /// <remarks>
    /// <b>まだ確定していません。</b>ここで <see cref="Wave"/> を別陣営の波に差し替えたり
    /// <see cref="IsAllowed"/> を false にしたりすれば、この番号は使われません。
    /// 実際に配られた番号は <see cref="SpawnedEventArgs.UnitId"/> を見てください。
    /// </remarks>
    public byte? UnitId { get; } = unitId;

    /// <summary>
    /// false にすると、この波は出ません。
    /// </summary>
    public bool IsAllowed { get; set; } = true;
}
