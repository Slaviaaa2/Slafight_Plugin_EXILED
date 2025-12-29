using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Hazards;
using Exiled.Events.EventArgs.Player;
using Hazards;
using MEC;
using Mirror;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MapExtensions;
using Slafight_Plugin_EXILED.ProximityChat;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.Abilities;

public class AllowEscapeAbility : AbilityBase
{
    // AbilityBase の抽象プロパティを実装
    protected override float DefaultCooldown => 999f;
    protected override int DefaultMaxUses => 1;

    // 完全デフォルト
    public AllowEscapeAbility(Player owner)
        : base(owner) { }

    // コマンドなどから上書きしたいとき用
    public AllowEscapeAbility(Player owner, float cooldownSeconds)
        : base(owner, cooldownSeconds) { }

    public AllowEscapeAbility(Player owner, float cooldownSeconds, int maxUses)
        : base(owner, cooldownSeconds, maxUses) { }

    protected override void ExecuteAbility(Player player)
    {
        foreach (var escapePlayer in PDEx.PDExPlayers.ToList())
        {
            if (escapePlayer == null) continue;
            escapePlayer.Position = Room.Random().WorldPosition(new Vector3(0f,0.25f,0f));
            escapePlayer.DisableEffect(EffectType.Slowness);
            escapePlayer.DisableEffect(EffectType.PocketCorroding);
            escapePlayer.EnableEffect(EffectType.Traumatized);
            PDEx.PDExPlayers.Remove(escapePlayer);
        }

        foreach (var kings in Player.List)
        {
            if (kings == null) continue;
            if (kings.GetCustomRole() == CRoleTypeId.Scp106 || (kings.GetCustomRole() == CRoleTypeId.None && kings.Role.Type == RoleTypeId.Scp106))
            {
                kings.RemoveAbility<AllowEscapeAbility>();
                Handler.CanUsePlayers.Remove(kings);
                Handler.ActivatedPlayers.Remove(kings);
                kings.Position = Room.Get(RoomType.Hcz106).WorldPosition(new Vector3(0f,0.25f,0f));
            }
        }
    }
}