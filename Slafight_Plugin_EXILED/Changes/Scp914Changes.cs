using Exiled.API.Enums;
using Exiled.Events.EventArgs.Scp914;
using MEC;
using PlayerRoles;
using Scp914;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.API.Features.Scp914;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;
using Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.ProximityChat;
using UnityEngine;
using Random = System.Random;

namespace Slafight_Plugin_EXILED.Changes;

/// <summary>
/// SCP-914 のアップグレード挙動を**一元管理**するファイル。
/// vanilla ItemType / Exiled CustomItem / CItem 派生すべての変換ルールを
/// <see cref="RegisterRules"/> にまとめて宣言し、<see cref="Scp914Registry"/> へ流す。
/// 実行時は本ファイルの <see cref="OnUpgradingPickup"/> /
/// <see cref="OnUpgradingInventoryItem"/> が Registry を引いて
/// <see cref="Scp914Dispatcher"/> に委譲する。
/// </summary>
public static class Scp914Changes
{
    private static readonly Random Random = new();

    public static void Register()
    {
        RegisterRules();

        Exiled.Events.Handlers.Scp914.UpgradingPickup += OnUpgradingPickup;
        Exiled.Events.Handlers.Scp914.UpgradingInventoryItem += OnUpgradingInventoryItem;
        Exiled.Events.Handlers.Scp914.UpgradingPlayer += Human;
    }

    public static void Unregister()
    {
        Exiled.Events.Handlers.Scp914.UpgradingPickup -= OnUpgradingPickup;
        Exiled.Events.Handlers.Scp914.UpgradingInventoryItem -= OnUpgradingInventoryItem;
        Exiled.Events.Handlers.Scp914.UpgradingPlayer -= Human;

        Scp914Registry.Clear();
    }

    // =====================================================================
    // 全ルール定義 — Vanilla / CustomItem / CItem を横断してここだけ見れば
    // 各アイテムがどう変換されるか分かる。
    // =====================================================================

    private static void RegisterRules()
    {
        RegisterWildcard();
        RegisterVanillaRules();
        RegisterCustomItemRules();
        RegisterCItemRules();
    }

    /// <summary>全アイテム共通の 1/6 ロール。Scp513 または CapybaraMissile に置き換え。</summary>
    private static void RegisterWildcard()
    {
        Scp914Registry.WildcardRule = Scp914Rule.Custom(ctx =>
        {
            var position = ctx.OutputPosition;
            if (UnityEngine.Random.Range(0, 10) == 0)
                CItem.Get<Scp513Item>()?.Spawn(position);
            else
                Scp914Dispatcher.TrySpawnCustomItem(typeof(CapybaraMissile), position);

            ctx.Pickup?.Destroy();
        }).WithChance(1f / 6f);
    }

