using System;

#nullable disable

namespace Slafight_Plugin_EXILED.API.Enums;

public enum SpecialEventType
{
    /// <summary>No Events!Yeah</summary>
    None = 0,
    /// <summary>OMEGA! BOOM!!!</summary>
    OmegaWarhead = 1,
    /// <summary>DELTA CHAOS</summary>
    [Obsolete("このイベントは面白みがなかったので、FacilityTerminationイベントに置き換えられました。")]
    OldDeltaWarhead = 2,
    /// <summary>CRYYYYYYYYYYY</summary>
    Scp096CryFuck = 3,
    /// <summary>BattleField 4</summary>
    Scp1509BattleField = 4,
    /// <summary>THE FIFTHIST'S RAID</summary>
    FifthistsRaid = 5,
    /// <summary>Nuclear Attack</summary>
    NuclearAttack = 6,
    /// <summary>Classic SCRAPPED. IT'S VERY F**</summary>
    ClassicEvent = 7,
    /// <summary>It's very tired</summary>
    OperationBlackout = 8,
    /// <summary>MEGABALL ATTACK YEAHHHH</summary>
    SnowWarriersAttack = 9,
    /// <summary>DELTA 2.0</summary>
    FacilityTermination = 10
}
