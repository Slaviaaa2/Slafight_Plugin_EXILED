using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using InventorySystem;
using JetBrains.Annotations;
using MEC;
using PlayerRoles;
using ProjectMER.Commands.Utility;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

// イベント系はエイリアス付けて衝突回避
using PlayerHandlers = Exiled.Events.Handlers.Player;
using MapHandlers = Exiled.Events.Handlers.Map;
using PlayerEvents = Exiled.Events.EventArgs.Player;
using MapEvents = Exiled.Events.EventArgs.Map;

namespace Slafight_Plugin_EXILED.API.Features
{
    public abstract class CRole
    {
        // 全インスタンスを追跡（重複登録防止）
        private static readonly HashSet<CRole> RegisteredInstances = new();

        // 全Roleタイプをキャッシュ
        private static readonly List<Type> RoleTypes;

        // UniqueRole 文字列 → CRole インスタンス（バリアント含む）
        private static readonly Dictionary<string, CRole> UniqueRoleToRole = new(StringComparer.OrdinalIgnoreCase);

        private static bool _eventsSubscribed;

        static CRole()
        {
            var asm = typeof(CRole).Assembly;
            RoleTypes = asm.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CRole)) && !t.IsAbstract)
                .Where(t => t.GetCustomAttributes(typeof(CRoleAutoRegisterIgnoreAttribute), true).Length == 0)
                .ToList();
        }

        /// <summary>
        /// 自動登録から除外したい CRole 用属性。
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public sealed class CRoleAutoRegisterIgnoreAttribute : Attribute { }

        // ==== Plugin から呼ぶ入り口 ====

        /// <summary>
        /// 全ての CRole 派生クラスをインスタンス化し、共通イベントに自動サブスクする。
        /// Plugin.OnEnabled から 1 回呼ぶ想定。
        /// </summary>
        public static void RegisterAllEvents()
        {
            if (!_eventsSubscribed)
            {
                // 共通イベントを CRole に 1 回だけサブスク
                PlayerHandlers.Dying += OnAnyPlayerDying;
                MapHandlers.AnnouncingScpTermination += OnAnyAnnouncingScpTermination;
                _eventsSubscribed = true;
            }

            foreach (var type in RoleTypes)
            {
                try
                {
                    var instance = (CRole)Activator.CreateInstance(type);

                    // UniqueRoleKey 即チェック（null/emptyは無視）
                    if (string.IsNullOrEmpty(instance.UniqueRoleKey))
                    {
                        Log.Warn($"CRole.RegisterAllEvents: {type.Name} has null/empty UniqueRoleKey, skipping");
                        continue;
                    }

                    instance.InternalRegisterEvents();
                }
                catch (Exception ex)
                {
                    Log.Error($"CRole.RegisterAllEvents failed for {type.Name}: {ex}");
                }
            }
        }

        /// <summary>
        /// 全ての CRole 派生クラスのイベント登録を解除する。
        /// Plugin.OnDisabled から 1 回呼ぶ想定。
        /// </summary>
        public static void UnregisterAllEvents()
        {
            foreach (var instance in RegisteredInstances.ToList())
                instance.InternalUnregisterEvents();

            RegisteredInstances.Clear();
            UniqueRoleToRole.Clear();

            if (_eventsSubscribed)
            {
                PlayerHandlers.Dying -= OnAnyPlayerDying;
                MapHandlers.AnnouncingScpTermination -= OnAnyAnnouncingScpTermination;
                _eventsSubscribed = false;
            }
        }

        // ==== 共通イベントハンドラ（static） ====

        private static void OnAnyPlayerDying(PlayerEvents.DyingEventArgs ev)
        {
            // ★奈落などでevやPlayerがnullになるケースに対応
            if (ev?.Player == null)
            {
                Log.Debug("OnAnyPlayerDying: ev or ev.Player is null, skipping");
                return;
            }

            string uniqueRole = ev.Player.UniqueRole;

            // ★HP0や変な死に方でUniqueRoleがnull/空になるケースを防ぐ
            if (string.IsNullOrEmpty(uniqueRole))
            {
                Log.Debug($"OnAnyPlayerDying: UniqueRole is null/empty for {ev.Player.Nickname}, skipping");
                return;
            }

            if (!UniqueRoleToRole.TryGetValue(uniqueRole, out var role))
            {
                // CRole管理外ユニークロールは無視
                return;
            }

            try
            {
                role.OnDying(ev);  // 対応する 1 ロールだけに発火
            }
            catch (Exception ex)
            {
                Log.Error($"CRole.OnDying error in {role.GetType().Name}: {ex}");
            }
        }

        private static void OnAnyAnnouncingScpTermination(MapEvents.AnnouncingScpTerminationEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            string uniqueRole = ev.Player.UniqueRole;
            if (string.IsNullOrEmpty(uniqueRole))
                return;

            if (!UniqueRoleToRole.TryGetValue(uniqueRole, out var role))
                return;

            try
            {
                role.OnDyingCassie(ev);
            }
            catch (Exception ex)
            {
                Log.Error($"CRole.OnDyingCassie error in {role.GetType().Name}: {ex}");
            }
        }

        // ==== インスタンス管理 ====

        private void InternalRegisterEvents()
        {
            if (RegisteredInstances.Add(this))
            {
                // UniqueRole → CRole マップ登録
                if (!string.IsNullOrEmpty(UniqueRoleKey))
                {
                    UniqueRoleToRole[UniqueRoleKey] = this;
                }

                RegisterEvents(); // 派生クラスで個別に使いたいイベント用（任意）
                Log.Debug($"CRole registered: {GetType().Name}");
            }
        }

        private void InternalUnregisterEvents()
        {
            if (RegisteredInstances.Remove(this))
            {
                // UniqueRole マップ解除
                if (!string.IsNullOrEmpty(UniqueRoleKey) &&
                    UniqueRoleToRole.TryGetValue(UniqueRoleKey, out var inst) &&
                    ReferenceEquals(inst, this))
                {
                    UniqueRoleToRole.Remove(UniqueRoleKey);
                }

                UnregisterEvents();
                Log.Debug($"CRole unregistered: {GetType().Name}");
            }
        }

        /// <summary>
        /// 派生クラス用フック（任意の追加イベントがあればここで）。
        /// 例: PlayerHandlers.Hurting += OnHurting; など。
        /// </summary>
        public virtual void RegisterEvents() { }

        /// <summary>
        /// 派生クラス用フック（任意の追加イベント解除）。
        /// </summary>
        public virtual void UnregisterEvents() { }

        // ==== CRole メタ情報 ====

        /// <summary>
        /// この CRole が表す CRoleTypeId。
        /// Check() や OnDying での判定に使う。
        /// </summary>
        protected abstract CRoleTypeId CRoleTypeId { get; set; }

        /// <summary>
        /// このロールが属する CTeam。CTeam 判定用。
        /// </summary>
        protected abstract CTeam Team { get; set; }

        /// <summary>
        /// 対応する UniqueRole 文字列。
        /// 例: "Scp682", "FIFTHIST", "F_Priest" など。
        /// </summary>
        protected abstract string UniqueRoleKey { get; set; }

        // ==== Player → CRole / Team 逆引きヘルパ ====

        public static CRoleTypeId GetRoleIdFromUnique(string uniqueRole)
        {
            if (string.IsNullOrEmpty(uniqueRole))
                return CRoleTypeId.None;

            return UniqueRoleToRole.TryGetValue(uniqueRole, out var role)
                ? role.CRoleTypeId
                : CRoleTypeId.None;
        }

        public static CTeam GetTeamFromUnique(string uniqueRole)
        {
            if (string.IsNullOrEmpty(uniqueRole))
                return CTeam.Others;

            return UniqueRoleToRole.TryGetValue(uniqueRole, out var role)
                ? role.Team
                : CTeam.Others;
        }

        /// <summary>
        /// Player がこの CRoleTypeId を持っているか判定。
        /// UniqueRole → CRoleTypeId マップ経由なので、バリアントもまとめて判定可能。
        /// </summary>
        protected bool Check([CanBeNull] Player player)
        {
            if (player == null) return false;
            return GetRoleIdFromUnique(player.UniqueRole) == CRoleTypeId;
        }

        // ==== 共通ロジック ====

        /// <summary>
        /// SCP 終了アナウンスを差し替えたい時用。
        /// isEnable=true と cassieString/localizedString を指定して呼ぶ想定。
        /// アナウンスされない(049-2)とかの時は OnDying を override して実装。
        /// </summary>
        protected virtual void OnDyingCassie(
            MapEvents.AnnouncingScpTerminationEventArgs ev,
            bool isEnable = false,
            string cassieString = null,
            string localizedString = null)
        {
            if (!isEnable) return;
            if (!Check(ev.Player)) return;

            ev.IsAllowed = false;
            RoleSpecificTextProvider.Clear(ev.Player);
            Exiled.API.Features.Cassie.MessageTranslated(cassieString, localizedString);
        }

        /// <summary>
        /// プレイヤー死亡時の共通処理。
        /// デフォルトでは「自分の CRoleTypeId を持っているなら RoleSpecificText をクリア」だけ。
        /// </summary>
        protected virtual void OnDying(PlayerEvents.DyingEventArgs ev)
        {
            // Dyingキャンセル済みなら何もしない
            if (!ev.IsAllowed) return;
            if (!Check(ev.Player)) return;

            RoleSpecificTextProvider.Clear(ev.Player);
        }

        /// <summary>
        /// SpawnFlags に応じて位置・インベントリを維持/復元する共通ロジック。
        /// 各ロールの SpawnRole から base.SpawnRole(...) を呼ぶ想定。
        /// </summary>
        public virtual void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
        {
            if (player == null)
            {
                Log.Error("CRole: SpawnRole failed. Reason: Player is null");
                return;
            }

            if (roleSpawnFlags == RoleSpawnFlags.None)
            {
                // 位置もインベントリも維持
                Vector3 savePosition = player.Position + new Vector3(0f, 0.1f, 0f);
                var items = player.Items.ToList();
                var ammos = player.Ammo.ToList();

                Timing.CallDelayed(1f, () =>
                {
                    player.Position = savePosition;
                    player.ClearInventory();

                    foreach (var item in items)
                        player.AddItem(item);

                    foreach (var ammo in ammos)
                        player.AddAmmo((AmmoType)ammo.Key, ammo.Value);
                });
            }
            else if (roleSpawnFlags == RoleSpawnFlags.AssignInventory)
            {
                // 位置だけ維持（インベントリはロール側で配る）
                Vector3 savePosition = player.Position + new Vector3(0f, 0.1f, 0f);
                Timing.CallDelayed(1f, () =>
                {
                    player.Position = savePosition;
                });
            }
            else if (roleSpawnFlags == RoleSpawnFlags.UseSpawnpoint)
            {
                // スポーンポイントに飛ばすけどインベントリは維持
                var items = player.Items.ToList();
                var ammos = player.Ammo.ToList();

                Timing.CallDelayed(1f, () =>
                {
                    player.ClearInventory();

                    foreach (var item in items)
                        player.AddItem(item);

                    foreach (var ammo in ammos)
                        player.AddAmmo((AmmoType)ammo.Key, ammo.Value);
                });
            }
            // RoleSpawnFlags.All の場合は「ロール側が自由にやる」想定で特に何もしない
        }
    }
}
