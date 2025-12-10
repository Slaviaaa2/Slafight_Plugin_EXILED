
#nullable enable
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI.Utilities;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace Slafight_Plugin_EXILED.Hints;

public class PlayerHUD
{
    public PlayerHUD()
    {
        Exiled.Events.Handlers.Player.Verified += ServerInfoHint;
        Exiled.Events.Handlers.Server.RoundStarted += PlayerHUDMain;
        Exiled.Events.Handlers.Player.ChangingRole += AllSyncHUD;
        Exiled.Events.Handlers.Server.RoundStarted += AllSyncHUD_;
        Exiled.Events.Handlers.Server.RestartingRound += DestroyHints;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += Spectate;
    }

    ~PlayerHUD()
    {
        Exiled.Events.Handlers.Player.Verified -= ServerInfoHint;
        Exiled.Events.Handlers.Server.RoundStarted -= PlayerHUDMain;
        Exiled.Events.Handlers.Player.ChangingRole -= AllSyncHUD;
        Exiled.Events.Handlers.Server.RoundStarted -= AllSyncHUD_;
        Exiled.Events.Handlers.Server.RestartingRound -= DestroyHints;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= Spectate;
    }

    private string ServerInfo_Text;
    private string PHUD_Role_Text;
    private string PHUD_Objective_Text;
    private string PHUD_Team_Text;
    private string PHUD_Event_Text;
    public void ServerInfoHint(VerifiedEventArgs ev)
    {
        PlayerUI ui = PlayerUI.Get(ev.Player);
        PlayerDisplay display = PlayerDisplay.Get(ev.Player);
        Hint ServerInfo = new Hint
        {
            Id = "ServerInfo",
            Text = "[<color=#008cff>Sharp Server</color>]",
            Alignment = HintAlignment.Center,
            SyncSpeed = HintSyncSpeed.UnSync,
            FontSize = 18,
            YCoordinate = 1050
        };
        display.AddHint(ServerInfo);
    }

    public void PlayerHUDMain()
    {
        int XCordinate = -350;
        foreach (Player player in Player.List)
        {
            PlayerUI ui = PlayerUI.Get(player);
            PlayerDisplay display = PlayerDisplay.Get(player);
            Hint PlayerHUD_Role = new Hint
            {
                Id = "PlayerHUD_Role",
                Text = "Role: " + player.CustomInfo,
                Alignment = HintAlignment.Left,
                SyncSpeed = HintSyncSpeed.Fastest,
                FontSize = 24,
                XCoordinate = XCordinate,
                YCoordinate = 860
            };
            Hint PlayerHUD_Objective = new Hint
            {
                Id = "PlayerHUD_Objective",
                Text = "Objective: " + "Undefined",
                Alignment = HintAlignment.Left,
                YCoordinate = 915,
                XCoordinate = XCordinate,
                SyncSpeed = HintSyncSpeed.Fastest,
                FontSize = 30
            };
            Hint PlayerHUD_Team = new Hint
            {
                Id = "PlayerHUD_Team",
                Text = "Team: " + "Undefined",
                Alignment = HintAlignment.Left,
                YCoordinate = 885,
                XCoordinate = XCordinate,
                SyncSpeed = HintSyncSpeed.Fastest,
                FontSize = 24
            };
            Hint PlayerHUD_Event = new Hint
            {
                Id = "PlayerHUD_Event",
                Text = "[Event]\n" + "<size=28>Undefined</size>",
                Alignment = HintAlignment.Left,
                SyncSpeed = HintSyncSpeed.Fast,
                FontSize = 26,
                XCoordinate = XCordinate,
                YCoordinate = 120
            };
            display.AddHint(PlayerHUD_Role);
            display.AddHint(PlayerHUD_Objective);
            display.AddHint(PlayerHUD_Team);
            display.AddHint(PlayerHUD_Event);
        }
    }

