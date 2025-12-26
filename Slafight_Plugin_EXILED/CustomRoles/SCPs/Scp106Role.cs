using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Hazards;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Utilities;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp106Role : CRole
{
    public override void RegisterEvents()
    {
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        base.UnregisterEvents();
    }
    
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scp106);
        player.UniqueRole = "Scp106";
        //player.MaxHealth = 1400;
        //player.Health = player.MaxHealth;
        //player.MaxHumeShield = 500;
        player.ClearInventory();

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Commander");
        player.SetCustomInfo("SCP-106");
        if (!AbilityBase.HasAbility(player.Id))
        {
            player.AddAbility<CreateSinkholeAbility>();
        }
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=red>SCP-106</color>\n若者の叫び声大好き爺。いっぱいPDに送り込もう！\nアビリティで陥没穴を創り出せるぞ！陥没穴は中に\n人を引き込めるから沢山作れ！",10f);
        });
    }
}