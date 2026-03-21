using Exiled.API.Enums;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

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
        foreach (Player targetPlayer in Player.List)
        {
            if (targetPlayer == null) continue;
            if (targetPlayer != player)
            {
                if (SpecificFlagsManager.HasFlag(targetPlayer, SpecificFlagType.AntiMemeEffectDisabled)) continue;
                if (Vector3.Distance(targetPlayer.Position, player.Position) <= 5f)
                {
                    if (targetPlayer.GetTeam() != CTeam.Fifthists)
                    {
                        targetPlayer.Explode(ProjectileType.Flashbang,player);
                    }
                    else
                    {
                        targetPlayer.EnableEffect(EffectType.Invigorated, 5f);
                    }
                }
            }
        }
    }
}