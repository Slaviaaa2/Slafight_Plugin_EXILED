using System;
using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Scp914;
using MEC;
using PlayerRoles;
using Scp914;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;
using Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.ProximityChat;
using UnityEngine;
using Random = System.Random;

namespace Slafight_Plugin_EXILED.Changes;

public static class Scp914Changes
{
    public static void Register()
    {
        Exiled.Events.Handlers.Scp914.UpgradingPickup += Normal;
        Exiled.Events.Handlers.Scp914.UpgradingPlayer += Human;
    }

    public static void Unregister()
    {
        Exiled.Events.Handlers.Scp914.UpgradingPickup -= Normal;
        Exiled.Events.Handlers.Scp914.UpgradingPlayer -= Human;
    }

    private static readonly Random Random = new();

    private static void Normal(UpgradingPickupEventArgs ev)
    {
        if (ev.Pickup == null) return;
        if (ev.Pickup.TryGetCustomItem(out var customItem))
        {
            // CUSTOM ITEM UPGRADES
        }
        else
        {
            // VANILLA ITEM UPGRADES
            switch (ev.Pickup.Type)
            {
                case ItemType.Adrenaline:
                    CustomItemExtensions.TrySpawn<SerumD>(ev.OutputPosition, out _);
                    ev.IsAllowed = false;
                    ev.Pickup.Destroy();
                    break;
                case ItemType.SCP500:
                    CustomItemExtensions.TrySpawn<ClassXMemoryForcePil>(ev.OutputPosition, out _);
                    ev.IsAllowed = false;
                    ev.Pickup.Destroy();
                    break;
                case ItemType.None:
                    break;
                case ItemType.KeycardJanitor:
                    if (Random.Next(0, 6) is 0)
                    {
                        CustomItemExtensions.TrySpawn<MasterCard>(ev.OutputPosition, out _);
                        ev.IsAllowed = false;
                        ev.Pickup.Destroy();
                    }
                    break;
                case ItemType.KeycardScientist:
                    if (Random.Next(0, 5) is 0)
                    {
                        CustomItemExtensions.TrySpawn<MasterCard>(ev.OutputPosition, out _);
                        ev.IsAllowed = false;
                        ev.Pickup.Destroy();
                    }
                    break;
                case ItemType.KeycardResearchCoordinator:
                    if (Random.Next(0, 4) is 0)
                    {
                        CustomItemExtensions.TrySpawn<MasterCard>(ev.OutputPosition, out _);
                        ev.IsAllowed = false;
                        ev.Pickup.Destroy();
                    }
                    break;
                case ItemType.KeycardZoneManager:
                    if (Random.Next(0, 4) is 0)
                    {
                        CustomItemExtensions.TrySpawn<MasterCard>(ev.OutputPosition, out _);
                        ev.IsAllowed = false;
                        ev.Pickup.Destroy();
                    }
                    break;
                case ItemType.KeycardGuard:
                    if (Random.Next(0, 4) is 0)
                    {
                        CustomItemExtensions.TrySpawn<MasterCard>(ev.OutputPosition, out _);
                        ev.IsAllowed = false;
                        ev.Pickup.Destroy();
                    }
                    break;
                case ItemType.KeycardMTFPrivate:
                    if (Random.Next(0, 3) is 0)
                    {
                        CustomItemExtensions.TrySpawn<MasterCard>(ev.OutputPosition, out _);
                        ev.IsAllowed = false;
                        ev.Pickup.Destroy();
                    }
                    break;
                case ItemType.KeycardContainmentEngineer:
                    if (Random.Next(0, 3) is 0)
                    {
                        CustomItemExtensions.TrySpawn<MasterCard>(ev.OutputPosition, out _);
                        ev.IsAllowed = false;
                        ev.Pickup.Destroy();
                    }
                    break;
                case ItemType.KeycardMTFOperative:
                    if (Random.Next(0, 2) is 0)
                    {
                        CustomItemExtensions.TrySpawn<MasterCard>(ev.OutputPosition, out _);
                        ev.IsAllowed = false;
                        ev.Pickup.Destroy();
                    }
                    break;
                case ItemType.KeycardMTFCaptain:
                    if (Random.Next(0, 2) is 0)
                    {
                        CustomItemExtensions.TrySpawn<MasterCard>(ev.OutputPosition, out _);
                        ev.IsAllowed = false;
                        ev.Pickup.Destroy();
                    }
                    break;
                case ItemType.KeycardFacilityManager:
                    if (Random.Next(0, 2) is 0)
                    {
                        CustomItemExtensions.TrySpawn<MasterCard>(ev.OutputPosition, out _);
                        ev.IsAllowed = false;
                        ev.Pickup.Destroy();
                    }
                    break;
                case ItemType.KeycardChaosInsurgency:
                    if (Random.Next(0, 2) is 0)
                    {
                        CustomItemExtensions.TrySpawn<MasterCard>(ev.OutputPosition, out _);
                        ev.IsAllowed = false;
                        ev.Pickup.Destroy();
                    }
                    break;
                case ItemType.KeycardO5:
                    break;
                case ItemType.Radio:
                    break;
                case ItemType.GunCOM15:
                    break;
                case ItemType.Medkit:
                    break;
                case ItemType.Flashlight:
                    break;
                case ItemType.MicroHID:
                    break;
                case ItemType.SCP207:
                    break;
                case ItemType.Ammo12gauge:
                    break;
                case ItemType.GunE11SR:
                    break;
                case ItemType.GunCrossvec:
                    break;
                case ItemType.Ammo556x45:
                    break;
                case ItemType.GunFSP9:
                    break;
                case ItemType.GunLogicer:
                    break;
                case ItemType.GrenadeHE:
                    break;
                case ItemType.GrenadeFlash:
                    break;
                case ItemType.Ammo44cal:
                    break;
                case ItemType.Ammo762x39:
                    break;
                case ItemType.Ammo9x19:
                    break;
                case ItemType.GunCOM18:
                    break;
                case ItemType.SCP018:
                    break;
                case ItemType.SCP268:
                    break;
                case ItemType.Painkillers:
                    break;
                case ItemType.Coin:
                    break;
                case ItemType.ArmorLight:
                    break;
                case ItemType.ArmorCombat:
                    break;
                case ItemType.ArmorHeavy:
                    break;
                case ItemType.GunRevolver:
                    break;
                case ItemType.GunAK:
                    break;
                case ItemType.GunShotgun:
                    break;
                case ItemType.SCP330:
                    break;
                case ItemType.SCP2176:
                    break;
                case ItemType.SCP244a:
                    CItem.Get<ThrowableScp244>()?.Spawn(ev.OutputPosition);
                    ev.IsAllowed = false;
                    ev.Pickup.Destroy();
                    break;
                case ItemType.SCP244b:
                    CItem.Get<ThrowableScp244>()?.Spawn(ev.OutputPosition);
                    ev.IsAllowed = false;
                    ev.Pickup.Destroy();
                    break;
                case ItemType.SCP1853:
                    break;
                case ItemType.ParticleDisruptor:
                    break;
                case ItemType.GunCom45:
                    break;
                case ItemType.SCP1576:
                    break;
                case ItemType.Jailbird:
                    break;
                case ItemType.AntiSCP207:
                    break;
                case ItemType.GunFRMG0:
                    break;
                case ItemType.GunA7:
                    break;
                case ItemType.Lantern:
                    break;
                case ItemType.SCP1344:
                    break;
                case ItemType.Snowball:
                    break;
                case ItemType.Coal:
                    break;
                case ItemType.SpecialCoal:
                    break;
                case ItemType.SCP1507Tape:
                    break;
                case ItemType.DebugRagdollMover:
                    break;
                case ItemType.SurfaceAccessPass:
                    break;
                case ItemType.GunSCP127:
                    break;
                case ItemType.KeycardCustomTaskForce:
                    break;
                case ItemType.KeycardCustomSite02:
                    break;
                case ItemType.KeycardCustomManagement:
                    break;
                case ItemType.KeycardCustomMetalCase:
                    break;
                case ItemType.MarshmallowItem:
                    break;
                case ItemType.SCP1509:
                    break;
                case ItemType.Scp021J:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    private static void Human(UpgradingPlayerEventArgs ev)
    {
        if (ev.KnobSetting == Scp914KnobSetting.VeryFine)
        {
            var value = Random.Next(0, 4);
            if (value == 0)
            {
                ev.Player?.Role.Set(RoleTypeId.Scp0492,RoleSpawnFlags.None);
                Timing.CallDelayed(1f, () =>
                {
                    ev.Player?.EnableEffect(EffectType.Scp207, 4);
                    ev.Player?.UniqueRole = "Zombified";
                    ev.Player?.SetCustomInfo("<color=#C50000>Zombified Subject</color>");
                    ev.Player?.SetScale(new Vector3((UnityEngine.Random.Range(0.01f, 1.08f)),
                        (UnityEngine.Random.Range(0.01f, 1.08f)), (UnityEngine.Random.Range(0.01f, 1.08f))));
                    if (ev.Player != null && !Handler.CanUsePlayers.Contains(ev.Player))
                    {
                        Handler.CanUsePlayers.Add(ev.Player);
                    }

                    if (ev.Player != null && !Handler.ActivatedPlayers.Contains(ev.Player))
                    {
                        Handler.ActivatedPlayers.Add(ev.Player);
                    }

                    ev.Player?.ShowHint("<size=24>体が魔改造されていく・・・！</size>");
                });
            }
        }
    }
}