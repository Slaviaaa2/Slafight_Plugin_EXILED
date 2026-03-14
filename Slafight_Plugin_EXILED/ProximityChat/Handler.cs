using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using PlayerRoles.Spectating;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;
using SpectatorRole = PlayerRoles.Spectating.SpectatorRole;

namespace Slafight_Plugin_EXILED.ProximityChat;

public static class Handler
{
    public static void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.VoiceChatting += OnPlayerUsingVoiceChat;
        Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestarted;
        
        Exiled.Events.Handlers.Player.ChangingRole += OnPlayerChangingRole;
    }
    
    public static void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.VoiceChatting -= OnPlayerUsingVoiceChat;
        Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestarted;

        Exiled.Events.Handlers.Player.ChangingRole -= OnPlayerChangingRole;
    }
    public static List<Player> ActivatedPlayers = new List<Player>();
    public static List<Player> CanUsePlayers = new List<Player>();
    
    public static List<CRoleTypeId> AllowedUniqueRoles = new()
    {
        CRoleTypeId.Zombified,
        CRoleTypeId.Scp3114
    };
    public static List<CRoleTypeId> OnlyProximityUnique = new()
    {
        CRoleTypeId.Zombified
    };
    
    public static List<RoleTypeId> AllowedRoleTypes = new List<RoleTypeId>()
    {
        RoleTypeId.Scp049,
        RoleTypeId.Scp939,
        RoleTypeId.Scp3114
    };
    public static List<RoleTypeId> OnlyProximity = new List<RoleTypeId>()
    {
        
    };

    private static void OnPlayerChangingRole(ChangingRoleEventArgs ev)
    {
        Timing.CallDelayed(1.1f, () =>
        {
            ActivatedPlayers.Remove(ev.Player);
            CanUsePlayers.Remove(ev.Player);
            if (ev.Player.GetCustomRole() == CRoleTypeId.None)
            {
                if (AllowedRoleTypes.Contains(ev.Player.Role))
                    CanUsePlayers.Add(ev.Player);
            }
            else if (AllowedUniqueRoles.Contains(ev.Player.GetCustomRole()))
            {
                CanUsePlayers.Add(ev.Player);
            }
            if (CanUsePlayers.Contains(ev.Player))
            {
                var listText = string.Join(", ", CanUsePlayers.ConvertAll(p => $"{p.Nickname}({p.Id})"));
                Log.Debug($"CanUsePlayers Updated. List: {listText}");
                if (ev.Player?.UniqueRole != "Zombified")
                {
                    ev.Player.ShowHint("近接チャット機能が利用可能です！",5f);
                }
                else
                {
                    ev.Player.ShowHint("近接チャットしか利用できません！",5f);
                }
            }
        });
    }
    
    private static void OnRoundRestarted()
    {
        ActivatedPlayers.Clear();
        CanUsePlayers.Clear();
    }
    
    // Handler クラス内のどこかに追加
    private static readonly List<(int NetId, long Ticks)> ResentMessages = new();

    // 古いエントリを捨てるための有効時間（ms）
    private const int SignatureLifetimeMs = 2000;

    private static void CleanupOldSignatures()
    {
        long threshold = DateTime.UtcNow.AddMilliseconds(-SignatureLifetimeMs).Ticks;
        ResentMessages.RemoveAll(sig => sig.Ticks < threshold);
    }

    
    public static void OnPlayerUsingVoiceChat(VoiceChattingEventArgs args)
    {
        // 自分が再送した Proximity は一切触らない
        if (args.VoiceMessage.Channel == VoiceChatChannel.Proximity)
            return;

        // SCPチャット以外は触らない
        if (args.VoiceMessage.Channel != VoiceChatChannel.ScpChat)
            return;

        // 対象ロールか？
        if (!CanUsePlayers.Contains(args.Player))
            return;

        // トグルONにしているか？
        if (!ActivatedPlayers.Contains(args.Player))
            return;

        // ここまで来た人だけ近接に変換
        SendProximityMessage(args.VoiceMessage, 5f);

        // 「OnlyProximity」の人だけ元のSCPチャットを殺すならこう
        if (OnlyProximity.Contains(args.Player.Role) ||
            OnlyProximityUnique.Contains(args.Player.GetCustomRole()))
            args.IsAllowed = false;
    }

    
    private static int _resendId = 1;
    private static readonly HashSet<int> ResentIds = new();

    private static void SendProximityMessage(VoiceMessage msg, float maxRange = 5f)
    {
        foreach (ReferenceHub referenceHub in ReferenceHub.AllHubs)
        {
            if (referenceHub.roleManager.CurrentRole is SpectatorRole spectator
                && !msg.Speaker.IsSpectatedBy(referenceHub))
                continue;

            if (referenceHub.roleManager.CurrentRole is not PlayerRoles.Voice.IVoiceRole voiceRole2)
                continue;

            if (Vector3.Distance(msg.Speaker.transform.position, referenceHub.transform.position) >= maxRange)
                continue;

            if (voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, VoiceChatChannel.Proximity)
                is VoiceChatChannel.None)
                continue;

            var newMsg = msg;                       // struct コピー
            newMsg.Channel = VoiceChatChannel.Proximity;
            referenceHub.connectionToClient.Send(newMsg);
        }
    }


}