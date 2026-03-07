using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Utilities;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp966Role : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.Scp966;
    protected override CTeam Team { get; set; } = CTeam.SCPs;
    protected override string UniqueRoleKey { get; set; } = "Scp966";

    // 966 が攻撃したターゲット情報用
    private static readonly Dictionary<Player, HashSet<Player>> AttackedTargets = new();

    // 可視状態
    private enum VisibilityState
    {
        Hidden,   // 完全透明 (Invisible 255)
        Visible,  // 通常可視 (Invisible 0, Fade 0)
        Faded,    // 半透明    (Invisible 0, Fade 中間)
    }

    // (966, 対象) ごとの可視状態
    private static readonly Dictionary<(Player scp966, Player target), VisibilityState> Visibility
        = new();

    // (966, victim) ごとに Faded の有効期限
    private static readonly Dictionary<(Player scp966, Player victim), float> FadeExpireTimes
        = new();

    // NVG 持ちの「今 3m 圏内かどうか」
    private readonly Dictionary<Player, bool> _nvgInRange = new();

    // 音再生デリゲート
    private static readonly Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio
        = EventHandler.CreateAndPlayAudio;

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.Hurting += OnHurting;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.Hurting -= OnHurting;
        base.UnregisterEvents();
    }

    public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);

        player.Role.Set(RoleTypeId.Scp3114); // モデル流用。純966モデルがあるなら Scp966 に
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 1500;
        player.Health = player.MaxHealth;
        player.MaxHumeShield = 100;

        AttackedTargets[player] = new HashSet<Player>();

        player.ClearInventory();
        player.SetCustomInfo("SCP-966");

        var spawnRoom = Room.Get(RoomType.LczGlassBox);
        var offset = new Vector3(0f, 1.5f, 0f);
        player.Position = spawnRoom.Position + spawnRoom.Rotation * offset;
        player.Rotation = spawnRoom.Rotation;

        // 基礎的に遅く
        player.EnableEffect(EffectType.Slowness, 40);
        // 本人は常時ナイトビジョン
        player.EnableEffect(EffectType.NightVision, 255);

        // 高速移動アビリティ付与
        player.AddAbility(new Scp966SpeedAbility(player));

        // 初期可視状態セット
        Timing.CallDelayed(0.2f, () => InitVisibility(player));

        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint(
                "<size=24><color=red>SCP-966</color>\n" +
                "ナイトビジョンゴーグルでのみ完全に視認される SCP。\n" +
                "攻撃した相手には一時的に半透明で姿を見せる。\n" +
                "アビリティで一時的に高速移動ができる。",
                10f);
        });

        Timing.RunCoroutine(StateCoroutine(player));
    }

    // ランダムなエコーボイス
    private void PlayEchoVoice(Vector3 position)
    {
        var random = new System.Random();
        int index = random.Next(1, 4); // 1 ~ 3
        CreateAndPlayAudio?.Invoke($"966Echo{index}.ogg", "Scp966", position,
            true, null, false, 999999999f, 0f);
    }

    /// <summary>
    /// 可視状態を FakeEffect だけで適用する。
    /// Hidden:   Invisible 255, Fade 0
    /// Visible:  Invisible 0,   Fade 0
    /// Faded:    Invisible 0,   Fade 100
    /// </summary>
    private void ApplyVisibility(Player scp966, Player target, VisibilityState state)
    {
        if (scp966 == null || target == null || !target.IsConnected) return;

        var key = (scp966, target);
        Visibility[key] = state;

        byte invisible = 0;
        byte fade = 0;

        switch (state)
        {
            case VisibilityState.Hidden:
                invisible = 255;
                fade = 0;
                break;

            case VisibilityState.Visible:
                invisible = 0;
                fade = 0;
                break;

            case VisibilityState.Faded:
                invisible = 0;
                fade = 100; // 半透明具合は好みで
                break;
        }

        scp966.SendFakeEffectTo(target, EffectType.Invisible, invisible);
        scp966.SendFakeEffectTo(target, EffectType.Fade, fade);
    }

    private void InitVisibility(Player scp966)
    {
        if (scp966 == null || !scp966.IsConnected) return;

        foreach (var pl in Player.List)
        {
            if (pl == null || pl == scp966) continue;

            if (pl.HasWornGoggle<NvgNormal>())
            {
                // NVG持ちには最初から見える
                ApplyVisibility(scp966, pl, VisibilityState.Visible);
            }
            else
            {
                // それ以外には完全透明
                ApplyVisibility(scp966, pl, VisibilityState.Hidden);
            }
        }
    }

    /// <summary>
    /// メインループ:
    /// - HUD 表示
    /// - NVG(NvgNormal)持ちが 3m 圏に「入った瞬間」に一度だけ声を鳴らす
    /// </summary>
    private IEnumerator<float> StateCoroutine(Player scp966)
    {
        while (scp966 != null && scp966.IsConnected && scp966.GetCustomRole() == CRoleTypeId.Scp966)
        {
            // HUD
            var attackedCount = AttackedTargets.TryGetValue(scp966, out var set) ? set.Count : 0;

            // NVG持ち 3m in/out 判定
            foreach (var target in Player.List.Where(p =>
                         p != null && p != scp966 && p.IsConnected && p.HasWornGoggle<NvgNormal>()))
            {
                float distance = Vector3.Distance(target.Position, scp966.Position);
                bool inRangeNow = distance <= 3f;

                bool wasInRange = _nvgInRange.TryGetValue(target, out var prev) && prev;

                // 外 → 中 に入った瞬間だけ音
                if (!wasInRange && inRangeNow)
                    PlayEchoVoice(target.Position);

                _nvgInRange[target] = inRangeNow;
            }

            // いなくなったプレイヤー掃除
            foreach (var key in _nvgInRange.Keys.ToList())
            {
                if (!Player.List.Contains(key))
                    _nvgInRange.Remove(key);
            }

            yield return Timing.WaitForSeconds(0.25f);
        }

        // 終了時掃除
        if (scp966 != null)
        {
            scp966.DisableEffect(EffectType.Fade);
            scp966.DisableEffect(EffectType.Invisible);
            scp966.DisableEffect(EffectType.NightVision);
            scp966.DisableEffect(EffectType.Slowness);
            scp966.DisableEffect(EffectType.MovementBoost);
        }
    }

    private void OnHurting(HurtingEventArgs ev)
    {
        // 966 が攻撃したとき
        if (ev.Attacker != null && ev.Attacker.GetCustomRole() == CRoleTypeId.Scp966)
        {
            var scp966 = ev.Attacker;
            var victim = ev.Player;

            ev.Amount = 10f;

            if (victim != null && victim.IsConnected)
            {
                if (!AttackedTargets.TryGetValue(scp966, out var set))
                    AttackedTargets[scp966] = set = new HashSet<Player>();

                set.Add(victim);

                var key = (scp966, victim);
                float now = Time.time;

                // 既に Faded 中かどうか
                bool hasFadeInfo = FadeExpireTimes.TryGetValue(key, out var expireTime);
                bool stillInFade = hasFadeInfo && expireTime > now;

                // 10 秒間 Faded 状態にする（Invisible 0, Fade 中間）
                FadeExpireTimes[key] = now + 10f;
                ApplyVisibility(scp966, victim, VisibilityState.Faded);

                // 10 秒後に Visible/Hidden に戻す（延長されていない場合だけ）
                Timing.CallDelayed(10f, () =>
                {
                    if (!FadeExpireTimes.TryGetValue(key, out var latest) || latest > Time.time)
                        return; // 別の攻撃で延長されている

                    if (scp966 != null && scp966.IsConnected &&
                        victim != null && victim.IsConnected)
                    {
                        var baseState = victim.HasWornGoggle<NvgNormal>()
                            ? VisibilityState.Visible
                            : VisibilityState.Hidden;

                        ApplyVisibility(scp966, victim, baseState);
                    }

                    FadeExpireTimes.Remove(key);
                });

                // Fade が切れてから初めての攻撃 → 「見えた瞬間」のジャンプスケア音
                if (!stillInFade)
                    PlayEchoVoice(victim.Position);
            }

            // 高速アビリティ中なら解除
            Scp966SpeedAbility.OnAttackedCancelSpeed(scp966);

            return;
        }

        // 966 が被弾したとき
        if (ev.Player != null && ev.Player.GetCustomRole() == CRoleTypeId.Scp966)
        {
            if (ev.Attacker != null && ev.Attacker.IsConnected)
                ev.Attacker.ShowHitMarker();
        }
    }

    protected override void OnDying(DyingEventArgs ev)
    {
        if (ev.Player != null && ev.Player.GetCustomRole() == CRoleTypeId.Scp966)
        {
            AttackedTargets.Remove(ev.Player);

            // 可視状態・Fadeテーブルから該当 966 を掃除
            foreach (var key in Visibility.Keys.Where(k => k.scp966 == ev.Player).ToList())
                Visibility.Remove(key);
            foreach (var key in FadeExpireTimes.Keys.Where(k => k.scp966 == ev.Player).ToList())
                FadeExpireTimes.Remove(key);
        }

        Exiled.API.Features.Cassie.Clear();
        Exiled.API.Features.Cassie.MessageTranslated(
            "SCP 9 6 6 Successfully Terminated .",
            "<color=red>SCP-966</color>の終了に成功しました。",
            true);

        base.OnDying(ev);
    }
}
