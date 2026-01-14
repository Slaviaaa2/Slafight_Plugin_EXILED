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

        [Description("")] public HIDTurret _HIDTurret { get; set; } = new(); // 1
        [Description("")] public KeycardFifthist _KeycardFifthist { get; set; } = new(); // 5
        [Description("")] public KeycardFifthistPriest _KeycardFifthistPriest { get; set; } = new(); // 6
        [Description("")] public ArmorInfantry _ArmorInfantry { get; set; } = new(); // 10
        [Description("")] public GunGoCRailgun _GunGoCRailgun { get; set; } = new(); // 50
        [Description("")] public GunN7CR _GunN7CR { get; set; } = new(); // 11
        [Description("")] public GunFSP18 _GunFSP18 { get; set; } = new(); // 2000
        [Description("")] public ArmorVip _ArmorVip { get; set; } = new(); // 12
        [Description("")] public MagicMissile _MagicMissile { get; set; } = new(); // 666
        [Description("")] public DummyRoad _DummyRoad { get; set; } = new(); // 1000
        [Description("")] public FakeGrenade _FakeGrenade { get; set; } = new(); // 700
        [Description("")] public KeycardSecurityChief _KeycardSecurityChief { get; set; } = new(); // 1100
        [Description("")] public KeycardConscripts _KeycardConscripts { get; set; } = new(); // 1101
        [Description("")] public Scp1425 _Scp1425 { get; set; } = new(); // 1102
        [Description("")] public MasterCard _MasterCard { get; set; } = new(); // 2002
        [Description("")] public PlayingCard _PlayingCard { get; set; } = new(); // 2003
        [Description("")] public Quarter _Quarter { get; set; } = new(); // 2004
        [Description("")] public OmegaWarheadAccess _OmegaWarheadAccess { get; set; } = new(); // 2005
        [Description("")] public GunSuperLogicer _GunSuperLogicer { get; set; } = new(); // 2006
        [Description("")] public GunFRMGX _GunFRMGX { get; set; } = new(); // 2007
        [Description("")] public SerumD _SerumD { get; set; } = new(); // 2008
        [Description("")] public AdvancedMedkit _AdvancedMedkit { get; set; } = new(); // 2009
        [Description("")] public SerumC _SerumC { get; set; } = new(); // 2010
        [Description("")] public GunN7Weltkrieg _GunN7Weltkrieg { get; set; } = new(); // 2011
        [Description("")] public SNAV300 _SNAV300 { get; set; } = new(); // 2012
        [Description("")] public SNAV310 _SNAV310 { get; set; } = new(); // 2013
        [Description("")] public SNAVUltimate _SnavUltimate { get; set; } = new(); // 2014

        [Description("")] public KeycardOld_ContainmentEngineer _KeycardOld_ContainmentEngineer { get; set; } = new(); // 100
        [Description("")] public KeycardOld_Janitor _KeycardOld_Janitor { get; set; } = new(); // 101
        [Description("")] public KeycardOld_Guard _KeycardOld_Guard { get; set; } = new(); // 103
        [Description("")] public KeycardOld_Scientist _KeycardOld_Scientist { get; set; } = new(); // 102
        [Description("")] public KeycardOld_ResearchSupervisor _KeycardOld_ResearchSupervisor { get; set; } = new(); // 107
        [Description("")] public KeycardOld_ZoneManager _KeycardOld_ZoneManager { get; set; } = new(); // 108
        [Description("")] public KeycardOld_FacilityManager _KeycardOld_FacilityManager { get; set; } = new(); // 109
        [Description("")] public KeycardOld_Cadet _KeycardOld_Cadet { get; set; } = new(); // 104
        [Description("")] public KeycardOld_Lieutenant _KeycardOld_Lieutenant { get; set; } = new(); // 105
        [Description("")] public KeycardOld_Commander _KeycardOld_Commander { get; set; } = new(); // 106
        [Description("")] public KeycardOld_O5 _KeycardOld_O5 { get; set; } = new(); // 110
    }
}