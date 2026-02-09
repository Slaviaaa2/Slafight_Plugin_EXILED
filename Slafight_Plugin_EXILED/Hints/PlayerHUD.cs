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
    private CoroutineHandle _taskSyncLoop;

    // 観戦者ID → 現在見ているプレイヤー
    private readonly Dictionary<int, Player> _spectateTargets = new();

    public PlayerHUD()
    {
        Exiled.Events.Handlers.Player.Verified += ServerInfoHint;
        Exiled.Events.Handlers.Server.RoundStarted += PlayerHUDMain;
        Exiled.Events.Handlers.Player.ChangingRole += AllSyncHUD;
        Exiled.Events.Handlers.Server.RoundStarted += AllSyncHUD_;
        Exiled.Events.Handlers.Server.RestartingRound += DestroyHints;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += Spectate;

        // 旧仕様と同じく、コルーチンはプラグイン生存中ずっと回す
        _specificAbilityLoop = Timing.RunCoroutine(SpecificInfoHudLoop());
        _abilityHudLoop = Timing.RunCoroutine(AbilityHudLoop());
        _taskSyncLoop = Timing.RunCoroutine(TaskSync());
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

        if (_taskSyncLoop.IsRunning)
            Timing.KillCoroutines(_taskSyncLoop);
    }

    private string ServerInfo_Text = string.Empty;
    private string PHUD_Role_Text = string.Empty;
    private string PHUD_Objective_Text = string.Empty;
    private string PHUD_Team_Text = string.Empty;
    private string PHUD_Event_Text = string.Empty;
    private string PHUD_Specific_Text = string.Empty;
    private string PHUD_Ability_Text = string.Empty;

    public void ServerInfoHint(VerifiedEventArgs ev)
    {
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

        // ラウンド中に途中参加した場合は HUD も作る + ロール同期
        if (!Round.IsLobby)
        {
            PlayerHUDSetup(ev.Player);
            ApplyRoleInfo(ev.Player, ev.Player);
        }
    }

    private void PlayerHUDSetup(Player player)
    {
        int XCordinate = -350;
        PlayerDisplay display = PlayerDisplay.Get(player);

        Hint PlayerHUD_Role = new()
        {
            Id = "PlayerHUD_Role",
            Text = "Role: " + player.CustomInfo,
            Alignment = HintAlignment.Left,
            SyncSpeed = HintSyncSpeed.Fastest,
            FontSize = 24,
            XCoordinate = XCordinate,
            YCoordinate = 860
        };
        Hint PlayerHUD_Objective = new()
        {
            Id = "PlayerHUD_Objective",
            Text = "Objective: " + "Undefined",
            Alignment = HintAlignment.Left,
            YCoordinate = 915,
            XCoordinate = XCordinate,
            SyncSpeed = HintSyncSpeed.Fastest,
            FontSize = 30
        };
        Hint PlayerHUD_Team = new()
        {
            Id = "PlayerHUD_Team",
            Text = "Team: " + "Undefined",
            Alignment = HintAlignment.Left,
            YCoordinate = 885,
            XCoordinate = XCordinate,
            SyncSpeed = HintSyncSpeed.Fastest,
            FontSize = 24
        };
        Hint PlayerHUD_Event = new()
        {
            Id = "PlayerHUD_Event",
            Text = "[Event]\n<size=28>Undefined</size>",
            Alignment = HintAlignment.Left,
            SyncSpeed = HintSyncSpeed.Fast,
            FontSize = 26,
            XCoordinate = XCordinate,
            YCoordinate = 120
        };
        Hint PlayerHUD_Specific = new()
        {
            Id = "PlayerHUD_Specific",
            Text = "",
            Alignment = HintAlignment.Left,
            SyncSpeed = HintSyncSpeed.Fastest,
            FontSize = 24,
            XCoordinate = XCordinate + 350,
            YCoordinate = 885
        };
        Hint PlayerHUD_Ability = new()
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
        // 旧仕様寄り：RoundStarted 時点で全員分 HUD 作成
        foreach (Player player in Player.List)
        {
            if (player == null || !player.IsConnected) continue;
            PlayerHUDSetup(player);
            ApplyRoleInfo(player, player);
        }
    }

    public void HintSync(SyncType syncType, string hintText, Player player)
    {
        PlayerDisplay display = PlayerDisplay.Get(player);

        switch (syncType)
        {
            case SyncType.ServerInfo:
                ServerInfo_Text = hintText;
                display.GetHint("ServerInfo").Text = ServerInfo_Text;
                break;
            case SyncType.PHUD_Role:
                PHUD_Role_Text = hintText;
                display.GetHint("PlayerHUD_Role").Text = "Role: " + PHUD_Role_Text;
                break;
            case SyncType.PHUD_Objective:
                PHUD_Objective_Text = hintText;
                display.GetHint("PlayerHUD_Objective").Text = "Objective: " + PHUD_Objective_Text;
                break;
            case SyncType.PHUD_Team:
                PHUD_Team_Text = hintText;
                display.GetHint("PlayerHUD_Team").Text = "Team: " + PHUD_Team_Text;
                break;
            case SyncType.PHUD_Event:
                PHUD_Event_Text = hintText;
                display.GetHint("PlayerHUD_Event").Text = "[Event]\n<size=28>" + PHUD_Event_Text + "</size>";
                break;
            case SyncType.PHUD_Ability:
                PHUD_Ability_Text = hintText;
                display.GetHint("PlayerHUD_Ability").Text = PHUD_Ability_Text;
                break;
        }
    }

    string? SyncTextRole = null;
    string? SyncTextTeam = null;
    string? SyncTextObjective = null;
    string? SyncTextEvent = null;

    // ========= ここからロール情報構築 =========

    private void ApplyRoleInfo(Player sourcePlayer, Player targetForHint)
    {
        if (sourcePlayer == null || !sourcePlayer.IsConnected)
            return;

        // FacilityTermination 中の財団側特殊処理
        if (SpecialEventsHandler.Instance.NowEvent == SpecialEventType.FacilityTermination)
        {
            var cteam = sourcePlayer.GetTeam();

            if ((cteam == CTeam.FoundationForces || cteam == CTeam.Guards) &&
                sourcePlayer.GetCustomRole() != CRoleTypeId.Sculpture)
            {
                SyncTextRole = $"<color=#00b7eb>{sourcePlayer.Role.Name}</color>";
                SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                SyncTextObjective = "財団に従い、人類を根絶させよ。";
                SyncTextEvent = Plugin.Singleton.SpecialEventsHandler.LocalizedEventName;

                HintSync(SyncType.PHUD_Role, SyncTextRole, targetForHint);
                HintSync(SyncType.PHUD_Objective, SyncTextObjective, targetForHint);
                HintSync(SyncType.PHUD_Team, SyncTextTeam, targetForHint);
                HintSync(SyncType.PHUD_Event, SyncTextEvent, targetForHint);
                return;
            }
        }

        var custom = sourcePlayer.GetCustomRole();

        if (custom != CRoleTypeId.None)
        {
            switch (custom)
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
                    SyncTextObjective = "第五を探し出し、救出し、従い、施設を占領せよ。";
                    break;
                case CRoleTypeId.FifthistPriest:
                    SyncTextRole = "<color=#ff00fa>Fifthist: Priest</color>";
                    SyncTextTeam = "<color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "あなたは幸福な事に第五の加護を受けている。全てを第五せよ！";
                    break;
                case CRoleTypeId.FifthistConvert:
                    SyncTextRole = "<color=#ff5ffa>Fifthist: Convert</color>";
                    SyncTextTeam = "<color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "あなたは第五教会の新入りだ。第五とは何かについて考え、理解し、そして従いなさい。";
                    break;
                case CRoleTypeId.FifthistGuidance:
                    SyncTextRole = "<color=#ff5ffa>Fifthist: Guidance</color>";
                    SyncTextTeam = "<color=#ff00fa>The Fifthists</color>";
                    SyncTextObjective = "杖を用い、第五主義を施設に広めなさい。あなたの導きは教会にとって重要です！";
                    break;

                // Chaos
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

                // Foundation Forces
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

                // Guards
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

                // Class-D
                case CRoleTypeId.Janitor:
                    SyncTextRole = "<color=#ee7600>Janitor</color>";
                    SyncTextTeam = "<color=#ee7600>Neutral - Side Chaos</color>";
                    SyncTextObjective = "施設から脱出せよ。また、汚物をグレネードで清掃せよ。";
                    break;

                // Other
                case CRoleTypeId.SnowWarrier:
                    SyncTextRole = "<b><color=#ffffff>SNOW WARRIER</color></b>";
                    SyncTextTeam = "<b><color=#ffffff>SNOW WARRIER's DIVISION</color></b>";
                    SyncTextObjective = "全施設にクリスマスと雪玉の正義を執行しろ";
                    break;

                // Special
                case CRoleTypeId.Sculpture:
                    SyncTextRole = "<color=#00b7eb>Sculpture</color>";
                    SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                    SyncTextObjective = "財団に従い、人類を根絶させよ。";
                    break;

                // GoC
                case CRoleTypeId.GoCSquadLeader or CRoleTypeId.GoCDeputy or CRoleTypeId.GoCMedic or CRoleTypeId.GoCThaumaturgist or CRoleTypeId.GoCCommunications or CRoleTypeId.GoCOperative:
                    SyncTextRole = $"<color=#0000c8>{sourcePlayer.GetCustomRole().ToString()}</color>";
                    SyncTextTeam = "<color=#0000c8>Global Occult Collision</color>";
                    SyncTextObjective = "人類第一に、財団に抵抗せよ。";
                    break;

                default:
                    ApplyTeamFallback(sourcePlayer);
                    break;
            }
        }
        else
        {
            ApplyTeamFallback(sourcePlayer);
        }

        SyncTextEvent = Plugin.Singleton.SpecialEventsHandler.LocalizedEventName;

        HintSync(SyncType.PHUD_Role, SyncTextRole ?? "", targetForHint);
        HintSync(SyncType.PHUD_Objective, SyncTextObjective ?? "", targetForHint);
        HintSync(SyncType.PHUD_Team, SyncTextTeam ?? "", targetForHint);
        HintSync(SyncType.PHUD_Event, SyncTextEvent ?? "", targetForHint);
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

    // ===== 全体同期 =====

    public void SyncTexts(Player? spectator = null, Player? spectatedTarget = null)
    {
        SyncTextRole = null;
        SyncTextTeam = null;
        SyncTextObjective = null;
        SyncTextEvent = null;

        // 両方 null → 全員分を自分自身で同期
        if (spectator is null && spectatedTarget is null)
        {
            foreach (Player player in Player.List)
            {
                if (player == null || !player.IsConnected) continue;
                if (player.Role.Team == Team.Dead) continue;

                ApplyRoleInfo(player, player);
            }
        }
        // 観戦者 + 対象が両方 not null → 対象の情報を観戦者に同期
        else if (spectator is not null && spectatedTarget is not null)
        {
            if (!spectatedTarget.IsConnected) return;
            if (spectatedTarget.Role.Team == Team.Dead) return;

            ApplyRoleInfo(spectatedTarget, spectator);
        }
    }

    public void AllSyncHUD(ChangingRoleEventArgs ev)
    {
        var player = ev.Player;
        if (player == null) return;

        // 少し待ってからそのプレイヤーだけ確実に再同期
        Timing.CallDelayed(0.5f, () =>
        {
            if (player.Role.Team == Team.Dead || !player.IsConnected) return;
            ApplyRoleInfo(player, player);
        });
    }

    public void AllSyncHUD_()
    {
        // RoundStarted 時の再同期（保険）
        SyncTexts();
    }

    // ===== 観戦時の同期 =====

    public void Spectate(ChangingSpectatedPlayerEventArgs ev)
    {
        var spectator = ev.Player;

        // 観戦解除
        if (ev.NewTarget == null)
        {
            _spectateTargets.Remove(spectator.Id);

            // 自分自身の HUD を戻す
            if (spectator.Role.Team != Team.Dead && spectator.IsConnected)
                ApplyRoleInfo(spectator, spectator);

            return;
        }

        Player target = ev.NewTarget;
        _spectateTargets[spectator.Id] = target;

        // 1. ロール HUD 同期
        SyncTexts(spectator, target);

        var display = PlayerDisplay.Get(spectator);

        // 2. Specific HUD 即時同期
        var specificHint = display.GetHint("PlayerHUD_Specific");
        if (specificHint != null)
        {
            string roleSpecific = RoleSpecificTextProvider.GetFor(target);
            PHUD_Specific_Text = roleSpecific;
            specificHint.Text = PHUD_Specific_Text;
        }

        // 3. Ability HUD 即時同期
        var abilityHint = display.GetHint("PlayerHUD_Ability");
        if (abilityHint != null)
        {
            string abilityText = BuildAbilityHud(target);
            PHUD_Ability_Text = abilityText;
            abilityHint.Text = PHUD_Ability_Text;
        }
    }

    public void DestroyHints()
    {
        foreach (Player player in Player.List)
        {
            var display = PlayerDisplay.Get(player);
            display.ClearHint();
        }

        // 観戦ターゲットもリセット
        _spectateTargets.Clear();

        // ★ コルーチンは止めない（旧仕様の安定性維持）
    }

    // ===== Ability HUD =====

    private string BuildAbilityHud(Player target)
    {
        if (target.Role.Team == Team.Dead || !target.IsAlive)
            return string.Empty;

        if (!AbilityManager.TryGetLoadout(target, out var loadout))
            return string.Empty;

        var active = loadout.ActiveAbility;
        if (active == null)
            return string.Empty;

        if (!AbilityBase.TryGetAbilityState(
                target,
                active,
                out bool canUse,
                out float cdRemain,
                out int usesLeft,
                out int maxUses))
            return string.Empty;

        string abilityKey = active.GetType().Name;
        string abilityName = AbilityLocalization.GetDisplayName(abilityKey, target);

        string cdText = canUse
            ? "<color=green>READY</color>"
            : $"<color=yellow>{(int)cdRemain}s</color>";

        string usesText = (maxUses < 0) ? "∞" : usesLeft.ToString();

        return $"<color=#ffcc00>[{abilityName}]</color> CD: {cdText} Uses: {usesText}";
    }

    private IEnumerator<float> TaskSync()
    {
        yield return Timing.WaitForSeconds(2f);

        for (;;)
        {
            if (Round.IsLobby)
            {
                yield return Timing.WaitForSeconds(1f);
                continue;
            }

            SyncTexts();
            yield return Timing.WaitForSeconds(3f);
        }
    }

    private IEnumerator<float> AbilityHudLoop()
    {
        yield return Timing.WaitForSeconds(0.5f);

        for (;;)
        {
            if (Round.IsLobby)
            {
                yield return Timing.WaitForSeconds(0.5f);
                continue;
            }

            foreach (var player in Player.List)
            {
                if (player == null || !player.IsAlive)
                    continue;

                var display = PlayerDisplay.Get(player);
                var abilityHint = display.GetHint("PlayerHUD_Ability");

                if (abilityHint == null)
                {
                    PlayerHUDSetup(player);
                    abilityHint = display.GetHint("PlayerHUD_Ability");
                    if (abilityHint == null)
                        continue;
                }

                // 観戦者ならターゲット側の Ability を見る
                var hudTarget = player;
                if (player.Role.Team == Team.Dead &&
                    _spectateTargets.TryGetValue(player.Id, out var t) &&
                    t != null && t.IsAlive)
                    hudTarget = t;

                string abilityText = BuildAbilityHud(hudTarget);
                PHUD_Ability_Text = abilityText;
                abilityHint.Text = PHUD_Ability_Text;
            }

            yield return Timing.WaitForSeconds(0.5f);
        }
    }

    private IEnumerator<float> SpecificInfoHudLoop()
    {
        yield return Timing.WaitForSeconds(1f);

        for (;;)
        {
            if (Round.IsLobby)
            {
                yield return Timing.WaitForSeconds(1f);
                continue;
            }

            foreach (var player in Player.List)
            {
                if (player == null || !player.IsAlive) continue;

                var hudTarget = player;
                if (player.Role.Team == Team.Dead &&
                    _spectateTargets.TryGetValue(player.Id, out var t) &&
                    t != null && t.IsAlive)
                    hudTarget = t;

                string roleSpecific = RoleSpecificTextProvider.GetFor(hudTarget);

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
                    specificHint.Text = string.Empty;
                    continue;
                }

                PHUD_Specific_Text = roleSpecific;
                specificHint.Text = PHUD_Specific_Text;
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }
}
