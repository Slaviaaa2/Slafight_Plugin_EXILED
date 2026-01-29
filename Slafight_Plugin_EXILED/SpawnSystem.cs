using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using Respawning.Waves;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED;

public class SpawnSystem
{
    // =====================
    //  種別
    // =====================

    public enum SpawnRoleKind
    {
        Vanilla,
        Custom,
    }

    public readonly record struct SpawnRoleKey
    {
        public SpawnRoleKind Kind { get; }
        public RoleTypeId Vanilla { get; }
        public CRoleTypeId Custom { get; }

        public SpawnRoleKey(RoleTypeId vanilla)
        {
            Kind = SpawnRoleKind.Vanilla;
            Vanilla = vanilla;
            Custom = CRoleTypeId.None;
        }

        public SpawnRoleKey(CRoleTypeId custom)
        {
            Kind = SpawnRoleKind.Custom;
            Custom = custom;
            Vanilla = RoleTypeId.None;
        }
    }

    public enum SpawnOverrideMode
    {
        None,       // 通常
        NextWave,   // 次のRespawningTeamを上書き
        Immediate,  // 即時Summon
    }

    // =====================
    //  内部イベント
    // =====================

    public class SpawningEventArgs : EventArgs
    {
        public SpawnTypeId SpawnType { get; }
        public bool IsMiniWave { get; }
        public Faction Faction { get; }

        // Cassie用: NATO_A / NATO_B ...
        public string CassieCallsign { get; }

        // 表示用: ALPHA-05 など
        public string DisplayCallsign { get; }

        // Waveの湧き人数
        public int SpawnCount { get; }

        // どのコンテキストで湧いたか（"Default" / "EventX"）
        public string ContextName { get; }

        public SpawningEventArgs(
            SpawnTypeId spawnType,
            bool isMiniWave,
            Faction faction,
            string contextName,
            string cassieCallsign = "",
            string displayCallsign = "",
            int spawnCount = 0)
        {
            SpawnType = spawnType;
            IsMiniWave = isMiniWave;
            Faction = faction;
            ContextName = contextName;
            CassieCallsign = cassieCallsign;
            DisplayCallsign = displayCallsign;
            SpawnCount = spawnCount;
        }
    }

    public static event EventHandler<SpawningEventArgs> Spawning;

    private static void OnSpawning(
        SpawnTypeId spawnType,
        bool isMiniWave,
        Faction faction,
        string contextName,
        string cassieCallsign = "",
        string displayCallsign = "",
        int spawnCount = 0)
    {
        Spawning?.Invoke(null, new SpawningEventArgs(
            spawnType,
            isMiniWave,
            faction,
            contextName,
            cassieCallsign,
            displayCallsign,
            spawnCount));
    }

    // =====================
    //  Config + Context
    // =====================

    public class SpawnConfig
    {
        // 元の「通常時」テーブル
        public Dictionary<SpawnTypeId, int> FoundationStaffWaveWeights { get; set; } = new()
        {
            { SpawnTypeId.MTF_NtfNormal, 80 },
            { SpawnTypeId.MTF_HDNormal,  20 },
        };

        public Dictionary<SpawnTypeId, int> FoundationEnemyWaveWeights { get; set; } = new()
        {
            { SpawnTypeId.GOI_ChaosNormal,    100 },
            { SpawnTypeId.GOI_FifthistNormal, 0   },
        };

        public Dictionary<SpawnTypeId, int> FoundationStaffMiniWaveWeights { get; set; } = new()
        {
            { SpawnTypeId.MTF_NtfBackup, 80 },
            { SpawnTypeId.MTF_HDBackup,  20 },
        };

        public Dictionary<SpawnTypeId, int> FoundationEnemyMiniWaveWeights { get; set; } = new()
        {
            { SpawnTypeId.GOI_ChaosBackup,    100 },
            { SpawnTypeId.GOI_FifthistBackup, 0   },
        };

        public Dictionary<SpawnTypeId, float> SpawnRatios { get; set; } = new()
        {
            { SpawnTypeId.MTF_NtfNormal,      1.0f },
            { SpawnTypeId.MTF_NtfBackup,      1.0f },
            { SpawnTypeId.MTF_HDNormal,       1.0f },
            { SpawnTypeId.MTF_HDBackup,       0.5f },
            { SpawnTypeId.GOI_ChaosNormal,    1.0f },
            { SpawnTypeId.GOI_ChaosBackup,    1.0f },
            { SpawnTypeId.GOI_FifthistNormal, 0.25f },
            { SpawnTypeId.GOI_FifthistBackup, 1f / 6f },
        };

