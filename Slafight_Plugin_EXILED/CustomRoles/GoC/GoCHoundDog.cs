using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.CustomStats;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Scp1344 = CustomPlayerEffects.Scp1344;

namespace Slafight_Plugin_EXILED.CustomRoles.GoC;

public class GoCHoundDog : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.GoCHoundDog;
    protected override CTeam Team { get; set; } = CTeam.GoC;
    protected override string UniqueRoleKey { get; set; } = "GoCHoundDog";

    public override void SpawnRole(Player? player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player!.Role.Set(RoleTypeId.NtfSpecialist);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 120;
        player.Health = player.MaxHealth;
        player.CustomHumeShieldStat.MaxValue = 200;
        player.CustomHumeShieldStat.CurValue = player.CustomHumeShieldStat.MaxValue;
        player.ClearInventory();
        player.TryAddCustomItem<ArmorVip>();
        player.TryAddCustomItem<GunSuperLogicer>();
        player.TryAddCustomItem<GunGoCRailgunFull>();
        player.TryAddCustomItem<CloakGenerator>();
        player.AddItem(ItemType.Adrenaline);
        player.AddItem(ItemType.Medkit);
        player.IsBypassModeEnabled = true;
            
        player.AddAmmo(AmmoType.Nato762,140);

        player.SetCustomInfo("Global Occult Collision: Hound Dog Mark II Combat Garment White Suit");
        Timing.RunCoroutine(Coroutine(player));
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<size=20><color=#0000c8>GoC: Hound Dog マークⅡ戦闘強化服Combat Garment(ホワイト・スーツ)</color>\nあーうーえーうーあーあー\nPassive: ホワイトスーツ\nホワイトスーツの超駆動により常時コーラ一本分の速度を提供する。\nPassive: VERITAS\n遠くにいる敵等を認識できる\nPassive: 自爆装置\n死亡、拘束され際に起動し、グレネード一個分の自爆を引き起こす。\nキーカード？知りませんそんなもの。顔パスって言葉がありましてね...",10f);
        });
    }

    protected override void OnDying(DyingEventArgs ev)
    {
        var grenade = Pickup.CreateAndSpawn(ItemType.GrenadeHE, ev.Player.Position, Quaternion.identity);
        if (grenade is GrenadePickup grenadeBase)
        {
            grenadeBase.FuseTime = 0.01f;
        }
        
        base.OnDying(ev);
    }

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.Handcuffing += OnCuffering;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.Handcuffing -= OnCuffering;
        base.UnregisterEvents();
    }

    private void OnCuffering(HandcuffingEventArgs ev)
    {
        if (!Check(ev.Target)) return;
        ev.Target.Kill("自爆機能による力");
        var grenade = Pickup.CreateAndSpawn(ItemType.GrenadeFlash, ev.Player.Position, Quaternion.identity);
        if (grenade is FlashbangProjectile grenadeBase)
        {
            grenadeBase.AdditionalBlindedEffect = 5.5f;
            grenadeBase.FuseTime = 0.01f;
        }
    }

    private IEnumerator<float> Coroutine(Player player)
    {
        while (true)
        {
            if (!Check(player)) yield break;
            if (!player.IsEffectActive<MovementBoost>())
            {
                player.EnableEffect(EffectType.MovementBoost, 25);
            }
            if (!player.IsEffectActive<Scp1344>())
            {
                player.EnableEffect(EffectType.Scp1344, 1);
            }
            yield return Timing.WaitForSeconds(3f);
        }
    }
}