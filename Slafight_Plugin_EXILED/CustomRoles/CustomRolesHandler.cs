using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.CustomStats;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using InventorySystem;
using InventorySystem.Items.Firearms.Modules.Scp127;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerStatsSystem;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using DamageHandlerBase = Exiled.API.Features.DamageHandlers.DamageHandlerBase;
using Light = Exiled.API.Features.Toys.Light;
using Object = UnityEngine.Object;

namespace Slafight_Plugin_EXILED.CustomRoles;

public class CustomRolesHandler
{
    public CustomRolesHandler()
    {
        Exiled.Events.Handlers.Player.Dying += DiedCassie;
        Exiled.Events.Handlers.Player.ChangingRole += CustomRoleRemover;
        Exiled.Events.Handlers.Player.SpawningRagdoll += CencellRagdoll;
        Exiled.Events.Handlers.Player.Hurting += CustomFriendlyFire_hurt;
        Exiled.Events.Handlers.Server.RoundStarted += RoundCoroutine;
        //Exiled.Events.Handlers.Player.ChangedItem += ForceHume;

        Exiled.Events.Handlers.Server.EndingRound += CancelEnd;
    }
    ~CustomRolesHandler()
    {
        Exiled.Events.Handlers.Player.Dying -= DiedCassie;
        Exiled.Events.Handlers.Player.ChangingRole -= CustomRoleRemover;
        Exiled.Events.Handlers.Player.SpawningRagdoll -= CencellRagdoll;
        Exiled.Events.Handlers.Player.Hurting -= CustomFriendlyFire_hurt;
        Exiled.Events.Handlers.Server.RoundStarted -= RoundCoroutine;
        //Exiled.Events.Handlers.Player.ChangedItem -= ForceHume;

        Exiled.Events.Handlers.Server.EndingRound -= CancelEnd;
    }

    public void RoundCoroutine()
    {
        Timing.CallDelayed(10f, () =>
        {
            Timing.RunCoroutine(FifthistCoroutine());
            if (Plugin.Singleton.Config.Season == 2)
            {
                Timing.RunCoroutine(SnowmanCoroutine());
            }
        });
    }