        public Dictionary<SpawnTypeId, Dictionary<SpawnRoleKey, (float maxCount, bool guaranteed)>> RoleTables
        { get; set; } = new()
        {
            // NTF
            {
                SpawnTypeId.MTF_NtfNormal, new()
                {
                    { new SpawnRoleKey(CRoleTypeId.NtfGeneral),    (1f,  false)  },
                    { new SpawnRoleKey(RoleTypeId.NtfCaptain),     (1f,  true)   },
                    { new SpawnRoleKey(CRoleTypeId.NtfLieutenant), (2f,  false)  },
                    { new SpawnRoleKey(RoleTypeId.NtfSergeant),    (2f,  false)  },
                    { new SpawnRoleKey(RoleTypeId.NtfPrivate),     (99f, true)   },
                }
            },
            {
                SpawnTypeId.MTF_NtfBackup, new()
                {
                    { new SpawnRoleKey(RoleTypeId.NtfSergeant), (1f,  true) },
                    { new SpawnRoleKey(RoleTypeId.NtfPrivate),  (99f, true) },
                }
            },

            // Hammer Down
            {
                SpawnTypeId.MTF_HDNormal, new()
                {
                    { new SpawnRoleKey(CRoleTypeId.HdMarshal),   (1f,  false)  },
                    { new SpawnRoleKey(CRoleTypeId.HdCommander), (2f,  true)   },
                    { new SpawnRoleKey(CRoleTypeId.HdInfantry),  (99f, false)  },
                }
            },
            {
                SpawnTypeId.MTF_HDBackup, new()
                {
                    { new SpawnRoleKey(CRoleTypeId.HdCommander), (1f,  true)  },
                    { new SpawnRoleKey(CRoleTypeId.HdInfantry),  (99f, false) },
                }
            },

            // Chaos
            {
                SpawnTypeId.GOI_ChaosNormal, new()
                {
                    { new SpawnRoleKey(CRoleTypeId.ChaosCommando), (1f,  false) },
                    { new SpawnRoleKey(RoleTypeId.ChaosRepressor), (2f,  false) },
                    { new SpawnRoleKey(CRoleTypeId.ChaosSignal),   (2f,  false) },
                    { new SpawnRoleKey(RoleTypeId.ChaosMarauder),  (2f,  false) },
                    { new SpawnRoleKey(RoleTypeId.ChaosRifleman),  (99f, false) },
                }
            },
            {
                SpawnTypeId.GOI_ChaosBackup, new()
                {
                    { new SpawnRoleKey(CRoleTypeId.ChaosSignal),  (1f,  true)  },
                    { new SpawnRoleKey(RoleTypeId.ChaosMarauder), (2f,  false) },
                    { new SpawnRoleKey(RoleTypeId.ChaosRifleman), (99f, false) },
                }
            },

            // Fifthist
            {
                SpawnTypeId.GOI_FifthistNormal, new()
                {
                    { new SpawnRoleKey(CRoleTypeId.FifthistPriest),  (1f,  true)  },
                    { new SpawnRoleKey(CRoleTypeId.FifthistRescure), (3f,  false) },
                    { new SpawnRoleKey(CRoleTypeId.FifthistConvert), (99f, false) }
                }
            },
            {
                SpawnTypeId.GOI_FifthistBackup, new()
                {
                    { new SpawnRoleKey(CRoleTypeId.FifthistRescure), (2f,  false) },
                    { new SpawnRoleKey(CRoleTypeId.FifthistConvert), (99f, false) }
                }
            },
        };

        public int ScpThresholdHigh    { get; set; } = 3;
        public int PlayerThresholdHigh { get; set; } = 6;

        public int S3005FifthistChance { get; set; } = 5;
        public bool NatoCallsign       { get; set; } = true;
    }

    // コンテキスト：通常 / イベント時にテーブルを差し替える用
    public class SpawnContext
    {
        public string Name { get; }
        public Dictionary<SpawnTypeId, int> FoundationStaffWaveWeights { get; }
        public Dictionary<SpawnTypeId, int> FoundationEnemyWaveWeights { get; }
        public Dictionary<SpawnTypeId, int> FoundationStaffMiniWaveWeights { get; }
        public Dictionary<SpawnTypeId, int> FoundationEnemyMiniWaveWeights { get; }
        public Dictionary<SpawnTypeId, Dictionary<SpawnRoleKey, (float maxCount, bool guaranteed)>> RoleTables { get; }

