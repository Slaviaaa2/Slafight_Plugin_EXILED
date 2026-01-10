using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using Subtitles;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED;

/// <summary>
/// リスポーン波を完全制御する統合スポーンシステム。
/// ・どの勢力を呼ぶか: WaveWeights
/// ・何人対象にするか: SpawnRatios
/// ・どのロールになるか: RoleTables (weight + guaranteed)
/// 全陣営 (NTF / HammerDown / Chaos / Fifthist / 新勢力) を1つの仕組みにまとめる。
/// </summary>
public class SpawnSystem
{
    // =====================
    //  Config 定義
    // =====================
    public class SpawnConfig
    {
        /// <summary>
        /// FoundationStaff (MTF側) のウェーブ時、
        /// どのSpawnTypeをどの重みで出すかを定義するテーブル。
        /// 重みは相対値（合計に対する比率）で、0以下は無視。
        /// </summary>
        public Dictionary<SpawnTypeId, int> FoundationStaffWaveWeights { get; set; } = new()
        {
            { SpawnTypeId.MTF_NtfNormal, 80 },
            { SpawnTypeId.MTF_HDNormal,  20 },
        };

        /// <summary>
        /// FoundationEnemy (Chaos/Fifthist) 通常ウェーブ用の重みテーブル。
        /// SCP-3005の有無でFifthist側の重みを書き換えて使う。
        /// </summary>
        public Dictionary<SpawnTypeId, int> FoundationEnemyWaveWeights { get; set; } = new()
        {
            { SpawnTypeId.GOI_ChaosNormal,    100 },
            { SpawnTypeId.GOI_FifthistNormal, 0   }, // 3005が居る時だけ有効化
        };

        /// <summary>
        /// MiniWave用。必要に応じて別の比率を定義。
        /// </summary>
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

        /// <summary>
        /// 各SpawnTypeごとの「何人対象にするか」の比率。
        /// ratio = 1.0 → 対象プレイヤー全員。
        /// ratio = 0.5 → 対象プレイヤーの半分。
        /// </summary>
        public Dictionary<SpawnTypeId, float> SpawnRatios { get; set; } = new()
        {
            { SpawnTypeId.MTF_HDNormal,       1.0f },
            { SpawnTypeId.MTF_HDBackup,       0.5f },
            { SpawnTypeId.GOI_FifthistNormal, 0.25f },
            { SpawnTypeId.GOI_FifthistBackup, 1f / 6f },
        };

        /// <summary>
        /// 各SpawnTypeごとの「ロールテーブル」。
        /// key: ロール, value: (weight, guaranteed)
        /// guaranteed = true のロールは最低1人は必ず湧く（人数が足りる限り）。
        /// weight は重み付き抽選用。合計1.0でなくてもOK。
        /// </summary>
        public Dictionary<SpawnTypeId, Dictionary<CRoleTypeId, (float weight, bool guaranteed)>> RoleTables
            { get; set; } = new()
        {
            // ----- Hammer Down -----
            {
                SpawnTypeId.MTF_HDNormal, new()
                {
                    { CRoleTypeId.HdMarshal,  (0.125f, true)  }, // Marshal最低1人
                    { CRoleTypeId.HdCommander, (0.25f,  false) },
                    { CRoleTypeId.HdInfantry,   (0.625f, false) },
                }
            },
            {
                SpawnTypeId.MTF_HDBackup, new()
                {
                    { CRoleTypeId.HdCommander, (0.25f,  true) },
                    { CRoleTypeId.HdInfantry,   (0.75f, false) },
                }
            },

            // ----- Fifthist -----
            {
                SpawnTypeId.GOI_FifthistNormal, new()
                {
                    { CRoleTypeId.FifthistPriest,  (0.3f, true)  }, // Priest最低1人
                    { CRoleTypeId.FifthistRescure, (0.5f, false) },
                    { CRoleTypeId.FifthistConvert, (0.2f, false) }
                }
            },
            {
                SpawnTypeId.GOI_FifthistBackup, new()
                {
                    { CRoleTypeId.FifthistRescure, (0.7f, false) },
                    { CRoleTypeId.FifthistConvert, (0.3f, false) }
                }
            },

            // ----- Chaos (サブロール用) -----
            {
                SpawnTypeId.GOI_ChaosNormal, new()
                {
                    { CRoleTypeId.ChaosCommando, (0.2f, false) },
                    { CRoleTypeId.ChaosSignal,   (0.15f, false) }
                }
            },
            {
                SpawnTypeId.GOI_ChaosBackup, new()
                {
                    { CRoleTypeId.ChaosSignal, (0.15f, false) },
                }
            },

            // ----- NTF (サブロール用) -----
            {
                SpawnTypeId.MTF_NtfNormal, new()
                {
                    { CRoleTypeId.NtfGeneral,    (0.15f, false) },
                    { CRoleTypeId.NtfLieutenant, (0.2f, false) },
                }
            },
            {
                SpawnTypeId.MTF_NtfBackup, new()
                {
                    { CRoleTypeId.NtfLieutenant, (0.2f, false) },
                }
            },

            // 新勢力を追加するときはここに:
            // { SpawnTypeId.GOI_NewForceNormal, new() { { CRoleTypeId.NewForceLeader, (0.1f, true) }, ... } }
        };

