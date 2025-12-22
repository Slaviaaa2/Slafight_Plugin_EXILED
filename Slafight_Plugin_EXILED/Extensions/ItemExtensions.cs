using System.Collections.Generic;
using Exiled.API.Features.Items;
using Exiled.CustomItems.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Extensions;

public static class ItemExtensions
{
    public static bool IsUpgradable(this Item item)
    {
        List<ItemType> allowed = new List<ItemType>()
        {
            ItemType.KeycardJanitor,
            ItemType.KeycardScientist,
            ItemType.KeycardResearchCoordinator,
            ItemType.KeycardContainmentEngineer,
            ItemType.KeycardZoneManager,
            ItemType.KeycardFacilityManager,
            ItemType.KeycardGuard,
            ItemType.KeycardChaosInsurgency,
            ItemType.KeycardMTFPrivate,
            ItemType.KeycardMTFOperative,
            ItemType.KeycardMTFCaptain
        };
        List<uint> allowedCustoms = new List<uint>()
        {
            1100,
            1101
        };
        if (CustomItem.TryGet(item, out var customItem))
        {
            if (allowedCustoms.Contains(customItem.Id))
            {
                return true;
            }
            else if (allowed.Contains(item.Type))
            {
                return true;
            }
        }
        else if (allowed.Contains(item.Type))
        {
            return true;
        }
        else
        {
            return false;
        }
        return false;
    }
    public static void UpgradeItem(this Item item)
    {
        if (!item.IsUpgradable()) return;
        var player = item.Owner;
        if (item.IsKeycard)
        {
            if (item.Type == ItemType.KeycardJanitor)
            {
                item.Destroy();
                var random = Random.Range(0, 4);
                if (random == 0)
                {
                    player.AddItem(ItemType.KeycardScientist);
                }
                else if (random == 1)
                {
                    player.AddItem(ItemType.KeycardZoneManager);
                }
                else if (random == 2)
                {
                    player.AddItem(ItemType.KeycardGuard);
                }
                else if (random == 3)
                {
                    CustomItem.TryGive(player, 1101);
                }
            }
            else if (item.Type == ItemType.KeycardGuard)
            {
                item.Destroy();
                CustomItem.TryGive(player, 1100);
            }
            else if (CustomItem.TryGet(item, out var customItem))
            {
                if (customItem.Id == 1100)
                {
                    item.Destroy();
                    player.AddItem(ItemType.KeycardMTFPrivate);
                }
                else if (customItem.Id == 1101)
                {
                    item.Destroy();
                    player.AddItem(ItemType.KeycardChaosInsurgency);
                }
            }
            else if (item.Type == ItemType.KeycardMTFPrivate)
            {
                item.Destroy();
                player.AddItem(ItemType.KeycardMTFOperative);
            }
            else if (item.Type == ItemType.KeycardMTFOperative)
            {
                item.Destroy();
                player.AddItem(ItemType.KeycardMTFCaptain);
            }
            else if (item.Type == ItemType.KeycardMTFCaptain)
            {
                item.Destroy();
                player.AddItem(ItemType.KeycardO5);
            }
            else if (item.Type == ItemType.KeycardScientist)
            {
                item.Destroy();
                player.AddItem(ItemType.KeycardResearchCoordinator);
            }
            else if (item.Type == ItemType.KeycardResearchCoordinator)
            {
                item.Destroy();
                player.AddItem(ItemType.KeycardContainmentEngineer);
            }
            else if (item.Type == ItemType.KeycardContainmentEngineer)
            {
                item.Destroy();
                player.AddItem(ItemType.KeycardFacilityManager);
            }
            else if (item.Type == ItemType.KeycardZoneManager)
            {
                item.Destroy();
                if (Random.Range(0, 2) == 0)
                {
                    player.AddItem(ItemType.KeycardResearchCoordinator);
                }
                else
                {
                    player.AddItem(ItemType.KeycardFacilityManager);
                }
            }
            else if (item.Type == ItemType.KeycardFacilityManager)
            {
                item.Destroy();
                player.AddItem(ItemType.KeycardO5);
            }
        }
    }
}