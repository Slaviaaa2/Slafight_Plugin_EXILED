using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using LabApi.Events.Arguments.ServerEvents;
using MEC;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using Slafight_Plugin_EXILED.API.Enums;
using Subtitles;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED;

public class SpawnSystem
{
    public SpawnSystem()
    {
        Exiled.Events.Handlers.Server.RespawningTeam += SpawnHandler;
    }

    ~SpawnSystem()
    {
        Exiled.Events.Handlers.Server.RespawningTeam -= SpawnHandler;
    }

    private bool isDefaultWave = true;
    
    public void SpawnHandler(RespawningTeamEventArgs ev)
    {
        if (isDefaultWave)
        {
            ev.IsAllowed = false;
            if (ev.NextKnownTeam == Faction.FoundationStaff)
            {
                int i=0;
                foreach (Player player in Player.List)
                {
                    if (player.Role.Team == Team.SCPs)
                    {
                        i++;
                    }
                }

                if (Player.Count >= 6 || i >= 3)
                {
                    if (Random.Range(0,3) == 0)
                    {
                        if (!ev.Wave.IsMiniWave)
                        {
                            SummonForces(SpawnTypeId.MTF_HDNormal);
                        }
                        else
                        {
                            SummonForces(SpawnTypeId.MTF_HDBackup);
                        }
                    }
                    else
                    {
                        if (!ev.Wave.IsMiniWave)
                        {
                            SummonForces(SpawnTypeId.MTF_NtfNormal);
                        }
                        else
                        {
                            SummonForces(SpawnTypeId.MTF_NtfBackup);
                        }
                    }
                }
                else
                {
                    if (!ev.Wave.IsMiniWave)
                    {
                        SummonForces(SpawnTypeId.MTF_NtfNormal);
                    }
                    else
                    {
                        SummonForces(SpawnTypeId.MTF_NtfBackup);
                    }
                }
            }
            else if (ev.NextKnownTeam == Faction.FoundationEnemy)
            {
                int i=0;
                int s3005 = 0;
                foreach (Player player in Player.List)
                {
                    if (player.Role.Team == Team.SCPs)
                    {
                        i++;
                    }

                    if (player.UniqueRole == "SCP-3005")
                    {
                        s3005++;
                    }
                }

                if (s3005 != 0)
                {
                    if (Random.Range(0, 5) == 4)
                    {
                        if (!ev.Wave.IsMiniWave)
                        {
                            SummonForces(SpawnTypeId.GOI_FifthistNormal);
                        }
                        else
                        {
                            SummonForces(SpawnTypeId.GOI_FifthistBackup);
                        }
                    }
                    else
                    {
                        if (!ev.Wave.IsMiniWave)
                        {
                            SummonForces(SpawnTypeId.GOI_ChaosNormal);
                        }
                        else
                        {
                            SummonForces(SpawnTypeId.GOI_ChaosBackup);
                        }
                    }
                }
                else
                {
                    if (!ev.Wave.IsMiniWave)
                    {
                        SummonForces(SpawnTypeId.GOI_ChaosNormal);
                    }
                    else
                    {
                        SummonForces(SpawnTypeId.GOI_ChaosBackup);
                    }
                }
            }
        }
    }

