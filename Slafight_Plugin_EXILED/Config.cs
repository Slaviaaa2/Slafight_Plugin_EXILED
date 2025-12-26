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
        [Description("Please Set Season Info. 0=normal,1=halloween,2=christmas,34...is not available now")]
        public int Season { get; set; } = 0;
        
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

        [Description("")] public HIDTurret _HIDTurret { get; set; } = new();
        [Description("")] public KeycardFifthist _KeycardFifthist { get; set; } = new();
        [Description("")] public KeycardFifthistPriest _KeycardFifthistPriest { get; set; } = new();
        [Description("")] public ArmorInfantry _ArmorInfantry { get; set; } = new();
        [Description("")] public GunGoCRailgun _GunGoCRailgun { get; set; } = new();
        [Description("")] public GunN7CR _GunN7CR { get; set; } = new();
        [Description("")] public ArmorVip _ArmorVip { get; set; } = new();
        [Description("")] public MagicMissile _MagicMissile { get; set; } = new();
        [Description("")] public DummyRoad _DummyRoad { get; set; } = new();
        [Description("")] public FakeGrenade _FakeGrenade { get; set; } = new();
        [Description("")] public KeycardSecurityChief _KeycardSecurityChief { get; set; } = new();
        [Description("")] public KeycardConscripts _KeycardConscripts { get; set; } = new();
        [Description("")] public Scp1425 _Scp1425 { get; set; } = new();

        [Description("")] public KeycardOld_ContainmentEngineer _KeycardOld_ContainmentEngineer { get; set; } = new();
        [Description("")] public KeycardOld_Janitor _KeycardOld_Janitor { get; set; } = new();
        [Description("")] public KeycardOld_Guard _KeycardOld_Guard { get; set; } = new();
        [Description("")] public KeycardOld_Scientist _KeycardOld_Scientist { get; set; } = new();
        [Description("")] public KeycardOld_ResearchSupervisor _KeycardOld_ResearchSupervisor { get; set; } = new();
        [Description("")] public KeycardOld_ZoneManager _KeycardOld_ZoneManager { get; set; } = new();
        [Description("")] public KeycardOld_FacilityManager _KeycardOld_FacilityManager { get; set; } = new();
        [Description("")] public KeycardOld_Cadet _KeycardOld_Cadet { get; set; } = new();
        [Description("")] public KeycardOld_Lieutenant _KeycardOld_Lieutenant { get; set; } = new();
        [Description("")] public KeycardOld_Commander _KeycardOld_Commander { get; set; } = new();
        [Description("")] public KeycardOld_O5 _KeycardOld_O5 { get; set; } = new();
    }
}