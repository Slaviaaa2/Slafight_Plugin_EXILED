using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.CustomRoles.ChaosInsurgency;
using Slafight_Plugin_EXILED.CustomRoles.ClassD;
using Slafight_Plugin_EXILED.CustomRoles.Fifthist;
using Slafight_Plugin_EXILED.CustomRoles.FoundationForces;
using Slafight_Plugin_EXILED.CustomRoles.GoC;
using Slafight_Plugin_EXILED.CustomRoles.Guards;
using Slafight_Plugin_EXILED.CustomRoles.Others;
using Slafight_Plugin_EXILED.CustomRoles.Scientist;
using Slafight_Plugin_EXILED.CustomRoles.SCPs;

namespace Slafight_Plugin_EXILED.Extensions;

public static class PlayerExtensions
{
    public static void SetRole(this Player player, RoleTypeId roleTypeId,
        RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        Log.Debug($"[SetRole-Vanilla] {player.Nickname} -> {roleTypeId} (flags: {roleSpawnFlags})");
        player.UniqueRole = null;
        switch (roleTypeId)
        {
            // ==== SCP ====
            case RoleTypeId.Scp173:
                player.SetRole(CRoleTypeId.Scp173);
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
                player.SetRole(CRoleTypeId.Scp106);
                break;

            case RoleTypeId.Scp0492:
                player.Role.Set(RoleTypeId.Scp0492, roleSpawnFlags);
                break;

            case RoleTypeId.Scp939:
                player.Role.Set(RoleTypeId.Scp939, roleSpawnFlags);
                break;

            case RoleTypeId.Scp3114:
                // Cast to SetRole(Custom)
                player.SetRole(CRoleTypeId.Scp3114);
                break;

            // ==== Neutrals ====
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
                foreach (var item in player.Items.ToList())
                {
                    if (item.Type == ItemType.KeycardChaosInsurgency)
                    {
                        player.RemoveItem(item);
                    }
                }
                player.TryAddCustomItem(1101);
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
            // ==== SCP ====
            case CRoleTypeId.Scp096Anger:
                new Scp096Anger().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.Scp3005:
                new Scp3005Role().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.Scp966:
                new Scp966Role().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.Scp3114:
                new Scp3114Role().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.Scp106:
                new Scp106Role().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.Scp682:
                new Scp682Role().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.Scp999:
                new Scp999Role().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.Scp173:
                new Scp173Role().SpawnRole(player, roleSpawnFlags);
                break;
            // ==== Fifthists ====
            case CRoleTypeId.FifthistRescure:
                new FifthistRescure().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.FifthistPriest:
                new FifthistPriest().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.FifthistConvert:
                new FifthistConvert().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.FifthistGuidance:
                new FifthistGuidance().SpawnRole(player, roleSpawnFlags);
                break;
            // ==== Chaos ====
            case CRoleTypeId.ChaosCommando:
                new ChaosCommando().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.ChaosSignal:
                new ChaosSignal().SpawnRole(player, roleSpawnFlags);
                break;
            // ==== NTF ====
            case CRoleTypeId.NtfLieutenant:
                new NtfAide().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.NtfGeneral:
                new NtfGeneral().SpawnRole(player, roleSpawnFlags);
                break;
            // ==== Hammer Down ====
            case CRoleTypeId.HdInfantry:
                new HdInfantry().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.HdCommander:
                new HdCommander().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.HdMarshal:
                new HdMarshal().SpawnRole(player, roleSpawnFlags);
                break;
            // ==== Guards ====
            case CRoleTypeId.EvacuationGuard:
                new EvacuationGuard().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.SecurityChief:
                new SecurityChief().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.ChamberGuard:
                new ChamberGuard().SpawnRole(player, roleSpawnFlags);
                break;
            // ==== Scientists ====
            case CRoleTypeId.ZoneManager:
                new ZoneManager().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.FacilityManager:
                new FacilityManager().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.Engineer:
                Plugin.Singleton.EngineerRole.SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.ObjectObserver:
                new ObjectObserver().SpawnRole(player, roleSpawnFlags);
                break;
            // ==== Class-D ====
            case CRoleTypeId.Janitor:
                new Janitor().SpawnRole(player, roleSpawnFlags);
                break;
            // ==== Flamingos ====
            // ==== Others ====
            case CRoleTypeId.SnowWarrier:
                new SnowWarrier().SpawnRole(player, roleSpawnFlags);
                break;
            // ==== Specials ====
            case CRoleTypeId.Sculpture:
                new Sculpture().SpawnRole(player, roleSpawnFlags);
                break;
            // ==== GoC ====
            case CRoleTypeId.GoCSquadLeader:
                new GoCSquadLeader().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.GoCDeputy:
                new GoCDeputy().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.GoCMedic:
                new GoCMedic().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.GoCThaumaturgist:
                new GoCThaumaturgist().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.GoCCommunications:
                new GoCCommunications().SpawnRole(player, roleSpawnFlags);
                break;
            case CRoleTypeId.GoCOperative:
                new GoCOperative().SpawnRole(player, roleSpawnFlags);
                break;
        }
    }

    public static void SetCustomInfo(this Player player,string Info)
    {
        player.CustomInfo = Info;
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;
    }

    public static void ClearCustomInfo(this Player player)
    {
        player.CustomInfo = null;
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea |= PlayerInfoArea.Badge;
        player.InfoArea |= PlayerInfoArea.CustomInfo;
        player.InfoArea |= PlayerInfoArea.UnitName;
        player.InfoArea |= PlayerInfoArea.PowerStatus;
        player.InfoArea |= PlayerInfoArea.Role;
    }
}
