using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.ProximityChat;
using UnityEngine;

namespace Slafight_Plugin_EXILED.MapExtensions;

public class PDEx
{
    public PDEx()
    {
        Exiled.Events.Handlers.Server.RoundStarted += setup;
        Exiled.Events.Handlers.Player.FailingEscapePocketDimension += JoinPDEx;
    }

    ~PDEx()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= setup;
        Exiled.Events.Handlers.Player.FailingEscapePocketDimension -= JoinPDEx;
    }

    public static List<Player> PDExPlayers = new();

    private void setup()
    {
        PDExPlayers.Clear();
    }

    private void JoinPDEx(FailingEscapePocketDimensionEventArgs ev)
    {
        if (Random.Range(0,5) == 0)
        {
            int i = 0;
            foreach (var _player in Player.List)
            {
                if (_player == null) continue;
                if (_player.GetCustomRole() == CRoleTypeId.Scp106 || (_player.GetCustomRole() == CRoleTypeId.None && _player.Role.Type == RoleTypeId.Scp106))
                {
                    i++;
                }
            }
            if (i<=0) return;
            ev.IsAllowed = false;
            ev.Player?.Position = CustomMap.PDExJoin;
            ev.Player?.DisableEffect(EffectType.PocketCorroding);
            ev.Player?.EnableEffect(EffectType.Slowness, 30);
            if (ev.Player != null)
            {
                PDExPlayers.Add(ev.Player);
            }
            foreach (var player in Player.List)
            {
                if (player == null) continue;
                if (player.GetCustomRole() == CRoleTypeId.Scp106 || (player.GetCustomRole() == CRoleTypeId.None && player.Role.Type == RoleTypeId.Scp106))
                {
                    player.Position = CustomMap.PDExJoinKing;
                    player.AddAbility(new AllowEscapeAbility(player));
                    player.ShowHint("アビリティ「AllowEscapeAbility」が付与されました。\n人間を釈放したくなったら使ってください\nまた、近接チャットも一時的に利用可能です！");
                    Handler.CanUsePlayers.Add(player);
                }
            }
        }
    }
}