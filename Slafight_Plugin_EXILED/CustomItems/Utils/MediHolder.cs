using System.Collections.Generic;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using LabApi.Events.Arguments.PlayerEvents;
using MEC;
using Slafight_Plugin_EXILED.API.Core.Features;
using Player = Exiled.API.Features.Player;

namespace Slafight_Plugin_EXILED.CustomItems.Utils;

public class MediHolder : CustomItem
{
    public override string Name => "MediHolder";
    public override string Description => "弾薬スロットに拾った回復アイテムを収納でき、使用することができる。";
    public override ItemType BaseType => ItemType.Medkit;
    public CoroutineHandle HintCoroutine;
    public List<ItemType> HolderInventory = [];
    private int _selected;
    
    protected override void OnCreated()
    {
        Exiled.Events.Handlers.Player.ChangedItem += OnChangedItem;
        Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
        base.OnCreated();
    }

    protected override void OnReleased()
    {
        Exiled.Events.Handlers.Player.ChangedItem -= OnChangedItem;
        Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
        base.OnReleased();
    }

    protected override void OnEquipped(Player player)
    {
        PlayerScope.Of(player).Delay(1.25f, _ =>
        {
            HintCoroutine = PlayerScope.Of(player).RunLoop(0.1f, p =>
            {
                if (HolderInventory.Count > 0)
                {
                    p.ShowHint($"<size=26><<color=yellow>{HolderInventory[_selected]}</color>></size>");
                }
                else
                {
                    p.ShowHint($"<size=26><<color=yellow>無し</color>></size>");
                }
            });
        });
        base.OnEquipped(player);
    }

    protected override void OnDropping(PlayerDroppingItemEventArgs ev)
    {
        if (!ev.Throw) return;
        ev.IsAllowed = false;
        var count = _selected + 1;
        if (HolderInventory.Count <= count)
        {
            _selected = 0;
        }
        else
        {
            _selected++;
        }
        base.OnDropping(ev);
    }

    protected override void OnUsing(PlayerUsingItemEventArgs ev)
    {
        ev.IsAllowed = false;
        if (HolderInventory.Count <= 0)
        {
            Owner.ShowHint("<size=26>アイテムが何も入っていません！</size>");
            return;
        }

        Owner.CurrentItem = Exiled.API.Features.Items.Item.Create(HolderInventory[_selected]);
        if (Owner.CurrentItem is Usable usable)
        {
            HolderInventory.RemoveAt(_selected);
            _selected = 0;
            PlayerScope.Of(Owner).Delay(1f, _ =>
            {
                usable.MaxCancellableTime = 0f;
                usable.IsUsing = true;
            });
        }
        base.OnUsing(ev);
    }

    protected override void OnPickedUp(Player player)
    {
        foreach (var itemType in HolderInventory)
            AddAmmo(player, itemType);

        base.OnPickedUp(player);
    }

    protected override void OnDropped(Player player)
    {
        foreach (var itemType in HolderInventory)
            RemoveAmmo(player, itemType);
        
        Timing.KillCoroutines(HintCoroutine);
        player.ShowHint("");

        base.OnDropped(player);
    }

    private void OnChangedItem(ChangedItemEventArgs ev)
    {
        if (ev.OldItem.Serial != Serial) return;
        Timing.KillCoroutines(HintCoroutine);
        Owner.ShowHint("");
    }

    private void OnPickingUpItem(PickingUpItemEventArgs ev)
    {
        if (ev.Pickup.Serial == Serial || ev.Player != Owner)
            return;

        if (HolderInventory.Count >= 3)
            return;

        if (Is<CustomItem>(ev.Pickup.Serial))
            return;

        if (ev.Pickup.Type is not (
            ItemType.Painkillers or
            ItemType.Medkit or
            ItemType.Adrenaline or
            ItemType.SCP500))
            return;

        ev.IsAllowed = false;

        HolderInventory.Add(ev.Pickup.Type);
        AddAmmo(ev.Player, ev.Pickup.Type);

        ev.Pickup.Destroy();
    }
    
    private static void AddAmmo(Player player, ItemType type)
    {
        if (player.Ammo.ContainsKey(type))
            player.Ammo[type]++;
        else
            player.Ammo.Add(type, 1);
    }

    private static void RemoveAmmo(Player player, ItemType type)
    {
        if (!player.Ammo.TryGetValue(type, out var amount) || amount == 0)
            return;

        player.Ammo[type]--;
    }
}