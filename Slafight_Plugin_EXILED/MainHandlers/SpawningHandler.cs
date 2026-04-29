using System;
using System.Linq;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

using Slafight_Plugin_EXILED.API.Interface;

namespace Slafight_Plugin_EXILED.MainHandlers;

public class SpawningHandler : IBootstrapHandler
{
    public static SpawningHandler Instance { get; private set; }
    public static void Register() { Instance = new(); }
    public static void Unregister() { Instance = null; }

    public SpawningHandler()
    {
        SpawnSystem.Spawning += OnSpawning;
        SpawnSystem.Spawned += OnSpawned;
    }

    ~SpawningHandler()
    {
        SpawnSystem.Spawning -= OnSpawning;
        SpawnSystem.Spawned -= OnSpawned;
    }
    private void OnSpawning(object sender, SpawnSystem.CustomSpawningEventArgs ev)
    {
        // SpawnType が既に確定している or Default 以外のコンテキストは触らない
        if (ev.SpawnType.HasValue)
            return;
        if (ev.NowContext.Name != "Default")
            return;

        // 3005 / FifthistPriest が場にいるかどうか
        var hasGodBlessedRolePlayer = Player.List.Any(p =>
            p.GetCustomRole() == CRoleTypeId.Scp3005 ||
            p.GetCustomRole() == CRoleTypeId.FifthistPriest);

        var w = ev.ContextOverride;

        // ===== 財団敵 (Chaos/Fifthist) 側の調整 =====
        if (ev.Faction == Faction.FoundationEnemy)
        {
            if (hasGodBlessedRolePlayer)
            {
                if (ev.IsMiniWave)
                {
                    if (w.ContainsKey(SpawnTypeId.GOI_FifthistBackup))
                        w[SpawnTypeId.GOI_FifthistBackup] = 40;
                    if (w.ContainsKey(SpawnTypeId.GOI_ChaosBackup))
                        w[SpawnTypeId.GOI_ChaosBackup] = 60;
                }
                else
                {
                    if (w.ContainsKey(SpawnTypeId.GOI_FifthistNormal))
                        w[SpawnTypeId.GOI_FifthistNormal] = 40;
                    if (w.ContainsKey(SpawnTypeId.GOI_ChaosNormal))
                        w[SpawnTypeId.GOI_ChaosNormal] = 60;
                }
            }
            else
            {
                if (ev.IsMiniWave)
                {
                    if (w.ContainsKey(SpawnTypeId.GOI_FifthistBackup))
                        w[SpawnTypeId.GOI_FifthistBackup] = 0;
                    if (w.ContainsKey(SpawnTypeId.GOI_ChaosBackup))
                        w[SpawnTypeId.GOI_ChaosBackup] = 100;
                }
                else
                {
                    if (w.ContainsKey(SpawnTypeId.GOI_FifthistNormal))
                        w[SpawnTypeId.GOI_FifthistNormal] = 0;
                    if (w.ContainsKey(SpawnTypeId.GOI_ChaosNormal))
                        w[SpawnTypeId.GOI_ChaosNormal] = 100;
                }
            }

            return;
        }

        // ===== 財団味方 (MTF) 側の調整 =====
        if (ev.Faction == Faction.FoundationStaff && hasGodBlessedRolePlayer)
        {
            if (ev.IsMiniWave)
            {
                if (w.ContainsKey(SpawnTypeId.MTF_NtfBackup))
                    w[SpawnTypeId.MTF_NtfBackup] = 40;
                if (w.ContainsKey(SpawnTypeId.MTF_HDBackup))
                    w[SpawnTypeId.MTF_HDBackup] = 20;

                if (w.ContainsKey(SpawnTypeId.MTF_SneBackup))
                    w[SpawnTypeId.MTF_SneBackup] = 40;
            }
            else
            {
                if (w.ContainsKey(SpawnTypeId.MTF_NtfNormal))
                    w[SpawnTypeId.MTF_NtfNormal] = 40;
                if (w.ContainsKey(SpawnTypeId.MTF_HDNormal))
                    w[SpawnTypeId.MTF_HDNormal] = 20;

                if (w.ContainsKey(SpawnTypeId.MTF_SneNormal))
                    w[SpawnTypeId.MTF_SneNormal] = 40;
            }
        }
    }

    private void OnSpawned(object sender, SpawnSystem.CustomSpawningEventArgs ev)
    {
        // Spawned は「実際に湧いた後」だけ飛んでくる前提なので IsAllowed チェックは不要
        if (!ev.SpawnType.HasValue)
            return;

        var spawnType = ev.SpawnType.Value;
        int spawnCount = ev.SpawnCount;

        Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio =
            EventHandler.CreateAndPlayAudio;

        switch (spawnType)
        {
            // Mobile Task Forces
            case SpawnTypeId.MTF_NtfNormal:
                CreateAndPlayAudio("_w_ntf.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999, 0);
                CassieHelper.AnnounceNtfArrival();
                break;
            case SpawnTypeId.MTF_NtfBackup:
                CreateAndPlayAudio("_w_ntf.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999, 0);
                CassieHelper.AnnounceNtfBackup();
                break;

            case SpawnTypeId.MTF_HDNormal:
                CreateAndPlayAudio("_w_hd.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999, 0);
                CassieHelper.AnnounceHdArrival();
                break;
            case SpawnTypeId.MTF_HDBackup:
                CreateAndPlayAudio("_w_hd.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999, 0);
                CassieHelper.AnnounceHdBackup();
                break;
            
            case SpawnTypeId.MTF_LastOperationNormal:
                CreateAndPlayAudio("_w_lo.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999, 0);
                CassieHelper.AnnounceLastOperationArrival();
                break;
            case SpawnTypeId.MTF_LastOperationBackup:
                CreateAndPlayAudio("_w_lo.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999, 0);
                CassieHelper.AnnounceLastOperationBackup();
                break;
            
            case SpawnTypeId.MTF_SneNormal:
                CreateAndPlayAudio("_w_sne.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999, 0);
                CassieHelper.AnnounceSneArrival();
                break;
            case SpawnTypeId.MTF_SneBackup:
                CreateAndPlayAudio("_w_sne.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999, 0);
                CassieHelper.AnnounceSneBackup();
                break;

            // ==== Groups of Interests ====
            case SpawnTypeId.GOI_ChaosNormal:
            case SpawnTypeId.GOI_ChaosBackup:
                CreateAndPlayAudio("_w_chaos.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999, 0);
                CassieHelper.AnnounceChaos(spawnCount);
                break;

            case SpawnTypeId.GOI_FifthistNormal:
            case SpawnTypeId.GOI_FifthistBackup:
                CreateAndPlayAudio("_w_fifthists.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999, 0);
                CassieHelper.AnnounceFifthist(spawnCount);
                break;

            case SpawnTypeId.GOI_GoCNormal:
            case SpawnTypeId.GOI_GoCBackup:
                CreateAndPlayAudio("_w_ungoc.ogg", "WaveTheme", Vector3.zero, true, null, false, 999999999, 0);
                CassieHelper.AnnounceGoCEnter(spawnCount);
                break;
        }
    }
}
