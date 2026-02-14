using Slafight_Plugin_EXILED.API.Enums;

namespace Slafight_Plugin_EXILED.MapExtensions;

public static class MapFlags
{
    public static bool FemurSetup => CustomMap._femurSetup;
    public static bool FemurBreaked => CustomMap._femurBreaked;
    public static bool IsOmegaStarted => OmegaWarhead.IsWarheadStarted;

    /// <summary>
    /// Get Season. Please look to 
    /// <see cref="Config.Season"/>
    /// </summary>
    public static SeasonTypeId GetSeason()
    {
        return Plugin.Singleton.Config.Season switch
        {
            0 => SeasonTypeId.None,
            1 => SeasonTypeId.Halloween,
            2 => SeasonTypeId.Christmas,
            3 => SeasonTypeId.April,
            _ => SeasonTypeId.None
        };
    }
}