        public SpawnContext(
            string name,
            Dictionary<SpawnTypeId, int> staffWeights,
            Dictionary<SpawnTypeId, int> enemyWeights,
            Dictionary<SpawnTypeId, int> staffMiniWeights,
            Dictionary<SpawnTypeId, int> enemyMiniWeights,
            Dictionary<SpawnTypeId, Dictionary<SpawnRoleKey, (float maxCount, bool guaranteed)>> roles)
        {
            Name = name;
            FoundationStaffWaveWeights = staffWeights;
            FoundationEnemyWaveWeights = enemyWeights;
            FoundationStaffMiniWaveWeights = staffMiniWeights;
            FoundationEnemyMiniWaveWeights = enemyMiniWeights;
            RoleTables = roles;
        }
    }

    public static SpawnConfig Config { get; } = new();

    // Context一覧と現在有効なContext
    public static Dictionary<string, SpawnContext> SpawnContexts { get; } = new();
    public static string ActiveContextName { get; private set; } = "Default";
    public static SpawnContext ActiveContext => SpawnContexts[ActiveContextName];

    // =====================
    //  状態フラグ
    // =====================

    private bool isDefaultWave = true;
    public static bool Disable = false;

    public static SpawnOverrideMode OverrideMode { get; private set; } = SpawnOverrideMode.None;
    public static SpawnTypeId? PendingOverrideType { get; private set; }
    public static bool PendingMiniWave { get; private set; }

    // =====================
    //  コンストラクタ
    // =====================

    public SpawnSystem()
    {
        // Context初期化（必要ならここにイベント用コンテキストを追加）
        if (!SpawnContexts.ContainsKey("Default"))
        {
            SpawnContexts["Default"] = new SpawnContext(
                "Default",
                Config.FoundationStaffWaveWeights,
                Config.FoundationEnemyWaveWeights,
                Config.FoundationStaffMiniWaveWeights,
                Config.FoundationEnemyMiniWaveWeights,
                Config.RoleTables
            );
        }

        Exiled.Events.Handlers.Server.RespawningTeam += SpawnHandler;
    }

    ~SpawnSystem()
    {
        Exiled.Events.Handlers.Server.RespawningTeam -= SpawnHandler;
    }

    // =====================
    //  外部からの切り替えAPI
    // =====================

    // Context切り替え（通常 / イベント）
    public static void SwitchSpawnContext(string contextName)
    {
        if (!SpawnContexts.ContainsKey(contextName))
        {
            Log.Warn($"SpawnSystem: Unknown context '{contextName}'");
            return;
        }

        ActiveContextName = contextName;
        Log.Info($"SpawnSystem: Active context switched to '{contextName}'");
    }

    // コンテキスト追加用（イベント開始時に独自テーブルを登録したい場合など）
    public static void RegisterSpawnContext(SpawnContext context)
    {
        SpawnContexts[context.Name] = context;
        Log.Info($"SpawnSystem: Context '{context.Name}' registered");
    }

    // 次のリスポーン波を強制的に特定のSpawnTypeにする
    public static void ReplaceNextSpawn(SpawnTypeId spawnType, bool isMiniWave = false)
    {
        OverrideMode = SpawnOverrideMode.NextWave;
        PendingOverrideType = spawnType;
        PendingMiniWave = isMiniWave;
        Log.Info($"SpawnSystem: Next spawn overridden to {spawnType} (Mini:{isMiniWave})");
    }

    // 即時に特殊SpawnTypeを湧かせる（Spectatorから）
    public static void ForceSpawnNow(SpawnTypeId spawnType, bool isMiniWave = false)
    {
        Log.Info($"SpawnSystem: ForceSpawnNow {spawnType} (Mini:{isMiniWave})");
        Instance?.SummonForces(spawnType, isMiniWave);
    }

    private static void ResetOverride()
    {
        OverrideMode = SpawnOverrideMode.None;
        PendingOverrideType = null;
    }

    // SpawnSystemインスタンスを外から参照したい場合用（必要ならプラグイン本体でセット）
    public static SpawnSystem Instance { get; set; }

    // =====================
    //  RespawningTeam
    // =====================

