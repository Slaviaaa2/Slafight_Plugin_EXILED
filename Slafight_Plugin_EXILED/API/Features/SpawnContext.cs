using System.Collections.Generic;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.MainHandlers;

namespace Slafight_Plugin_EXILED.API.Features
{
    /// <summary>
    /// 出現率テーブル＋使用する UnitPack 群から構成されるスポーンコンテキスト。
    /// </summary>
    public class SpawnContext
    {
        public string Name { get; }

        public Dictionary<SpawnTypeId, int> FoundationStaffWaveWeights { get; }
        public Dictionary<SpawnTypeId, int> FoundationEnemyWaveWeights { get; }
        public Dictionary<SpawnTypeId, int> FoundationStaffMiniWaveWeights { get; }
        public Dictionary<SpawnTypeId, int> FoundationEnemyMiniWaveWeights { get; }

        // UnitPack からマージ済みの RoleTables
        public Dictionary<SpawnTypeId, Dictionary<SpawnSystem.SpawnRoleKey, (float maxCount, bool guaranteed)>> RoleTables { get; }

        public SpawnContext(
            string name,
            Dictionary<SpawnTypeId, int> staffWeights,
            Dictionary<SpawnTypeId, int> enemyWeights,
            Dictionary<SpawnTypeId, int> staffMiniWeights,
            Dictionary<SpawnTypeId, int> enemyMiniWeights,
            params UnitPack[] packs)
        {
            Name = name;
            FoundationStaffWaveWeights = staffWeights;
            FoundationEnemyWaveWeights = enemyWeights;
            FoundationStaffMiniWaveWeights = staffMiniWeights;
            FoundationEnemyMiniWaveWeights = enemyMiniWeights;

            RoleTables = new();

            // UnitPack から RoleTables をマージ
            foreach (var pack in packs)
            {
                if (pack == null)
                    continue;

                foreach (var (spawnType, table) in pack.RoleTables)
                {
                    if (!RoleTables.TryGetValue(spawnType, out var target))
                    {
                        target = new Dictionary<SpawnSystem.SpawnRoleKey, (float maxCount, bool guaranteed)>();
                        RoleTables[spawnType] = target;
                    }

                    foreach (var (key, cfg) in table)
                    {
                        // 同一キーがあった場合は上書き（必要ならログを出す）
                        target[key] = cfg;
                    }
                }
            }
        }
    }
}