    public void SummonForces(SpawnTypeId spawnType)
    {
        isDefaultWave = false;
        int RandomSel;
        string designation = String.Empty;
        string natoForce = String.Empty;
        List<string> NatoForce = new List<string>()
        {
            "NATO_A", 
            "NATO_B", 
            "NATO_C", 
            "NATO_D", 
            "NATO_E", 
            "NATO_F", 
            "NATO_G", 
            "NATO_H", 
            "NATO_I", 
            "NATO_J", 
            "NATO_K", 
            "NATO_L", 
            "NATO_M", 
            "NATO_N", 
            "NATO_O", 
            "NATO_P", 
            "NATO_Q", 
            "NATO_R", 
            "NATO_S", 
            "NATO_T", 
            "NATO_U", 
            "NATO_V", 
            "NATO_W", 
            "NATO_X",
            "NATO_Y",
            "NATO_Z"
        };
        natoForce = NatoForce.RandomItem();
        int natoForceNum = Random.Range(1,20);
        natoForceNum.ToString("00");
        // SIDE - FOUNDATION FORCES
        if (spawnType == SpawnTypeId.MTF_NtfNormal)
        {
            designation = "Nine-tailed Fox";
            Respawn.ForceWave(SpawnableFaction.NtfWave);
            bool captainSpawned = false;
            Timing.CallDelayed(20f, () =>
            {
                foreach (Player player in Player.List)
                {
                    if (player.Role == RoleTypeId.Spectator)
                    {
                        if (!captainSpawned)
                        {
                            player.Role.Set(RoleTypeId.NtfCaptain);
                            captainSpawned = true;
                            continue;
                        }

                        RandomSel = Random.Range(0, 5);
                        if (RandomSel==0)
                        {
                            List<string> subroles = new List<string>()
                            {
                                "NtfAide"
                            };
                            if (subroles.Count <= 0) break;
                            RandomSel = Random.Range(0, subroles.Count);
                            // Subroles: Aide
                            if (RandomSel==0)
                            {
                                Slafight_Plugin_EXILED.Plugin.Singleton.CR_NtfAide.SpawnRole(player);
                            }
                        }
                    }
                }
            });
        }
        else if (spawnType == SpawnTypeId.MTF_NtfBackup)
        {
            designation = "Nine-tailed Fox";
            Respawn.ForceWave(SpawnableFaction.NtfMiniWave);
            bool captainSpawned = false;
            Timing.CallDelayed(20f, () =>
            {
                foreach (Player player in Player.List)
                {
                    if (player.Role == RoleTypeId.Spectator)
                    {
                        if (!captainSpawned)
                        {
                            player.Role.Set(RoleTypeId.NtfCaptain);
                            captainSpawned = true;
                            continue;
                        }

                        RandomSel = Random.Range(0, 5);
                        if (RandomSel==0)
                        {
                            List<string> subroles = new List<string>()
                            {
                                //"NtfAide"
                            };
                            if (subroles.Count <= 0) break;
                            RandomSel = Random.Range(0, subroles.Count);
                            // Subroles: Aide
                            if (RandomSel==0)
                            {
                                Slafight_Plugin_EXILED.Plugin.Singleton.CR_NtfAide.SpawnRole(player);
                            }
                        }
                    }
                }
            });
        }
        else if (spawnType == SpawnTypeId.MTF_HDNormal)
        {
            designation = "Hammer Down";
            int i=0;
            int ii=0;
            foreach (Player player in Player.List)
            {
                if (player.Role == RoleTypeId.Spectator)
                {
                    if (ii==0)
                    {
                        Slafight_Plugin_EXILED.Plugin.Singleton.CR_HdCommander.SpawnRole(player);
                        i++;
                        ii++;
                        continue;
                    }
                    Slafight_Plugin_EXILED.Plugin.Singleton.CR_HdInfantry.SpawnRole(player);
                    i++;
                }
                //if (i >= Math.Truncate(Player.List.Count/4f)) break;
            }
            Cassie.MessageTranslated($"Mobile Task Force Unit Nu 7 designated {natoForce} {natoForceNum} has entered the facility . This Forces Work Ninetailedfox Task and operated by O5 Command . Were Stop Containment Breach",
                $"<b><color=#353535>機動部隊Nu-7 \"下される鉄槌 - ハンマーダウン\"-{natoForce}-{natoForceNum}</color></b>が施設に到着しました。" +
                $"<split>本部隊は<color=#5bc5ff>Epsilon-11 \"九尾狐\"</color>の任務の代替としてO5評議会に招集された物です。<split><b>必ず収容違反を食い止めます</b>",false,true);
        }
        else if (spawnType == SpawnTypeId.MTF_HDBackup)
        {
            designation = "Hammer Down";
            int i=0;
            int ii=0;
            foreach (Player player in Player.List)
            {
                if (player.Role == RoleTypeId.Spectator)
                {
                    if (ii==0)
                    {
                        Slafight_Plugin_EXILED.Plugin.Singleton.CR_HdCommander.SpawnRole(player);
                        i++;
                        ii++;
                        continue;
                    }
                    Slafight_Plugin_EXILED.Plugin.Singleton.CR_HdInfantry.SpawnRole(player);
                    i++;
                }
                if (i >= Math.Truncate(Player.List.Count/2f)) break;
            }
            Cassie.MessageTranslated($"Her man down Backup unit has entered the facility .",
                $"<b><color=#353535>下される鉄槌 - ハンマーダウンの予備部隊</color></b>が施設に到着しました。",false,true);
        }
        // SIDE - CHAOS INSURGENCY
        else if (spawnType == SpawnTypeId.GOI_ChaosNormal)
        {
            designation = "Chaos Insurgents";
            Respawn.ForceWave(SpawnableFaction.ChaosWave);
        }
        else if (spawnType == SpawnTypeId.GOI_ChaosBackup)
        {
            designation = "Chaos Insurgents";
            Respawn.ForceWave(SpawnableFaction.ChaosMiniWave);
        }
        // SIDE - FIFTHIST
        else if (spawnType == SpawnTypeId.GOI_FifthistNormal)
        {
            designation = "Fifthist Forces";
            int i=0;
            foreach (Player player in Player.List)
            {
                if (player.Role == RoleTypeId.Spectator)
                {
                    CustomRole.Get("FIFTHIST")?.AddRole(player);
                    i++;
                }
                if (i >= Math.Truncate(Player.List.Count/4f)) break;
            }
            Cassie.MessageTranslated($"Attention All personnel . Detected {i} pitch_1.05 5 5 5 pitch_1 Forces in Gate B .",$"全職員に通達。Gate Bに{i}人の第五主義者が検出されました。",false,true);
        }
        else if (spawnType == SpawnTypeId.GOI_FifthistBackup)
        {
            designation = "Fifthist Forces";
            int i=0;
            foreach (Player player in Player.List)
            {
                if (player.Role == RoleTypeId.Spectator)
                {
                    CustomRole.Get("FIFTHIST")?.AddRole(player);
                    i++;
                }
                if (i >= Math.Truncate(Player.List.Count/6f)) break;
            }
            Cassie.MessageTranslated($"Attention All personnel . Detected {i} pitch_1.05 5 5 5 pitch_1 Forces in Gate B .",$"全職員に通達。Gate Bに{i}人の第五主義者が検出されました。",false,true);
        }

        Timing.CallDelayed(0.02f, () =>
        {
            isDefaultWave = true;
        });
    }
}