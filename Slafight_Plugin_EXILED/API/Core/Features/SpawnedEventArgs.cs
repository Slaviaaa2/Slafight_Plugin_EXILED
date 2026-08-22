using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Respawning.NamingRules;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// 波を出した直後の情報です。曲やアナウンスはこれを拾って出します。
/// </summary>
public sealed class SpawnedEventArgs(SpawnSet wave, IReadOnlyList<Player> players, byte? unitId, string unitName) : EventArgs
{
    /// <summary>
    /// 実際に出た波です。
    /// </summary>
    public SpawnSet Wave { get; } = wave;

    /// <summary>
    /// この波で実際に役職を割り当てられたプレイヤーです。割り当てた順に並びます。
    /// </summary>
    /// <remarks>
    /// <see cref="SpawningEventArgs.Candidates"/> とは別物です。あちらは対象になりえた観戦者、
    /// こちらは実際に出た人だけで、役職の割り当てに失敗した人は含まれません。
    /// </remarks>
    public IReadOnlyList<Player> Players { get; } = players ?? [];

    /// <summary>
    /// この波の全員に割り当てた部隊番号 (UnitId) です。
    /// FoundationStaff の波でなければ null。
    /// </summary>
    /// <remarks>
    /// <see cref="SpawnSystem"/> が <see cref="NamingRulesManager.ServerGenerateName"/> で
    /// 新しい部隊名を 1 つ作り、その添字をこの波の全員に配っています。
    /// 既に出ている部隊とは重なりません。
    /// </remarks>
    public byte? UnitId { get; } = unitId;

    /// <summary>
    /// この波に付いた部隊名です。<c>ALPHA-01</c> の形式。
    /// FoundationStaff の波でなければ null。
    /// </summary>
    /// <remarks>
    /// <see cref="NamingRulesManager.GeneratedNames"/> ではなく
    /// <see cref="UnitNamingRule.LastGeneratedName"/> から取っています。
    /// 前者はホストクライアントが <c>UnitNameMessage</c> を処理してから増えるので、
    /// 採番した直後にはまだ載っていません。後者は同期で入るため確実です。
    /// </remarks>
    public string UnitName { get; } = unitName;

    /// <summary>
    /// 実際に割り当てた人数です。
    /// </summary>
    public int SpawnCount => Players.Count;
}
