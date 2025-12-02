using System;
using ASS.Example.PlayerMenuExamples;
using Exiled.CustomItems.API;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.Features;
using LabApi.Events.CustomHandlers;
using Slafight_Plugin_EXILED.CustomItems;
using Slafight_Plugin_EXILED.CustomRoles;
using Slafight_Plugin_EXILED.SpecialEvents;

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
        public override Version Version => new Version(1,2,0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 10, 2);

        public EventHandler EventHandler { get; set; }
        public SpecialEventsHandler SpecialEventsHandler { get; set; }
        public CustomMap CustomMap { get; set; }
        public CustomRolesHandler CustomRolesHandler { get; set; }
        public LabApiHandler LabApiHandler { get; set; }
        public SS_Handler SS_Handler { get; set; }
        public SS_Handler_Exiled SS_Handler_Exiled { get; set; }

        public HIDTurret _HIDTurret;
        public KeycardFifthist _KeycardFifthist;
        // Enable & Disable
        public override void OnEnabled()
        {
            Singleton = this;
            EventHandler = new EventHandler();
            SpecialEventsHandler = new SpecialEventsHandler();
            CustomMap = new CustomMap();
            CustomRolesHandler = new CustomRolesHandler();
            LabApiHandler = new();
            SS_Handler = new();
            SS_Handler_Exiled = new();
            CustomHandlersManager.RegisterEventsHandler(LabApiHandler);
            CustomHandlersManager.RegisterEventsHandler(SS_Handler);

            _HIDTurret = new();
            _KeycardFifthist = new();
            Config.HidTurretConfig.Register();
            Config.KeycardFifthistConfig.Register();
            
            Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.InitAddEvent();
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Singleton = null!;
            CustomHandlersManager.UnregisterEventsHandler(LabApiHandler);
            CustomHandlersManager.UnregisterEventsHandler(SS_Handler);
            LabApiHandler = null;
            SS_Handler = null;
            
            _HIDTurret = null;
            _KeycardFifthist = null;
            CustomItem.UnregisterItems();
            CustomRole.UnregisterRoles();
            
            base.OnDisabled();
        }
    }
}