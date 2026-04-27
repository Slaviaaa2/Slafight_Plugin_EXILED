using Exiled.API.Enums;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class GunGoCRailgunFull : CItemWeapon
{
    public override string DisplayName => "GoCレールガン(正式)";
    public override string Description => "にゃー";

    protected override string UniqueKey => "GunGoCRailgunFull";
    protected override ItemType BaseItem => ItemType.ParticleDisruptor;

    protected override byte    MagazineSize => 70;
    protected override Vector3 Scale        => new(1.15f, 1f, 1.15f);

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => CustomColor.Gold.ToUnityColor();

    private bool _isProcessing;

    /// <summary>命中: 即時 5000 ダメージの Explosion 再ヒット。再帰防止フラグ付き。</summary>
    protected override void OnHurtingOthers(HurtingEventArgs ev)
    {
        if (_isProcessing) return;
        if (ev.Attacker is null) return;

        _isProcessing = true;
        try
        {
            ev.Amount = 0f;
            ev.Player?.ExplodeEffect(ProjectileType.FragGrenade);
            ev.Player?.Hurt(ev.Attacker, 5000f, DamageType.Explosion);
            ev.Attacker?.ShowHitMarker();
        }
        finally
        {
            _isProcessing = false;
        }
    }

    /// <summary>床から拾った瞬間に MaxAmmo=70 / Ammo=70 を強制。</summary>
    protected override void OnPickingUp(PickingUpItemEventArgs ev)
    {
        if (ev.Pickup is FirearmPickup pickup)
        {
            pickup.MaxAmmo = 70;
            pickup.Ammo    = 70;
        }
    }
}
