using System;
using Exiled.CustomItems.API;
using Exiled.CustomItems.API.Features;
using Exiled.Events.Features;
using LabApi.Events.CustomHandlers;
using Slafight_Plugin_EXILED.CustomItems;

namespace Slafight_Plugin_EXILED
{
    using Exiled.API.Features;
    public class Plugin : Plugin<Config>
    {
        public static Plugin Singleton { get; set; } = null!;
        // Plugin Info
        public override string Name => "Slafight_Plugin_EXILED";
        public override string Author => "Slaviaaa_2";
        public override string Prefix => "Slafight_Plugin_EXILED";
        public override Version Version => new Version(1,0,0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 10, 1);

        public EventHandler EventHandler { get; set; }
        public CustomMap CustomMap { get; set; }

        public HIDTurret _HIDTurret;
        // Enable & Disable
        public override void OnEnabled()
        {
            Singleton = this;
            EventHandler = new EventHandler();
            CustomMap = new CustomMap();

            _HIDTurret = new();
            Config.HidTurretConfig.Register();
        }

        public override void OnDisabled()
        {
            Singleton = null!;
            _HIDTurret = null;
            Config.HidTurretConfig.Unregister();
        }
    }
}