        // 高脅威判定用
        public int ScpThresholdHigh    { get; set; } = 3;
        public int PlayerThresholdHigh { get; set; } = 6;

        // SCP-3005が居る時のFifthist出現確率 (Weights方式と併用するなら片方に寄せる)
        public int S3005FifthistChance { get; set; } = 5;

        public bool HdUseNatoCallsign { get; set; } = true;
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
    //  RespawningTeam ハンドラ
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

        if (ev.NextKnownTeam == Faction.FoundationStaff)
        {
            HandleFoundationStaffWave(ev);
        }
        else if (ev.NextKnownTeam == Faction.FoundationEnemy)
        {
            HandleFoundationEnemyWave(ev);
        }
    }

    private void HandleFoundationStaffWave(RespawningTeamEventArgs ev)
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
            // 高脅威時はHD側の重みを増やす例
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

        var picked = PickWeightedSpawnType(weights);
        if (picked != null)
            SummonForces(picked.Value, ev.Wave.IsMiniWave);
    }

    private void HandleFoundationEnemyWave(RespawningTeamEventArgs ev)
    {
        bool has3005 = Player.List.Any(p => p.GetCustomRole() == CRoleTypeId.Scp3005);

        var weights = new Dictionary<SpawnTypeId, int>(
            ev.Wave.IsMiniWave
                ? Config.FoundationEnemyMiniWaveWeights
                : Config.FoundationEnemyWaveWeights
        );

        if (has3005)
        {
            // SCP-3005が居るときはFifthist有効化
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
            // 3005なし → Chaosのみ
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

        var picked = PickWeightedSpawnType(weights);
        if (picked != null)
            SummonForces(picked.Value, ev.Wave.IsMiniWave);
    }

    // =====================
    //  SummonForces & 共通割り当て
    // =====================

    public void SummonForces(SpawnTypeId spawnType, bool isMiniWave)
    {
        isDefaultWave = false;
        string callsign = Config.HdUseNatoCallsign ? GenerateNatoCallsign() : string.Empty;

        switch (spawnType)
        {
            // ----- NTF：バニラwave + サブロール付与 -----
            case SpawnTypeId.MTF_NtfNormal:
                Respawn.ForceWave(SpawnableFaction.NtfWave);
                Timing.CallDelayed(8f, () =>
                {
                    AssignTeamRoles(
                        SpawnTypeId.MTF_NtfNormal,
                        playerFilter: p => p.Role == RoleTypeId.Spectator
                    );
                });
                break;

            case SpawnTypeId.MTF_NtfBackup:
                Respawn.ForceWave(SpawnableFaction.NtfMiniWave);
                Timing.CallDelayed(8f, () =>
                {
                    AssignTeamRoles(
                        SpawnTypeId.MTF_NtfBackup,
                        playerFilter: p => p.Role == RoleTypeId.Spectator
                    );
                });
                break;

            // ----- Hammer Down：Spectatorから完全自前スポーン -----
            case SpawnTypeId.MTF_HDNormal:
            case SpawnTypeId.MTF_HDBackup:
                AssignTeamRoles(
                    spawnType,
                    playerFilter: p => p.Role == RoleTypeId.Spectator
                );
                if (spawnType == SpawnTypeId.MTF_HDNormal)
                    CassieHelper.AnnounceHdArrival(callsign);
                else
                    CassieHelper.AnnounceHdBackup();
                break;

            // ----- Chaos：バニラwave + サブロール付与 -----
            case SpawnTypeId.GOI_ChaosNormal:
                Respawn.ForceWave(SpawnableFaction.ChaosWave);
                Timing.CallDelayed(8f, () =>
                {
                    AssignTeamRoles(
                        SpawnTypeId.GOI_ChaosNormal,
                        playerFilter: p => p.Role == RoleTypeId.Spectator
                    );
                });
                break;

            case SpawnTypeId.GOI_ChaosBackup:
                Respawn.ForceWave(SpawnableFaction.ChaosMiniWave);
                Timing.CallDelayed(8f, () =>
                {
                    AssignTeamRoles(
                        SpawnTypeId.GOI_ChaosBackup,
                        playerFilter: p => p.Role == RoleTypeId.Spectator
                    );
                });
                break;

            // ----- Fifthist：Spectatorから完全自前スポーン -----
            case SpawnTypeId.GOI_FifthistNormal:
            case SpawnTypeId.GOI_FifthistBackup:
                AssignTeamRoles(
                    spawnType,
                    playerFilter: p => p.Role == RoleTypeId.Spectator
                );
                // 何人Fifthistになったか数えてアナウンス
                int count = Player.List.Count(p =>
                    p.GetCustomRole() is CRoleTypeId.FifthistPriest or CRoleTypeId.FifthistRescure);
                CassieHelper.AnnounceFifthist(count);
                break;

            // 新勢力を追加する場合も:
            // case SpawnTypeId.GOI_NewForceNormal:
            //     AssignTeamRoles(SpawnTypeId.GOI_NewForceNormal, p => p.Role == RoleTypeId.Spectator);
            //     CassieHelper.AnnounceNewForce(...);
            //     break;
        }

        Timing.CallDelayed(0.02f, () => isDefaultWave = true);
    }

    /// <summary>
    /// 汎用ロール割り当て:
    /// spawnTypeに対応するRoleTableを取得し、
    /// playerFilterで絞り込んだプレイヤーから
    /// ・guaranteed=trueロールを最低1人ずつ確湧き
    /// ・残り枠をweightによる重み付き抽選で埋める
    /// </summary>
    private void AssignTeamRoles(
        SpawnTypeId spawnType,
        Func<Player, bool> playerFilter,
        int? fixedCount = null
    )
    {
        if (!Config.RoleTables.TryGetValue(spawnType, out var table) || table.Count == 0)
            return;

        var candidates = Player.List
            .Where(playerFilter)
            .Shuffle()
            .ToList();

        if (candidates.Count == 0)
            return;

        // 対象人数決定（fixedCount優先、なければSpawnRatiosから計算）
        int targetCount;
        if (fixedCount.HasValue)
        {
            targetCount = Math.Min(fixedCount.Value, candidates.Count);
        }
        else
        {
            float ratio = 1.0f;
            if (Config.SpawnRatios.TryGetValue(spawnType, out var r))
                ratio = r;

            targetCount = (int)Math.Truncate(candidates.Count * ratio);
            if (targetCount <= 0)
                targetCount = candidates.Count;
        }

        // guaranteedロールを最低1人ずつ付与
        var guaranteedRoles = table
            .Where(kvp => kvp.Value.guaranteed)
            .Select(kvp => kvp.Key)
            .ToList();

        int index = 0;

        foreach (var role in guaranteedRoles)
        {
            if (index >= targetCount)
                break;

            var player = candidates[index];
            player.SetRole(role, RoleSpawnFlags.All);
            index++;
        }

        if (index >= targetCount)
            return;

        // 残り枠に weight 抽選
        var weightTable = table
            .Where(kvp => kvp.Value.weight > 0f)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.weight);

        for (; index < targetCount; index++)
        {
            var player = candidates[index];
            var picked = PickWeightedRole(weightTable);
            if (picked != null)
                player.SetRole(picked.Value, RoleSpawnFlags.All);
        }
    }

    // =====================
    //  重み付き抽選系
    // =====================

    private SpawnTypeId? PickWeightedSpawnType(Dictionary<SpawnTypeId, int> weights)
    {
        var valid = weights.Where(kvp => kvp.Value > 0).ToList();
        if (!valid.Any())
            return null;

        int total = valid.Sum(kvp => kvp.Value);
        int roll  = Random.Range(0, total); // int版はmax排他 [web:42]

        int cum = 0;
        foreach (var kvp in valid)
        {
            cum += kvp.Value;
            if (roll < cum)
                return kvp.Key;
        }

        return valid.First().Key;
    }

    private CRoleTypeId? PickWeightedRole(Dictionary<CRoleTypeId, float> chances)
    {
        var valid = chances.Where(kvp => kvp.Value > 0f).ToList();
        if (!valid.Any())
            return null;

        float total = valid.Sum(kvp => kvp.Value);
        if (total <= 0f)
            return null;

        float rand = Random.Range(0f, total); // float版はmax含む [web:42]
        float cum  = 0f;

        foreach (var kvp in valid)
        {
            cum += kvp.Value;
            if (rand <= cum)
                return kvp.Key;
        }

        return valid.First().Key;
    }

    // =====================
    //  NATOコールサイン & Cassie
    // =====================

    private string GenerateNatoCallsign()
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
        return $"{natoForceL}-{natoForceNum:00}";
    }
}

