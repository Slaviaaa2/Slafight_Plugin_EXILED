using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles
{
    public class CustomRolesHandler
    {
        private CoroutineHandle _fifthistHandle;
        private CoroutineHandle _snowHandle;
        private CoroutineHandle _roundLockerHandle;

        // 特殊勝利でラウンドを終わらせている最中か
        public bool IsSpecialWinEnding { get; set; }

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

        // ===== 共通リセット =====

        private void KillAllCoroutines()
        {
            if (_fifthistHandle.IsRunning)
                Timing.KillCoroutines(_fifthistHandle);
            if (_snowHandle.IsRunning)
                Timing.KillCoroutines(_snowHandle);
            if (_roundLockerHandle.IsRunning)
                Timing.KillCoroutines(_roundLockerHandle);
        }

        public void ResetAbilities()
        {
            IsSpecialWinEnding = false;
            AbilityResetUtil.ResetAllAbilities();
            KillAllCoroutines();
        }

        public void AbilityResetInRoundRestarting()
        {
            IsSpecialWinEnding = false;
            AbilityManager.Loadouts.Clear();
            AbilityBase.RevokeAllPlayers();
            KillAllCoroutines();
        }

        // ===== ラウンド開始: 勝利コルーチン登録 =====

        public void RoundCoroutine()
        {
            Timing.CallDelayed(10f, () =>
            {
                if (!Round.IsStarted || Round.IsLobby)
                    return;

                KillAllCoroutines();
                IsSpecialWinEnding = false;

                _fifthistHandle = Timing.RunCoroutine(FifthistWinCoroutine());
                _snowHandle     = Timing.RunCoroutine(SnowWarrierWinCoroutine());
                _roundLockerHandle = Timing.RunCoroutine(RoundLocker());
            });
        }

        // ===== 特殊勝利コルーチン =====

        // Fifthist 勝利: Fifthists + SCP-999 + 観戦/チュートリアルだけ
        private IEnumerator<float> FifthistWinCoroutine()
        {
            for (;;)
            {
                if (!Round.IsStarted || Round.IsLobby)
                    yield break;

                var players = Player.List.ToList();

                if (players.IsOnlyTeam(CTeam.Fifthists))
                {
                    Round.IsLocked = false;
                    CTeam.Fifthists.EndRound();
                    yield break;
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        // SnowWarrier 勝利: SnowWarrier + SCP-999 + 観戦/チュートリアルだけ
        private IEnumerator<float> SnowWarrierWinCoroutine()
        {
            for (;;)
            {
                if (!Round.IsStarted || Round.IsLobby)
                    yield break;

                var players = Player.List.ToList();

                if (players.IsOnlyTeam(CTeam.Others, "snow"))
                {
                    Round.IsLocked = false;
                    CTeam.Others.EndRound("SW_WIN");
                    yield break;
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        // ===== 通常用 RoundLock =====

        private readonly List<CRoleTypeId> uniques = new()
        {
            CRoleTypeId.FifthistRescure,
            CRoleTypeId.FifthistPriest,
            CRoleTypeId.FifthistConvert,
            CRoleTypeId.SnowWarrier
        };

        private bool HasUniqueRoleAlive()
        {
            foreach (var player in Player.List)
            {
                if (player == null || !player.IsAlive)
                    continue;

                if (uniques.Contains(player.GetCustomRole()))
                    return true;
            }

            return false;
        }

        // バニラの EndingRound を延命するためのロック
        public void CancelEnd(EndingRoundEventArgs ev)
        {
            // 特殊勝利中は EndingRound に一切干渉しない
            if (IsSpecialWinEnding)
                return;

            if (!HasUniqueRoleAlive())
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
                // 特殊勝利で終わる流れに入っているならロック解除して終わる
                if (IsSpecialWinEnding)
                {
                    Round.IsLocked = false;
                    yield break;
                }

                int count = 0;
                foreach (var player in Player.List)
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
    }
}
