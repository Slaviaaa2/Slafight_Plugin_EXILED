namespace Slafight_Plugin_EXILED.MapExtensions;

public static class MapFlags
{
    public static bool FemurSetup => CustomMap._femurSetup;
    public static bool FemurBreaked => CustomMap._femurBreaked;
    public static bool IsOmegaStarted => OmegaWarhead.IsWarheadStarted;
}