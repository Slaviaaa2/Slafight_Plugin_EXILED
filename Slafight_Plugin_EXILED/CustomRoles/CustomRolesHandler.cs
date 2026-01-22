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
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomRoles.SCPs;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using DamageHandlerBase = Exiled.API.Features.DamageHandlers.DamageHandlerBase;
using Light = Exiled.API.Features.Toys.Light;
using Object = UnityEngine.Object;

namespace Slafight_Plugin_EXILED.CustomRoles
{
    public class CustomRolesHandler
    {
        public CustomRolesHandler()
        {
            Exiled.Events.Handlers.Player.Dying += DiedCassie;
            Exiled.Events.Handlers.Player.ChangingRole += CustomRoleRemover;
            Exiled.Events.Handlers.Player.SpawningRagdoll += CencellRagdoll;
            Exiled.Events.Handlers.Player.Hurting += CustomFriendlyFire_hurt;
            Exiled.Events.Handlers.Server.RoundStarted += RoundCoroutine;

            Exiled.Events.Handlers.Server.EndingRound += CancelEnd;
            Exiled.Events.Handlers.Server.WaitingForPlayers += ResetAbilities;
            Exiled.Events.Handlers.Server.RestartingRound += AbilityResetInRoundRestarting;
        }

        ~CustomRolesHandler()
        {
            Exiled.Events.Handlers.Player.Dying -= DiedCassie;
            Exiled.Events.Handlers.Player.ChangingRole -= CustomRoleRemover;
            Exiled.Events.Handlers.Player.SpawningRagdoll -= CencellRagdoll;
            Exiled.Events.Handlers.Player.Hurting -= CustomFriendlyFire_hurt;
            Exiled.Events.Handlers.Server.RoundStarted -= RoundCoroutine;

            Exiled.Events.Handlers.Server.EndingRound -= CancelEnd;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= ResetAbilities;
            Exiled.Events.Handlers.Server.RestartingRound -= AbilityResetInRoundRestarting;
        }

        public void ResetAbilities()
        {
            AbilityResetUtil.ResetAllAbilities();
        }

        public void RoundCoroutine()
        {
            Timing.CallDelayed(10f, () =>
            {
                Timing.RunCoroutine(FifthistCoroutine());
                Timing.RunCoroutine(NewAICoroutine());
                if (Plugin.Singleton.Config.Season == 2)
                    Timing.RunCoroutine(SnowmanCoroutine());
            });
        }

        private IEnumerator<float> FifthistCoroutine()
        {
            List<string> fifthistRoles = new()
            {
                "FIFTHIST",
                "SCP-3005",         // UniqueRole 依存のまま運用
                "F_Priest",
                "FifthistConvert"
            };

            for (;;)
            {
                if (Round.IsLobby)
                    yield break;

                int nonFifthists = 0;
                int fifthists = 0;

                foreach (Player player in Player.List)
                {
                    if (player == null || !player.IsAlive)
                        continue;

                    if (!fifthistRoles.Contains(player.UniqueRole) && player.GetCustomRole() != CRoleTypeId.Scp999)
                        nonFifthists++;
                    else
                        fifthists++;
                }

                if (nonFifthists == 0 && fifthists != 0)
                    EndRound(Team.SCPs, "FIFTHIST_WIN");

                yield return Timing.WaitForSeconds(1f);
            }
        }

        private IEnumerator<float> SnowmanCoroutine()
        {
            for (;;)
            {
                if (Round.IsLobby)
                    yield break;

                int nonSnow = 0;
                int snow = 0;

                foreach (Player player in Player.List)
                {
                    if (player == null || !player.IsAlive)
                        continue;

                    if (player.UniqueRole != "SnowWarrier" && player.GetCustomRole() != CRoleTypeId.Scp999)
                        nonSnow++;
                    else
                        snow++;
                }

                if (nonSnow == 0 && snow != 0)
                    EndRound(Team.ChaosInsurgency, "SW_WIN");

                yield return Timing.WaitForSeconds(1f);
            }
        }
        
