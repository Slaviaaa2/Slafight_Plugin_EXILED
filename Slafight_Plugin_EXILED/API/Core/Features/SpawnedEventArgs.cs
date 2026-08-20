using System;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// 波を出した直後の情報です。曲やアナウンスはこれを拾って出します。
/// </summary>
public sealed class SpawnedEventArgs(SpawnSet wave, int spawnCount) : EventArgs
{
    /// <summary>
    /// 実際に出た波です。
    /// </summary>
    public SpawnSet Wave { get; } = wave;

    /// <summary>
    /// 実際に割り当てた人数です。
    /// </summary>
    public int SpawnCount { get; } = spawnCount;
}
