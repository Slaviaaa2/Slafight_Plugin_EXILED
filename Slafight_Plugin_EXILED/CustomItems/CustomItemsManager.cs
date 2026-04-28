using Exiled.CustomItems.API;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

namespace Slafight_Plugin_EXILED.CustomItems;

public static class CustomItemsManager
{
    public static void RegisterAllItems()
    {
        new ArmorInfantry().Register();
        new ArmorVip().Register();
        new CaneOfTheStars().Register();
        new FakeGrenade().Register();
        new FlashBangE().Register();
        new GoCRecruitPaper().Register();
        new NeutralizeGrenade().Register();
        new Scp1425().Register();
        new ClassZMemoryForcePil().Register();
        new CUA_SpyKit().Register();
        new CapybaraMissile().Register();
        new DninoueMissile().Register();
    }
}