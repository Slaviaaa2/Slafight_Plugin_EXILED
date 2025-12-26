using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Pickups;
using LightContainmentZoneDecontamination;
using MEC;
using PlayerRoles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.SpecialEvents.Events;

public class Scp1509BattleField
{
    public void Scp1509BattleFieldEvent()
    {
        var EventHandler = Plugin.Singleton.EventHandler;
        var SpecialEventHandler = Plugin.Singleton.SpecialEventsHandler;
        Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;
        int eventPID = SpecialEventHandler.EventPID;
        int battlefield_map = Random.Range(1,5); // X以上Y未満ということらしい。1,4に設定して地獄を見た
        DecontaminationController.Singleton.DecontaminationOverride = DecontaminationController.DecontaminationStatus.Disabled;
        DecontaminationController.DeconBroadcastDeconMessage = "除染は取り消されました";
        
        if (eventPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;

        List<ItemType> keepPickups = new List<ItemType>() { ItemType.Painkillers,ItemType.Medkit,ItemType.Adrenaline };
        foreach (Pickup pickup in Pickup.List)
        {
            if (!keepPickups.Contains(pickup.Type))
            {
                pickup.Destroy();
            }
        }

        Timing.CallDelayed(1.11f, () =>
        {
int i = 0;
        if (Random.Range(1,3) == 1)
        {
            foreach (Player player in Player.List)
            {
                if (i%2==0)
                {
                    player.Role.Set(RoleTypeId.ChaosRifleman);
                    player.ClearInventory();
                    player.AddItem(ItemType.SCP1509);
                    player.AddItem(ItemType.Medkit);
                    player.AddItem(ItemType.Medkit);
                    player.AddItem(ItemType.Adrenaline);
                    player.AddItem(ItemType.SCP500);
                    player.AddItem(ItemType.SCP500);
                    player.AddItem(ItemType.ArmorCombat);
                }
                else
                {
                    player.Role.Set(RoleTypeId.NtfPrivate);
                    player.ClearInventory();
                    player.AddItem(ItemType.SCP1509);
                    player.AddItem(ItemType.Medkit);
                    player.AddItem(ItemType.Medkit);
                    player.AddItem(ItemType.Adrenaline);
                    player.AddItem(ItemType.SCP500);
                    player.AddItem(ItemType.SCP500);
                    player.AddItem(ItemType.ArmorCombat);
                }

                i++;
            }
        }
        else
        {
            foreach (Player player in Player.List)
            {
                if (i%2==1)
                {
                    player.Role.Set(RoleTypeId.ChaosRifleman);
                    player.ClearInventory();
                    player.AddItem(ItemType.SCP1509);
                    player.AddItem(ItemType.Medkit);
                    player.AddItem(ItemType.Medkit);
                    player.AddItem(ItemType.Adrenaline);
                    player.AddItem(ItemType.SCP500);
                    player.AddItem(ItemType.SCP500);
                    player.AddItem(ItemType.ArmorCombat);
                }
                else
                {
                    player.Role.Set(RoleTypeId.NtfPrivate);
                    player.ClearInventory();
                    player.AddItem(ItemType.SCP1509);
                    player.AddItem(ItemType.Medkit);
                    player.AddItem(ItemType.Medkit);
                    player.AddItem(ItemType.Adrenaline);
                    player.AddItem(ItemType.SCP500);
                    player.AddItem(ItemType.SCP500);
                    player.AddItem(ItemType.ArmorCombat);
                }

                i++;
            }
        }
        });

        List<ElevatorType> lockEvTypes = new List<ElevatorType>() { ElevatorType.GateA,ElevatorType.GateB,ElevatorType.LczA,ElevatorType.LczB };
        foreach (Lift lift in Lift.List){
            if (lockEvTypes.Contains(lift.Type))
            {
                lift.TryStart(0,false);
            }
        }

        foreach (Door door in Door.List)
        {
            if (door.Type == DoorType.ElevatorGateA || door.Type == DoorType.ElevatorGateB ||  door.Type == DoorType.ElevatorLczB || door.Type == DoorType.ElevatorLczA)
            {
                door.Lock(DoorLockType.AdminCommand);
            }
        }
        // Map Selection
        if (battlefield_map == 1)
        {
            // Surface
            foreach (Door door in Door.List)
            {
                door.IsOpen = true;
                door.Lock(DoorLockType.AdminCommand);
            }
        }
        else if (battlefield_map == 2)
        {
            // Entrance
            List<DoorType> closeDoor = new List<DoorType>() { DoorType.CheckpointGateA,DoorType.CheckpointGateB };
            foreach (Door door in Door.List)
            {
                if (closeDoor.Contains(door.Type))
                {
                    door.IsOpen = false;
                }
                else
                {
                    door.IsOpen = true;
                }
                door.Lock(DoorLockType.AdminCommand);
            }
            foreach (Player player in Player.List)
            {
                if (player.Role == RoleTypeId.ChaosRifleman)
                {
                    Vector3 chaosspawn = Door.Get(DoorType.GateA).Position;
                    player.Position = chaosspawn + new Vector3(0f,2f,0f);
                }
                else if (player.Role == RoleTypeId.NtfPrivate)
                {
                    Vector3 ntfspawn = Door.Get(DoorType.GateB).Position;
                    player.Position = ntfspawn + new Vector3(0f,2f,0f);
                }
            }
        }
        else if (battlefield_map == 3)
        {
            // Heavy Containment
            List<DoorType> closeDoor = new List<DoorType>() { DoorType.CheckpointGateA,DoorType.CheckpointGateB };
            foreach (Door door in Door.List)
            {
                if (closeDoor.Contains(door.Type))
                {
                    door.IsOpen = false;
                }
                else
                {
                    door.IsOpen = true;
                }
                door.Lock(DoorLockType.AdminCommand);
            }
            foreach (Player player in Player.List)
            {
                if (player.Role == RoleTypeId.ChaosRifleman)
                {
                    Vector3 chaosspawn = Door.Get(DoorType.CheckpointEzHczA).Position;
                    player.Position = chaosspawn + new Vector3(0f,2f,0f);
                }
                else if (player.Role == RoleTypeId.NtfPrivate)
                {
                    Vector3 ntfspawn = Door.Get(DoorType.CheckpointEzHczB).Position;
                    player.Position = ntfspawn + new Vector3(0f,2f,0f);
                }
            }
        }
        else if (battlefield_map == 4)
        {
            // Light Containment
            List<DoorType> closeDoor = new List<DoorType>() { DoorType.Scp914Door,DoorType.Scp914Gate };
            foreach (Door door in Door.List)
            {
                if (closeDoor.Contains(door.Type))
                {
                    door.IsOpen = false;
                }
                else
                {
                    door.IsOpen = true;
                }
                door.Lock(DoorLockType.AdminCommand);
            }

            foreach (Player player in Player.List)
            {
                if (player.Role == RoleTypeId.ChaosRifleman)
                {
                    Vector3 chaosspawn = Door.Get(DoorType.CheckpointLczA).Position;
                    player.Position = chaosspawn + new Vector3(0f,2f,0f);
                }
                else if (player.Role == RoleTypeId.NtfPrivate)
                {
                    Vector3 ntfspawn = Door.Get(DoorType.CheckpointLczB).Position;
                    player.Position = ntfspawn + new Vector3(0f,2f,0f);
                }
            }
        }
    }
}