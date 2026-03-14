using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.MainHandlers;

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
    //  Config
    // =====================

    public class SpawnConfig
    {
        public Dictionary<SpawnTypeId, int> FoundationStaffWaveWeights { get; set; } = new()
        {
            { SpawnTypeId.MTF_NtfNormal, 80 },
            { SpawnTypeId.MTF_HDNormal,  20 },
            { SpawnTypeId.MTF_SneNormal, 0 },
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
            { SpawnTypeId.MTF_SneBackup, 0 },
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

        public int ScpThresholdHigh    { get; set; } = 3;
        public int PlayerThresholdHigh { get; set; } = 6;

        public int S3005FifthistChance { get; set; } = 5;
        public bool NatoCallsign       { get; set; } = true;
    }

    public static SpawnConfig Config { get; } = new();

    // =====================
    //  CustomSpawningEventArgs
    // =====================

    public class CustomSpawningEventArgs : EventArgs
    {
        /// <summary>この Wave のスポーンを許可するかどうか。</summary>
        public bool IsAllowed { get; set; } = true;

        /// <summary>現在有効なコンテキスト。</summary>
        public SpawnContext NowContext { get; }

        /// <summary>
        /// この Wave で使用する weights。
        /// デフォルトでは NowContext からコピーされた値で初期化される。
        /// </summary>
        public Dictionary<SpawnTypeId, int> ContextOverride { get; }

        /// <summary>
        /// スポーンさせる SpawnTypeId。
        /// null の場合は ContextOverride から抽選される。
        /// </summary>
        public SpawnTypeId? SpawnType { get; set; }

        /// <summary>今回の Wave の陣営。</summary>
        public Faction Faction { get; }

        /// <summary>MiniWave かどうか。</summary>
        public bool IsMiniWave { get; }

        /// <summary>元の RespawningTeamEventArgs。</summary>
        public RespawningTeamEventArgs SourceEventArgs { get; }

        /// <summary>実際にスポーンさせた人数（スポーン前は 0）。</summary>
        public int SpawnCount { get; set; }

        /// <summary>Cassie 用コールサイン（NATO_A など）。</summary>
        public string CassieCallsign { get; set; } = string.Empty;

        /// <summary>表示用コールサイン（ALPHA-05 など）。</summary>
        public string DisplayCallsign { get; set; } = string.Empty;

        public CustomSpawningEventArgs(
            RespawningTeamEventArgs sourceEventArgs,
            SpawnContext nowContext,
            Dictionary<SpawnTypeId, int> baseWeights,
            Faction faction,
            bool isMiniWave)
        {
            SourceEventArgs = sourceEventArgs;
            NowContext = nowContext;
            Faction = faction;
            IsMiniWave = isMiniWave;
            ContextOverride = new Dictionary<SpawnTypeId, int>(baseWeights);
        }
    }

    /// <summary>
    /// スポーン決定前に呼ばれるイベント。
    /// ContextOverride / SpawnType / IsAllowed を通じて今回の Wave を自由にカスタマイズできる。
    /// </summary>
    public static event EventHandler<CustomSpawningEventArgs> Spawning;

    /// <summary>
    /// スポーン処理完了後に呼ばれるイベント。
    /// 引数は CustomSpawningEventArgs を使い回し、SpawnType / SpawnCount / Callsign などが埋まった状態になる。
    /// </summary>
    public static event EventHandler<CustomSpawningEventArgs> Spawned;

    // =====================
    //  状態フラグ
    // =====================

    private bool _isDefaultWave = true;
    public static bool Disable = false;

    public static SpawnOverrideMode OverrideMode { get; private set; } = SpawnOverrideMode.None;
    public static SpawnTypeId? PendingOverrideType { get; private set; }
    public static bool PendingMiniWave { get; private set; }

    public static SpawnSystem Instance { get; set; }

    // =====================
    //  コンストラクタ
    // =====================

    public SpawnSystem()
    {
        Instance = this;
        Exiled.Events.Handlers.Server.RespawningTeam += SpawnHandler;
    }

    ~SpawnSystem()
    {
        Exiled.Events.Handlers.Server.RespawningTeam -= SpawnHandler;
    }

    // =====================
    //  外部からの切り替えAPI
    // =====================

    public static void ReplaceNextSpawn(SpawnTypeId spawnType, bool isMiniWave = false)
    {
        OverrideMode = SpawnOverrideMode.NextWave;
        PendingOverrideType = spawnType;
        PendingMiniWave = isMiniWave;
        Log.Info($"SpawnSystem: Next spawn overridden to {spawnType} (Mini:{isMiniWave})");
    }

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

        // 即時オーバーライド（NextWave）
        if (OverrideMode == SpawnOverrideMode.NextWave && PendingOverrideType.HasValue)
        {
            ev.IsAllowed = false;
            SummonForces(PendingOverrideType.Value, PendingMiniWave);
            ResetOverride();
            return;
        }

        if (!_isDefaultWave)
            return;

        ev.IsAllowed = false;

        SpawnTypeId? decided = null;

        if (ev.NextKnownTeam == Faction.FoundationStaff)
            decided = DecideFoundationStaffType(ev);
        else if (ev.NextKnownTeam == Faction.FoundationEnemy)
            decided = DecideFoundationEnemyType(ev);

        if (decided is null)
        {
            Log.Warn($"SpawnSystem: No spawn type decided for {ev.NextKnownTeam} in context '{SpawnContextRegistry.ActiveContextName}'");
            return;
        }

        SummonForces(decided.Value, ev.Wave.IsMiniWave);
    }

    // =====================
    //  Decide: FoundationStaff
    // =====================

    private SpawnTypeId? DecideFoundationStaffType(RespawningTeamEventArgs ev)
    {
        var ctx = SpawnContextRegistry.ActiveContext;
        if (ctx == null)
            return null;

        int scpCount = Player.List.Count(p => p.Role.Team == Team.SCPs);
        bool highThreat = Player.Count >= Config.PlayerThresholdHigh ||
                          scpCount      >= Config.ScpThresholdHigh;

        var baseWeights = new Dictionary<SpawnTypeId, int>(
            ev.Wave.IsMiniWave
                ? ctx.FoundationStaffMiniWaveWeights
                : ctx.FoundationStaffWaveWeights
        );

        if (highThreat)
        {
            if (ev.Wave.IsMiniWave)
            {
                if (baseWeights.ContainsKey(SpawnTypeId.MTF_HDBackup))
                    baseWeights[SpawnTypeId.MTF_HDBackup] *= 2;
            }
            else
            {
                if (baseWeights.ContainsKey(SpawnTypeId.MTF_HDNormal))
                    baseWeights[SpawnTypeId.MTF_HDNormal] *= 2;
            }
        }

        var args = new CustomSpawningEventArgs(
            ev,
            ctx,
            baseWeights,
            Faction.FoundationStaff,
            ev.Wave.IsMiniWave
        );

        Spawning?.Invoke(null, args);

        if (!args.IsAllowed)
            return null;

        if (args.SpawnType.HasValue)
            return args.SpawnType.Value;

        return PickWeightedSpawnType(args.ContextOverride);
    }

    // =====================
    //  Decide: FoundationEnemy
    // =====================

    private SpawnTypeId? DecideFoundationEnemyType(RespawningTeamEventArgs ev)
    {
        var ctx = SpawnContextRegistry.ActiveContext;
        if (ctx == null)
            return null;

        var baseWeights = new Dictionary<SpawnTypeId, int>(
            ev.Wave.IsMiniWave
                ? ctx.FoundationEnemyMiniWaveWeights
                : ctx.FoundationEnemyWaveWeights
        );

        var args = new CustomSpawningEventArgs(
            ev,
            ctx,
            baseWeights,
            Faction.FoundationEnemy,
            ev.Wave.IsMiniWave
        );

        // 3005/Fifthist などの特殊処理は全部ここにぶら下がるハンドラ側で書く
        Spawning?.Invoke(null, args);

        if (!args.IsAllowed)
            return null;

        if (args.SpawnType.HasValue)
            return args.SpawnType.Value;

        return PickWeightedSpawnType(args.ContextOverride);
    }

    // =====================
    //  SummonForces
    // =====================

    public void SummonForces(SpawnTypeId spawnType, bool isMiniWave)
    {
        _isDefaultWave = false;

        var specs = Player.List
            .Where(p =>
                p.Role == RoleTypeId.Spectator &&
                p.GetCustomRole() == CRoleTypeId.None)
            .ToList();

        int spawnCount = specs.Count;

        string cassieCallsign  = string.Empty;
        string displayCallsign = string.Empty;

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
                or SpawnTypeId.MTF_LastOperationNormal or SpawnTypeId.MTF_LastOperationBackup
                => Faction.FoundationStaff,

            SpawnTypeId.GOI_ChaosNormal or SpawnTypeId.GOI_ChaosBackup
                or SpawnTypeId.GOI_FifthistNormal or SpawnTypeId.GOI_FifthistBackup
                or SpawnTypeId.GOI_GoCNormal or SpawnTypeId.GOI_GoCBackup
                => Faction.FoundationEnemy,

            _ => Faction.Unclassified
        };

        // Spawned イベントに渡す Args を組み立てる
        var ctx = SpawnContextRegistry.ActiveContext;
        var dummyWeights = new Dictionary<SpawnTypeId, int>(); // ここでは使わないが型上必要

        var spawnedArgs = new CustomSpawningEventArgs(
            sourceEventArgs: null,
            nowContext: ctx,
            baseWeights: dummyWeights,
            faction: faction,
            isMiniWave: isMiniWave)
        {
            SpawnType = spawnType,
            SpawnCount = spawnCount,
            CassieCallsign = cassieCallsign,
            DisplayCallsign = displayCallsign,
        };

        Spawned?.Invoke(null, spawnedArgs);

        Timing.CallDelayed(0.02f, () => _isDefaultWave = true);
    }

    // =====================
    //  汎用ロール割り当て
    // =====================

    private void AssignTeamRoles(
        SpawnTypeId spawnType,
        Func<Player, bool> playerFilter,
        int? fixedCount = null)
    {
        var ctx = SpawnContextRegistry.ActiveContext;
        if (ctx == null)
            return;

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
        {
            Log.Warn($"SpawnSystem: No valid spawn types in '{SpawnContextRegistry.ActiveContextName}'");
            return null;
        }

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