using System;
using System.Collections.Generic;
using System.Linq;
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
            Exiled.Events.Handlers.Player.ChangingRole += CustomRoleRemover;
            Exiled.Events.Handlers.Player.Hurting += CustomFriendlyFire_hurt;
            Exiled.Events.Handlers.Server.RoundStarted += RoundCoroutine;

            Exiled.Events.Handlers.Server.EndingRound += CancelEnd;
            Exiled.Events.Handlers.Server.WaitingForPlayers += ResetAbilities;
            Exiled.Events.Handlers.Server.RestartingRound += AbilityResetInRoundRestarting;
        }

        ~CustomRolesHandler()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= CustomRoleRemover;
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
                Timing.RunCoroutine(FifthistWinCoroutine());
                Timing.RunCoroutine(NewAICoroutine());
                if (Plugin.Singleton.Config.Season == 2)
                    Timing.RunCoroutine(SnowWarrierWinCoroutine());
            });
        }

        private IEnumerator<float> FifthistWinCoroutine()
        {
            for (;;)
            {
                if (!Round.IsStarted || Round.IsLobby)
                    yield break;

                var players = Player.List
                    .Where(p => p != null && p.IsAlive && p.Role.Type != RoleTypeId.Spectator)
                    .ToList();

                //Log.Debug($"[FifthistWin] AliveNonSpec={players.Count}");

                if (players.IsOnlyTeam(CTeam.Fifthists))
                {
                    Log.Debug("[FifthistWin] Triggered");
                    Round.IsLocked = false;
                    EndRound(CTeam.Fifthists);  // 自分のEndRound呼び出しに修正
                    yield break;
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        private IEnumerator<float> SnowWarrierWinCoroutine()
        {
            for (;;)
            {
                if (!Round.IsStarted || Round.IsLobby)
                    yield break;

                var players = Player.List
                    .Where(p => p != null && p.IsAlive && p.Role.Type != RoleTypeId.Spectator)
                    .ToList();

                //Log.Debug($"[SnowWarrierWin] AliveNonSpec={players.Count}");

                if (players.IsOnlyTeam(CTeam.Others, "snow"))
                {
                    Log.Debug("[SnowWarrierWin] Triggered");
                    Round.IsLocked = false;
                    EndRound(CTeam.Others, "SW_WIN");  // 自分のEndRound呼び出しに修正
                    yield break;
                }

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
            CRoleTypeId.FifthistGuidance,
            CRoleTypeId.GoCOperative,
            CRoleTypeId.GoCDeputy,
            CRoleTypeId.GoCMedic,
            CRoleTypeId.GoCThaumaturgist,
            CRoleTypeId.GoCCommunications,
            CRoleTypeId.GoCOperative,
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

        public void EndRound(CTeam winnerTeam = CTeam.SCPs, string specificReason = null)
        {
            switch (winnerTeam)
            {
                case CTeam.SCPs:
                    Round.KillsByScp = 999;
                    Round.EndRound(true);
                    break;

                case CTeam.Fifthists:
                    Round.KillsByScp = 555;
                    foreach (Player player in Player.List){
                        player.ShowHint("<b><size=80><color=#ff00fa>第五教会</color>の勝利</size></b>", 555f);
                        Intercom.TrySetOverride(player, true);
                    }

                    Timing.CallDelayed(10f, () =>
                    {
                        if (Round.IsLobby) return;
                        Round.Restart(false);
                    });
                    break;

                case CTeam.ChaosInsurgency:
                    Round.EscapedDClasses = 999;
                    Round.EndRound(true);
                    break;

                case CTeam.ClassD:
                    Round.EscapedDClasses = 999;
                    Round.EndRound(true);
                    break;

                case CTeam.FoundationForces:
                case CTeam.Scientists:
                    Round.EscapedScientists = 999;
                    Round.EndRound(true);
                    break;

                case CTeam.Others:
                    Round.EscapedDClasses = 999;
                    if (specificReason == "SW_WIN")
                    {
                        foreach (Player player in Player.List){
                            player.ShowHint("<b><size=80><color=#ffffff>雪の戦士達</color>の勝利</size></b>", 555f);
                            Intercom.TrySetOverride(player, true);
                        }

                        Timing.CallDelayed(10f, () =>
                        {
                            if (Round.IsLobby) return;
                            Round.Restart(false);
                        });
                    }
                    else
                    {
                        foreach (Player player in Player.List){
                            player.ShowHint("<b><size=80><color=#ffffff>UNKNOWN TEAM</color>の勝利</size></b>", 555f);
                            Intercom.TrySetOverride(player, true);
                        }

                        Timing.CallDelayed(10f, () =>
                        {
                            if (Round.IsLobby) return;
                            Round.Restart(false);
                        });
                    }
                    break;

                case CTeam.GoC:
                    Round.EscapedDClasses = 999;
                    if (specificReason == "SavedHumanity")
                    {
                        foreach (var player in Player.List){
                            player.ShowHint("<b><size=80><color=#0000c8>人類</color>の勝利</size></b>", 555f);
                            Intercom.TrySetOverride(player, true);
                        }

                        Timing.CallDelayed(10f, () =>
                        {
                            if (Round.IsLobby) return;
                            Round.Restart(false);
                        });
                    }
                    else
                    {
                        foreach (var player in Player.List){
                            player.ShowHint("<b><size=80><color=#0000c8>世界オカルト連合</color>の勝利</size></b>", 555f);
                            Intercom.TrySetOverride(player, true);
                        }

                        Timing.CallDelayed(10f, () =>
                        {
                            if (Round.IsLobby) return;
                            Round.Restart(false);
                        });
                    }
                    break;

                default:
                    Round.EndRound(true);
                    break;
            }
        }
    }
}