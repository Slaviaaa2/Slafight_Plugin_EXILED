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
    //  汎用
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

    // =====================
    //  内部イベント定義
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

        // Waveの湧き人数（旧fifthistCountを統一）
        public int SpawnCount { get; }

        public SpawningEventArgs(
            SpawnTypeId spawnType,
            bool isMiniWave,
            Faction faction,
            string cassieCallsign = "",
            string displayCallsign = "",
            int spawnCount = 0)
        {
            SpawnType = spawnType;
            IsMiniWave = isMiniWave;
            Faction = faction;
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
        string cassieCallsign = "",
        string displayCallsign = "",
        int spawnCount = 0)
    {
        Spawning?.Invoke(null, new SpawningEventArgs(
            spawnType,
            isMiniWave,
            faction,
            cassieCallsign,
            displayCallsign,
            spawnCount));
    }

    // =====================
    //  Config 定義
    // =====================

    public class SpawnConfig
    {
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
                    { new SpawnRoleKey(CRoleTypeId.HdMarshal),   (1f,  true)  },
                    { new SpawnRoleKey(CRoleTypeId.HdCommander), (2f,  false) },
                    { new SpawnRoleKey(CRoleTypeId.HdInfantry),  (99f, false) },
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
                    { new SpawnRoleKey(CRoleTypeId.ChaosCommando), (2f,  false) },
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

    public static SpawnConfig Config { get; } = new();

    private bool isDefaultWave = true;

    public static bool Disable = false;

    public SpawnSystem()
    {
        Exiled.Events.Handlers.Server.RespawningTeam += SpawnHandler;
    }

    ~SpawnSystem()
    {
        Exiled.Events.Handlers.Server.RespawningTeam -= SpawnHandler;
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

        var weights = new Dictionary<SpawnTypeId, int>(
            ev.Wave.IsMiniWave
                ? Config.FoundationStaffMiniWaveWeights
                : Config.FoundationStaffWaveWeights
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

        var weights = new Dictionary<SpawnTypeId, int>(
            ev.Wave.IsMiniWave
                ? Config.FoundationEnemyMiniWaveWeights
                : Config.FoundationEnemyWaveWeights
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
        int spawnCount = 0; // 今回その勢力として湧かせる人数

        // 先に対象 Spectator を確定させておく
        var specs = Player.List
            .Where(p =>
                p.Role == RoleTypeId.Spectator &&
                p.GetCustomRole() == CRoleTypeId.None)
            .ToList();

        spawnCount = specs.Count;

        // HD 用コールサイン生成
        if (Config.NatoCallsign)
        {
            var nato = GenerateNatoCallsignFull();
            cassieCallsign  = nato.cassie;
            displayCallsign = nato.display;
        }

        // Spectator から対象ロールを割り当て（候補は specs に固定）
        AssignTeamRoles(
            spawnType,
            playerFilter: p => specs.Contains(p),
            fixedCount: null);

        // Faction 判定
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

        // spawnCount を今回 wave の人数として渡す
        OnSpawning(spawnType, isMiniWave, faction, cassieCallsign, displayCallsign, spawnCount);

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
        if (!Config.RoleTables.TryGetValue(spawnType, out var table) || table.Count == 0)
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

        // cassie: NATO_A / display: ALPHA-05
        return (natoForce, $"{natoForceL}-{natoForceNum:00}");
    }
}

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
