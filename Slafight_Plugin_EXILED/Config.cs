using System.ComponentModel;
using Exiled.API.Interfaces;
using Slafight_Plugin_EXILED.CustomItems;

namespace Slafight_Plugin_EXILED
{
    using Exiled.API.Interfaces;
    public class Config : IConfig
    {
        [Description("Set Enable or Disable")]
        public bool IsEnabled { get; set; } = true;
        [Description("Show Debug Logs?")]
        public bool Debug { get; set; } = true;
        
        [Description("")]
        public string AudioReferences { get; set; } = "C:\\Users\\zeros\\AppData\\Roaming\\EXILED\\ServerContents\\";
        
        [Description("")]
        public bool WarheadLockAllowed { get; set; } = true;
        [Description("")]
        public float WarheadLockTimeMultiplier { get; set; } = 0.75f;
        
        [Description("")]
        public bool EventAllowed { get; set; } = true;
        [Description("")]
        public float OwBoomTime { get; set; } = 160f;
        [Description("")]
        public float DwBoomTime { get; set; } = 100f;
        
        [Description("")]
        public HIDTurret HidTurretConfig { get; set; } = new();
        [Description("")]
        public KeycardFifthist KeycardFifthistConfig { get; set; } = new();
    }
}