    /// <summary>vanilla ItemType → Custom/CItem の変換ルール。</summary>
    private static void RegisterVanillaRules()
    {
        Scp914Registry.RegisterVanilla(ItemType.Adrenaline, new()
        {
            All = Scp914Rule.ToCustomItem<SerumD>(),
        });

        Scp914Registry.RegisterVanilla(ItemType.SCP500, new()
        {
            All = Scp914Rule.ToCustomItem<ClassXMemoryForcePil>(),
        });

        Scp914Registry.RegisterVanilla(ItemType.KeycardJanitor, new()
        {
            All = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 6f),
        });
        Scp914Registry.RegisterVanilla(ItemType.KeycardScientist, new()
        {
            All = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 5f),
        });
        Scp914Registry.RegisterVanilla(ItemType.KeycardResearchCoordinator, new()
        {
            All = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 4f),
        });
        Scp914Registry.RegisterVanilla(ItemType.KeycardZoneManager, new()
        {
            All = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 4f),
        });
        Scp914Registry.RegisterVanilla(ItemType.KeycardGuard, new()
        {
            All = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 4f),
        });
        Scp914Registry.RegisterVanilla(ItemType.KeycardMTFPrivate, new()
        {
            All = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 3f),
        });
        Scp914Registry.RegisterVanilla(ItemType.KeycardContainmentEngineer, new()
        {
            OneToOne = Scp914Rule.ToCItem<Toolbox>(),
            Rough    = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 3f),
            Coarse   = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 3f),
            Fine     = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 3f),
            VeryFine = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 3f),
        });
        Scp914Registry.RegisterVanilla(ItemType.KeycardMTFOperative, new()
        {
            All = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 2f),
        });
        Scp914Registry.RegisterVanilla(ItemType.KeycardMTFCaptain, new()
        {
            All = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 2f),
        });
        Scp914Registry.RegisterVanilla(ItemType.KeycardFacilityManager, new()
        {
            All = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 2f),
        });
        Scp914Registry.RegisterVanilla(ItemType.KeycardChaosInsurgency, new()
        {
            Coarse   = Scp914Rule.ToCustomItem<KeycardConscripts>().WithChance(1f / 4f),
            Rough    = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 2f),
            OneToOne = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 2f),
            Fine     = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 2f),
            VeryFine = Scp914Rule.ToCustomItem<MasterCard>().WithChance(1f / 2f),
        });
        Scp914Registry.RegisterVanilla(ItemType.KeycardO5, new()
        {
            Fine     = Scp914Rule.ToCustomItem<OmegaWarheadAccess>().WithChance(1f / 2f),
            VeryFine = Scp914Rule.ToCustomItem<OmegaWarheadAccess>().WithChance(1f / 2f),
        });

        Scp914Registry.RegisterVanilla(ItemType.Radio, new()
        {
            VeryFine = Scp914Rule.ToCustomItem<SNAV300>(),
        });
        Scp914Registry.RegisterVanilla(ItemType.MicroHID, new()
        {
            Coarse = Scp914Rule.ToCustomItem<HIDTurret>().WithChance(1f / 2f),
        });
        Scp914Registry.RegisterVanilla(ItemType.GrenadeFlash, new()
        {
            Fine = Scp914Rule.ToCustomItem<FlashBangE>().WithChance(1f / 3f),
        });
        Scp914Registry.RegisterVanilla(ItemType.SCP268, new()
        {
            VeryFine = Scp914Rule.ToCustomItem<CloakGenerator>().WithChance(1f / 4f),
        });
        Scp914Registry.RegisterVanilla(ItemType.Coin, new()
        {
            Coarse = Scp914Rule.ToCustomItem<Quarter>(),
        });
        Scp914Registry.RegisterVanilla(ItemType.GunRevolver, new()
        {
            Fine = Scp914Rule.ToCustomItem<GunTacticalRevolver>().WithChance(1f / 2f),
        });
        Scp914Registry.RegisterVanilla(ItemType.SCP244a, new()
        {
            Fine     = Scp914Rule.ToCItem<ThrowableScp244>(),
            VeryFine = Scp914Rule.ToCItem<ThrowableScp244>(),
        });
        Scp914Registry.RegisterVanilla(ItemType.SCP244b, new()
        {
            Fine     = Scp914Rule.ToCItem<ThrowableScp244>(),
            VeryFine = Scp914Rule.ToCItem<ThrowableScp244>(),
        });
        Scp914Registry.RegisterVanilla(ItemType.SCP1344, new()
        {
            Coarse = Scp914Rule.ToCustomItem<NvgBlue>(),
        });
    }

    /// <summary>Exiled CustomItem → Custom/CItem の変換ルール。</summary>
    private static void RegisterCustomItemRules()
    {
        Scp914Registry.RegisterCustomItem<KeycardFifthist>(new()
        {
            Coarse   = Scp914Rule.ToCustomItem<Scp1425>(),
            Fine     = Scp914Rule.ToCustomItem<KeycardFifthistPriest>(),
            VeryFine = Scp914Rule.ToCustomItem<MagicMissile>().WithChance(1f / 3f),
        });

        Scp914Registry.RegisterCustomItem<KeycardFifthistPriest>(new()
        {
            Coarse   = Scp914Rule.ToCustomItem<KeycardFifthist>(),
            Fine     = Scp914Rule.ToCustomItem<MagicMissile>(),
            VeryFine = Scp914Rule.ToCustomItem<CaneOfTheStars>(),
        });

        Scp914Registry.RegisterCustomItem<Scp1425>(new()
        {
            OneToOne = Scp914Rule.ToCustomItem<GoCRecruitPaper>(),
        });

        Scp914Registry.RegisterCustomItem<GoCRecruitPaper>(new()
        {
            OneToOne = Scp914Rule.ToCustomItem<Scp1425>(),
        });
    }

    /// <summary>CItem → Vanilla/Custom/CItem の変換ルール。</summary>
    private static void RegisterCItemRules()
    {
        Scp914Registry.RegisterCItem<ThrowableScp244>(new()
        {
            Rough    = Scp914Rule.Destroy,
            Coarse   = Scp914Rule.ToVanilla(_ =>
                UnityEngine.Random.Range(0, 2) == 0 ? ItemType.SCP244a : ItemType.SCP244b),
            OneToOne = Scp914Rule.Keep,
            Fine     = Scp914Rule.Destroy,
            VeryFine = Scp914Rule.Destroy,
        });
    }

    // =====================================================================
    // ディスパッチ
    // =====================================================================

    /// <summary>
    /// 優先順位:
    /// 1. Wildcard (1/6) — 当選で Scp513 / CapybaraMissile に置き換え、他の処理打ち切り
    /// 2. CustomItem (Registry ヒット) — Registry 経由で変換
    /// 3. CItem (Registry ヒット) — Registry 経由で変換
    /// 4. Vanilla (Registry ヒット) — Registry 経由で変換
    /// どれにも当たらない場合は何もしない (vanilla 914 / CItem デフォルト挙動に任せる)
    /// </summary>
    private static void OnUpgradingPickup(UpgradingPickupEventArgs ev)
    {
        if (ev?.Pickup == null) return;

        if (Scp914Registry.WildcardRule is { } wildcard
            && Scp914Dispatcher.ApplyPickup(wildcard, ev))
        {
            return;
        }

        if (ev.Pickup.TryGetCustomItem(out var customItem) && customItem != null)
        {
            if (Scp914Registry.TryGetForCustomItem(customItem, out var customRules)
                && customRules != null
                && customRules.Get(ev.KnobSetting) is { } customRule)
            {
                Scp914Dispatcher.ApplyPickup(customRule, ev);
            }
            return;
        }

        if (CItem.TryGet(ev.Pickup, out var cItem) && cItem != null)
        {
            if (Scp914Registry.TryGetForCItem(cItem, out var cItemRules)
                && cItemRules != null
                && cItemRules.Get(ev.KnobSetting) is { } cItemRule)
            {
                Scp914Dispatcher.ApplyPickup(cItemRule, ev);
            }
            return;
        }

        if (Scp914Registry.TryGetVanilla(ev.Pickup.Type, out var vanillaRules)
            && vanillaRules != null
            && vanillaRules.Get(ev.KnobSetting) is { } vanillaRule)
        {
            Scp914Dispatcher.ApplyPickup(vanillaRule, ev);
        }
    }

    /// <summary>
    /// インベントリアップグレード版。Wildcard は床専用につき無し。
    /// 出力は <see cref="Scp914Dispatcher.ApplyInventory"/> が AddOrDrop で捌く。
    /// </summary>
    private static void OnUpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
    {
        if (ev?.Item == null || ev.Player == null) return;

        if (ev.Item.TryGetCustomItem(out var customItem) && customItem != null)
        {
            if (Scp914Registry.TryGetForCustomItem(customItem, out var customRules)
                && customRules != null
                && customRules.Get(ev.KnobSetting) is { } customRule)
            {
                Scp914Dispatcher.ApplyInventory(customRule, ev);
            }
            return;
        }

        if (CItem.TryGet(ev.Item, out var cItem) && cItem != null)
        {
            if (Scp914Registry.TryGetForCItem(cItem, out var cItemRules)
                && cItemRules != null
                && cItemRules.Get(ev.KnobSetting) is { } cItemRule)
            {
                Scp914Dispatcher.ApplyInventory(cItemRule, ev);
            }
            return;
        }

        if (Scp914Registry.TryGetVanilla(ev.Item.Type, out var vanillaRules)
            && vanillaRules != null
            && vanillaRules.Get(ev.KnobSetting) is { } vanillaRule)
        {
            Scp914Dispatcher.ApplyInventory(vanillaRule, ev);
        }
    }

    // ==== プレイヤー本体 (VeryFine で稀にゾンビ化) ====

    private static void Human(UpgradingPlayerEventArgs ev)
    {
        if (ev.KnobSetting != Scp914KnobSetting.VeryFine) return;
        if (Random.Next(0, 4) != 0) return;

        ev.Player?.Role.Set(RoleTypeId.Scp0492, RoleSpawnFlags.None);
        Timing.CallDelayed(1f, () =>
        {
            ev.Player?.EnableEffect(EffectType.Scp207, 4);
            if (ev.Player != null)
            {
                ev.Player.UniqueRole = "Zombified";
                ev.Player.SetCustomInfo("<color=#C50000>Zombified Subject</color>");
                ev.Player.SetScale(new Vector3(
                    UnityEngine.Random.Range(0.01f, 1.08f),
                    UnityEngine.Random.Range(0.01f, 1.08f),
                    UnityEngine.Random.Range(0.01f, 1.08f)));
                if (!Handler.CanUsePlayers.Contains(ev.Player))
                    Handler.CanUsePlayers.Add(ev.Player);
                if (!Handler.ActivatedPlayers.Contains(ev.Player))
                    Handler.ActivatedPlayers.Add(ev.Player);
                ev.Player.ShowHint("<size=24>体が魔改造されていく・・・！</size>");
            }
        });
    }
}
