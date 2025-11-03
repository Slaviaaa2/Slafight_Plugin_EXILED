using Exiled.API.Interfaces;
using Slafight_Plugin_EXILED.CustomItems;

namespace Slafight_Plugin_EXILED
{
    using Exiled.API.Interfaces;
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        
        public bool SkeletonSpawnAllowed { get; set; } = true;
        public float SkeletonSpawnChance { get; set; } = 0.25f;
        
        public float WarheadLockTimeMultipler { get; set; } = 0.75f;
        
        public bool EventAllowed { get; set; } = true;
        public bool OW_Allowed { get; set; } = true;
        public bool DW_Allowed { get; set; } = true;
        public bool CF_Allowed { get; set; } = true;
        
        public HIDTurret HidTurretConfig { get; set; } = new();
    }
}