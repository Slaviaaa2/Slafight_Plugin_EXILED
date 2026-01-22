using System;
using System.Collections.Generic;
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
using UnityEngine;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED.Abilities;

public class SoundOfFifthAbility : AbilityBase
{
    // AbilityBase の抽象プロパティを実装
    protected override float DefaultCooldown => 20f;
    protected override int DefaultMaxUses => -1;

    // 完全デフォルト
    public SoundOfFifthAbility(Player owner)
        : base(owner) { }

    // コマンドなどから上書きしたいとき用
    public SoundOfFifthAbility(Player owner, float cooldownSeconds)
        : base(owner, cooldownSeconds) { }

    public SoundOfFifthAbility(Player owner, float cooldownSeconds, int maxUses)
        : base(owner, cooldownSeconds, maxUses) { }

    protected override void ExecuteAbility(Player player)
    {
        foreach (Player _player in Player.List)
        {
            if (_player == null || player == null) continue;
            if (_player != player)
            {
                if (Vector3.Distance(_player.Position, player.Position) <= 5f)
                {
                    if (_player.GetTeam() != CTeam.Fifthists)
                    {
                        _player.Explode(ProjectileType.Flashbang,player);
                    }
                    else
                    {
                        _player.EnableEffect(EffectType.Invigorated, 5f);
                    }
                }
            }
        }
    }
}