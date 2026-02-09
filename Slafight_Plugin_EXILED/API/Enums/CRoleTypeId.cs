using System;

#nullable disable

namespace Slafight_Plugin_EXILED.API.Enums;

public enum CRoleTypeId
{
    // ==== None ====
    None,
    
    // ==== SCP ==== 
    Scp096Anger,
    Scp3005,
    Scp966,
    Scp682,
    Scp999,
    Scp106,
    Scp3114,
    Scp173,
    
    // ==== Fifthists ====
    FifthistRescure,
    FifthistPriest,
    FifthistConvert,
    FifthistGuidance,
    
    // ==== Chaos ====
    ChaosCommando,
    ChaosSignal,
    
    // ==== NTF ====
    NtfLieutenant,
    NtfGeneral,
    
    // ==== Hammer Down (財団軍) ====
    HdInfantry,
    HdCommander,
    HdMarshal,
    
    // ==== Guards (警備系) ====
    EvacuationGuard,
    SecurityChief,
    ChamberGuard,
    
    // ==== Scientists (科学者側) ====
    ZoneManager,
    FacilityManager,
    Engineer,
    ObjectObserver,
    
    // ==== Class-D系 ====
    Janitor,
    
    // ==== GoC ====
    GoCSquadLeader,
    GoCDeputy,
    GoCMedic,
    GoCThaumaturgist,
    GoCCommunications,
    GoCOperative,
    
    // ==== Others ====
    SnowWarrier,
    Zombified,
    Sculpture
}