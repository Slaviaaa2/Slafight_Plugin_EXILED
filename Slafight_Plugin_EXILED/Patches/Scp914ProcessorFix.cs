using System;
using Exiled.API.Features;
using InventorySystem;
using Scp914;
using Scp914.Processors;
using ServerHandlers = Exiled.Events.Handlers.Server;

namespace Slafight_Plugin_EXILED.Patches;

/// <summary>
/// ExMod-Team/EXILED#718 対策。
/// BaseGame の <see cref="Scp914Upgrader"/> は ProcessPickup / ProcessPlayer の冒頭で
/// <see cref="Scp914ItemProcessor"/> を持たない ItemType を早期 return / continue で握り潰す。
/// Exiled の Scp914.UpgradingPickup / UpgradingInventoryItem は transpiler で
/// その先の Stloc に差し込まれているため、processor が無いと Exiled イベントすら発火しない。
/// 結果 CustomKeycard 等の prefab に processor を付け忘れているアイテムは
/// 914 を完全スルーする不具合となる。
///
/// 対策として <see cref="InventoryItemLoader.AvailableItems"/> を走査し、
/// processor が無い ItemBase prefab に「同じ ItemType を返すだけ」の
/// passthrough <see cref="StandardItemProcessor"/> を <c>AddComponent</c> する。
/// BaseGame 内部の TryGetProcessor が成功するようになるため、
/// processor 未登録アイテムでも Exiled の 914 イベント発火経路に乗る。
/// </summary>
public static class Scp914ProcessorFix
{
    private static bool _subscribed;

    public static void Register()
    {
        if (_subscribed) return;
        ServerHandlers.WaitingForPlayers += Apply;
        _subscribed = true;

        // WaitingForPlayers を待たずにすぐ一度走らせておく
        // (プラグインを round 中にリロードした場合のため)
        Apply();
    }

    public static void Unregister()
    {
        if (!_subscribed) return;
        ServerHandlers.WaitingForPlayers -= Apply;
        _subscribed = false;
    }

    private static void Apply()
    {
        if (InventoryItemLoader.AvailableItems == null) return;

        int added = 0;
        foreach (var kv in InventoryItemLoader.AvailableItems)
        {
            var itemType = kv.Key;
            var itemBase = kv.Value;
            if (itemBase == null) continue;
            if (itemBase.TryGetComponent<Scp914ItemProcessor>(out _)) continue;

            try
            {
                var processor = itemBase.gameObject.AddComponent<StandardItemProcessor>();
                var arr = new[] { itemType };
                processor._roughOutputs = arr;
                processor._coarseOutputs = arr;
                processor._oneToOneOutputs = arr;
                processor._fineOutputs = arr;
                processor._veryFineOutputs = arr;
                processor._fireUpgradeTrigger = false;

                added++;
                Log.Debug($"[Scp914ProcessorFix] injected passthrough processor: {itemType}");
            }
            catch (Exception ex)
            {
                Log.Warn($"[Scp914ProcessorFix] failed to inject for {itemType}: {ex}");
            }
        }

        if (added > 0)
            Log.Info($"[Scp914ProcessorFix] injected {added} passthrough Scp914ItemProcessors for missing ItemTypes");
    }
}
