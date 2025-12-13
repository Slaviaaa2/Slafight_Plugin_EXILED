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
        [Description("")]
        public KeycardFifthistPriest KeycardFifthistPriestConfig { get; set; } = new();
        [Description("")]
        public ArmorInfantry ArmorInfantryConfig { get; set; } = new();
        [Description("")]
        public GunGoCRailgun GunGoCRailgunConfig { get; set; } = new();
        [Description("")]
        public GunN7CR GunN7CRConfig { get; set; } = new();
        [Description("")]
        public ArmorVip ArmorVipConfig { get; set; } = new();
        
        [Description("")]
        public KeycardOld_ContainmentEngineer KeycardOld_ContainmentEngineerConfig { get; set; } = new();
        public KeycardOld_Janitor KeycardOld_JanitorConfig { get; set; } = new();
        public KeycardOld_Guard KeycardOld_GuardConfig { get; set; } = new();
        public KeycardOld_Scientist KeycardOld_ScientistConfig { get; set; } = new();
        public KeycardOld_ResearchSupervisor KeycardOld_ResearchSupervisorConfig { get; set; } = new();
        public KeycardOld_ZoneManager KeycardOld_ZoneManagerConfig { get; set; } = new();
        public KeycardOld_FacilityManager KeycardOld_FacilityManagerConfig { get; set; } = new();
        public KeycardOld_Cadet KeycardOld_CadetConfig { get; set; } = new();
        public KeycardOld_Lieutenant KeycardOld_LieutenantConfig { get; set; } = new();
        public KeycardOld_Commander KeycardOld_CommanderConfig { get; set; } = new();
        public KeycardOld_O5 KeycardOld_O5Config { get; set; } = new();
    }
}