#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Utilities;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomMaps;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.MainHandlers;
using Slafight_Plugin_EXILED.SpecialEvents;
using UnityEngine;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace Slafight_Plugin_EXILED.Hints;

public class PlayerHUD
{
    private CoroutineHandle _specificAbilityLoop;
    private CoroutineHandle _abilityHudLoop;
    private CoroutineHandle _taskSyncLoop;
    private CoroutineHandle _debugHudLoop;

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
        _debugHudLoop = Timing.RunCoroutine(DebugHudLoop());
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
        
        if (_debugHudLoop.IsRunning)
            Timing.KillCoroutines(_debugHudLoop);
    }

    private string ServerInfo_Text = string.Empty;
    private string PHUD_Role_Text = string.Empty;
    private string PHUD_Objective_Text = string.Empty;
    private string PHUD_Team_Text = string.Empty;
    private string PHUD_Event_Text = string.Empty;
    private string PHUD_Specific_Text = string.Empty;
    private string PHUD_Ability_Text = string.Empty;
    private string PHUD_Debug_Text = string.Empty;

    // =========================================================
    // ヘルパー
    // =========================================================

    /// <summary>プレイヤーが安全に操作できる状態かどうか確認する</summary>
    private static bool IsPlayerValid(Player? p)
    {
        try
        {
            return p != null && p.IsConnected && p.ReferenceHub != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>PlayerDisplay を安全に取得する。失敗時は null を返す</summary>
    private static PlayerDisplay? TryGetDisplay(Player p)
    {
        try
        {
            return PlayerDisplay.Get(p.ReferenceHub);
        }
        catch
        {
            return null;
        }
    }

    // =========================================================
    // ServerInfoHint / Setup / Main
    // =========================================================

    public void ServerInfoHint(VerifiedEventArgs ev)
    {
        if (ev?.Player == null) return; // FIX: nullガード

        var display = TryGetDisplay(ev.Player);
        if (display == null) return;

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
        if (!IsPlayerValid(player)) return; // FIX: nullガード

        var display = TryGetDisplay(player);
        if (display == null) return;

        int XCordinate = -350;

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
        Hint PlayerHUD_Debug = new()
        {
            Id = "PlayerHUD_Debug",
            Text = "",
            Alignment = HintAlignment.Left,
            SyncSpeed = HintSyncSpeed.Fast,
            FontSize = 24,
            XCoordinate = XCordinate,
            YCoordinate = 260
        };

        display.AddHint(PlayerHUD_Role);
        display.AddHint(PlayerHUD_Objective);
        display.AddHint(PlayerHUD_Team);
        display.AddHint(PlayerHUD_Event);
        display.AddHint(PlayerHUD_Specific);
        display.AddHint(PlayerHUD_Ability);
        display.AddHint(PlayerHUD_Debug);
    }

    public void PlayerHUDMain()
    {
        // 旧仕様寄り：RoundStarted 時点で全員分 HUD 作成
        foreach (Player player in Player.List.ToList()) // FIX: ToList()
        {
            if (!IsPlayerValid(player)) continue;
            PlayerHUDSetup(player);
            ApplyRoleInfo(player, player);
        }
    }

    // =========================================================
    // HintSync
    // =========================================================

    public void HintSync(SyncType syncType, string hintText, Player player)
    {
        if (!IsPlayerValid(player)) return; // FIX: nullガード

        var display = TryGetDisplay(player);
        if (display == null) return;

        try
        {
            switch (syncType)
            {
                case SyncType.ServerInfo:
                    ServerInfo_Text = hintText;
                    var si = display.GetHint("ServerInfo");
                    if (si != null) si.Text = ServerInfo_Text;
                    break;
                case SyncType.PHUD_Role:
                    PHUD_Role_Text = hintText;
                    var role = display.GetHint("PlayerHUD_Role");
                    if (role != null) role.Text = "Role: " + PHUD_Role_Text;
                    break;
                case SyncType.PHUD_Objective:
                    PHUD_Objective_Text = hintText;
                    var obj = display.GetHint("PlayerHUD_Objective");
                    if (obj != null) obj.Text = "Objective: " + PHUD_Objective_Text;
                    break;
                case SyncType.PHUD_Team:
                    PHUD_Team_Text = hintText;
                    var team = display.GetHint("PlayerHUD_Team");
                    if (team != null) team.Text = "Team: " + PHUD_Team_Text;
                    break;
                case SyncType.PHUD_Event:
                    PHUD_Event_Text = hintText;
                    var ev = display.GetHint("PlayerHUD_Event");
                    if (ev != null) ev.Text = "[Event]\n<size=28>" + PHUD_Event_Text + "</size>";
                    break;
                case SyncType.PHUD_Ability:
                    PHUD_Ability_Text = hintText;
                    var ab = display.GetHint("PlayerHUD_Ability");
                    if (ab != null) ab.Text = PHUD_Ability_Text;
                    break;
                case SyncType.PHUD_Debug:
                    PHUD_Debug_Text = hintText;
                    var db = display.GetHint("PlayerHUD_Debug");
                    if (db != null) db.Text = PHUD_Debug_Text;
                    break;
            }
        }
        catch (Exception e)
        {
            Log.Debug($"[HintSync] Exception for {player.Nickname}: {e.Message}");
        }
    }

    // =========================================================
    // ロール情報構築
    // =========================================================

    string? SyncTextRole = null;
    string? SyncTextTeam = null;
    string? SyncTextObjective = null;
    string? SyncTextEvent = null;

    private void ApplyRoleInfo(Player sourcePlayer, Player targetForHint)
    {
        if (!IsPlayerValid(sourcePlayer)) return; // FIX: nullガード
        if (!IsPlayerValid(targetForHint)) return; // FIX: nullガード

        try
        {
            // FacilityTermination 中の財団側特殊処理
            if (SpecialEventsHandler.Instance.NowEvent == SpecialEventType.FacilityTermination)
            {
                var cteam = sourcePlayer.GetTeam();

                if (cteam is CTeam.FoundationForces or CTeam.Guards &&
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
                    // SCPs
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
                    case CRoleTypeId.Scp035:
                        SyncTextRole = "<color=#c50000>SCP-035</color>";
                        SyncTextTeam = "<color=#c50000>The SCPs</color>";
                        SyncTextObjective = "あなたは仮面に乗っ取られ、精神が不安定になっている。<color=red>核弾頭を起動しろ</color>";
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
                    case CRoleTypeId.ChaosTacticalUnit:
                        SyncTextRole = "<color=#228b22>Chaos Insurgency Tactical Unit</color>";
                        SyncTextTeam = "<color=#228b22>Chaos Insurgency</color>";
                        SyncTextObjective = "Dクラス職員を救出し、施設を略奪せよ。";
                        break;
                    case CRoleTypeId.ChaosBreaker:
                        SyncTextRole = "<color=#228b22>Chaos Insurgency Breaker</color>";
                        SyncTextTeam = "<color=#228b22>Chaos Insurgency</color>";
                        SyncTextObjective = "Dクラス職員を救出し、施設を略奪せよ。";
                        break;
                    case CRoleTypeId.ChaosUndercoverAgent:
                        SyncTextRole = "<color=#228b22>Chaos Insurgency Undercover Agent</color>";
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
                    case CRoleTypeId.SnePurify:
                        SyncTextRole = "<color=#FF1493>MTF Eta-10: Purify</color>";
                        SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                        SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                        break;
                    case CRoleTypeId.SneNeutralitist:
                        SyncTextRole = "<color=#FF1493>MTF Eta-10: Neutralitist</color>";
                        SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                        SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                        break;
                    case CRoleTypeId.SneGears:
                        SyncTextRole = "<color=#FF1493>MTF Eta-10: Gears</color>";
                        SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                        SyncTextObjective = "研究員を救出し、施設の秩序を守護せよ。";
                        break;
                    case CRoleTypeId.SneOperator:
                        SyncTextRole = "<color=#FF1493>MTF Eta-10: Operator</color>";
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
                    case CRoleTypeId.CandyWarrierApril or CRoleTypeId.CandyWarrierHalloween:
                        SyncTextRole = "<b><color=#ffffff>CANDY WARRIER</color></b>";
                        SyncTextTeam = "<b><color=#ffffff>CANDY WARRIER's DIVISION</color></b>";
                        SyncTextObjective = "全施設にFunnyなお菓子の正義を執行しろ";
                        break;

                    // Special
                    case CRoleTypeId.Sculpture:
                        SyncTextRole = "<color=#00b7eb>Sculpture</color>";
                        SyncTextTeam = "<color=#00b7eb>The Foundation</color>";
                        SyncTextObjective = "財団に従い、人類を根絶させよ。";
                        break;

                    case CRoleTypeId.SergeyMakarov:
                        SyncTextRole = "<color=#dc143c>Facility Manager - Sergey Makarov</color>";
                        SyncTextTeam = "<color=#faff86>The Foundation</color>";
                        SyncTextObjective = "持てる全てを使い、<color=#228b22><b>奴ら</b></color>への<color=red><b>復讐</b></color>を果たせ";
                        break;

                    case CRoleTypeId.SergeyMakarovAwaken:
                        SyncTextRole = "<color=red>Cursemaster - Sergey Makarov</color>";
                        SyncTextTeam = "<color=#a0a0a0>Alone</color>";
                        SyncTextObjective = "<color=red><b>邪魔者を滅ぼし、サイト-02から毒を浄化せよ</b></color>";
                        break;

                    // GoC
                    case CRoleTypeId.GoCOperative:
                        SyncTextRole = "<color=#0000c8>Broken Dagger: Operative</color>";
                        SyncTextTeam = "<color=#0000c8>Global Occult Coalition</color>";
                        SyncTextObjective = "人類第一に、財団に抵抗せよ。";
                        break;
                    case CRoleTypeId.GoCThaumaturgist:
                        SyncTextRole = "<color=#0000c8>Broken Dagger: Thaumaturgist</color>";
                        SyncTextTeam = "<color=#0000c8>Global Occult Coalition</color>";
                        SyncTextObjective = "人類第一に、財団に抵抗せよ。";
                        break;
                    case CRoleTypeId.GoCCommunications:
                        SyncTextRole = "<color=#0000c8>Broken Dagger: Communications</color>";
                        SyncTextTeam = "<color=#0000c8>Global Occult Coalition</color>";
                        SyncTextObjective = "人類第一に、財団に抵抗せよ。";
                        break;
                    case CRoleTypeId.GoCMedic:
                        SyncTextRole = "<color=#0000c8>Broken Dagger: Medic</color>";
                        SyncTextTeam = "<color=#0000c8>Global Occult Coalition</color>";
                        SyncTextObjective = "人類第一に、財団に抵抗せよ。";
                        break;
                    case CRoleTypeId.GoCDeputy:
                        SyncTextRole = "<color=#0000c8>Broken Dagger: Deputy</color>";
                        SyncTextTeam = "<color=#0000c8>Global Occult Coalition</color>";
                        SyncTextObjective = "人類第一に、財団に抵抗せよ。";
                        break;
                    case CRoleTypeId.GoCSquadLeader:
                        SyncTextRole = "<color=#0000c8>Broken Dagger: Squad Leader</color>";
                        SyncTextTeam = "<color=#0000c8>Global Occult Coalition</color>";
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
        catch (Exception e)
        {
            Log.Debug($"[ApplyRoleInfo] Exception for {sourcePlayer?.Nickname}: {e.Message}");
        }
    }

    private void ApplyTeamFallback(Player player)
    {
        if (!IsPlayerValid(player)) return; // FIX: nullガード

        switch (player.Role?.Team) // FIX: Role nullガード
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
                SyncTextRole = "<color=#ffffff>" + player.Role?.Name + "</color>";
                SyncTextTeam = "<color=#ffffff>[Unknown]</color>";
                SyncTextObjective = "[Unknown]";
                break;
        }
    }

    // =========================================================
    // 全体同期
    // =========================================================

    public void SyncTexts(Player? spectator = null, Player? spectatedTarget = null)
    {
        SyncTextRole = null;
        SyncTextTeam = null;
        SyncTextObjective = null;
        SyncTextEvent = null;

        // 両方 null → 全員分を自分自身で同期
        if (spectator is null && spectatedTarget is null)
        {
            foreach (Player player in Player.List.ToList()) // FIX: ToList()
            {
                if (!IsPlayerValid(player)) continue;
                if (player.Role?.Team == Team.Dead) continue;

                ApplyRoleInfo(player, player);
            }
        }
        // 観戦者 + 対象が両方 not null → 対象の情報を観戦者に同期
        else if (spectator is not null && spectatedTarget is not null)
        {
            if (!IsPlayerValid(spectatedTarget)) return; // FIX: IsPlayerValidで一括確認
            if (spectatedTarget.Role?.Team == Team.Dead) return;

            ApplyRoleInfo(spectatedTarget, spectator);
        }
    }

    public void AllSyncHUD(ChangingRoleEventArgs ev)
    {
        var player = ev?.Player; // FIX: nullガード
        if (player == null) return;

        Timing.CallDelayed(0.5f, () =>
        {
            if (!IsPlayerValid(player)) return; // FIX: 遅延後の生存確認
            if (player.Role?.Team == Team.Dead) return;
            ApplyRoleInfo(player, player);
        });
    }

    public void AllSyncHUD_()
    {
        SyncTexts();
    }

    // =========================================================
    // 観戦時の同期
    // =========================================================

    public void Spectate(ChangingSpectatedPlayerEventArgs ev)
    {
        // FIX: ev・spectator の nullガード
        if (ev?.Player == null) return;
        var spectator = ev.Player;
        if (!IsPlayerValid(spectator)) return;

        // 観戦解除（NewTarget が null）
        if (ev.NewTarget == null)
        {
            _spectateTargets.Remove(spectator.Id);

            // 自分自身の HUD を戻す
            if (IsPlayerValid(spectator) && spectator.Role?.Team != Team.Dead)
                ApplyRoleInfo(spectator, spectator);

            return;
        }

        var target = ev.NewTarget;

        // FIX: ターゲットの安全確認
        if (!IsPlayerValid(target)) return;

        _spectateTargets[spectator.Id] = target;

        // 1. ロール HUD 同期
        SyncTexts(spectator, target);

        // FIX: PlayerDisplay 取得を安全なヘルパーで実施
        var display = TryGetDisplay(spectator);
        if (display == null) return;

        // 2. Specific HUD 即時同期
        var specificHint = display.GetHint("PlayerHUD_Specific");
        if (specificHint != null)
        {
            try
            {
                specificHint.Text = RoleSpecificTextProvider.GetFor(target);
            }
            catch (Exception e)
            {
                Log.Debug($"[Spectate] Specific hint error: {e.Message}");
            }
        }

        // 3. Ability HUD 即時同期
        var abilityHint = display.GetHint("PlayerHUD_Ability");
        if (abilityHint != null)
        {
            try
            {
                abilityHint.Text = BuildAbilityHud(target);
            }
            catch (Exception e)
            {
                Log.Debug($"[Spectate] Ability hint error: {e.Message}");
            }
        }
    }

    // =========================================================
    // DestroyHints
    // =========================================================

    public void DestroyHints()
    {
        foreach (Player player in Player.List.ToList()) // FIX: ToList()
        {
            if (!IsPlayerValid(player)) continue; // FIX: nullガード
            try
            {
                var display = TryGetDisplay(player);
                display?.ClearHint();
            }
            catch (Exception e)
            {
                Log.Debug($"[DestroyHints] Exception for {player?.Nickname}: {e.Message}");
            }
        }

        _spectateTargets.Clear();

        // ★ コルーチンは止めない（旧仕様の安定性維持）
    }

    // =========================================================
    // Ability HUD
    // =========================================================

    private string BuildAbilityHud(Player target)
    {
        if (!IsPlayerValid(target)) return string.Empty; // FIX: nullガード
        if (!target.IsAlive) return string.Empty;

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

    // =========================================================
    // コルーチン
    // =========================================================

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

            foreach (var player in Player.List.ToList()) // FIX: ToList()
            {
                // FIX: IsPlayerValid で一括確認
                if (!IsPlayerValid(player)) continue;

                var display = TryGetDisplay(player);
                if (display == null) continue;

                var abilityHint = display.GetHint("PlayerHUD_Ability");
                if (abilityHint == null)
                {
                    PlayerHUDSetup(player);
                    abilityHint = display.GetHint("PlayerHUD_Ability");
                    if (abilityHint == null) continue;
                }

                // 観戦者ならターゲット側の Ability を見る
                var hudTarget = player;
                if (player.Role?.Team == Team.Dead &&
                    _spectateTargets.TryGetValue(player.Id, out var t) &&
                    IsPlayerValid(t) && t.IsAlive) // FIX: IsPlayerValid で一括確認
                    hudTarget = t;

                try
                {
                    abilityHint.Text = BuildAbilityHud(hudTarget);
                }
                catch (Exception e)
                {
                    Log.Debug($"[AbilityHudLoop] Exception for {player.Nickname}: {e.Message}");
                }
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

            foreach (var player in Player.List.ToList()) // FIX: ToList()
            {
                if (!IsPlayerValid(player)) continue; // FIX: IsPlayerValid で一括確認

                // 観戦者ならターゲット側の情報を見る
                var hudTarget = player;
                if (player.Role?.Team == Team.Dead &&
                    _spectateTargets.TryGetValue(player.Id, out var t) &&
                    IsPlayerValid(t) && t.IsAlive) // FIX: IsPlayerValid で一括確認
                    hudTarget = t;

                var display = TryGetDisplay(player);
                if (display == null) continue;

                var specificHint = display.GetHint("PlayerHUD_Specific");
                if (specificHint == null)
                {
                    PlayerHUDSetup(player);
                    specificHint = display.GetHint("PlayerHUD_Specific");
                    if (specificHint == null) continue;
                }

                try
                {
                    string roleSpecific = RoleSpecificTextProvider.GetFor(hudTarget);

                    specificHint.Text = string.IsNullOrEmpty(roleSpecific)
                        ? string.Empty
                        : roleSpecific;
                }
                catch (Exception e)
                {
                    Log.Debug($"[SpecificInfoHudLoop] Exception for {player.Nickname}: {e.Message}");
                }
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }
    
    /// <summary>
    /// デバッグモード ON のプレイヤーに対して 0.1 秒ごとに
    /// PHUD_Debug ヒントを更新するループ。
    /// </summary>
    private IEnumerator<float> DebugHudLoop()
    {
        yield return Timing.WaitForSeconds(0.5f);
 
        for (;;)
        {
            if (Round.IsLobby)
            {
                yield return Timing.WaitForSeconds(0.5f);
                continue;
            }
 
            foreach (var player in Player.List.ToList())
            {
                if (!IsPlayerValid(player)) continue;
                if (!DebugModeHandler.IsDebugMode(player)) continue;
 
                var display = TryGetDisplay(player);
                if (display == null) continue;
 
                var hint = display.GetHint("PlayerHUD_Debug");
                if (hint == null)
                {
                    PlayerHUDSetup(player);
                    hint = display.GetHint("PlayerHUD_Debug");
                    if (hint == null) continue;
                }
 
                try
                {
                    hint.Text = BuildDebugHud(player);
                }
                catch (Exception e)
                {
                    Log.Debug($"[DebugHudLoop] Exception for {player.Nickname}: {e.Message}");
                }
            }
 
            yield return Timing.WaitForSeconds(0.1f);
        }
    }
    
    private static string BuildDebugHud(Player player)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<size=18><color=#ffff00>[DEBUG MODE]</color>");

        // ── ロール・チーム情報 ────────────────────────────────────────
        sb.AppendLine(
            $"<color=#aaaaaa>Role:</color> {player.Role?.Name ?? "None"}  " +
            $"<color=#aaaaaa>Team:</color> {player.Role?.Team.ToString() ?? "None"}  " +
            $"<color=#aaaaaa>CRole:</color> {player.GetCustomRole()}  " +
            $"<color=#aaaaaa>CTeam:</color> {player.GetTeam()}"
        );

        // ── 座標・ルーム情報（リアルタイム） ─────────────────────────
        var pos  = player.Position;
        var room = player.CurrentRoom;
        sb.AppendLine(
            $"<color=#aaaaaa>World:</color> ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})  " +
            $"<color=#aaaaaa>Room:</color> {room?.Type.ToString() ?? "None"} " +
            $"<color=#aaaaaa>Zone:</color> {player.Zone.ToString()}"
        );
        if (room != null)
        {
            var invRot     = Quaternion.Inverse(room.Rotation);
            var localPos   = invRot * (pos - room.Position);
            var localEuler = invRot.eulerAngles;
            var roomEuler  = room.Rotation.eulerAngles;
            sb.AppendLine(
                $"<color=#aaaaaa>Local:</color> ({localPos.x:F2}, {localPos.y:F2}, {localPos.z:F2})  " +
                $"<color=#aaaaaa>LocalRot:</color> ({localEuler.x:F1}, {localEuler.y:F1}, {localEuler.z:F1})"
            );
            sb.AppendLine(
                $"<color=#aaaaaa>RoomRot:</color> ({roomEuler.x:F1}, {roomEuler.y:F1}, {roomEuler.z:F1})"
            );
        }

        // ── 最後に触ったドア情報 ─────────────────────────────────────
        if (DebugModeHandler.TryGetDoor(player, out var door))
        {
            sb.AppendLine(
                $"<color=#aaaaaa>Door:</color> {door.DoorType}  " +
                $"<color=#aaaaaa>Name:</color> {door.DoorName}  " +
                $"<color=#aaaaaa>Room:</color> {door.RoomType}"
            );
            sb.AppendLine(
                $"<color=#aaaaaa>DoorLocal:</color> ({door.LocalPos.x:F2}, {door.LocalPos.y:F2}, {door.LocalPos.z:F2})  " +
                $"<color=#aaaaaa>DoorRot:</color> ({door.LocalEuler.x:F1}, {door.LocalEuler.y:F1}, {door.LocalEuler.z:F1})"
            );
            sb.AppendLine(
                $"<color=#aaaaaa>DoorRoomRot:</color> ({door.RoomEuler.x:F1}, {door.RoomEuler.y:F1}, {door.RoomEuler.z:F1})"
            );
        }
        else
        {
            sb.AppendLine("<color=#666666>Door: -- (ドアに触れると更新)</color>");
        }

        // ── ラウンド状態フラグ ────────────────────────────────────────
        static string Bool(bool v) => v ? "<color=green>T</color>" : "<color=red>F</color>";
        sb.AppendLine(
            $"<color=#aaaaaa>Round:</color> " +
            $"InProgress={Bool(Round.InProgress)}  " +
            $"IsStarted={Bool(Round.IsStarted)}  " +
            $"IsEnded={Bool(Round.IsEnded)}  " +
            $"IsLobby={Bool(Round.IsLobby)}  " +
            $"IsLocked={Bool(Round.IsLocked)}  " +
            $"IsLobbyLocked={Bool(Round.IsLobbyLocked)}"
        );
        sb.AppendLine(
            $"<color=#aaaaaa>Elapsed:</color> {Round.ElapsedTime:mm\\:ss}  " +
            $"<color=#aaaaaa>UptimeRounds:</color> {Round.UptimeRounds}  " +
            $"<color=#aaaaaa>All Players:</color> {Player.List.Count} " +
            $"<color=#aaaaaa>Connected Players:</color> {Player.List.Count(p => !p.IsNPC)} " +
            $"<color=#aaaaaa>Npcs:</color> {Npc.List.Count} "
        );

        // ── 核弾頭タイマー情報 ───────────────────────────────────────
        if (Warhead.IsInProgress)
        {
            sb.AppendLine(
                $"<color=#ff4444>Warhead:</color> " +
                $"DetonationTimer={Warhead.DetonationTimer:F1}  " +
                $"RealTimer={Warhead.RealDetonationTimer:F1}  " +
                $"IsLocked={Bool(Warhead.IsLocked)} " +
                $"IsBooming={Bool(MapFlags.IsWarheadBooming)} "
            );
        }
        else
        {
            sb.AppendLine("<color=#666666>Warhead: Not active</color>");
        }
        
        // ── 有効なエフェクト一覧 ─────────────────────────────────────
        var activeEffects = player.ActiveEffects.ToList();
        if (activeEffects.Count == 0)
        {
            sb.AppendLine("<color=#666666>Effects: None</color>");
        }
        else
        {
            sb.AppendLine("<color=#aaaaaa>Effects:</color>");
            foreach (var effect in activeEffects)
            {
                string duration = effect.Duration > 0f
                    ? $"{effect.TimeLeft:F0}"
                    : "∞";
                sb.AppendLine(
                    $"- <color=#88ddff>{effect.GetType().Name,-24}</color>" +
                    $"| Intensity: {effect.Intensity,-3} Duration: {duration}"
                );
            }
        }

        // ────────────────────────────────────────────────────────────
        // ★ 新しい項目はここに追加するだけでOK
        // 例:
        // sb.AppendLine($"<color=#aaaaaa>HP:</color> {player.Health:F0}/{player.MaxHealth:F0}");
        // ────────────────────────────────────────────────────────────

        sb.Append("</size>");
        return sb.ToString();
}
}