        private IEnumerator<float> NewAICoroutine()
        {
            for (;;)
            {
                if (Round.IsLobby)
                    yield break;

                int scpCount = 0;
                bool only079 = true;

                // CTeamごとのカウント
                int foundationCount = 0;  // FoundationForces + Scientists + Guards をまとめて「財団側」として扱うならここ
                int classDCount = 0;
                int chaosCount = 0;
                int otherCount = 0;       // Fifthists, GoC, UIU, SerpentsHand など

                List<Player> scps = new();
                List<Player> scp079s = new();

                foreach (Player player in Player.List)
                {
                    if (player == null || !player.IsAlive)
                        continue;

                    var team = player.GetTeam();  // CTeam
                    var roleType = player.Role.Type;

                    if (team == CTeam.SCPs)
                    {
                        scpCount++;
                        scps.Add(player);

                        if (roleType == RoleTypeId.Scp079)
                            scp079s.Add(player);
                        else
                            only079 = false;
                    }
                    else if (team == CTeam.FoundationForces || team == CTeam.Scientists || team == CTeam.Guards)
                    {
                        foundationCount++;
                    }
                    else if (team == CTeam.ClassD)
                    {
                        classDCount++;
                    }
                    else if (team == CTeam.ChaosInsurgency)
                    {
                        chaosCount++;
                    }
                    else
                    {
                        otherCount++;
                    }
                }

                // 「SCP陣営がSCP-079のみ」
                bool scpSideIsOnly079 = scpCount > 0 && only079 && scp079s.Count == scpCount;

                // 「SCP以外のCTeamが一つだけ」
                int nonScpTeamsAlive = 0;
                if (foundationCount > 0) nonScpTeamsAlive++;
                if (classDCount > 0)     nonScpTeamsAlive++;
                if (chaosCount > 0)      nonScpTeamsAlive++;
                if (otherCount > 0)      nonScpTeamsAlive++;

                bool onlyOneNonScpTeam = nonScpTeamsAlive == 1;

                if (scpSideIsOnly079 && onlyOneNonScpTeam)
                {
                    // ここで079をkill
                    foreach (var p in scp079s)
                    {
                        // 好きなダメージ種別でOK
                        p.Kill("Terminated by C.A.S.S.I.E.","SCP-079 has been terminated by Central Autonomic Service System for Internal Emergencies.");
                    }

                    // 条件一度満たしたらこのコルーチンは終了
                    yield break;
                }

                yield return Timing.WaitForSeconds(1f);
            }
}
        
        List<CRoleTypeId> uniques = new()
        {
            CRoleTypeId.FifthistRescure,
            CRoleTypeId.FifthistPriest,
            CRoleTypeId.FifthistConvert,
            CRoleTypeId.SnowWarrier
        };

        public void CancelEnd(EndingRoundEventArgs ev)
        {
            int count = 0;

            foreach (Player player in Player.List)
            {
                if (uniques.Contains(player.GetCustomRole()))
                    count++;
            }

            if (count == 0)
                return;

            ev.IsAllowed = false;
            Round.IsLocked = true;
            Timing.RunCoroutine(RoundLocker());
        }

