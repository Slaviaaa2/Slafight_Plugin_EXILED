using Exiled.CustomItems.API;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

namespace Slafight_Plugin_EXILED.CustomItems;

public static class CustomItemsManager
{
    public static void RegisterAllItems()
    {
        new AdvancedMedkit().Register();
        new AntiMemeGoggle().Register();
        new ArmorInfantry().Register();
        new ArmorVip().Register();
        new CaneOfTheStars().Register();
        new DummyRoad().Register();
        new FakeGrenade().Register();
        new FlashBangE().Register();
        new GoCRecruitPaper().Register();
        new GunCOM77().Register();
        new GunFRMGX().Register();
        new GunFSP18().Register();
        new GunGoCRailgun().Register();
        new GunGoCTurret().Register();
        new GunN7CR().Register();
        new GunN7Weltkrieg().Register();
        new GunProject90().Register();
        new GunSuperLogicer().Register();
        new HIDTurret().Register();
        new KeycardFifthist().Register();
        new KeycardFifthistPriest().Register();
        new KeycardOld_Cadet().Register();
        new KeycardOld_Commander().Register();
        new KeycardOld_ContainmentEngineer().Register();
        new KeycardOld_FacilityManager().Register();
        new KeycardOld_Guard().Register();
        new KeycardOld_Janitor().Register();
        new KeycardOld_Lieutenant().Register();
        new KeycardOld_O5().Register();
        new KeycardOld_ResearchSupervisor().Register();
        new KeycardOld_Scientist().Register();
        new KeycardOld_ZoneManager().Register();
        new MagicMissile().Register();
        new NeutralizeGrenade().Register();
        new OmegaWarheadAccess().Register();
        new Quarter().Register();
        new Scp148().Register();
        new Scp1425().Register();
        new SerumC().Register();
        new SerumD().Register();
        new SNAV300().Register();
        new SNAV310().Register();
        new SNAVUltimate().Register();
        new ClassXMemoryForcePil().Register();
        new ClassZMemoryForcePil().Register();
        new GunGoCRailgunFull().Register();
        new GunTacticalRevolver().Register();
        new NvgNormal().Register();
        new CUA_SpyKit().Register();
        new Veritas().Register();
        new CloakGenerator().Register();
        new NvgRed().Register();
        new NvgBlue().Register();
        new CapybaraMissile().Register();
        new DninoueMissile().Register();
    }
}