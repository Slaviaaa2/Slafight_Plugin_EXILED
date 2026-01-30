using System;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED;

public class SpawningHandler
{
    public SpawningHandler()
    {
        SpawnSystem.Spawning += OnSpawning;
    }

    ~SpawningHandler()
    {
        SpawnSystem.Spawning -= OnSpawning;
    }

    private void OnSpawning(object sender, SpawnSystem.SpawningEventArgs ev)
    {
        Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
        switch (ev.SpawnType)
        {
            case SpawnTypeId.MTF_NtfNormal:
                CreateAndPlayAudio("_w_ntf.ogg","WaveTheme",Vector3.zero,true,null,false,999999999,0);
                CassieHelper.AnnounceNtfArrival();
                break;
            case SpawnTypeId.MTF_NtfBackup:
                CreateAndPlayAudio("_w_ntf.ogg","WaveTheme",Vector3.zero,true,null,false,999999999,0);
                CassieHelper.AnnounceNtfBackup();
                break;
            case SpawnTypeId.MTF_HDNormal:
                CreateAndPlayAudio("_w_hd.ogg","WaveTheme",Vector3.zero,true,null,false,999999999,0);
                CassieHelper.AnnounceHdArrival();
                break;
            case SpawnTypeId.MTF_HDBackup:
                CreateAndPlayAudio("_w_hd.ogg","WaveTheme",Vector3.zero,true,null,false,999999999,0);
                CassieHelper.AnnounceHdBackup();
                break;

            case SpawnTypeId.GOI_ChaosNormal:
            case SpawnTypeId.GOI_ChaosBackup:
                CreateAndPlayAudio("_w_chaos.ogg","WaveTheme",Vector3.zero,true,null,false,999999999,0);
                CassieHelper.AnnounceChaos(ev.SpawnCount);
                break;
            case SpawnTypeId.GOI_FifthistNormal:
            case SpawnTypeId.GOI_FifthistBackup:
                CreateAndPlayAudio("_w_fifthists.ogg","WaveTheme",Vector3.zero,true,null,false,999999999,0);
                CassieHelper.AnnounceFifthist(ev.SpawnCount);
                break;
            case SpawnTypeId.GOI_GoCNormal:
            case SpawnTypeId.GOI_GoCBackup:    
                CreateAndPlayAudio("_w_ungoc.ogg","WaveTheme",Vector3.zero,true,null,false,999999999,0);
                CassieHelper.AnnounceGoCEnter(ev.SpawnCount);
                break;
            
            case SpawnTypeId.MTF_LastOperationNormal:
                CreateAndPlayAudio("_w_lo.ogg","WaveTheme",Vector3.zero,true,null,false,999999999,0);
                CassieHelper.AnnounceLastOperationArrival();
                break;
            case SpawnTypeId.MTF_LastOperationBackup:
                CreateAndPlayAudio("_w_lo.ogg","WaveTheme",Vector3.zero,true,null,false,999999999,0);
                CassieHelper.AnnounceLastOperationBackup();
                break;
        }
    }
}