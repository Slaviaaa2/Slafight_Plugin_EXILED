using Exiled.API.Enums;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;
using Firearm = Exiled.API.Features.Items.Firearm;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class GunGoCTurret : CItemWeapon
{
    public override string DisplayName => "GoC Turret";
    public override string Description => "テストアイテム。";

    protected override string UniqueKey => "GunGoCTurret";
    protected override ItemType BaseItem => ItemType.MicroHID;

    protected override byte    MagazineSize => 1;
    protected override Vector3 Scale        => new(1.15f, 1f, 1.15f);

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => CustomColor.Gold.ToUnityColor();

    private bool _isProcessing;

    /// <summary>命中: 即時 2000 ダメージの Explosion 再ヒット。再帰防止フラグ付き。</summary>
    protected override void OnHurtingOthers(HurtingEventArgs ev)
    {
        if (_isProcessing) return;
        if (ev.Attacker is null) return;

        _isProcessing = true;
        try
        {
            ev.Amount = 0f;
            ev.Player?.ExplodeEffect(ProjectileType.FragGrenade);
            ev.Player?.Hurt(ev.Attacker, 2000f, DamageType.Explosion);
            ev.Attacker?.ShowHitMarker();
        }
        finally
        {
            _isProcessing = false;
        }
    }

    /// <summary>FirearmPickup なら MaxAmmo=1/Ammo=1 を強制 (MicroHID は no-op)。</summary>
    protected override void OnPickingUp(PickingUpItemEventArgs ev)
    {
        if (ev.Pickup is FirearmPickup pickup)
        {
            pickup.MaxAmmo = 1;
            pickup.Ammo    = 1;
        }
    }

    /// <summary>1 発未使用で無いまま手放したら破棄 (MicroHID は no-op)。</summary>
    protected override void OnDropping(DroppingItemEventArgs ev)
    {
        if (ev.Item is Firearm firearm && firearm.TotalAmmo != 1)
            firearm.Destroy();
    }
}
