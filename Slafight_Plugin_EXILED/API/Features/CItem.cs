using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.Handlers;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;
using JetBrains.Annotations;
using MEC;
using Slafight_Plugin_EXILED.API.Enums;
using UnityEngine;
using Item = Exiled.API.Features.Items.Item;
using Player = Exiled.API.Features.Player;

namespace Slafight_Plugin_EXILED.API.Features
{
    public abstract class CItem
    {
        private static readonly HashSet<CItem> RegisteredInstances = new();
        private static readonly List<Type> ItemTypes;
        private static readonly Dictionary<ushort, CItem> SerialToItem = new();
        private static readonly Dictionary<ItemType, List<CItem>> ItemTypeToItems = new();

        // ==== Parse用辞書 ====
        private static readonly Dictionary<string, Type> ItemNameToType = new(StringComparer.OrdinalIgnoreCase);

        private static bool _eventsSubscribed;

        static CItem()
        {
            var asm = typeof(CItem).Assembly;
            ItemTypes = asm.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CItem)) && !t.IsAbstract)
                .ToList();
        }

        [AttributeUsage(AttributeTargets.Class)]
        public sealed class CItemAutoRegisterIgnoreAttribute : Attribute { }

        public static void RegisterAllEvents()
        {
            if (!_eventsSubscribed)
            {
                Exiled.Events.Handlers.Player.UsingItem += OnAnyUsingItem;
                Exiled.Events.Handlers.Player.DroppingItem += OnAnyDroppingItem;
                Exiled.Events.Handlers.Player.PickingUpItem += OnAnyPickingUpItem;
                _eventsSubscribed = true;
            }

            foreach (var type in ItemTypes)
            {
                try
                {
                    var instance = (CItem)Activator.CreateInstance(type);
                    if (instance.ItemType == ItemType.None) continue;

                    bool autoRegisterEvents = type.GetCustomAttributes(typeof(CItemAutoRegisterIgnoreAttribute), true).Length == 0;
                    instance.InternalRegisterItem(autoRegisterEvents);

                    // Parse辞書構築
                    var name = type.Name;
                    ItemNameToType[name] = type;
                    ItemNameToType[name.Replace("Custom", "")] = type;
                    ItemNameToType[instance.ItemType.ToString()] = type;
                }
                catch (Exception ex)
                {
                    Log.Error($"CItem.RegisterAllEvents failed for {type.Name}: {ex}");
                }
            }
        }

        public static void UnregisterAllEvents()
        {
            foreach (var instance in RegisteredInstances.ToList())
                instance.InternalUnregisterItem();

            RegisteredInstances.Clear();
            SerialToItem.Clear();
            ItemTypeToItems.Clear();
            ItemNameToType.Clear();

            if (_eventsSubscribed)
            {
                Exiled.Events.Handlers.Player.UsingItem -= OnAnyUsingItem;
                Exiled.Events.Handlers.Player.DroppingItem -= OnAnyDroppingItem;
                Exiled.Events.Handlers.Player.PickingUpItem -= OnAnyPickingUpItem;
                _eventsSubscribed = false;
            }
        }

        // ==== Serial判定API ====
        public static CItem GetItemFromSerial(ushort serial) => SerialToItem.TryGetValue(serial, out var item) ? item : null;
        public static CItem GetItemFromPickup(Pickup pickup) => pickup?.Serial != null ? GetItemFromSerial(pickup.Serial) : null;
        public static CItem GetItemFromPlayer(Player player, ItemType itemType)
        {
            var item = player.Items.FirstOrDefault(i => i.Type == itemType);
            return item != null ? GetItemFromSerial(item.Serial) : null;
        }
        public static bool IsCustomItem(ushort serial) => SerialToItem.ContainsKey(serial);

        // ==== Parse API ====
        public static bool TryParseItem(string input, out Type itemType) => ItemNameToType.TryGetValue(input, out itemType);
        public static List<string> GetAllItemNames() => ItemNameToType.Keys.ToList();

        // ==== 共通イベントハンドラ ====
        private static void OnAnyUsingItem(UsingItemEventArgs ev)
        {
            if (ev?.Player?.CurrentItem == null) return;
            GetItemFromSerial(ev.Player.CurrentItem.Serial)?.OnUsingItem(ev);
        }

        private static void OnAnyDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev?.Item == null) return;
            GetItemFromSerial(ev.Item.Serial)?.OnDroppingItem(ev);
        }

        private static void OnAnyPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev?.Pickup == null) return;
            var item = GetItemFromPickup(ev.Pickup);
            if (item != null)
            {
                item.OnPickingUpItem(ev);
                if (ev.IsAllowed) item.OnObtained(ev.Player);
            }
        }

        private static void OnItemObtained(Player player, ushort serial) => GetItemFromSerial(serial)?.OnObtained(player);

        // ==== インスタンス管理 ====
        private void InternalRegisterItem(bool autoRegisterEvents)
        {
            if (RegisteredInstances.Add(this))
            {
                if (!ItemTypeToItems.TryGetValue(ItemType, out var list))
                {
                    list = new List<CItem>();
                    ItemTypeToItems[ItemType] = list;
                }
                list.Add(this);

                if (autoRegisterEvents) RegisterEvents();
                Log.Debug($"CItem registered: {GetType().Name} (ItemType={ItemType})");
            }
        }

        private void InternalUnregisterItem()
        {
            if (RegisteredInstances.Remove(this))
            {
                if (Serial != 0 && SerialToItem.TryGetValue(Serial, out var inst) && ReferenceEquals(inst, this))
                    SerialToItem.Remove(Serial);

                if (ItemTypeToItems.TryGetValue(ItemType, out var list))
                {
                    list.Remove(this);
                    if (list.Count == 0) ItemTypeToItems.Remove(ItemType);
                }

                UnregisterEvents();
                Log.Debug($"CItem unregistered: {GetType().Name}");
            }
        }

        public virtual void RegisterEvents() { }
        public virtual void UnregisterEvents() { }

        protected abstract ItemType ItemType { get; }
        public ushort Serial { get; protected set; }

        // ==== virtualイベント ====
        protected virtual void OnUsingItem(UsingItemEventArgs ev) { }
        protected virtual void OnDroppingItem(DroppingItemEventArgs ev) { }
        protected virtual void OnPickingUpItem(PickingUpItemEventArgs ev) { }
        protected virtual void OnObtained(Player player) { }

        // ==== スポーンAPI ====
        public Item AddToPlayer(Player player)
        {
            var item = player.AddItem(ItemType);
            if (item != null)
            {
                Serial = item.Serial;
                SerialToItem[Serial] = this;
                OnItemObtained(player, Serial);
                Log.Debug($"CItem added: {GetType().Name} Serial={Serial}");
            }
            return item;
        }

        public Pickup SpawnPickup(Vector3 position, Quaternion? rotation = null, ushort count = 1, Player previousOwner = null)
        {
            var pickup = Pickup.CreateAndSpawn(ItemType, position, rotation ?? Quaternion.identity, previousOwner);
            if (pickup != null)
            {
                Serial = pickup.Serial;
                SerialToItem[Serial] = this;
                Log.Debug($"CItem pickup spawned: {GetType().Name} Serial={Serial}");
            }
            return pickup;
        }

        public void RegisterExistingItem(Item item)
        {
            Serial = item.Serial;
            SerialToItem[Serial] = this;
            Log.Debug($"CItem registered existing: {GetType().Name} Serial={Serial}");
        }

        // ==== GiveOrDrop ====
        public Pickup GiveOrDrop(Player player, Vector3? dropPosition = null)
        {
            if (player.IsInventoryFull)
            {
                var position = dropPosition ?? (player.Position + Vector3.up * 0.5f);
                return SpawnPickup(position);
            }
            else
            {
                var item = AddToPlayer(player);
                return item != null ? null : GiveOrDrop(player, dropPosition);
            }
        }

        // ==== static API ====
        public static Pickup GiveOrDropItem(Player player, Type cItemType, Vector3? dropPosition = null)
        {
            var item = Activator.CreateInstance(cItemType) as CItem;
            return item?.GiveOrDrop(player, dropPosition);
        }

        public static Item GiveItem(Player player, Type cItemType)
        {
            var item = Activator.CreateInstance(cItemType) as CItem;
            return item?.AddToPlayer(player);
        }
        
        // ==== CItemにCheckItem追加（CRole.Check相当） ====

        /// <summary>
        /// CRole.Check相当：このCItemのSerialか判定
        /// </summary>
        protected bool CheckItem(Item item)
        {
            if (item == null) return false;
            return SerialToItem.TryGetValue(item.Serial, out var cItem) && ReferenceEquals(cItem, this);
        }

        /// <summary>
        /// Pickup版
        /// </summary>
        protected bool CheckPickup(Pickup pickup)
        {
            if (pickup == null) return false;
            return SerialToItem.TryGetValue(pickup.Serial, out var cItem) && ReferenceEquals(cItem, this);
        }

        /// <summary>
        /// Static版：任意SerialがこのCItemか
        /// </summary>
        public static bool CheckSerial(ushort serial, CItem expectedItem)
        {
            return SerialToItem.TryGetValue(serial, out var item) && ReferenceEquals(item, expectedItem);
        }
    }
}
