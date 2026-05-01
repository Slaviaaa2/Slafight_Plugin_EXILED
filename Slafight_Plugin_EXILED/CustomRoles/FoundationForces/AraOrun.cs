using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using VoiceChat;

namespace Slafight_Plugin_EXILED.CustomRoles.FoundationForces;

public class AraOrun : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.AraOrun;
    protected override CTeam Team { get; set; } = CTeam.FoundationForces;
    protected override string UniqueRoleKey { get; set; } = "AraOrun";

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.ReceivingVoiceMessage += TrashMessages;
        Exiled.Events.Handlers.Player.VoiceChatting += DenySpeakInScpChat;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.ReceivingVoiceMessage -= TrashMessages;
        Exiled.Events.Handlers.Player.VoiceChatting -= DenySpeakInScpChat;
        base.UnregisterEvents();
    }

    public override void SpawnRole(Player? player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player!.Role.Set(RoleTypeId.Scp079);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
            
        player.SetCustomInfo("Ara Orun");
        Timing.CallDelayed(0.05f, () =>
        {
            if (player.Role is Scp079Role scp079Role)
            {
                scp079Role.Level = 5;
                scp079Role.Energy = scp079Role.MaxEnergy;
            }
            player.ShowHint($"<size=24><color={CTeam.FoundationForces.GetTeamColor()}>アラ・オルン</color>\nわーくいんぷろぐれすだにゃ",10f);
        });
    }

    private void TrashMessages(ReceivingVoiceMessageEventArgs ev)
    {
        if (!Check(ev.Player)) return;
        if (ev.VoiceMessage.Channel == VoiceChatChannel.ScpChat)
        {
            ev.IsAllowed = false;
        }
    }

    private void DenySpeakInScpChat(VoiceChattingEventArgs ev)
    {
        if (!Check(ev.Player)) return;
        if (ev.VoiceMessage.Channel == VoiceChatChannel.ScpChat)
        {
            ev.IsAllowed = false;
        }
    }
}