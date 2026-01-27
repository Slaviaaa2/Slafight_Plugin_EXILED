using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp3114Role : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.Scp3114;
    protected override CTeam Team { get; set; } = CTeam.SCPs;
    protected override string UniqueRoleKey { get; set; } = "Scp3114";

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Scp3114.Disguised += ExtendTime;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Scp3114.Disguised -= ExtendTime;
        base.UnregisterEvents();
    }
    
    public override void SpawnRole(Player player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scp3114);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 3114;
        player.Health = player.MaxHealth;
        player.ClearInventory();

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Commander");
        player.CustomInfo = "<color=#C50000>SCP-3114</color>";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
            
        Room SpawnRoom = Room.Get(RoomType.Hcz127);
        Log.Debug(SpawnRoom.Position);
        Vector3 offset = new Vector3(0f,13f,0f);
        player.Position = SpawnRoom.Position + SpawnRoom.Rotation * offset;
        player.Rotation = SpawnRoom.Rotation;
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<color=red>SCP-3114</color>\nSkeleton Pan for you!",10f);
            Ragdoll classd = Ragdoll.CreateAndSpawn(RoleTypeId.ClassD, "D-9341","For You",SpawnRoom.Position + SpawnRoom.Rotation * offset,SpawnRoom.Rotation);
            Ragdoll scientist = Ragdoll.CreateAndSpawn(RoleTypeId.Scientist, "Dr. Maynard","For You",SpawnRoom.Position + SpawnRoom.Rotation * offset,SpawnRoom.Rotation);
        });
    }

    private void ExtendTime(Exiled.Events.EventArgs.Scp3114.DisguisedEventArgs ev)
    {
        ev.Scp3114.DisguiseDuration = 300f;
    }
}