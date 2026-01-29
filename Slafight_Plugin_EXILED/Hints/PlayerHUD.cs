#nullable enable
using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI.Utilities;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.SpecialEvents;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace Slafight_Plugin_EXILED.Hints;

public class PlayerHUD
{
    private CoroutineHandle _specificAbilityLoop;
    private CoroutineHandle _abilityHudLoop;

    public PlayerHUD()
    {
        Exiled.Events.Handlers.Player.Verified += ServerInfoHint;
        Exiled.Events.Handlers.Server.RoundStarted += PlayerHUDMain;
        Exiled.Events.Handlers.Player.ChangingRole += AllSyncHUD;
        Exiled.Events.Handlers.Server.RoundStarted += AllSyncHUD_;
        Exiled.Events.Handlers.Server.RestartingRound += DestroyHints;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += Spectate;

        _specificAbilityLoop = Timing.RunCoroutine(SpecificInfoHudLoop());
        _abilityHudLoop = Timing.RunCoroutine(AbilityHudLoop());
    }

    ~PlayerHUD()
    {
        Exiled.Events.Handlers.Player.Verified -= ServerInfoHint;
        Exiled.Events.Handlers.Server.RoundStarted -= PlayerHUDMain;
        Exiled.Events.Handlers.Player.ChangingRole -= AllSyncHUD;
        Exiled.Events.Handlers.Server.RoundStarted -= AllSyncHUD_;
        Exiled.Events.Handlers.Server.RestartingRound -= DestroyHints;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= Spectate;

        if (_specificAbilityLoop.IsRunning)
            Timing.KillCoroutines(_specificAbilityLoop);

        if (_abilityHudLoop.IsRunning)
            Timing.KillCoroutines(_abilityHudLoop);
    }

    private string ServerInfo_Text;
    private string PHUD_Role_Text;
    private string PHUD_Objective_Text;
    private string PHUD_Team_Text;
    private string PHUD_Event_Text;
    private string PHUD_Specific_Text;
    private string PHUD_Ability_Text;

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

