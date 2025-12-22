using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.CustomRoles.SCPs;

namespace Slafight_Plugin_EXILED.Extensions;

public static class PlayerExtensions
{
    public static void SetRole(this Player player, RoleTypeId roleTypeId,
        RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        Log.Debug($"[SetRole-Vanilla] {player.Nickname} -> {roleTypeId} (flags: {roleSpawnFlags})");
        switch (roleTypeId)
        {
            // ==== SCP ====
            case RoleTypeId.Scp173:
                player.Role.Set(RoleTypeId.Scp173, roleSpawnFlags);
                player.Position = EventHandler.Scp173SpawnPoint;
                player.EnableEffect(EffectType.Slowness, 95, 60f);
                break;

            case RoleTypeId.Scp049:
                player.Role.Set(RoleTypeId.Scp049, roleSpawnFlags);
                break;

            case RoleTypeId.Scp079:
                player.Role.Set(RoleTypeId.Scp079, roleSpawnFlags);
                break;

            case RoleTypeId.Scp096:
                player.Role.Set(RoleTypeId.Scp096, roleSpawnFlags);
                break;

            case RoleTypeId.Scp106:
                player.Role.Set(RoleTypeId.Scp106, roleSpawnFlags);
                break;

            case RoleTypeId.Scp0492:
                player.Role.Set(RoleTypeId.Scp0492, roleSpawnFlags);
                break;

            case RoleTypeId.Scp939:
                player.Role.Set(RoleTypeId.Scp939, roleSpawnFlags);
                break;

            case RoleTypeId.Scp3114:
                Plugin.Singleton.CRScp3114Role.SpawnRole(player, roleSpawnFlags);
                break;

            // ==== 人間ロール ====
            case RoleTypeId.ClassD:
                player.Role.Set(RoleTypeId.ClassD, roleSpawnFlags);
                break;

            case RoleTypeId.Scientist:
                player.Role.Set(RoleTypeId.Scientist, roleSpawnFlags);
                break;

            case RoleTypeId.FacilityGuard:
                player.Role.Set(RoleTypeId.FacilityGuard, roleSpawnFlags);
                break;

            // ==== NTF ====
            case RoleTypeId.NtfPrivate:
                player.Role.Set(RoleTypeId.NtfPrivate, roleSpawnFlags);
                break;

            case RoleTypeId.NtfSergeant:
                player.Role.Set(RoleTypeId.NtfSergeant, roleSpawnFlags);
                break;

            case RoleTypeId.NtfCaptain:
                player.Role.Set(RoleTypeId.NtfCaptain, roleSpawnFlags);
                break;

            case RoleTypeId.NtfSpecialist:
                player.Role.Set(RoleTypeId.NtfSpecialist, roleSpawnFlags);
                break;

            // ==== Chaos ====
            case RoleTypeId.ChaosConscript:
                player.Role.Set(RoleTypeId.ChaosConscript, roleSpawnFlags);
                break;

            case RoleTypeId.ChaosRifleman:
                player.Role.Set(RoleTypeId.ChaosRifleman, roleSpawnFlags);
                break;

            case RoleTypeId.ChaosMarauder:
                player.Role.Set(RoleTypeId.ChaosMarauder, roleSpawnFlags);
                break;

            case RoleTypeId.ChaosRepressor:
                player.Role.Set(RoleTypeId.ChaosRepressor, roleSpawnFlags);
                break;
            
            // ==== Flamingos ===
            case RoleTypeId.AlphaFlamingo:
                player.Role.Set(RoleTypeId.AlphaFlamingo, roleSpawnFlags);
                break;
            case RoleTypeId.Flamingo:
                player.Role.Set(RoleTypeId.Flamingo, roleSpawnFlags);
                break;
            case RoleTypeId.ZombieFlamingo:
                player.Role.Set(RoleTypeId.ZombieFlamingo, roleSpawnFlags);
                break;
            case RoleTypeId.NtfFlamingo:
                player.Role.Set(RoleTypeId.NtfFlamingo, roleSpawnFlags);
                break;
            case RoleTypeId.ChaosFlamingo:
                player.Role.Set(RoleTypeId.ChaosFlamingo, roleSpawnFlags);
                break;
            
            // ==== Others ===
            case RoleTypeId.Spectator:
                player.Role.Set(RoleTypeId.Spectator, roleSpawnFlags);
                break;
            
            case RoleTypeId.Overwatch:
                player.Role.Set(RoleTypeId.Overwatch, roleSpawnFlags);
                break;
            
            case RoleTypeId.Filmmaker:
                player.Role.Set(RoleTypeId.Filmmaker, roleSpawnFlags);
                break;
            
            case RoleTypeId.Tutorial:
                player.Role.Set(RoleTypeId.Tutorial, roleSpawnFlags);
                break;
        }
        player.UniqueRole = null;
    }

    public static void SetRole(this Player player, CRoleTypeId roleTypeId,
        RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        Log.Debug($"[SetRole-Custom] {player.Nickname} -> {roleTypeId} (flags: {roleSpawnFlags})");
        switch (roleTypeId)
        {
            case CRoleTypeId.None:
                player.UniqueRole = null;
                break;
            case CRoleTypeId.Scp096Anger:
                var scp096Anger = new Scp096Anger();
                scp096Anger.SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.Scp3005:
                Plugin.Singleton.CustomRolesHandler.Spawn3005(player, roleSpawnFlags);
                break;
            case CRoleTypeId.Scp966:
                Plugin.Singleton.CR_Scp966Role.SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.FifthistRescure:
                Plugin.Singleton.CustomRolesHandler.SpawnFifthist(player, roleSpawnFlags);
                break;
            case CRoleTypeId.FifthistPriest:
                Plugin.Singleton.CustomRolesHandler.SpawnF_Priest(player, roleSpawnFlags);
                break;
            case CRoleTypeId.ChaosCommando:
                Plugin.Singleton.CustomRolesHandler.SpawnChaosCommando(player, roleSpawnFlags);
                break;
            case CRoleTypeId.NtfLieutenant:
                Plugin.Singleton.CR_NtfAide.SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.HdInfantry:
                Plugin.Singleton.CR_HdInfantry.SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.HdCommander:
                Plugin.Singleton.CR_HdCommander.SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.EvacuationGuard:
                Plugin.Singleton.CR_ESGuard.SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.ZoneManager:
                Plugin.Singleton.CR_ZoneManager.SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.FacilityManager:
                Plugin.Singleton.CR_FacilityManager.SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.FifthistConvert:
                break;
            case CRoleTypeId.Janitor:
                Plugin.Singleton.CR_Janitor.SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.SnowWarrier:
                Plugin.Singleton.CustomRolesHandler.SpawnSnowWarrier(player, roleSpawnFlags);
                break;
            case CRoleTypeId.Scp682:
                Scp682Role scp682Role = new Scp682Role();
                scp682Role.SpawnRole(player, roleSpawnFlags);
                break;
        }
    }
}