    public void SpawnHandler(RespawningTeamEventArgs ev)
    {
        if (Disable)
        {
            ev.IsAllowed = false;
            return;
        }

        // オーバーライド（NextWave）が設定されていたらそれを優先
        if (OverrideMode == SpawnOverrideMode.NextWave && PendingOverrideType.HasValue)
        {
            ev.IsAllowed = false;
            SummonForces(PendingOverrideType.Value, PendingMiniWave);
            ResetOverride();
            return;
        }

        if (!isDefaultWave)
            return;

        ev.IsAllowed = false;

        SpawnTypeId? decided = null;

        if (ev.NextKnownTeam == Faction.FoundationStaff)
            decided = DecideFoundationStaffType(ev);
        else if (ev.NextKnownTeam == Faction.FoundationEnemy)
            decided = DecideFoundationEnemyType(ev);

        if (decided is null)
            return;

        SummonForces(decided.Value, ev.Wave.IsMiniWave);
    }

    private SpawnTypeId? DecideFoundationStaffType(RespawningTeamEventArgs ev)
    {
        int scpCount = Player.List.Count(p => p.Role.Team == Team.SCPs);
        bool highThreat = Player.Count >= Config.PlayerThresholdHigh ||
                          scpCount      >= Config.ScpThresholdHigh;

        var ctx = ActiveContext;

        var weights = new Dictionary<SpawnTypeId, int>(
            ev.Wave.IsMiniWave
                ? ctx.FoundationStaffMiniWaveWeights
                : ctx.FoundationStaffWaveWeights
        );

        if (highThreat)
        {
            if (ev.Wave.IsMiniWave)
            {
                if (weights.ContainsKey(SpawnTypeId.MTF_HDBackup))
                    weights[SpawnTypeId.MTF_HDBackup] *= 2;
            }
            else
            {
                if (weights.ContainsKey(SpawnTypeId.MTF_HDNormal))
                    weights[SpawnTypeId.MTF_HDNormal] *= 2;
            }
        }

        return PickWeightedSpawnType(weights);
    }

    private SpawnTypeId? DecideFoundationEnemyType(RespawningTeamEventArgs ev)
    {
        bool has3005 = Player.List.Any(p => p.GetCustomRole() == CRoleTypeId.Scp3005);

        var ctx = ActiveContext;

        var weights = new Dictionary<SpawnTypeId, int>(
            ev.Wave.IsMiniWave
                ? ctx.FoundationEnemyMiniWaveWeights
                : ctx.FoundationEnemyWaveWeights
        );

        if (has3005)
        {
            if (ev.Wave.IsMiniWave)
            {
                weights[SpawnTypeId.GOI_FifthistBackup] = 40;
                weights[SpawnTypeId.GOI_ChaosBackup]    = 60;
            }
            else
            {
                weights[SpawnTypeId.GOI_FifthistNormal] = 40;
                weights[SpawnTypeId.GOI_ChaosNormal]    = 60;
            }
        }
        else
        {
            if (ev.Wave.IsMiniWave)
            {
                weights[SpawnTypeId.GOI_FifthistBackup] = 0;
                weights[SpawnTypeId.GOI_ChaosBackup]    = 100;
            }
            else
            {
                weights[SpawnTypeId.GOI_FifthistNormal] = 0;
                weights[SpawnTypeId.GOI_ChaosNormal]    = 100;
            }
        }

        return PickWeightedSpawnType(weights);
    }

    // =====================
    //  SummonForces
    // =====================

    public void SummonForces(SpawnTypeId spawnType, bool isMiniWave)
    {
        isDefaultWave = false;

        string cassieCallsign  = string.Empty;
        string displayCallsign = string.Empty;

        var specs = Player.List
            .Where(p =>
                p.Role == RoleTypeId.Spectator &&
                p.GetCustomRole() == CRoleTypeId.None)
            .ToList();

        int spawnCount = specs.Count;

        if (Config.NatoCallsign)
        {
            var nato = GenerateNatoCallsignFull();
            cassieCallsign  = nato.cassie;
            displayCallsign = nato.display;
        }

        AssignTeamRoles(
            spawnType,
            playerFilter: p => specs.Contains(p),
            fixedCount: null);

        Faction faction = spawnType switch
        {
            SpawnTypeId.MTF_NtfNormal or SpawnTypeId.MTF_NtfBackup
                or SpawnTypeId.MTF_HDNormal or SpawnTypeId.MTF_HDBackup
                => Faction.FoundationStaff,

            SpawnTypeId.GOI_ChaosNormal or SpawnTypeId.GOI_ChaosBackup
                or SpawnTypeId.GOI_FifthistNormal or SpawnTypeId.GOI_FifthistBackup
                => Faction.FoundationEnemy,

            _ => Faction.Unclassified
        };

        OnSpawning(spawnType, isMiniWave, faction, ActiveContextName, cassieCallsign, displayCallsign, spawnCount);

        Timing.CallDelayed(0.02f, () => isDefaultWave = true);
    }

