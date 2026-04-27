using Exiled.CustomItems.API;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

namespace Slafight_Plugin_EXILED.CustomItems;

public static class CustomItemsManager
{
    public static void RegisterAllItems()
    {
        new AntiMemeGoggle().Register();
        new ArmorInfantry().Register();
        new ArmorVip().Register();
        new CaneOfTheStars().Register();
        new DummyRoad().Register();
        new FakeGrenade().Register();
        new FlashBangE().Register();
        new GoCRecruitPaper().Register();
        new GunGoCRailgun().Register();
        new GunGoCTurret().Register();
        new HIDTurret().Register();
        new MagicMissile().Register();
        new NeutralizeGrenade().Register();
        new Scp1425().Register();
        new SNAV300().Register();
        new SNAV310().Register();
        new SNAVUltimate().Register();
        new ClassZMemoryForcePil().Register();
        new GunGoCRailgunFull().Register();
        new GunTacticalRevolver().Register();
        new NvgNormal().Register();
        new CUA_SpyKit().Register();
        new Veritas().Register();
        new NvgRed().Register();
        new NvgBlue().Register();
        new CapybaraMissile().Register();
        new DninoueMissile().Register();
    }
}