    public void HintSync(SyncType syncType,string hintText,Player player)
    {
        if (syncType == SyncType.ServerInfo)
        {
            ServerInfo_Text = hintText;
            PlayerDisplay display = PlayerDisplay.Get(player);
            display.GetHint("ServerInfo").Text = ServerInfo_Text;
        }
        else if (syncType == SyncType.PHUD_Role)
        {
            PHUD_Role_Text = hintText;
            PlayerDisplay display = PlayerDisplay.Get(player);
            display.GetHint("PlayerHUD_Role").Text = "Role: " + PHUD_Role_Text;
        }
        else if (syncType == SyncType.PHUD_Objective)
        {
            PHUD_Objective_Text = hintText;
            PlayerDisplay display = PlayerDisplay.Get(player);
            display.GetHint("PlayerHUD_Objective").Text = "Objective: " + PHUD_Objective_Text;
        }
        else if (syncType == SyncType.PHUD_Team)
        {
            PHUD_Team_Text = hintText;
            PlayerDisplay display = PlayerDisplay.Get(player);
            display.GetHint("PlayerHUD_Team").Text = "Team: " + PHUD_Team_Text;
        }
        else if (syncType == SyncType.PHUD_Event)
        {
            PHUD_Event_Text = hintText;
            PlayerDisplay display = PlayerDisplay.Get(player);
            display.GetHint("PlayerHUD_Event").Text = "[Event]\n" + "<size=28>"+PHUD_Event_Text+"</size>";
        }
    }