        private IEnumerator<float> RoundLocker()
        {
            for (;;)
            {
                int count = 0;

                foreach (Player player in Player.List)
                {
                    if (uniques.Contains(player.GetCustomRole()))
                        count++;
                }

                if (count == 0)
                {
                    Round.IsLocked = false;
                    yield break;
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        // ★ ここから下は SCP-3005 を除くスポーン系

        public void SpawnFifthist(Player player, RoleSpawnFlags roleSpawnFlags)
        {
            player.Role.Set(RoleTypeId.Tutorial);
            int maxHealth = 150;

            player.UniqueRole = "FIFTHIST";
            player.CustomInfo = "<color=#FF0090>Fifthist Rescure</color>";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.MaxHealth = maxHealth;
            player.Health = maxHealth;

            player.ShowHint(
                "<color=#ff00fa>第五教会 救出師</color>\n非常に<color=#ff00fa>第五的</color>な存在を脱出させなければいけない",
                10);

            Room spawnRoom = Room.Get(RoomType.Surface);
            Vector3 offset = new(0f, 0f, 0f);
            player.Position = new Vector3(124f, 289f, 21f);
            // player.Rotation = spawnRoom.Rotation;

            player.ClearInventory();
            Log.Debug("Giving Items to Fifthist");
            player.AddItem(ItemType.GunSCP127);
            player.AddItem(ItemType.ArmorHeavy);
            CustomItem.TryGive(player, 5, false);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.GrenadeHE);
        }

        public void SpawnF_Priest(Player player, RoleSpawnFlags roleSpawnFlags)
        {
            player.Role.Set(RoleTypeId.Tutorial);
            int maxHealth = 555;

            player.UniqueRole = "F_Priest";
            player.Scale = new Vector3(1.1f, 1.1f, 1.1f);
            player.CustomInfo = "<color=#FF0090>Fifthist Priest</color>";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.MaxHealth = maxHealth;
            player.Health = maxHealth;

            player.ShowHint(
                "<color=#ff00fa>第五教会 司祭</color>\n非常に<color=#ff00fa>第五的</color>な存在の恩寵を受けた第五主義者。\n施設を占領せよ！",
                10);

            Room spawnRoom = Room.Get(RoomType.Surface);
            Vector3 offset = new(0f, 0f, 0f);
            player.Position = new Vector3(124f, 289f, 21f);
            // player.Rotation = spawnRoom.Rotation;

            player.ClearInventory();
            Log.Debug("Giving Items to F_Priest");
            player.AddItem(ItemType.GunSCP127);
            player.AddItem(ItemType.ArmorHeavy);
            CustomItem.TryGive(player, 6, false);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.GrenadeHE);

            var light = Light.Create(Vector3.zero);
            light.Position = player.Transform.position + new Vector3(0f, -0.08f, 0f);
            light.Transform.parent = player.Transform;
            light.Scale = new Vector3(1f, 1f, 1f);
            light.Range = 10f;
            light.Intensity = 1.25f;
            light.Color = Color.magenta;
            
            Timing.RunCoroutine(Scp3005Coroutine(player));
        }
        
        private IEnumerator<float> Scp3005Coroutine(Player player)
        {
            for (;;)
            {
                if (player.GetCustomRole() != CRoleTypeId.FifthistPriest)
                    yield break;

                foreach (Player target in Player.List)
                {
                    if (target == null || target == player || !target.IsAlive)
                        continue;

                    if (target.GetTeam() == CTeam.Fifthists || target.GetCustomRole() == CRoleTypeId.Scp3005)
                        continue;

                    float distance = Vector3.Distance(player.Position, target.Position);
                    if (distance <= 2.75f)
                    {
                        target.Hurt(25f, "<color=#ff00fa>第五的</color>な力による影響");
                        player.ShowHitMarker();
                    }
                }

                yield return Timing.WaitForSeconds(1.5f);
            }
        }

        public void SpawnChaosCommando(Player player, RoleSpawnFlags roleSpawnFlags)
        {
            player.Role.Set(RoleTypeId.ChaosRepressor);
            int maxHealth = 100;

            player.UniqueRole = "CI_Commando";
            player.CustomInfo = "Chaos Insurgency Commando";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.MaxHealth = maxHealth;
            player.Health = maxHealth;

            player.CustomHumeShieldStat.MaxValue = 25;
            player.CustomHumeShieldStat.CurValue = 25;
            player.CustomHumeShieldStat.ShieldRegenerationMultiplier = 1.05f;

            player.ShowHint(
                "<color=#228b22>カオス コマンド―</color>\nサイトに対する略奪を円滑にするために迅速な制圧を実行する実力者\nインサージェンシーによってヒュームシールド改造をされている。",
                10);

            Room spawnRoom = Room.Get(RoomType.Surface);
            Vector3 offset = new(0f, 0f, 0f);
            // player.Position = new Vector3(124f,289f,21f);

            player.ClearInventory();
            Log.Debug("Giving Items to CI_Commando");
            player.AddItem(ItemType.GunLogicer);
            player.AddItem(ItemType.ArmorHeavy);
            player.AddItem(ItemType.KeycardChaosInsurgency);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.Adrenaline);
            player.AddItem(ItemType.GrenadeHE);

            player.AddAmmo(AmmoType.Nato762, 800);
        }

