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
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp966Role
{
    public Scp966Role()
    {
        Exiled.Events.Handlers.Scp3114.Disguising += ExtendTime;
        Exiled.Events.Handlers.Player.Dying += DiedCassieAnnounce;
        Exiled.Events.Handlers.Player.Hurting += Hurting;
    }

    ~Scp966Role()
    {
        Exiled.Events.Handlers.Scp3114.Disguising -= ExtendTime;
        Exiled.Events.Handlers.Player.Dying -= DiedCassieAnnounce;
        Exiled.Events.Handlers.Player.Hurting -= Hurting;
    }
    
    int speedLevel=0;
    
    public void SpawnRole(Player player)
    {
        player.Role.Set(RoleTypeId.Scp3114);
        player.UniqueRole = "Scp966";
        Timing.CallDelayed(0.01f, () =>
        {
            player.MaxHealth = 1400;
            player.Health = player.MaxHealth;
            player.MaxHumeShield = 500;
            player.ClearInventory();

            //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Commander");
            player.CustomInfo = "<color=#C50000>SCP-966</color>";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            
            Room SpawnRoom = Room.Get(RoomType.LczGlassBox);
            Log.Debug(SpawnRoom.Position);
            Vector3 offset = new Vector3(0f,1.5f,0f);
            player.Position = SpawnRoom.Position + SpawnRoom.Rotation * offset;
            player.Rotation = SpawnRoom.Rotation;
            Timing.CallDelayed(0.05f, () =>
            {
                player.ShowHint("<color=red>SCP-966</color>\n透明～！",10f);
            });
            Timing.RunCoroutine(Coroutine(player));
        });
    }

    private IEnumerator<float> Coroutine(Player player)
    {
        for (;;)
        {
            if (player.UniqueRole != "Scp966")
            {
                Slafight_Plugin_EXILED.Plugin.Singleton.PlayerHUD.HintSync(SyncType.PHUD_Specific,"",player);
                player.DisableEffect(EffectType.Invisible);
                player.DisableEffect(EffectType.NightVision);
                player.DisableEffect(EffectType.Slowness);
                player.DisableEffect(EffectType.MovementBoost);
                yield break;
            }

            if (UnityEngine.Random.Range(0,3)==0)
            {
                player.DisableEffect(EffectType.Invisible);
                player.CurrentRoom.RoomLightController.ServerFlickerLights(0.5f);
            }
            else
            {
                player.EnableEffect(EffectType.Invisible);
            }
            player.EnableEffect(EffectType.NightVision,255);
            if (speedLevel <= 0)
            {
                player.EnableEffect(EffectType.Slowness,20);
            }
            else if (speedLevel == 1)
            {
                player.EnableEffect(EffectType.Slowness,10);
            }
            else if (speedLevel == 2)
            {
                player.EnableEffect(EffectType.Slowness,0);
            }
            else if (speedLevel == 3)
            {
                player.EnableEffect(EffectType.MovementBoost, 10);
            }
            else if (speedLevel >= 4)
            {
                player.EnableEffect(EffectType.MovementBoost, 20);
            }
            
            Slafight_Plugin_EXILED.Plugin.Singleton.PlayerHUD.HintSync(SyncType.PHUD_Specific,("Speed Level: "+(Math.Abs(speedLevel+1))+"/5"),player);
            yield return Timing.WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));
        }
    }

    public void ExtendTime(Exiled.Events.EventArgs.Scp3114.DisguisingEventArgs ev)
    {
        if (ev.Player.UniqueRole == "Scp966")
        {
            ev.IsAllowed = false;
            ev.Ragdoll.Destroy();
            if (speedLevel <= 4)
            {
                speedLevel++;
            }
            else
            {
                ev.Player.Heal(35f);
            }
        }
    }

    public void Hurting(HurtingEventArgs ev)
    {
        if (ev.Attacker?.UniqueRole == "Scp966")
        {
            ev.Amount = 15f;
        }
    }
    public void DiedCassieAnnounce(DyingEventArgs ev)
    {
        if (ev.Player.UniqueRole == "SCP-966")
        {
            //SchematicObject schematicObject = ObjectSpawner.SpawnSchematic("SCP3005",ev.Player.Position,ev.Player.Rotation,Vector3.one,null);
            Cassie.Clear();
            Cassie.MessageTranslated("SCP 9 6 6 Successfully Terminated .","<color=red>SCP-966</color>の終了に成功しました。");
        }
    }
}