public static class CassieHelper
{
    public static void AnnounceHdArrival(string callsign)
    {
        Exiled.API.Features.Cassie.MessageTranslated(
            $"MtfUnit Nu 7 Designated {callsign} HasEntered AllRemaining This Forces Work Epsilon 11 Task and operated by O5 Command . for Big Containment Breachs .",
            $"<b><color=#353535>機動部隊Nu-7 \"下される鉄槌 - ハンマーダウン\"-{callsign}</color></b>が施設に到着しました。残存する全職員は、機動部隊が目的地に到着するまで、標準避難プロトコルに従って行動してください。" +
            $"<split>本部隊は<color=#5bc5ff>Epsilon-11 \"九尾狐\"</color>の任務の代替として大規模な収容違反の対応の為O5評議会に招集されました。",
            true);
    }

    public static void AnnounceHdBackup()
    {
        Exiled.API.Features.Cassie.MessageTranslated(
            "Her man down Backup unit has entered the facility .",
            "<b><color=#353535>下される鉄槌 - ハンマーダウンの予備部隊</color></b>が施設に到着しました。",
            true);
    }

    public static void AnnounceFifthist(int count)
    {
        Exiled.API.Features.Cassie.MessageTranslated(
            $"Attention All personnel . Detected {count} $pitch_1.05 5 5 5 $pitch_1 Forces in Gate B .",
            $"全職員に通達。Gate Bに{count}人の第五主義者が検出されました。",
            true);
    }
}

public static class EnumerableExtensions
{
    /// <summary>
    /// IEnumerableをFisher-Yates方式でシャッフルする簡易拡張。
    /// Player.Listなどに対して .Shuffle().ToList() でランダム順序を取得できる。
    /// </summary>
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