using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Utilities;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp966Role : CRole
{
    private static readonly Dictionary<Player, int> SpeedLevels = new();
    
    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Scp3114.Disguising += ExtendTime;
        Exiled.Events.Handlers.Player.Dying += DiedCassie;
        Exiled.Events.Handlers.Player.Hurting += Hurting;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Scp3114.Disguising -= ExtendTime;
        Exiled.Events.Handlers.Player.Dying -= DiedCassie;
        Exiled.Events.Handlers.Player.Hurting -= Hurting;
        base.UnregisterEvents();
    }
    
    public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scp3114);
        player.UniqueRole = "Scp966";
        player.MaxHealth = 1400;
        player.Health = player.MaxHealth;
        player.MaxHumeShield = 500;
        SpeedLevels[player] = 0;
        player.ClearInventory();

        player.CustomInfo = "lor=#C50000>SCP-966</color>";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
            
        Room SpawnRoom = Room.Get(RoomType.LczGlassBox);
        Log.Debug(SpawnRoom.Position);
        Vector3 offset = new Vector3(0f, 1.5f, 0f);
        player.Position = SpawnRoom.Position + SpawnRoom.Rotation * offset;
        player.Rotation = SpawnRoom.Rotation;
        
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("lor=red>SCP-966</color>\n透明～！", 10f);
        });
        Timing.RunCoroutine(Coroutine(player));
    }

    private IEnumerator<float> Coroutine(Player player)
    {
        for (;;)
        {
            var speedLevel = SpeedLevels[player];
            if (player.GetCustomRole() != CRoleTypeId.Scp966)
            {
                // ★ 修正: RoleSpecificTextProvider を使用
                RoleSpecificTextProvider.Set(player, "");
                player.DisableEffect(EffectType.Invisible);
                player.DisableEffect(EffectType.NightVision);
                player.DisableEffect(EffectType.Slowness);
                player.DisableEffect(EffectType.MovementBoost);
                yield break;
            }

            if (UnityEngine.Random.Range(0, 3) == 0)
            {
                player.DisableEffect(EffectType.Invisible);
                player.CurrentRoom.RoomLightController.ServerFlickerLights(0.5f);
            }
            else
            {
                player.EnableEffect(EffectType.Invisible);
            }
            
            player.EnableEffect(EffectType.NightVision, 255);
            if (speedLevel <= 0)
            {
                player.EnableEffect(EffectType.Slowness, 20);
            }
            else if (speedLevel == 1)
            {
                player.EnableEffect(EffectType.Slowness, 10);
            }
            else if (speedLevel == 2)
            {
                player.EnableEffect(EffectType.Slowness, 0);
            }
            else if (speedLevel == 3)
            {
                player.EnableEffect(EffectType.MovementBoost, 10);
            }
            else if (speedLevel >= 4)
            {
                player.EnableEffect(EffectType.MovementBoost, 20);
            }
            
            // ★ 修正: RoleSpecificTextProvider を使用
            RoleSpecificTextProvider.Set(player, 
                "Speed Level: " + (Math.Abs(speedLevel + 1)) + "/5");
            
            yield return Timing.WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));
        }
    }

    private void ExtendTime(Exiled.Events.EventArgs.Scp3114.DisguisingEventArgs ev)
    {
        if (ev.Player?.GetCustomRole() != CRoleTypeId.Scp966) return;
    
        SpeedLevels[ev.Player] = SpeedLevels.GetValueOrDefault(ev.Player) + 1;
        if (SpeedLevels[ev.Player] > 4)
        {
            ev.Player.Heal(35f);
            SpeedLevels[ev.Player] = 4;
        }
        ev.IsAllowed = false;
        ev.Ragdoll.Destroy();
    }

    private void Hurting(HurtingEventArgs ev)
    {
        if (ev.Attacker?.GetCustomRole() == CRoleTypeId.Scp966)
        {
            ev.Amount = 15f;
        }
    }

    private void DiedCassie(DyingEventArgs ev)
    {
        if (ev.Player?.GetCustomRole() == CRoleTypeId.Scp966)
        {
            SpeedLevels.Remove(ev.Player);
            Exiled.API.Features.Cassie.MessageTranslated(
                "SCP 9 6 6 Successfully Terminated .",
                "lor=red>SCP-966</color>の終了に成功しました。");
        }
    }
}