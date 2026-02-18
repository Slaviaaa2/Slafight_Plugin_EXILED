using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Hazards;
using Exiled.API.Features.Items;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp106;
using Exiled.Events.EventArgs.Warhead;
using HintServiceMeow.Core.Utilities;
using InventorySystem.Items.Scp1509;
using MEC;
using Mirror;
using PlayerRoles;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Changes;
using Slafight_Plugin_EXILED.CustomMaps;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MainHandlers;
using Slafight_Plugin_EXILED.SpecialEvents;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp035Role : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.Scp035;
    protected override CTeam Team { get; set; } = CTeam.SCPs;
    protected override string UniqueRoleKey { get; set; } = "Scp035";

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.Dying += OnDyingByRole;
        Exiled.Events.Handlers.Warhead.Starting += OnStartingByRole;
        OmegaWarhead.OmegaWarheadStarting += OnStartingByRoleOnOmega;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.Dying -= OnDyingByRole;
        Exiled.Events.Handlers.Warhead.Starting -= OnStartingByRole;
        OmegaWarhead.OmegaWarheadStarting -= OnStartingByRoleOnOmega;
        base.UnregisterEvents();
    }
    
    public struct Scp035State
    {
        public Scp035StateType NowState;
        public float ChangeStateTimeAwaiting;
    }
    
    Dictionary<int, Scp035State> states = new();
    
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Tutorial);
        player.ChangeAppearance(RoleTypeId.Scientist);
        player.MaxHealth = 2500f;
        player.Health = player.MaxHealth;
        player.MaxArtificialHealth = 500f;
        player.ArtificialHealth = 500f;
        player.UniqueRole = UniqueRoleKey;
        player.ClearInventory();
        player.AddItem(ItemType.KeycardScientist);
        player.AddItem(ItemType.Painkillers);
        player.SetCustomInfo("<color=#C50000>SCP-035</color>");
        TryChangeState(player, new Scp035State()
        {
            NowState = Scp035StateType.Stable,
            ChangeStateTimeAwaiting = 180f,
        });

        player.Position = Room.Get(RoomType.Hcz939).WorldPosition(Vector3.up * 0.65f);
        
        player.TryWear("SCP035", out var schematicObject,new Vector3(0f, 0.65f, 0.175f));
        schematicObject.Scale += new Vector3(0.08f, 0.08f, 0.08f);
        LabApi.Features.Wrappers.Player.Get(player.NetId)!.DestroySchematic(schematicObject);

        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<size=24><color=red>SCP-035</color>\n" +
                            "愚かな博士が仮面をつけて乗っ取れた！\n" +
                            "但し、博士がなんとかしようと仮面に抵抗している為精神状態が不安定です。\n" +
                            "あなたの最終的な目標は<color=red>施設の破壊</color>です。\n" +
                            "精神が安定している時は比較的人間達に友好的に接し、そうでない時は\n「触手」を用いて邪魔をさせないようにし、弾頭へと向かいましょう。\n" +
                            "<color=yellow>※通常時は博士、発狂時はチュートリアルの見た目になります。" +
                            "※RPがとても重要となります。頑張って！</color></size>",
                15f);
        });
        Timing.RunCoroutine(Coroutine(player));
    }

    protected override void OnDying(DyingEventArgs ev)
    {
        states.Remove(ev.Player.Id, out _);
        Exiled.API.Features.Cassie.MessageTranslated("SCP 0 3 5 Recontained successfully .", "<color=red>SCP-035</color>の再収容に成功しました。");
        base.OnDying(ev);
    }

    private void OnDyingByRole(DyingEventArgs ev)
    {
        if (!Check(ev.Attacker)) return;
        ev.Attacker.ArtificialHealth += 35f;
    }

    private void OnStartingByRole(StartingEventArgs ev)
    {
        var state = GetState(ev.Player);
        if (!Check(ev.Player) || state.NowState == Scp035StateType.FullyAwaken) return;
        state.NowState = Scp035StateType.FullyAwaken;
        state.ChangeStateTimeAwaiting = 1;
        TryChangeState(ev.Player, state);
    }

    private void OnStartingByRoleOnOmega(object sender, OmegaWarheadStartingEventArgs ev)
    {
        var state = GetState(ev.Player);
        if (!Check(ev.Player) || state.NowState == Scp035StateType.FullyAwaken) return;
        state.NowState = Scp035StateType.FullyAwaken;
        state.ChangeStateTimeAwaiting = 1;
        TryChangeState(ev.Player, state);
    }

    public void Cleanup(Player player)
    {
        states.Remove(player.Id, out _);
        RoleSpecificTextProvider.Clear(player);
        if (AbilityBase.HasAbility<Scp035TentacleAbility>(player))
            player.RemoveAbility<Scp035TentacleAbility>();
    }

    public bool TryChangeState(Player player, Scp035StateType newState)
    {
        try
        {
            if (states.TryGetValue(player.Id, out var playerState))
            {
                playerState.NowState = newState;
                states[player.Id] = playerState;
                return true;
            }
            return states.TryAdd(player.Id, new Scp035State(){NowState = newState});
        }
        catch (Exception e)
        {
            Log.Warn($"Failed to change {player.Nickname}'s SCP-035 State. Reason:\n{e}");
            return false;
        }
    }
    public bool TryChangeState(Player player, Scp035State newState)
    {
        try
        {
            if (states.TryGetValue(player.Id, out var playerState))
            {
                states[player.Id] = newState;
                return true;
            }
            return states.TryAdd(player.Id, newState);
        }
        catch (Exception e)
        {
            Log.Warn($"Failed to change {player.Nickname}'s SCP-035 State. Reason:\n{e}");
            return false;
        }
    }

    public Scp035State GetState(Player player)
    {
        states.TryGetValue(player.Id, out var value);
        return value;
    }

    public string GetStateLoc(Scp035StateType stateType)
    {
        return stateType switch
        {
            Scp035StateType.Stable => "<color=green>安定</color>",
            Scp035StateType.Unstable => "<color=yellow>不安定</color>",
            Scp035StateType.Awaken => "<color=red>発狂／覚醒</color>",
            Scp035StateType.FullyAwaken => "<color=red><b>完全覚醒</b></color>",
            _ => "[不明]"
        };
    }

    private bool Trigger(Player player, Scp035StateType stateType)
    {
        if (player == null) return false;
        switch (stateType)
        {
            case Scp035StateType.Stable:
                player.DisableAllEffects();
                if (AbilityBase.HasAbility<Scp035TentacleAbility>(player))
                    player.RemoveAbility<Scp035TentacleAbility>();
                player.ChangeAppearance(RoleTypeId.Scientist);
                player.ShowHint($"<color=green>安定</color>状態へと移行しました！\n現在精神は比較的安定しており、人々に危害を与える必要は無いでしょう。\nアビリティ「触手」が無効化されました。\n<color=green>人々と友好的に接しましょう</color>");
                return true;
            case Scp035StateType.Unstable:
                player.EnableEffect(EffectType.Poisoned, 10);
                player.ShowHint($"<color=yellow>不安定</color>状態へと移行しました！\n現在精神は揺れ動いており、常に回復が必要でしょう。\n腐蝕が再開しました。\n<color=yellow>人々に警告を与え、己の生存を心掛けましょう。</color>");
                return true;
            case Scp035StateType.Awaken:
                player.DisableEffect(EffectType.Poisoned);
                if (!AbilityBase.HasAbility<Scp035TentacleAbility>(player))
                    player.AddAbility(new Scp035TentacleAbility(player));
                player.EnableEffect(EffectType.Invigorated, 20);
                player.EnableEffect(EffectType.BodyshotReduction, 30);
                player.EnableEffect(EffectType.DamageReduction, 30);
                player.ChangeAppearance(RoleTypeId.Tutorial);
                player.ShowHint($"<color=red>発狂／覚醒</color>状態へと移行しました！\n現在精神は支配されており、己の為に全てを犠牲にする必要があるでしょう。\n腐蝕が止まり、アビリティ「触手」が使用可能になりました！\n<color=red>ためらう必要はない。出る事だけを考えるのだ。</color>");
                return true;
            case Scp035StateType.FullyAwaken:
                player.DisableEffect(EffectType.Poisoned);
                if (!AbilityBase.HasAbility<Scp035TentacleAbility>(player))
                    player.AddAbility(new Scp035TentacleAbility(player));
                player.EnableEffect(EffectType.Invigorated, 30);
                player.EnableEffect(EffectType.BodyshotReduction, 40);
                player.EnableEffect(EffectType.DamageReduction, 40);
                player.EnableEffect(EffectType.MovementBoost, 5);
                player.ChangeAppearance(RoleTypeId.Tutorial);
                player.ShowHint($"<color=red><b>完全覚醒</b></color>状態へと移行しました！\n現在精神は完全に支配されており、もはや受け入れるしかないでしょう！\nアビリティ「触手」が利用可能になりました！");
                return true;
        }

        return false;
    }

    private IEnumerator<float> Coroutine(Player player)
    {
        while (true)
        {
            if (player == null || player.GetCustomRole() != CRoleTypeId.Scp035 || !Round.InProgress)
            {
                Cleanup(player);
                yield break;
            }
            var state = GetState(player);
            var value = (int)state.ChangeStateTimeAwaiting;
            if (value < 0f)
                value = -1;
            RoleSpecificTextProvider.Set(player,
                value > 0f
                    ? $"状態：{GetStateLoc(state.NowState)}\n変化まで：{(int)state.ChangeStateTimeAwaiting}"
                    : $"状態：{GetStateLoc(state.NowState)}\n変化まで：<color=red><b>抵抗不可能</b></color>");
            state.ChangeStateTimeAwaiting -= 0.1f;
            if (state.ChangeStateTimeAwaiting <= 0)
            {
                state.NowState = state.NowState switch
                {
                    Scp035StateType.Stable => Scp035StateType.Unstable,
                    Scp035StateType.Unstable => Scp035StateType.Awaken,
                    Scp035StateType.Awaken => Scp035StateType.Stable,
                    _ => state.NowState
                };
                Trigger(player, state.NowState);
                if (state.NowState != Scp035StateType.FullyAwaken)
                {
                    state.ChangeStateTimeAwaiting = 180f;
                }
            }

            if (!player.TryGetEffect(EffectType.Poisoned, out _) && state.NowState == Scp035StateType.Unstable)
                player.EnableEffect(EffectType.Poisoned, 10);
            
            TryChangeState(player, state);
            yield return Timing.WaitForSeconds(0.1f);
        }
    }
}