        public void SpawnSnowWarrier(Player player, RoleSpawnFlags roleSpawnFlags)
        {
            player.Role.Set(RoleTypeId.ChaosRifleman, RoleSpawnFlags.All);
            player.Role.Set(RoleTypeId.Tutorial, RoleSpawnFlags.AssignInventory);
            Plugin.Singleton.LabApiHandler.SchemSnowWarrier(LabApi.Features.Wrappers.Player.Get(player.ReferenceHub));

            int maxHealth = 1000;

            Timing.CallDelayed(0.05f, () =>
            {
                player.UniqueRole = "SnowWarrier";
                player.CustomInfo = "<color=#FFFFFF>SNOW WARRIER</color>";
                player.InfoArea |= PlayerInfoArea.Nickname;
                player.InfoArea &= ~PlayerInfoArea.Role;
                player.MaxHealth = maxHealth;
                player.Health = maxHealth;
                player.EnableEffect(EffectType.Slowness, 10);

                player.ShowHint(
                    "<color=white>SNOW WARRIER</color>\n非常に<color=#ffffff>雪玉的</color>である。そうは思わんかね？",
                    10);

                player.AddItem(ItemType.SCP1509);
                player.AddItem(ItemType.GunCOM18);
                player.AddItem(ItemType.ArmorHeavy);
                player.AddItem(ItemType.SCP500);
                player.AddItem(ItemType.SCP500);
                player.AddItem(ItemType.KeycardO5);

                player.AddAmmo(AmmoType.Nato9, 50);
            });
        }

        public void CustomFriendlyFire_hurt(HurtingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Player == null)
                return;

            if (ev.Attacker.UniqueRole == "FIFTHIST" && ev.Player.UniqueRole == "SCP-3005")
            {
                ev.IsAllowed = false;
                ev.Attacker.Hurt(15f, "<color=#ff00fa>第五的存在</color>に反逆した為");
                ev.Attacker.ShowHint("<color=#ff00fa>第五的存在</color>に反逆するとは何事か！？", 5f);
            }
        }

        public void DiedCassie(DyingEventArgs ev)
        {
            // ここは他ロール用の CASSIE があれば実装、それ以外は空でOK
        }

        public void CencellRagdoll(SpawningRagdollEventArgs ev)
        {
            // ここも他ロール用に使うなら実装、なければ空でOK
        }

        public void CustomRoleRemover(ChangingRoleEventArgs ev)
        {
            Log.Debug($"[CustomRoleRemover] Reset ALL for {ev.Player?.Nickname} (role change {ev.Player?.Role} -> {ev.NewRole})");

            ev.Player!.UniqueRole = null;
            ev.Player.CustomInfo = null;
            ev.Player.Scale = new Vector3(1f, 1f, 1f);
            
            ev.Player.ClearCustomInfo();

            var player = ev.Player;

            AbilityManager.ClearSlots(player);
            AbilityBase.RevokeAbility(player.Id);

            Timing.CallDelayed(1f, () =>
            {
                try
                {
                    if (Plugin.Singleton?.PlayerHUD != null && player != null && player.IsConnected)
                        Plugin.Singleton.PlayerHUD.HintSync(SyncType.PHUD_Specific, string.Empty, player);
                    RoleSpecificTextProvider.Clear(player);
                }
                catch
                {
                    // 無視してOK
                }
            });
        }

        public void AbilityResetInRoundRestarting()
        {
            AbilityManager.Loadouts.Clear();
            AbilityBase.RevokeAllPlayers();
        }

        public void EndRound(Team winnerTeam = Team.SCPs, string specificReason = null)
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
                    player.ShowHint("<b><size=80><color=#ff00fa>第五教会</color>の勝利</size></b>", 8f);
                    Timing.CallDelayed(1f, () => { Round.Restart(false); });
                }
            }
            else if ((winnerTeam == Team.ChaosInsurgency || winnerTeam == Team.ClassD) && specificReason == null)
            {
                Round.EscapedDClasses = 999;
                Round.EndRound(true);
            }
            else if (winnerTeam == Team.ChaosInsurgency && specificReason == "SW_WIN")
            {
                foreach (Player player in Player.List)
                {
                    player.ShowHint("<b><size=80><color=#ffffff>雪の戦士達</color>の勝利</size></b>", 8f);
                    Timing.CallDelayed(1f, () => { Round.Restart(false); });
                }
            }
            else if (winnerTeam == Team.FoundationForces || winnerTeam == Team.Scientists)
            {
                Round.EscapedScientists = 999;
                Round.EndRound(true);
            }
            else
            {
                Round.EndRound(true);
            }
        }
    }
}