using System;

#nullable disable

namespace Slafight_Plugin_EXILED.API.Enums;

public enum SpecialEventType
{
    /// <summary>No Events!Yeah</summary>
    None,
    
    /// <summary>OMEGA! BOOM!!!</summary>
    OmegaWarhead,
    
    /// <summary>DELTA CHAOS</summary>
    [Obsolete("このイベントは面白みがなかったので、FacilityTerminationイベントに置き換えられました。")]
    OldDeltaWarhead,
    
    /// <summary>CRYYYYYYYYYYY</summary>
    Scp096CryFuck,
    
    /// <summary>BattleField 4</summary>
    Scp1509BattleField,
    
    /// <summary>THE FIFTHIST'S RAID</summary>
    FifthistsRaid,
    
    /// <summary>Nuclear Attack</summary>
    NuclearAttack,
    
    /// <summary>Classic SCRAPPED. IT'S VERY F**</summary>
    ClassicEvent,
    
    /// <summary>It's very tired</summary>
    OperationBlackout,
    
    /// <summary>MEGABALL ATTACK YEAHHHH</summary>
    SnowWarriersAttack,
    
    /// <summary>DELTA 2.0</summary>
    FacilityTermination
}