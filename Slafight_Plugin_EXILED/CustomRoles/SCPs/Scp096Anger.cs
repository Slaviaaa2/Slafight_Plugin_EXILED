using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp096;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp096Anger : CRole
{
    private static readonly Dictionary<Player, Vector3> ShyGuyPositions = new();
    
    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Scp096.CalmingDown += EndlessAnger;
        Exiled.Events.Handlers.Scp096.Enraging += CleanShyDummy;
        Exiled.Events.Handlers.Player.Dying += OnDying;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Scp096.CalmingDown -= EndlessAnger;
        Exiled.Events.Handlers.Scp096.Enraging -= CleanShyDummy;
        Exiled.Events.Handlers.Player.Dying -= OnDying;
        base.UnregisterEvents();
    }

    private void OnDying(DyingEventArgs ev)
    {
        ShyGuyPositions.Remove(ev.Player);
    }

    private void StartAnger(Player player)
    {
        foreach (Door door in Door.List)
        {
            if (door.Type == DoorType.HeavyContainmentDoor && door.Room.Type == RoomType.Hcz096)
            {
                door.Lock(DoorLockType.AdminCommand);
            }
        }
            
        Vector3 shyguyPosition = ShyGuyPositions[player];
        Vector3 spawnPoint = new Vector3(shyguyPosition.x + 1f, shyguyPosition.y + 0f, shyguyPosition.z);
        Npc term_npc = Npc.Spawn("for096", RoleTypeId.ClassD, false, position: spawnPoint);
        term_npc.Transform.localEulerAngles = new Vector3(0, -90, 0);
    }
        
    private void EndlessAnger(Exiled.Events.EventArgs.Scp096.CalmingDownEventArgs ev)
    {
        if (ev.Player?.UniqueRole == "Scp096_Anger")
        {
            ev.IsAllowed = false;
            ev.ShouldClearEnragedTimeLeft = true;
        }
    }

    private void CleanShyDummy(Exiled.Events.EventArgs.Scp096.EnragingEventArgs ev)
    {
        if (ev.Player?.UniqueRole == "Scp096_Anger")
        {
            foreach (Npc npc in Npc.List)
            {
                if (npc.CustomName == "for096")
                {
                    npc.Destroy();
                }
            }
            ev.InitialDuration = float.MaxValue;
        }
    }

    public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scp096);
        player.UniqueRole = "Scp096_Anger";
        player.SetCustomInfo("SCP-096: ANGER");
        player.MaxArtificialHealth = 1000;
        player.MaxHealth = 5000;
        player.Health = 5000;
        
        // 安全なMovementBoost付与
        player.EnableEffect(EffectType.MovementBoost, 50, 999f);
        
        player.ShowHint(
            "<color=red>SCP-096: ANGER</color>\nSCP-096の怒りと悲しみが頂点に達し、その化身へと変貌して大いなる力を手に入れた。\n<color=red>とにかく破壊しまくれ！！！！！</color>",
            10);
        player.Transform.eulerAngles = new Vector3(0, -90, 0);
        
        // Dictionaryに保存
        ShyGuyPositions[player] = player.Position;
        
        Log.Debug("Scp096: Anger was Spawned!");
        Timing.CallDelayed(0.1f, () => StartAnger(player));
    }
}
