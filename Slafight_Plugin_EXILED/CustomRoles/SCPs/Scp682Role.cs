using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using HintServiceMeow.Core.Utilities;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp939;
using Slafight_Plugin_EXILED.API.Enums;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp682Role
{
    // SCRAPPED!!!!!!!!!!!!!
    
    public Scp682Role()
    {
        Exiled.Events.Handlers.Player.Dying += DiedCassieAnnounce;
        Exiled.Events.Handlers.Player.Hurting += Hurting;
    }

    ~Scp682Role()
    {
        Exiled.Events.Handlers.Player.Dying -= DiedCassieAnnounce;
        Exiled.Events.Handlers.Player.Hurting -= Hurting;
    }
    
    private float speedLevel = 1;
    
    public void SpawnRole(Player player)
    {
        player.Role.Set(RoleTypeId.Scp173);
        //Vector3 pos = player.Position;
        player.Role.Set(RoleTypeId.Scp939,RoleSpawnFlags.None);
        player.UniqueRole = "Scp682";
        Timing.CallDelayed(0.01f, () =>
        {
            player.MaxHealth = 999;
            player.Health = player.MaxHealth;
            player.MaxHumeShield = 10000;
            player.ClearInventory();

            //player.Position = pos;
            player.SetFakeScale(new Vector3(1.75f, 0.75f, 3f),Player.List);
            player.CustomInfo = "<color=#C50000>SCP-682</color>";
            player.InfoArea |= PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.Role;
            Timing.CallDelayed(0.05f, () =>
            {
                player.ShowHint("<color=red>SCP-682</color>\n長く眠っていた為視界がぼやけている。でも頑張って無双しろ！！！",10f);
            });
            Timing.RunCoroutine(Coroutine(player));
        });
    }

    private IEnumerator<float> Coroutine(Player player)
    {
        for (;;)
        {
            if (player.UniqueRole != "Scp682")
            {
                yield break;
            }
            player.SetScale(new Vector3(1.75f, 0.75f, 3f)*speedLevel,Player.List);
            speedLevel *= 1.00001f;
            Slafight_Plugin_EXILED.Plugin.Singleton.PlayerHUD.HintSync(SyncType.PHUD_Specific,("Big Multiplier: "+speedLevel),player);
            yield return Timing.WaitForSeconds(1f);
        }
    }

    public void Hurting(HurtingEventArgs ev)
    {
        if (ev.Attacker?.UniqueRole == "Scp682")
        {
            ev.Amount *= speedLevel;
        }
    }
    
    public void DiedCassieAnnounce(DyingEventArgs ev)
    {
        if (ev.Player.UniqueRole == "SCP-682")
        {
            Slafight_Plugin_EXILED.Plugin.Singleton.PlayerHUD.HintSync(SyncType.PHUD_Specific,"",ev.Player);
            //SchematicObject schematicObject = ObjectSpawner.SpawnSchematic("SCP3005",ev.Player.Position,ev.Player.Rotation,Vector3.one,null);
            Cassie.Clear();
            Cassie.MessageTranslated("SCP 6 8 2 Successfully Neutralized .","<color=red>SCP-966</color>の無力化に成功しました。");
        }
    }
}