        if (!Round.IsLobby)
            PlayerHUDSetup(ev.Player);
    }

    private void PlayerHUDSetup(Player player)
    {
        int XCordinate = -350;
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
        Hint PlayerHUD_Specific = new Hint
        {
            Id = "PlayerHUD_Specific",
            Text = "",
            Alignment = HintAlignment.Left,
            SyncSpeed = HintSyncSpeed.Fastest,
            FontSize = 24,
            XCoordinate = XCordinate + 350,
            YCoordinate = 885
        };
        Hint PlayerHUD_Ability = new Hint
        {
            Id = "PlayerHUD_Ability",
            Text = "",
            Alignment = HintAlignment.Left,
            SyncSpeed = HintSyncSpeed.Fastest,
            FontSize = 24,
            XCoordinate = XCordinate + 350,
            YCoordinate = 855
        };

        display.AddHint(PlayerHUD_Role);
        display.AddHint(PlayerHUD_Objective);
        display.AddHint(PlayerHUD_Team);
        display.AddHint(PlayerHUD_Event);
        display.AddHint(PlayerHUD_Specific);
        display.AddHint(PlayerHUD_Ability);
    }

    public void PlayerHUDMain()
    {
        foreach (Player player in Player.List)
            PlayerHUDSetup(player);
    }

    public void HintSync(SyncType syncType, string hintText, Player player)
    {
        PlayerDisplay display = PlayerDisplay.Get(player);

        if (syncType == SyncType.ServerInfo)
        {
            ServerInfo_Text = hintText;
            display.GetHint("ServerInfo").Text = ServerInfo_Text;
        }
        else if (syncType == SyncType.PHUD_Role)
        {
            PHUD_Role_Text = hintText;
            display.GetHint("PlayerHUD_Role").Text = "Role: " + PHUD_Role_Text;
        }
        else if (syncType == SyncType.PHUD_Objective)
        {
            PHUD_Objective_Text = hintText;
            display.GetHint("PlayerHUD_Objective").Text = "Objective: " + PHUD_Objective_Text;
        }
        else if (syncType == SyncType.PHUD_Team)
        {
            PHUD_Team_Text = hintText;
            display.GetHint("PlayerHUD_Team").Text = "Team: " + PHUD_Team_Text;
        }
        else if (syncType == SyncType.PHUD_Event)
        {
            PHUD_Event_Text = hintText;
            display.GetHint("PlayerHUD_Event").Text = "[Event]\n" + "<size=28>" + PHUD_Event_Text + "</size>";
        }
        else if (syncType == SyncType.PHUD_Ability)
        {
            PHUD_Ability_Text = hintText;
            display.GetHint("PlayerHUD_Ability").Text = PHUD_Ability_Text;
        }
    }

    string? SyncTextRole = null;
    string? SyncTextTeam = null;
    string? SyncTextObjective = null;
    string? SyncTextEvent = null;

    private void ApplyRoleInfo(Player sourcePlayer, Player targetForHint)
    {
        var custom = sourcePlayer.GetCustomRole();
        //Log.Debug($"[HUD] {sourcePlayer.Nickname} UniqueRole={sourcePlayer.UniqueRole}, Custom={custom}, Vanilla={sourcePlayer.Role.Type}");
        if (sourcePlayer.GetCustomRole() != CRoleTypeId.None)
        {
            switch (sourcePlayer.GetCustomRole())
            {
                // SCiPs
                case CRoleTypeId.Scp096Anger:
                    SyncTextRole = "<color=#c50000>SCP-096: ANGER</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "怒りに任せ、施設中で暴れまわれ！！！";
                    break;
                case CRoleTypeId.Scp3114:
                    SyncTextRole = "<color=#c50000>SCP-3114</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "皆に素敵なサプライズをして驚かせましょう！";
                    break;
                case CRoleTypeId.Scp966:
                    SyncTextRole = "<color=#c50000>SCP-966</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "背後から忍び寄り、奴らに恐怖を与えよ！";
                    break;
                case CRoleTypeId.Scp682:
                    SyncTextRole = "<color=#c50000>SCP-682</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "無敵の爬虫類の力を見せてやれ！！！";
                    break;
                case CRoleTypeId.Zombified:
                    SyncTextRole = "<color=#c50000>Zombified Subject</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "何らかの要因でゾンビの様になってしまった。とにかく暴れろ！";
                    break;
                case CRoleTypeId.Scp106:
                    SyncTextRole = "<color=#c50000>SCP-106</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "自身の欲求に従い、財団職員共を弄べ！";
                    break;
                case CRoleTypeId.Scp999:
                    SyncTextRole = "<color=#ff1493>SCP-999</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color>";
                    SyncTextObjective = "可愛いペットとして施設を歩き回れ！　※勝敗に影響しません。良い感じに遊んでね！";
                    break;
                // Fifthists
                case CRoleTypeId.Scp3005:
                    SyncTextRole = "<color=#ff00fa>SCP-3005</color>";
                    SyncTextTeam = "<color=#c50000>The SCPs</color> - <color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "第五教会に道を示し、施設を占領せよ";
                    break;
                case CRoleTypeId.FifthistRescure:
                    SyncTextRole = "<color=#ff00fa>Fifthist: Rescue</color>";
                    SyncTextTeam = "<color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "第五に従い、施設を占領せよ";
                    break;
                case CRoleTypeId.FifthistPriest:
                    SyncTextRole = "<color=#ff00fa>Fifthist: Priest</color>";
                    SyncTextTeam = "<color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "全てを第五せよ";
                    break;
                case CRoleTypeId.FifthistConvert:
                    SyncTextRole = "<color=#ff5ffa>Fifthist: Convert</color>";
                    SyncTextTeam = "<color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "第五に従い、施設を占領せよ";
                    break;
                // Chaos Insurgents
                case CRoleTypeId.ChaosCommando:
                    SyncTextRole = "<color=#228b22>Chaos Insurgency Commando</color>";
                    SyncTextTeam = "<color=#228b22>Chaos Insurgency</color>";
                    SyncTextObjective = "Dクラス職員を救出し、施設を略奪せよ。";
                    break;
                case CRoleTypeId.ChaosSignal:
                    SyncTextRole = "<color=#228b22>Chaos Insurgency Signal</color>";
                    SyncTextTeam = "<color=#228b22>Chaos Insurgency</color>";
                    SyncTextObjective = "Dクラス職員を救出し、施設を略奪せよ。";
                    break;
                // The Foundation Forces
                case CRoleTypeId.NtfLieutenant:
                    SyncTextRole = "<color=#00b7eb>MTF E-11: Lieutenant</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                    break;
                case CRoleTypeId.NtfGeneral:
                    SyncTextRole = "<color=blue>MTF E-11: General</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                    break;
                case CRoleTypeId.HdInfantry:
                    SyncTextRole = "<color=#353535>MTF Nu-7: Infantry</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                    break;
                case CRoleTypeId.HdCommander:
                    SyncTextRole = "<color=#252525>MTF Nu-7: Commander</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                    break;
                case CRoleTypeId.HdMarshal:
                    SyncTextRole = "<color=#151515>MTF Nu-7: Marshal</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                    break;
                // Scientists
                case CRoleTypeId.ZoneManager:
                    SyncTextRole = "<color=#00ffff>Zone Manager</color>";
                    SyncTextTeam = "<color=#faff86>Neutral - Side Foundation</color>";
                    SyncTextObjective = "施設からの脱出を目指しながら、警備職員達を監督せよ";
                    break;
                case CRoleTypeId.FacilityManager:
                    SyncTextRole = "<color=#dc143c>Facility Manager</color>";
                    SyncTextTeam = "<color=#faff86>Neutral - Side Foundation</color>";
                    SyncTextObjective = "施設からの脱出を目指しながら、サイトの行く末を監督せよ";
                    break;
                case CRoleTypeId.Engineer:
                    SyncTextRole = "<color=#faff86>Engineer</color>";
                    SyncTextTeam = "<color=#faff86>Neutral - Side Foundation</color>";
                    SyncTextObjective = "様々なタスクをこなし、最強の弾頭を起動せよ！";
                    break;
                case CRoleTypeId.ObjectObserver:
                    SyncTextRole = "<color=#faff86>Object Observer</color>";
                    SyncTextTeam = "<color=#faff86>Neutral - Side Foundation</color>";
                    SyncTextObjective = "オブジェクトに注意しながら、施設から脱出せよ。";
                    break;
                // Facility Guards
                case CRoleTypeId.EvacuationGuard:
                    SyncTextRole = "<color=#00b7eb>Emergency Evacuation Guard</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "職員達を上部階層へ避難させ、施設の秩序を守護せよ。";
                    break;
                case CRoleTypeId.SecurityChief:
                    SyncTextRole = "<color=#00b7eb>Security Chief</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "職員達を地上へ脱出させ、施設の秩序を守護せよ。";
                    break;
                case CRoleTypeId.ChamberGuard:
                    SyncTextRole = "<color=#00b7eb>Chamber Guard</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "Dクラスとオブジェクトに注意し、確実に職員達を避難させよ。";
                    break;
                // Class-D Personnel
                case CRoleTypeId.Janitor:
                    SyncTextRole = "<color=#ee7600>Janitor</color>";
                    SyncTextTeam = "<color=#ee7600>Neutral - Side Chaos</color>";
                    SyncTextObjective = "施設から脱出せよ。また、汚物をグレネードで清掃せよ。";
                    break;
                // Other Unknown Threads
                case CRoleTypeId.SnowWarrier:
                    SyncTextRole = "<b><color=#ffffff>SNOW WARRIER</color></b>";
                    SyncTextTeam = "<b><color=#ffffff>SNOW WARRIER's DIVISION</color></b>";
                    SyncTextObjective = "全施設にクリスマスと雪玉の正義を執行しろ";
                    break;
                // Special Roles
                case CRoleTypeId.Sculpture:
                    SyncTextRole = "<color=#00b7eb>Sculpture</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "財団に従い、人類を根絶させよ。";
                    break;
                default:
                    ApplyTeamFallback(sourcePlayer);
                    break;
            }
            if (SpecialEventsHandler.Instance.NowEvent == SpecialEventType.FacilityTermination && sourcePlayer.GetTeam() == CTeam.FoundationForces)
            {
                SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                SyncTextObjective = "財団に従い、人類を根絶させよ。";
            }
        }
        else
        {
            ApplyTeamFallback(sourcePlayer);
            if (SpecialEventsHandler.Instance.NowEvent == SpecialEventType.FacilityTermination && sourcePlayer.GetTeam() == CTeam.FoundationForces)
            {
                SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                SyncTextObjective = "財団に従い、人類を根絶させよ。";
            }
        }

        SyncTextEvent = Plugin.Singleton.SpecialEventsHandler.LocalizedEventName;
        HintSync(SyncType.PHUD_Role, SyncTextRole, targetForHint);
        HintSync(SyncType.PHUD_Objective, SyncTextObjective, targetForHint);
        HintSync(SyncType.PHUD_Team, SyncTextTeam, targetForHint);
        HintSync(SyncType.PHUD_Event, SyncTextEvent, targetForHint);
    }

    private void ApplyTeamFallback(Player player)
    {
        switch (player.Role.Team)
        {
            case Team.ClassD:
                SyncTextRole = "<color=#ee7600>" + player.Role.Name + "</color>";
                SyncTextTeam = "<color=#ee7600>Neutral - Side Chaos</color>";
                SyncTextObjective = "施設から脱出せよ";
                break;
            case Team.Scientists:
                SyncTextRole = "<color=#faff86>" + player.Role.Name + "</color>";
                SyncTextTeam = "<color=#faff86>Neutral - Side Foundation</color>";
                SyncTextObjective = "施設から脱出せよ";
                break;
            case Team.ChaosInsurgency:
                SyncTextRole = "<color=#228b22>" + player.Role.Name + "</color>";
                SyncTextTeam = "<color=#228b22>Chaos Insurgency</color>";
                SyncTextObjective = "Dクラス職員を救出し、施設を略奪せよ。";
                break;
            case Team.FoundationForces:
                SyncTextRole = "<color=#00b7eb>" + player.Role.Name + "</color>";
                SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                break;
            case Team.SCPs:
                SyncTextRole = "<color=#c50000>" + player.Role.Name + "</color>";
                SyncTextTeam = "<color=#c50000>The SCPs</color>";
                SyncTextObjective = "己の本能・復讐心と利益の為に動け";
                break;
            case Team.Flamingos:
                SyncTextRole = "<color=#ff96de>" + player.Role.Name + "</color>";
                SyncTextTeam = "<color=#ff96de>The Flamingos</color>";
                SyncTextObjective = "フラミンゴ！";
                break;
            default:
                SyncTextRole = "<color=#ffffff>" + player.Role.Name + "</color>";
                SyncTextTeam = "<color=#ffffff>[Unknown]</color>";
                SyncTextObjective = "[Unknown]";
                break;
        }
    }

    public void SyncTexts(Player? _player = null, Player spectatetarget = null)
    {
        SyncTextRole = null;
        SyncTextTeam = null;
        SyncTextObjective = null;
        SyncTextEvent = null;

        if (_player == null && spectatetarget == null)
        {
            foreach (Player player in Player.List)
            {
                if (player == null) continue;
                if (player.Role.Team == Team.Dead) continue;

                ApplyRoleInfo(player, player);
            }
        }
        else if (_player != null && spectatetarget != null)
        {
            Player player = spectatetarget;
            if (player == null) return;
            if (player.Role.Team == Team.Dead) return;

            ApplyRoleInfo(player, _player);
        }
        else
        {
            //Log.Debug("Called SyncTexts it's not proper procedure.");
        }
    }

    public void AllSyncHUD(ChangingRoleEventArgs ev)
    {
        Timing.CallDelayed(1.05f, () =>
        {
            SyncTexts(null);
        });
    }

    public void Spectate(ChangingSpectatedPlayerEventArgs ev)
    {
        if (ev.NewTarget == null) return;
        Player player = ev.NewTarget;
        SyncTexts(ev.Player, player);
    }

    public void AllSyncHUD_()
    {
        SyncTexts();
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
            SyncTexts();
            yield return Timing.WaitForSeconds(30);
        }
    }

    // Ability HUD を組み立て
    private string BuildAbilityHud(Player player)
    {
        if (player.Role.Team == Team.Dead || !player.IsAlive)
            return string.Empty;

        if (!AbilityManager.TryGetLoadout(player, out var loadout))
            return string.Empty;

        var active = loadout.ActiveAbility;
        if (active == null)
            return string.Empty;

        if (!AbilityBase.TryGetAbilityState(player, active,
                out bool canUse, out float cdRemain, out int usesLeft, out int maxUses))
            return string.Empty;

        string abilityKey = active.GetType().Name;
        string abilityName = AbilityLocalization.GetDisplayName(abilityKey, player);

        string cdText = canUse ? "<color=green>READY</color>" :
            $"<color=yellow>{(int)cdRemain}s</color>";

        string usesText = (maxUses < 0) ? "∞" : usesLeft.ToString();

        return $"<color=#ffcc00>[{abilityName}]</color> CD: {cdText} Uses: {usesText}";
    }

    // ロール固有テキストのループ（PHUD_Specific）
    private IEnumerator<float> SpecificInfoHudLoop()
    {
        yield return Timing.WaitForSeconds(1f);

        for (;;)
        {
            foreach (var player in Player.List)
            {
                if (player == null || !player.IsAlive) continue;

                string roleSpecific = RoleSpecificTextProvider.GetFor(player);

                var display = PlayerDisplay.Get(player);
                var specificHint = display.GetHint("PlayerHUD_Specific");
                if (specificHint == null)
                {
                    PlayerHUDSetup(player);
                    specificHint = display.GetHint("PlayerHUD_Specific");
                    if (specificHint == null) continue;
                }

                if (string.IsNullOrEmpty(roleSpecific))
                {
                    // ★ ロール固有テキストがなくなったら消す
                    specificHint.Text = string.Empty;
                    continue;
                }

                PHUD_Specific_Text = roleSpecific;
                specificHint.Text = PHUD_Specific_Text;
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }

    // Ability 用 HUD ループ（PHUD_Ability）
    private IEnumerator<float> AbilityHudLoop()
    {
        yield return Timing.WaitForSeconds(0.5f);

        for (;;)
        {
            foreach (var player in Player.List)
            {
                if (player == null || !player.IsAlive) 
                    continue;

                var display = PlayerDisplay.Get(player);
                var abilityHint = display.GetHint("PlayerHUD_Ability");

                if (abilityHint == null)
                {
                    //Log.Debug($"[HUD] PlayerHUD_Ability missing for {player.Nickname}, recreating...");
                    PlayerHUDSetup(player);
                    abilityHint = display.GetHint("PlayerHUD_Ability");
                    if (abilityHint == null)
                        continue;
                }

                string abilityText = BuildAbilityHud(player);
                PHUD_Ability_Text = abilityText;

                abilityHint.Text = PHUD_Ability_Text;
            }

            yield return Timing.WaitForSeconds(0.5f);
        }
    }
}