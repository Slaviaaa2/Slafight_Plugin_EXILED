using Exiled.CustomItems.API;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;

namespace Slafight_Plugin_EXILED.CustomItems;

public static class CustomItemsManager
{
    public static void RegisterAllItems()
    {
        new FakeGrenade().Register();
        new FlashBangE().Register();
        new NeutralizeGrenade().Register();
        new CUA_SpyKit().Register();
        new CapybaraMissile().Register();
        new DninoueMissile().Register();
    }
}