using Slafight_Plugin_EXILED.API.Enums;

namespace Slafight_Plugin_EXILED.CustomMaps;

public static class MapFlags
{
    public static bool FemurSetup => CustomMapMainHandler._femurSetup;
    public static bool FemurBreaked => CustomMapMainHandler._femurBreaked;
    public static bool IsOmegaStarted => OmegaWarhead.IsWarheadStarted;
    public static bool IsWarheadBooming => WarheadBoomEffectHandler.IsBooming;

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