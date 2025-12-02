using System.Collections.Generic;
using ASS.Events.EventArgs;
using ASS.Events.Handlers;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using PlayerRoles.Voice;
using UnityEngine;
using Utils.Networking;
using VoiceChat;
using VoiceChat.Networking;

namespace Slafight_Plugin_EXILED;

public class SS_Handler_Exiled
{
    public SS_Handler_Exiled()
    {
        /*
        SettingEvents.KeybindPressed += Speak;
        Exiled.Events.Handlers.Player.VoiceChatting += SpeakBySCiP;
        Exiled.Events.Handlers.Server.RestartingRound += initSpeakingPlayers;
        Exiled.Events.Handlers.Player.Left += leftSpeakingPlayers;
        Exiled.Events.Handlers.Player.Dying += dyingSpeakingPlayers;
        */
    }

    ~SS_Handler_Exiled()
    {
        /*
        SettingEvents.KeybindPressed -= Speak;
        Exiled.Events.Handlers.Player.VoiceChatting -= SpeakBySCiP;
        Exiled.Events.Handlers.Server.RestartingRound -= initSpeakingPlayers;
        Exiled.Events.Handlers.Player.Left -= leftSpeakingPlayers;
        Exiled.Events.Handlers.Player.Dying -= dyingSpeakingPlayers;
        */
    }

    private List<Player> speakingPlayers = new List<Player>() { };

    private void initSpeakingPlayers()
    {
        speakingPlayers.Clear();
    }

    private void leftSpeakingPlayers(LeftEventArgs ev)
    {
        speakingPlayers.Remove(ev.Player);
    }

    private void dyingSpeakingPlayers(DyingEventArgs ev)
    {
        speakingPlayers.Remove(ev.Player);
    }
    public void Speak(KeybindPressedEventArgs ev)
    {
        if (ev.Player == null || ev.Keybind == null) return;
        if (ev.Keybind.Id==-11&&ev.Player.Role.GetTeam() == Team.SCPs&&ev.Keybind.IsPressed)
        {
            foreach (Player player in Player.List)
            {
                if (ev.Player.PlayerId == player.Id)
                {
                    List<string> CanSpeakUnique = new List<string>() {  };
                    List<RoleTypeId> CanSpeakRole = new List<RoleTypeId>() { RoleTypeId.Scp049 };
                    if (player.UniqueRole != null)
                    {
                        if (CanSpeakUnique.Contains(player.UniqueRole))
                        {
                            switch (speakingPlayers.Contains(player))
                            {
                                case true:
                                    speakingPlayers.Remove(player);
                                    player.ShowHint("交流VCMode: <color=red>OFF</color>");
                                    break;
                                case false:
                                    speakingPlayers.Add(player);
                                    player.ShowHint("交流VCMode: <color=green>ON</color>");
                                    break;
                            }
                        }
                        break;
                    }
                    else
                    {
                        if (CanSpeakRole.Contains(player.Role))
                        {
                            switch (speakingPlayers.Contains(player))
                            {
                                case true:
                                    speakingPlayers.Remove(player);
                                    player.ShowHint("交流VCMode: <color=red>OFF</color>");
                                    break;
                                case false:
                                    speakingPlayers.Add(player);
                                    player.ShowHint("交流VCMode: <color=green>ON</color>");
                                    break;
                            }
                        }
                    }
                    
                    break;
                }
            }
        }
    }

    public void SpeakBySCiP(VoiceChattingEventArgs ev)
    {
        VoiceMessage msg = ev.VoiceMessage;
        if (ev.Player.Role.Team == Team.SCPs)
        {
            if (speakingPlayers.Contains(ev.Player))
            {
                ev.IsAllowed = false;
                foreach (Player player in Player.List)
                {
                    if (Vector3.Distance(player.Position,ev.Player.Position) <= 12.5)
                    {
                        player.ReferenceHub.connectionToClient.Send(msg);
                    }
                }
            }
        }
    }
}