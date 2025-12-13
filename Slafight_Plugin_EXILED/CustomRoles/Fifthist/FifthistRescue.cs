using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.Fifthist;

public class FifthistRescue : CustomRole
{
    public override uint Id { get; set; } = 1;
    public override string Name { get; set; } = "FIFTHIST";
    public override string Description { get; set; } = "<color=#ff00fa>第五教会 救出師</color>\n非常に<color=#ff00fa>第五的</color>な存在を脱出させなければいけない";
    public override string CustomInfo { get; set; } = "<color=#ff00fa>FIFTHIST RESCURE - 第五教会 救出師</color>";
    public override int MaxHealth { get; set; } = 150;
    public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;

    public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new Dictionary<AmmoType, ushort>();

    public override void AddRole(Player player)
    {
        player.Health = MaxHealth;
        Room SpawnRoom = Room.Get(RoomType.Surface);
        Log.Debug(SpawnRoom.Position);
        Vector3 offset = new Vector3(0f,0f,0f);
        player.Position = new Vector3(124f,289f,21f);//SpawnRoom.Position + SpawnRoom.Rotation * offset;
        //player.Rotation = SpawnRoom.Rotation;
        base.AddRole(player);
    }

    protected override void RoleAdded(Player player)
    {
        player.CustomInfo = "<color=magenta>FIFTHIST RESCURE - 第五教会 救出師</color>";
        player.InfoArea |= PlayerInfoArea.Nickname;
        base.RoleAdded(player);
    }

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting += CustomFriendlyFire_hurt;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting -= CustomFriendlyFire_hurt;
        base.UnsubscribeEvents();
    }
    
    private void CustomFriendlyFire_hurt(HurtingEventArgs ev)
    {
        if (ev.Attacker == null || ev.Player == null)
            return; // 攻撃者またはプレイヤーがnullの場合は処理終了
        if (Check(ev.Attacker))
        {
            if (ev.Player?.UniqueRole == "SCP-3005")
            {
                ev.IsAllowed = false;
                ev.Attacker.Hurt(15f,"<color=#ff00fa>第五的存在</color>に反逆した為");
                ev.Attacker.ShowHint("<color=#ff00fa>第五的存在</color>に反逆するとは何事か！？",5f);
            }
        }
    }
}