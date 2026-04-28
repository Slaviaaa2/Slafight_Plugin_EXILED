using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using MapHandlers = Exiled.Events.Handlers.Map;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class NeutralizeGrenade : CItem
{
    public override string DisplayName => "対反ミーム無力化グレネード";
    public override string Description => "反ミーム存在及びその影響を受けた者を一時的に無力化し、ダメージを与える。";

    protected override string UniqueKey => "NeutralizeGrenade";
    protected override ItemType BaseItem => ItemType.GrenadeHE;

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => CustomColor.Purple.ToUnityColor();

    public override void RegisterEvents()
    {
        MapHandlers.ExplodingGrenade += OnExploding;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        MapHandlers.ExplodingGrenade -= OnExploding;
        base.UnregisterEvents();
    }

    protected override void OnThrowingRequest(ThrowingRequestEventArgs ev)
    {
        if (ev.Item is ExplosiveGrenade eg)
            eg.FuseTime = 0.5f;
    }

    protected override void OnPickingUp(PickingUpItemEventArgs ev)
    {
        ev.Player.SetCategoryLimit(ItemCategory.Grenade,
            (sbyte)(ev.Player.GetCategoryLimit(ItemCategory.Grenade) + 1));
    }

    protected override void OnDropping(DroppingItemEventArgs ev)
    {
        ev.Player.SetCategoryLimit(ItemCategory.Grenade,
            (sbyte)(ev.Player.GetCategoryLimit(ItemCategory.Grenade) - 1));
    }

    private void OnExploding(ExplodingGrenadeEventArgs ev)
    {
        if (!Check(ev.Projectile)) return;
        var fifthists = ev.TargetsToAffect.Where(player => player.IsFifthist()).ToList();
        ev.TargetsToAffect.Clear();
        foreach (var player in fifthists)
        {
            player.EnableEffect(EffectType.SinkHole, 55, 20f);
            if (player.GetTeam() == CTeam.Fifthists)
                player.Hurt(25f, DamageType.Explosion);
            else
                player.Hurt(5000f, DamageType.Explosion);
            ev.Player?.ShowHitMarker();
        }
    }
}