    public void AllSyncHUD(ChangingRoleEventArgs ev)
    {
        Timing.CallDelayed(1.05f, () =>
        {
        string SyncTextRole = null;
        string SyncTextTeam = null;
        string SyncTextObjective = null;
        string SyncTextEvent = null;
        foreach (Player player in Player.List)
        {
            if (player == null) continue;
            if (player.CustomInfo != null)
            {
                // SCiPs
                if (player.UniqueRole == "Scp096_Anger")
                {
                    SyncTextRole = "<color=#c50000>"+"SCP-096: ANGER"+"</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "己の本能・復讐心と利益の為に動け";
                }
                // Fifthists
                if (player.UniqueRole == "SCP-3005")
                {
                    SyncTextRole = "<color=#ff00fa>"+"SCP-3005"+"</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color> - <color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "第五教会に道を示し、施設を占領せよ";
                }
                if (player.UniqueRole == "FIFTHIST")
                {
                    SyncTextRole = "<color=#ff00fa>"+"Fifthist: Rescue"+"</color>";
                    SyncTextTeam = "<color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "第五に従い、施設を占領せよ";
                }
                if (player.UniqueRole == "F_Priest")
                {
                    SyncTextRole = "<color=#ff00fa>"+"Fifthist: Priest"+"</color>";
                    SyncTextTeam = "<color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "全てを第五せよ";
                }
                // Chaos Insurgents
                if (player.UniqueRole == "CI_Commando")
                {
                    SyncTextRole = "<color=#228b22>"+"CI: Commando"+"</color>";
                    SyncTextTeam = "<color=#228b22>Chaos Insurgency</color>";
                    SyncTextObjective = "Dクラス職員を救出し、施設を略奪せよ。";
                }
                // The Foundation Forces
                if (player.UniqueRole == "NtfAide")
                {
                    SyncTextRole = "<color=#252525>"+"MTF E-11: Aide"+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                if (player.UniqueRole == "HdInfantry")
                {
                    SyncTextRole = "<color=#353535>"+"MTF Nu-7: Infantry"+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                if (player.UniqueRole == "HdCommander")
                {
                    SyncTextRole = "<color=#252525>"+"MTF Nu-7: Commander"+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                // Scientists
                // Class-D Personnel
                // Other Unknown Threads
            }
            else
            {
                if (player.Role.Team == Team.ClassD)
                {
                    SyncTextRole = "<color=#ee7600>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#ee7600>Neutral - Side Chaos</color>";
                    SyncTextObjective = "施設から脱出せよ";
                }
                else if (player.Role.Team == Team.Scientists)
                {
                    SyncTextRole = "<color=#faff86>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#faff86>Neutral - Side Foundation</color>";
                    SyncTextObjective = "施設から脱出せよ";
                }
                else if (player.Role.Team == Team.ChaosInsurgency)
                {
                    SyncTextRole = "<color=#228b22>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#228b22>Chaos Insurgency</color>";
                    SyncTextObjective = "Dクラス職員を救出し、施設を略奪せよ。";
                }
                else if (player.Role.Team == Team.FoundationForces)
                {
                    SyncTextRole = "<color=#00b7eb>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                else if (player.Role.Team == Team.SCPs)
                {
                    SyncTextRole = "<color=#c50000>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "己の本能・復讐心と利益の為に動け";
                }
                else if (player.Role.Team == Team.Dead)
                {
                    continue;
                    SyncTextRole = "<color=#727472>"+player.Role.Name+"</color>";
                    SyncTextTeam = "The Dead";
                    SyncTextObjective = "観戦しましょう";
                }
                else if (player.Role.Team == Team.Flamingos)
                {
                    SyncTextRole = "<color=#ff96de>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#ff96de>The Flamingos</color>";
                    SyncTextObjective = "フラミンゴ！";
                }
                else
                {
                    SyncTextRole = "<color=#ffffff>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#ffffff>[Unknown]</color>";
                    SyncTextObjective = "[Unknown]";
                }
            }
            SyncTextEvent = Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.localizedEventName;
            HintSync(SyncType.PHUD_Role,SyncTextRole,player);
            HintSync(SyncType.PHUD_Objective,SyncTextObjective,player);
            HintSync(SyncType.PHUD_Team,SyncTextTeam,player);
            HintSync(SyncType.PHUD_Event,SyncTextEvent,player);
        }
        });
    }

    public void Spectate(ChangingSpectatedPlayerEventArgs ev)
    {
        Player player = ev.NewTarget;
        string SyncTextRole = null;
        string SyncTextTeam = null;
        string SyncTextObjective = null;
        string SyncTextEvent = null;
            if (player == null) return;
            if (player.CustomInfo != null)
            {
                // SCiPs
                if (player.UniqueRole == "Scp096_Anger")
                {
                    SyncTextRole = "<color=#c50000>"+"SCP-096: ANGER"+"</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "己の本能・復讐心と利益の為に動け";
                }
                // Fifthists
                if (player.UniqueRole == "SCP-3005")
                {
                    SyncTextRole = "<color=#ff00fa>"+"SCP-3005"+"</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color> - <color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "第五教会に道を示し、施設を占領せよ";
                }
                if (player.UniqueRole == "FIFTHIST")
                {
                    SyncTextRole = "<color=#ff00fa>"+"Fifthist: Rescue"+"</color>";
                    SyncTextTeam = "<color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "第五に従い、施設を占領せよ";
                }
                // Chaos Insurgents
                if (player.UniqueRole == "CI_Commando")
                {
                    SyncTextRole = "<color=#228b22>"+"CI: Commando"+"</color>";
                    SyncTextTeam = "<color=#228b22>Chaos Insurgency</color>";
                    SyncTextObjective = "Dクラス職員を救出し、施設を略奪せよ。";
                }
                // The Foundation Forces
                if (player.UniqueRole == "NtfAide")
                {
                    SyncTextRole = "<color=#252525>"+"MTF E-11: Aide"+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                if (player.UniqueRole == "HdInfantry")
                {
                    SyncTextRole = "<color=#353535>"+"MTF Nu-7: Infantry"+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                if (player.UniqueRole == "HdCommander")
                {
                    SyncTextRole = "<color=#252525>"+"MTF Nu-7: Commander"+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                // Scientists
                // Class-D Personnel
                // Other Unknown Threads
            }
            else
            {
                if (player.Role.Team == Team.ClassD)
                {
                    SyncTextRole = "<color=#ee7600>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#ee7600>Neutral - Side Chaos</color>";
                    SyncTextObjective = "施設から脱出せよ";
                }
                else if (player.Role.Team == Team.Scientists)
                {
                    SyncTextRole = "<color=#faff86>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#faff86>Neutral - Side Foundation</color>";
                    SyncTextObjective = "施設から脱出せよ";
                }
                else if (player.Role.Team == Team.ChaosInsurgency)
                {
                    SyncTextRole = "<color=#228b22>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#228b22>Chaos Insurgency</color>";
                    SyncTextObjective = "Dクラス職員を救出し、施設を略奪せよ。";
                }
                else if (player.Role.Team == Team.FoundationForces)
                {
                    SyncTextRole = "<color=#00b7eb>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                else if (player.Role.Team == Team.SCPs)
                {
                    SyncTextRole = "<color=#c50000>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "己の本能・復讐心と利益の為に動け";
                }
                else if (player.Role.Team == Team.Dead)
                {
                    return;
                    SyncTextRole = "<color=#727472>"+player.Role.Name+"</color>";
                    SyncTextTeam = "The Dead";
                    SyncTextObjective = "観戦しましょう";
                }
                else if (player.Role.Team == Team.Flamingos)
                {
                    SyncTextRole = "<color=#ff96de>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#ff96de>The Flamingos</color>";
                    SyncTextObjective = "フラミンゴ！";
                }
                else
                {
                    SyncTextRole = "<color=#ffffff>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#ffffff>[Unknown]</color>";
                    SyncTextObjective = "[Unknown]";
                }
            }
            HintSync(SyncType.PHUD_Role,SyncTextRole,ev.Player);
            HintSync(SyncType.PHUD_Objective,SyncTextObjective,ev.Player);
            HintSync(SyncType.PHUD_Team,SyncTextTeam,ev.Player);
    }
    public void AllSyncHUD_()
    {
        string SyncTextRole = null;
        string SyncTextTeam = null;
        string SyncTextObjective = null;
        string SyncTextEvent = null;
        foreach (Player player in Player.List)
        {
            if (player == null) continue;
            if (player.CustomInfo != null)
            {
                // SCiPs
                if (player.UniqueRole == "Scp096_Anger")
                {
                    SyncTextRole = "<color=#c50000>"+"SCP-096: ANGER"+"</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "己の本能・復讐心と利益の為に動け";
                }
                // Fifthists
                if (player.UniqueRole == "SCP-3005")
                {
                    SyncTextRole = "<color=#ff00fa>"+"SCP-3005"+"</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color> - <color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "第五教会に道を示し、施設を占領せよ";
                }
                if (player.UniqueRole == "FIFTHIST")
                {
                    SyncTextRole = "<color=#ff00fa>"+"Fifthist: Rescue"+"</color>";
                    SyncTextTeam = "<color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "第五に従い、施設を占領せよ";
                }
                // Chaos Insurgents
                if (player.UniqueRole == "CI_Commando")
                {
                    SyncTextRole = "<color=#228b22>"+"CI: Commando"+"</color>";
                    SyncTextTeam = "<color=#228b22>Chaos Insurgency</color>";
                    SyncTextObjective = "Dクラス職員を救出し、施設を略奪せよ。";
                }
                // The Foundation Forces
                if (player.UniqueRole == "NtfAide")
                {
                    SyncTextRole = "<color=#252525>"+"MTF E-11: Aide"+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                if (player.UniqueRole == "HdInfantry")
                {
                    SyncTextRole = "<color=#353535>"+"MTF Nu-7: Infantry"+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                if (player.UniqueRole == "HdCommander")
                {
                    SyncTextRole = "<color=#252525>"+"MTF Nu-7: Commander"+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                // Scientists
                // Class-D Personnel
                // Other Unknown Threads
            }
            else
            {
                if (player.Role.Team == Team.ClassD)
                {
                    SyncTextRole = "<color=#ee7600>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#ee7600>Neutral - Side Chaos</color>";
                    SyncTextObjective = "施設から脱出せよ";
                }
                else if (player.Role.Team == Team.Scientists)
                {
                    SyncTextRole = "<color=#faff86>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#faff86>Neutral - Side Foundation</color>";
                    SyncTextObjective = "施設から脱出せよ";
                }
                else if (player.Role.Team == Team.ChaosInsurgency)
                {
                    SyncTextRole = "<color=#228b22>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#228b22>Chaos Insurgency</color>";
                    SyncTextObjective = "Dクラス職員を救出し、施設を略奪せよ。";
                }
                else if (player.Role.Team == Team.FoundationForces)
                {
                    SyncTextRole = "<color=#00b7eb>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                }
                else if (player.Role.Team == Team.SCPs)
                {
                    SyncTextRole = "<color=#c50000>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "己の本能・復讐心と利益の為に動け";
                }
                //else if (player.Role.Team == Team.Dead)
                //{
                //    SyncTextRole = "<color=#727472>"+player.Role.Name+"</color>";
                //    SyncTextTeam = "The Dead";
                //    SyncTextObjective = "観戦しましょう";
                //}
                else if (player.Role.Team == Team.Flamingos)
                {
                    SyncTextRole = "<color=#ff96de>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#ff96de>The Flamingos</color>";
                    SyncTextObjective = "フラミンゴ！";
                }
                else
                {
                    SyncTextRole = "<color=#ffffff>"+player.Role.Name+"</color>";
                    SyncTextTeam = "<color=#ffffff>[Unknown]</color>";
                    SyncTextObjective = "[Unknown]";
                }
            }
            SyncTextEvent = Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.localizedEventName;
            HintSync(SyncType.PHUD_Role,SyncTextRole,player);
            HintSync(SyncType.PHUD_Objective,SyncTextObjective,player);
            HintSync(SyncType.PHUD_Team,SyncTextTeam,player);
            HintSync(SyncType.PHUD_Event,SyncTextEvent,player);
        }
    }

    public void DestroyHints()
    {
        PlayerDisplay display;
        foreach (Player player in Player.List)
        {
            display = PlayerDisplay.Get(player);
            display.ClearHint();
        }
    }

    private IEnumerator<float> TaskSync()
    {
        for (;;)
        {
            AllSyncHUD_();
            yield return Timing.WaitForSeconds(30);
        }
    }
}