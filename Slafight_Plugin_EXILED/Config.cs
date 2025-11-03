using System.ComponentModel;
using Exiled.API.Interfaces;
using Slafight_Plugin_EXILED.CustomItems;

namespace Slafight_Plugin_EXILED
{
    using Exiled.API.Interfaces;
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = true;
        
        public string AudioReferences { get; set; } = "C:\\Users\\zeros\\AppData\\Roaming\\EXILED\\ServerContents\\";
        // "C:\\Users\\zeros\\AppData\\Roaming\\EXILED\\ServerContents\\"
        public bool SkeletonSpawnAllowed { get; set; } = true;
        public float SkeletonSpawnChance { get; set; } = 0.25f;
        
        public bool WarheadLockAllowed { get; set; } = true;
        public float WarheadLockTimeMultiplier { get; set; } = 0.75f;
        
        public bool EventAllowed { get; set; } = true;
        public bool OW_Allowed { get; set; } = true;
        public float OW_BoomTime { get; set; } = 160f;
        public bool DW_Allowed { get; set; } = true;
        public float DW_BoomTime { get; set; } = 100f;
        public bool CF_Allowed { get; set; } = true;
        
        public HIDTurret HidTurretConfig { get; set; } = new();
    }
}