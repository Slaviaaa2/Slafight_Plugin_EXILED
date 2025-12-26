using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Scp914;
using PlayerRoles;
using Scp914;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.ProximityChat;
using UnityEngine;

namespace Slafight_Plugin_EXILED;

public class Scp914Changes
{
    public Scp914Changes()
    {
        //Exiled.Events.Handlers.Scp914.UpgradingInventoryItem += UpgradeItem;
        //Exiled.Events.Handlers.Scp914.UpgradedPickup += UpgradeItem;
        //Exiled.Events.Handlers.Scp914.UpgradingPlayer += UpgradeOrganism;
    }

    ~Scp914Changes()
    {
        //Exiled.Events.Handlers.Scp914.UpgradingInventoryItem -= UpgradeItem;
        //Exiled.Events.Handlers.Scp914.UpgradedPickup -= UpgradeItem;
        //Exiled.Events.Handlers.Scp914.UpgradingPlayer -= UpgradeOrganism;
    }

    public void UpgradeItem(UpgradingInventoryItemEventArgs ev)
    {
        if (ev.Item.IsKeycard)
        {
            ev.IsAllowed = false;
            ev.Item?.UpgradeItem();
        }
    }
    public void UpgradeItem(UpgradedPickupEventArgs ev)
    {
        if (ev.Pickup.Type.IsKeycard())
        {
            ev.Pickup?.UpgradeItem();
        }
    }

    public void UpgradeOrganism(UpgradingPlayerEventArgs ev)
    {
        var random = Random.Range(0f, 1f);
        if (ev.KnobSetting == Scp914KnobSetting.Rough)
        {
            if (random <= 0.5)
            {
                ev.Player?.Role.Set(RoleTypeId.Scp0492,RoleSpawnFlags.None);
                ev.Player?.EnableEffect(EffectType.Scp207, 4);
                ev.Player?.UniqueRole = "Zombified";
                ev.Player?.CustomInfo = "<color=#C50000>Zombified Subject</color>";
                ev.Player?.InfoArea |= PlayerInfoArea.Nickname;
                ev.Player?.InfoArea &= ~PlayerInfoArea.Role;
                ev.Player?.SetScale(new Vector3(0.8f,0.8f,0.8f));
                if (!Handler.CanUsePlayers.Contains(ev.Player))
                {
                    Handler.CanUsePlayers.Add(ev.Player);
                }
                if (!Handler.ActivatedPlayers.Contains(ev.Player))
                {
                    Handler.ActivatedPlayers.Add(ev.Player);
                }
            }
            else
            {
                ev.Player?.EnableEffect(EffectType.Asphyxiated, 1, 30f);
            }
        }
        else if (ev.KnobSetting == Scp914KnobSetting.Coarse)
        {
            if (random <= 0.5)
            {
                ev.Player?.EnableEffect(EffectType.Flashed, 1, 3);
                ev.OutputPosition = Room.Get(RoomType.LczClassDSpawn).WorldPosition(new Vector3(0f, 0.1f, 0f));
            }
            else
            {
                
            }
        }
    }
}