using Exiled.API.Enums;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;
using Firearm = Exiled.API.Features.Items.Firearm;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class GunGoCRailgun : CItemWeapon
{
    public override string DisplayName => "GoCレールガン(実験式)";
    public override string Description =>
        "GoCのホワイトスーツに搭載される予定の主砲を財団との協定の一環として歩兵用に改造した物。\n" +
        "<color=red>一発のみ撃てる。最大6000ダメの即死級武器</color>";

    protected override string UniqueKey => "GunGoCRailgun";
    protected override ItemType BaseItem => ItemType.ParticleDisruptor;

    protected override byte    MagazineSize => 1;
    protected override Vector3 Scale        => new(1.15f, 1f, 1.15f);

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => CustomColor.Gold.ToUnityColor();

    private bool _isProcessing;

    /// <summary>
    /// 命中: 即時 2000 ダメージの Explosion を再ヒット。
    /// CItem.OnAnyHurting を経由した Hurt 呼び出しで再帰してしまうため処理中フラグでガード。
    /// </summary>
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

    /// <summary>床から拾った瞬間に MaxAmmo=1 / Ammo=1 を強制。</summary>
    protected override void OnPickingUp(PickingUpItemEventArgs ev)
    {
        if (ev.Pickup is FirearmPickup pickup)
        {
            pickup.MaxAmmo = 1;
            pickup.Ammo    = 1;
        }
    }

    /// <summary>未使用 (1 発残り) で無いまま手放したら破棄。</summary>
    protected override void OnDropping(DroppingItemEventArgs ev)
    {
        if (ev.Item is Firearm firearm && firearm.TotalAmmo != 1)
            firearm.Destroy();
    }
}