    // =====================
    //  汎用ロール割り当て
    // =====================

    private void AssignTeamRoles(
        SpawnTypeId spawnType,
        Func<Player, bool> playerFilter,
        int? fixedCount = null)
    {
        var ctx = ActiveContext;

        if (!ctx.RoleTables.TryGetValue(spawnType, out var table) || table.Count == 0)
            return;

        var candidates = Player.List
            .Where(playerFilter)
            .Shuffle()
            .ToList();

        if (candidates.Count == 0)
            return;

        int targetCount;
        if (fixedCount.HasValue)
        {
            targetCount = Math.Min(fixedCount.Value, candidates.Count);
        }
        else
        {
            var ratio = Config.SpawnRatios.GetValueOrDefault(spawnType, 1.0f);

            targetCount = (int)Math.Truncate(candidates.Count * ratio);
            if (targetCount <= 0)
                targetCount = candidates.Count;
        }

        var slots = new List<SpawnRoleKey>();

        foreach (var kvp in table)
        {
            var roleKey = kvp.Key;
            var (maxCount, guaranteed) = kvp.Value;
            int max = (int)maxCount;
            if (max <= 0) continue;

            if (guaranteed && slots.Count < targetCount)
            {
                slots.Add(roleKey);
                max--;
            }

            for (int i = 0; i < max && slots.Count < targetCount; i++)
                slots.Add(roleKey);
        }

        if (slots.Count < targetCount && table.Count > 0)
        {
            var filler = table
                .OrderByDescending(kvp => kvp.Value.maxCount)
                .First().Key;

            while (slots.Count < targetCount)
                slots.Add(filler);
        }

        slots = slots.Shuffle().ToList();
        int assignCount = Math.Min(targetCount, Math.Min(slots.Count, candidates.Count));

        for (int i = 0; i < assignCount; i++)
        {
            var player = candidates[i];
            var key    = slots[i];

            switch (key.Kind)
            {
                case SpawnRoleKind.Vanilla:
                    player.SetRole(key.Vanilla);
                    break;

                case SpawnRoleKind.Custom:
                    player.SetRole(key.Custom);
                    break;
            }
        }
    }

    private SpawnTypeId? PickWeightedSpawnType(Dictionary<SpawnTypeId, int> weights)
    {
        var valid = weights.Where(kvp => kvp.Value > 0).ToList();
        if (!valid.Any())
            return null;

        int total = valid.Sum(kvp => kvp.Value);
        int roll  = Random.Range(0, total);

        int cum = 0;
        foreach (var kvp in valid)
        {
            cum += kvp.Value;
            if (roll < cum)
                return kvp.Key;
        }

        return valid.First().Key;
    }

    // Cassie用(NATO_Aなど)と表示用(ALPHA-05)を両方返す
    private (string cassie, string display) GenerateNatoCallsignFull()
    {
        List<string> NatoForce = new()
        {
            "NATO_A","NATO_B","NATO_C","NATO_D","NATO_E","NATO_F","NATO_G","NATO_H","NATO_I","NATO_J",
            "NATO_K","NATO_L","NATO_M","NATO_N","NATO_O","NATO_P","NATO_Q","NATO_R","NATO_S","NATO_T",
            "NATO_U","NATO_V","NATO_W","NATO_X","NATO_Y","NATO_Z"
        };

        List<string> NatoForceL = new()
        {
            "ALPHA","BRAVO","CHARLIE","DELTA","ECHO","FOXTROT","GOLF","HOTEL","INDIA","JULIETT",
            "KILO","LIMA","MIKE","NOVEMBER","OSCAR","PAPA","QUEBEC","ROMEO","SIERRA","TANGO",
            "UNIFORM","VICTOR","WHISKEY","XRAY","YANKEE","ZULU"
        };

        string natoForce  = NatoForce.RandomItem();
        string natoForceL = NatoForceL[NatoForce.IndexOf(natoForce)];
        int natoForceNum  = Random.Range(1, 20);

        return (natoForce, $"{natoForceL}-{natoForceNum:00}");
    }
}

// =====================
//  Shuffle拡張
// =====================

public static class EnumerableExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        int n = list.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int j = Random.Range(i, n);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}