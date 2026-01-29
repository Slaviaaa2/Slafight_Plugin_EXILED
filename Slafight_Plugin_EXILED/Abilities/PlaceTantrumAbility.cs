using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Hazards;
using Exiled.API.Features.Roles;
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
using Object = System.Object;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.Abilities;

public class PlaceTantrumAbility : AbilityBase
{
    // AbilityBase の抽象プロパティを実装
    protected override float DefaultCooldown => 80f;
    protected override int DefaultMaxUses => -1;

    // 完全デフォルト
    public PlaceTantrumAbility(Player owner)
        : base(owner) { }

    // コマンドなどから上書きしたいとき用
    public PlaceTantrumAbility(Player owner, float cooldownSeconds)
        : base(owner, cooldownSeconds) { }

    public PlaceTantrumAbility(Player owner, float cooldownSeconds, int maxUses)
        : base(owner, cooldownSeconds, maxUses) { }

    protected override void ExecuteAbility(Player player)
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                TantrumHazard.PlaceTantrum(player.GetRandomSquarePosition(5f));
            }
            catch (Exception e)
            {
                Log.Error(e);
                return;
            }
        }
    }
}