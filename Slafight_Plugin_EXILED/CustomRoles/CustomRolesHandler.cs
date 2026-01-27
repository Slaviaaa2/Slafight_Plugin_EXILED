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
        private CoroutineHandle _fifthistHandle;
        private CoroutineHandle _snowHandle;
        private CoroutineHandle _aiHandle;
        private CoroutineHandle _roundLockerHandle;

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

            KillAllCoroutines();
        }

        private void KillAllCoroutines()
        {
            if (_fifthistHandle.IsRunning)
                Timing.KillCoroutines(_fifthistHandle);
            if (_snowHandle.IsRunning)
                Timing.KillCoroutines(_snowHandle);
            if (_aiHandle.IsRunning)
                Timing.KillCoroutines(_aiHandle);
            if (_roundLockerHandle.IsRunning)
                Timing.KillCoroutines(_roundLockerHandle);
        }

        public void ResetAbilities()
        {
            AbilityResetUtil.ResetAllAbilities();
            KillAllCoroutines();
        }

        public void AbilityResetInRoundRestarting()
        {
            AbilityManager.Loadouts.Clear();
            AbilityBase.RevokeAllPlayers();
            KillAllCoroutines();
        }

        public void RoundCoroutine()
        {
            Timing.CallDelayed(10f, () =>
            {
                if (!Round.IsStarted || Round.IsLobby)
                    return;

                KillAllCoroutines();

                _fifthistHandle = Timing.RunCoroutine(FifthistCoroutine());
                _aiHandle = Timing.RunCoroutine(NewAICoroutine());

                if (Plugin.Singleton.Config.Season == 2)
                    _snowHandle = Timing.RunCoroutine(SnowmanCoroutine());
            });
        }

        private IEnumerator<float> FifthistCoroutine()
        {
            List<string> fifthistRoles = new()
            {
                "FIFTHIST",
                "SCP-3005",
                "F_Priest",
                "FifthistConvert"
            };

            for (;;)
            {
                if (!Round.IsStarted || Round.IsLobby)
                    yield break;

                int nonFifthists = 0;
                int fifthists = 0;

                foreach (Player player in Player.List)
                {
                    if (player == null || !player.IsAlive)
                        continue;

                    if (!fifthistRoles.Contains(player.UniqueRole) &&
                        player.GetCustomRole() != CRoleTypeId.Scp999)
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
                if (!Round.IsStarted || Round.IsLobby)
                    yield break;

                int nonSnow = 0;
                int snow = 0;

                foreach (Player player in Player.List)
                {
                    if (player == null || !player.IsAlive)
                        continue;

                    if (player.UniqueRole != "SnowWarrier" &&
                        player.GetCustomRole() != CRoleTypeId.Scp999)
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
                if (!Round.IsStarted || Round.IsLobby)
                    yield break;

                int scpCount = 0;
                bool only079 = true;

                int foundationCount = 0;
                int classDCount = 0;
                int chaosCount = 0;
                int otherCount = 0;

                List<Player> scps = new();
                List<Player> scp079s = new();

                foreach (Player player in Player.List)
                {
                    if (player == null || !player.IsAlive)
                        continue;

                    var team = player.GetTeam();
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

                bool scpSideIsOnly079 = scpCount > 0 && only079 && scp079s.Count == scpCount;

                int nonScpTeamsAlive = 0;
                if (foundationCount > 0) nonScpTeamsAlive++;
                if (classDCount > 0)     nonScpTeamsAlive++;
                if (chaosCount > 0)      nonScpTeamsAlive++;
                if (otherCount > 0)      nonScpTeamsAlive++;

                bool onlyOneNonScpTeam = nonScpTeamsAlive == 1;

                if (scpSideIsOnly079 && onlyOneNonScpTeam)
                {
                    foreach (var p in scp079s)
                    {
                        p.Kill("Terminated by C.A.S.S.I.E.",
                            "SCP-079 has been terminated by Central Autonomic Service System for Internal Emergencies.");
                    }

                    yield break;
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        // ===== 勝利条件用ユニークロール =====

        private readonly List<CRoleTypeId> uniques = new()
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

            if (_roundLockerHandle.IsRunning)
                Timing.KillCoroutines(_roundLockerHandle);

            _roundLockerHandle = Timing.RunCoroutine(RoundLocker());
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

        // ===== Spawn 系（CI_Commando だけ残す） =====

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

        // ===== その他イベント =====

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

        public void DiedCassie(DyingEventArgs ev) { }

        public void CencellRagdoll(SpawningRagdollEventArgs ev) { }

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
                }
            });
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
                    player.ShowHint("<b><size=80><color=#ff00fa>第五教会</color>の勝利</size></b>", 8f);

                Timing.CallDelayed(1f, () => { Round.Restart(false); });
            }
            else if ((winnerTeam == Team.ChaosInsurgency || winnerTeam == Team.ClassD) && specificReason == null)
            {
                Round.EscapedDClasses = 999;
                Round.EndRound(true);
            }
            else if (winnerTeam == Team.ChaosInsurgency && specificReason == "SW_WIN")
            {
                foreach (Player player in Player.List)
                    player.ShowHint("<b><size=80><color=#ffffff>雪の戦士達</color>の勝利</size></b>", 8f);

                Timing.CallDelayed(1f, () => { Round.Restart(false); });
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