    private IEnumerator<float> FifthistCoroutine()
    {
        for (;;)
        {
            if (Round.IsLobby)
            {
                yield break;
            }
            int i = 0;
            foreach (Player player in Player.List)
            {
                if (player == null || !player.IsAlive) continue;
                if (player.UniqueRole != "FIFTHIST" && player.UniqueRole != "SCP-3005" && player.UniqueRole != "F_Priest")
                {
                    i++;
                }
            }
            int ii = 0;
            foreach (Player player in Player.List)
            {
                if (player == null || !player.IsAlive) continue;
                if (player.UniqueRole == "FIFTHIST" || player.UniqueRole == "SCP-3005" || player.UniqueRole == "F_Priest")
                {
                    ii++;
                }
            }
            if (i==0 && ii!=0)
            {
                EndRound(Team.SCPs,"FIFTHIST_WIN");
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }
    private IEnumerator<float> SnowmanCoroutine()
    {
        for (;;)
        {
            if (Round.IsLobby)
            {
                yield break;
            }
            int i = 0;
            foreach (Player player in Player.List)
            {
                if (player == null || !player.IsAlive) continue;
                if (player.UniqueRole != "SnowWarrier")
                {
                    i++;
                }
            }
            int ii = 0;
            foreach (Player player in Player.List)
            {
                if (player == null || !player.IsAlive) continue;
                if (player.UniqueRole == "SnowWarrier")
                {
                    ii++;
                }
            }
            if (i==0 && ii!=0)
            {
                EndRound(Team.ChaosInsurgency,"SW_WIN");
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }

    public void CancelEnd(EndingRoundEventArgs ev)
    {
        int i = 0;
        List<string> Uniques = new List<string>()
        {
            "FIFTHIST",
            "F_Priest",
            "SnowWarrier"
        };
        foreach (Player player in Player.List)
        {
            if (Uniques.Contains(player.UniqueRole))
            {
                i++;
            }
        }
        if (i!=0)
        {
            ev.IsAllowed = false;
            Round.IsLocked = true;
            Timing.RunCoroutine(RoundLocker());
        }
    }

    private IEnumerator<float> RoundLocker()
    {
        for (;;)
        {
            int i = 0;
            List<string> Uniques = new List<string>()
            {
                "FIFTHIST",
                "F_Priest",
                "SnowWarrier"
            };
            foreach (Player player in Player.List)
            {
                if (Uniques.Contains(player.UniqueRole))
                {
                    i++;
                }
            }

            if (i==0)
            {
                Round.IsLocked = false;
                yield break;
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }

    public void Spawn3005(Player player)
    {
        player.Role.Set(RoleTypeId.Scp0492);
        Slafight_Plugin_EXILED.Plugin.Singleton.LabApiHandler.Schem3005(player);
        Vector3 offset;
        int MaxHealth = 55555;

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","SCP-3005");
        
        Timing.CallDelayed(0.05f, () =>
        {
            player.UniqueRole = "SCP-3005";
            player.CustomInfo = "<color=#FF0090>SCP-3005</color>";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.MaxHealth = MaxHealth;
            player.Health = MaxHealth;
            Log.Debug("3005");
            player.EnableEffect(EffectType.MovementBoost,50);
            
            player.ShowHint(
                "<color=red>SCP-3005</color>\n非常に<color=#ff00fa>第五的</color>である。そうは思わんかね？",
                10);
            Room SpawnRoom = Room.Get(RoomType.LczPlants);
            Log.Debug(SpawnRoom.Position);
            offset = new Vector3(0f,7.35f,0f);
            player.Position = SpawnRoom.Position + SpawnRoom.Rotation * offset;
            player.Rotation = SpawnRoom.Rotation;
            Timing.RunCoroutine(Scp3005Coroutine(player));
        });
    }
    
    public void SpawnFifthist(Player player)
    {
        player.Role.Set(RoleTypeId.Tutorial);
        Vector3 offset;
        int MaxHealth = 150;

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","FIFTHIST RESCURE");
        
        Timing.CallDelayed(0.05f, () =>
        {
            player.UniqueRole = "FIFTHIST";
            player.CustomInfo = "<color=#FF0090>Fifthist Rescure</color>";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.MaxHealth = MaxHealth;
            player.Health = MaxHealth;
            
            player.ShowHint(
                "<color=#ff00fa>第五教会 救出師</color>\n非常に<color=#ff00fa>第五的</color>な存在を脱出させなければいけない",
                10);
            Room SpawnRoom = Room.Get(RoomType.Surface);
            Log.Debug(SpawnRoom.Position);
            offset = new Vector3(0f,0f,0f);
            player.Position = new Vector3(124f,289f,21f);//SpawnRoom.Position + SpawnRoom.Rotation * offset;
            //player.Rotation = SpawnRoom.Rotation;
            
            player.ClearInventory();
            Log.Debug("Giving Items to Fifthist");
            player.AddItem(ItemType.GunSCP127);
            player.AddItem(ItemType.ArmorHeavy);
            CustomItem.TryGive(player, 5,false);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.GrenadeHE);
        });
    }
    
    public void SpawnF_Priest(Player player)
    {
        player.Role.Set(RoleTypeId.Tutorial);
        Vector3 offset;
        int MaxHealth = 555;

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","FIFTHIST PRIEST");
        
        Timing.CallDelayed(0.05f, () =>
        {
            player.UniqueRole = "F_Priest";
            player.Scale = new Vector3(1.2f,1.2f,1.2f);
            player.CustomInfo = "<color=#FF0090>Fifthist Priest</color>";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.MaxHealth = MaxHealth;
            player.Health = MaxHealth;
            
            player.ShowHint(
                "<color=#ff00fa>第五教会 司祭</color>\n非常に<color=#ff00fa>第五的</color>な存在の恩寵を受けた第五主義者。\n施設を占領せよ！",
                10);
            Room SpawnRoom = Room.Get(RoomType.Surface);
            Log.Debug(SpawnRoom.Position);
            offset = new Vector3(0f,0f,0f);
            player.Position = new Vector3(124f,289f,21f);//SpawnRoom.Position + SpawnRoom.Rotation * offset;
            //player.Rotation = SpawnRoom.Rotation;
            
            player.ClearInventory();
            Log.Debug("Giving Items to F_Priest");
            player.AddItem(ItemType.GunSCP127);
            player.AddItem(ItemType.ArmorHeavy);
            CustomItem.TryGive(player, 6,false);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.GrenadeHE);
            
            var light = Light.Create(Vector3.zero);
            light.Position = player.Transform.position + new Vector3(0f, -0.08f, 0f);
            light.Transform.parent = player.Transform;
            light.Scale = new Vector3(1f,1f,1f);
            light.Range = 10f;
            light.Intensity = 1.25f;
            light.Color = Color.magenta;
            
            Timing.RunCoroutine(Scp3005Coroutine(player));
        });
    }

    public void SpawnChaosCommando(Player player)
    {
        player.Role.Set(RoleTypeId.ChaosRepressor);
        Vector3 offset;
        int MaxHealth = 100;

        //PlayerExtensions.OverrideRoleName(player,"","Chaos Insurgency Commando");
        
        Timing.CallDelayed(0.05f, () =>
        {
            player.UniqueRole = "CI_Commando";
            player.CustomInfo = "Chaos Insurgency Commando";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.MaxHealth = MaxHealth;
            player.Health = MaxHealth;
            player.CustomHumeShieldStat._maxValueOverride = true;
            player.CustomHumeShieldStat.MaxValue = 25;
            player.CustomHumeShieldStat.CurValue = 25;
            player.CustomHumeShieldStat.ShieldRegenerationMultiplier = 1.05f;
            
            player.ShowHint(
                "<color=#228b22>カオス コマンド―</color>\nサイトに対する略奪を円滑にするために迅速な制圧を実行する実力者\nインサージェンシーによってヒュームシールド改造をされている。",
                10);
            Room SpawnRoom = Room.Get(RoomType.Surface);
            Log.Debug(SpawnRoom.Position);
            offset = new Vector3(0f,0f,0f);
            //player.Position = new Vector3(124f,289f,21f);//SpawnRoom.Position + SpawnRoom.Rotation * offset;
            //player.Rotation = SpawnRoom.Rotation;
            
            player.ClearInventory();
            Log.Debug("Giving Items to CI_Commando");
            player.AddItem(ItemType.GunLogicer);
            player.AddItem(ItemType.ArmorHeavy);
            player.AddItem(ItemType.KeycardChaosInsurgency);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.GrenadeHE);
            
            player.AddAmmo(AmmoType.Nato762,800);
        });
    }
    
    public void SpawnSnowWarrier(Player player)
    {
        player.Role.Set(RoleTypeId.ChaosRifleman,RoleSpawnFlags.All);
        player.Role.Set(RoleTypeId.Tutorial,RoleSpawnFlags.AssignInventory);
        Slafight_Plugin_EXILED.Plugin.Singleton.LabApiHandler.SchemSnowWarrier(player);
        //Vector3 offset;
        int MaxHealth = 1000;

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","SCP-3005");
        
        Timing.CallDelayed(0.05f, () =>
        {
            player.UniqueRole = "SnowWarrier";
            player.CustomInfo = "<color=#FFFFFF>SNOW WARRIER</color>";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.MaxHealth = MaxHealth;
            player.Health = MaxHealth;
            player.EnableEffect(EffectType.Slowness,10);
            
            player.ShowHint(
                "<color=white>SNOW WARRIER</color>\n非常に<color=#ffffff>雪玉的</color>である。そうは思わんかね？",
                10);

            player.AddItem(ItemType.SCP1509);
            player.AddItem(ItemType.GunCOM18);
            player.AddItem(ItemType.ArmorHeavy);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.KeycardO5);
            
            player.AddAmmo(AmmoType.Nato9,50);
        });
    }

    public void CustomFriendlyFire_hurt(HurtingEventArgs ev)
    {
        if (ev.Attacker == null || ev.Player == null)
            return; // 攻撃者またはプレイヤーがnullの場合は処理終了
        if (ev.Attacker?.UniqueRole == "FIFTHIST")
        {
            if (ev.Player?.UniqueRole == "SCP-3005")
            {
                ev.IsAllowed = false;
                ev.Attacker.Hurt(15f,"<color=#ff00fa>第五的存在</color>に反逆した為");
                ev.Attacker.ShowHint("<color=#ff00fa>第五的存在</color>に反逆するとは何事か！？",5f);
            }
        }
    }

    public void CryFuckSpawn(Player player)
    {
        Timing.CallDelayed(0.05f, () =>
        {
            // Very Fuckin Stupid Code.
            Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.CryFuckSpawned = true;
            player.Role.Set(RoleTypeId.Scp096);
            player.UniqueRole = "Scp096_Anger";
            
            player.MaxArtificialHealth = 1000;
            player.MaxHealth = 5000;
            player.Health = 5000;
            player.EnableEffect(EffectType.Slowness,25);
            player.ShowHint(
                "<color=red>SCP-096: ANGER</color>\nSCP-096の怒りと悲しみが頂点に達し、その化身へと変貌して大いなる力を手に入れた。\n<color=red>とにかく破壊しまくれ！！！！！</color>",
                10);
            player.Transform.eulerAngles = new Vector3(0, -90, 0);
            Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.ShyguyPosition = player.Position;
            Log.Debug("Scp096: Anger was Spawned!");
            Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.StartAnger();
        });
    }

    public void DiedCassie(DyingEventArgs ev)
    {
        Log.Debug(ev.Player.UniqueRole);
        if (ev.Player.UniqueRole == "SCP-3005")
        {
            //SchematicObject schematicObject = ObjectSpawner.SpawnSchematic("SCP3005",ev.Player.Position,ev.Player.Rotation,Vector3.one,null);
            Exiled.API.Features.Cassie.Clear();
            Exiled.API.Features.Cassie.MessageTranslated("SCP 3 0 0 5 contained successfully by $pitch_.85 Anti- $pitch_1 Me mu Protocol.","<color=red>SCP-3005</color> は、アンチミームプロトコルにより再収用されました",true,false);
        }
        else if (ev.Player.UniqueRole == "SCP-966")
        {
            //SchematicObject schematicObject = ObjectSpawner.SpawnSchematic("SCP3005",ev.Player.Position,ev.Player.Rotation,Vector3.one,null);
            Exiled.API.Features.Cassie.Clear();
            CassieExtensions.CassieTranslated("SCP 9 6 6 contained successfully by $pitch_.85 Anti- $pitch_1 Me mu Protocol.","<color=red>SCP-3005</color> は、アンチミームプロトコルにより再収用されました",true);
        }
    }

    public void CencellRagdoll(SpawningRagdollEventArgs ev)
    {
        if (ev.Player.UniqueRole == "SCP-3005")
        {
            ev.IsAllowed = false;
        }
    }
    public void CustomRoleRemover(ChangingRoleEventArgs ev)
    {
        ev.Player.UniqueRole = null;
        ev.Player.CustomInfo = null;
        ev.Player.Scale = new Vector3(1f, 1f, 1f);
    }

    public void EndRound(Team winnerTeam = Team.SCPs,string specificReason = null)
    {
        if (winnerTeam == Team.SCPs && specificReason == null)
        {
            Round.KillsByScp = 999;
            Round.EndRound(true);
        }
        else if (winnerTeam == Team.SCPs && specificReason == "FIFTHIST_WIN")
        {
            Round.KillsByScp = 555;
            foreach (Player player in Player.List)
            {
                player.ShowHint("<b><size=80><color=#ff00fa>第五教会</color>の勝利</size></b>",8f);
                Timing.CallDelayed(1f, () =>
                {
                    Round.Restart(false);
                });
            }
        }
        else if ((winnerTeam == Team.ChaosInsurgency || winnerTeam == Team.ClassD) && specificReason == null)
        {
            if (specificReason == null)
            {
                Round.EscapedDClasses = 999;
                Round.EndRound(true);
            }
        }
        else if (winnerTeam == Team.ChaosInsurgency && specificReason == "SW_WIN")
        {
            foreach (Player player in Player.List)
            {
                player.ShowHint("<b><size=80><color=#ffffff>雪の戦士達</color>の勝利</size></b>",8f);
                Timing.CallDelayed(1f, () =>
                {
                    Round.Restart(false);
                });
            }
        }
        else if (winnerTeam == Team.FoundationForces || winnerTeam == Team.Scientists)
        {
            if (specificReason == null)
            {
                Round.EscapedScientists = 999;
                Round.EndRound(true);
            }
        }
        else
        {
            Round.EndRound(true);
        }
    }

    private IEnumerator<float> Scp3005Coroutine(Player player)
    {
        for (;;)
        {
            float distance;
            foreach (Player _player in Player.List)
            {
                if (_player != player && _player.Role.Team != Team.SCPs && (_player.UniqueRole != "FIFTHIST"||_player.UniqueRole == "F_Priest"))
                {
                    distance = Vector3.Distance(player.Position,_player.Position);
                    if (distance <= 2.75f)
                    {
                        _player.Hurt(25f,"<color=#ff00fa>第五的</color>な力による影響");
                        player.ShowHitMarker();
                    }
                }
            }

            if (player.UniqueRole == "SCP-3005")
            {
                if (Plugin.Singleton.LabApiHandler._activatedAntiMemeProtocolInPast)
                {
                    player.DisableEffect(EffectType.Slowness);
                    player.EnableEffect(EffectType.MovementBoost, 25);
                }
                else
                {
                    player.DisableEffect(EffectType.MovementBoost);
                    player.EnableEffect(EffectType.Slowness, 25);
                }
                if (Slafight_Plugin_EXILED.Plugin.Singleton.LabApiHandler.activatedAntiMemeProtocol)
                {
                    player.Hurt(100f,"<color=#ff00fa>アンチミームプロトコロル</color>により終了された");
                }
            }

            if (player.UniqueRole != "SCP-3005" && player.UniqueRole != "F_Priest")
            {
                yield break;
            }
            yield return Timing.WaitForSeconds(1.5f);
        }
    }
}
