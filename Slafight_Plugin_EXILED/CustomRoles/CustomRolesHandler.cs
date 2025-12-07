using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.CustomStats;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Features.Roles;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using InventorySystem;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using DamageHandlerBase = Exiled.API.Features.DamageHandlers.DamageHandlerBase;
using Light = Exiled.API.Features.Toys.Light;
using Object = UnityEngine.Object;

namespace Slafight_Plugin_EXILED.CustomRoles;

public class CustomRolesHandler
{
    public CustomRolesHandler()
    {
        Exiled.Events.Handlers.Player.Dying += DiedCassieAnnounce;
        Exiled.Events.Handlers.Player.ChangingRole += CustomRoleRemover;
        Exiled.Events.Handlers.Player.SpawningRagdoll += CencellRagdoll;
        Exiled.Events.Handlers.Player.Hurting += CustomFriendlyFire_hurt;
    }
    ~CustomRolesHandler()
    {
        Exiled.Events.Handlers.Player.Dying -= DiedCassieAnnounce;
        Exiled.Events.Handlers.Player.ChangingRole -= CustomRoleRemover;
        Exiled.Events.Handlers.Player.SpawningRagdoll -= CencellRagdoll;
        Exiled.Events.Handlers.Player.Hurting -= CustomFriendlyFire_hurt;
    }

    public static void OverrideRoleName(Player player, string CustomInfo, string DisplayName, string RoleName, string Color)
    {
        // Custom Role Name Area
        player.InfoArea |= PlayerInfoArea.CustomInfo;
        // Hide Things
        player.InfoArea &= ~PlayerInfoArea.Role;
        player.InfoArea &= ~PlayerInfoArea.Nickname;
        
        if (CustomInfo is null || CustomInfo.Length < 1)
        {
            player.ReferenceHub.nicknameSync.Network_customPlayerInfoString = $"<color={Color}>{DisplayName}\n{player.UniqueRole}</color>";
        }
        else
        {
            player.ReferenceHub.nicknameSync.Network_customPlayerInfoString = $"<color={Color}>{CustomInfo}\n{DisplayName}\n{player.UniqueRole}</color>";
        }
    }

    public void Spawn3005(Player player)
    {
        player.Role.Set(RoleTypeId.Scp0492);
        Slafight_Plugin_EXILED.Plugin.Singleton.LabApiHandler.Schem3005(player);
        Vector3 offset;
        int MaxHealth = 55555;

        player.CustomInfo = "<color=#C50000>SCP-3005</color>";
        
        Timing.CallDelayed(1.5f, () =>
        {
            player.UniqueRole = "SCP-3005";

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

        player.CustomInfo = "<color=#ff00fa>FIFTHIST RESCURE - 第五教会 救出師</color>";
        
        Timing.CallDelayed(0.05f, () =>
        {
            player.UniqueRole = "FIFTHIST";

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
            player.AddItem(ItemType.GunSCP127);
            player.AddItem(ItemType.ArmorHeavy);
            CustomItem.TryGive(player, 5,false);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.GrenadeHE);
        });
    }

    public void SpawnChaosCommando(Player player)
    {
        player.Role.Set(RoleTypeId.ChaosRepressor);
        Vector3 offset;
        int MaxHealth = 100;

        player.CustomInfo = "<color=#228b22>CI Commando - カオス コマンド―</color>";
        
        Timing.CallDelayed(0.05f, () =>
        {
            player.UniqueRole = "CI_Commando";

            player.MaxHealth = MaxHealth;
            player.Health = MaxHealth;
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
            OverrideRoleName(player, "",player.DisplayNickname, "SCP-096: ANGER", "#C50000");
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

    public void DiedCassieAnnounce(DyingEventArgs ev)
    {
        Log.Debug(ev.Player.UniqueRole);
        if (ev.Player.UniqueRole == "SCP-3005")
        {
            //SchematicObject schematicObject = ObjectSpawner.SpawnSchematic("SCP3005",ev.Player.Position,ev.Player.Rotation,Vector3.one,null);
            Cassie.Clear();
            Cassie.MessageTranslated("SCP 3 0 0 5 contained successfully by Anti me mu Protocol.","<color=red>SCP-3005</color> は、アンチミームプロトコルにより再収用されました",true,false);
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
                Round.RestartSilently();
            }
        }
        else if (winnerTeam == Team.ChaosInsurgency || winnerTeam == Team.ClassD)
        {
            if (specificReason == null)
            {
                Round.EscapedDClasses = 999;
                Round.EndRound(true);
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
                if (_player != player && _player.Role.Team != Team.SCPs && _player.UniqueRole != "FIFTHIST")
                {
                    distance = Vector3.Distance(player.Position,_player.Position);
                    if (distance <= 2.75f)
                    {
                        _player.Hurt(25f,"<color=#ff00fa>第五的</color>な力による影響");
                    }
                }
            }

            if (Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.isFifthistsRaidActive)
            {
                if (player.Position.x >= 120f && player.Position.x <= 125f)
                {
                    if (player.Position.y >= 280f)
                    {
                        if (player.Position.z >= 18f && player.Position.z <= 25f)
                        {
                            player.UniqueRole = null;
                            SpawnFifthist(player);
                            EndRound(Team.SCPs,"FIFTHIST_WIN");
                        }
                    }
                }
            }

            if (Slafight_Plugin_EXILED.Plugin.Singleton.LabApiHandler._activatedAntiMemeProtocolInPast)
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

            if (player.UniqueRole != "SCP-3005")
            {
                yield break;
            }
            yield return Timing.WaitForSeconds(1.5f);
        }
    }
}
