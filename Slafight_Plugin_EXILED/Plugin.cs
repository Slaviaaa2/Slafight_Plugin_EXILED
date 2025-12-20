using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.CustomItems.API;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.Features;
using LabApi.Events.CustomHandlers;
using Slafight_Plugin_EXILED.CustomItems;
using Slafight_Plugin_EXILED.CustomRoles;
using Slafight_Plugin_EXILED.CustomRoles.FoundationForces;
using Slafight_Plugin_EXILED.SpecialEvents;
using System.Text.Json;
using HarmonyLib;
using Slafight_Plugin_EXILED.CustomRoles.SCPs;
using Slafight_Plugin_EXILED.Hints;
using Slafight_Plugin_EXILED.ProximityChat;
using Slafight_Plugin_EXILED.SpecialEvents.Events;
using UserSettings.ServerSpecific;

namespace Slafight_Plugin_EXILED
{
    using Exiled.API.Features;
    public class Plugin : Plugin<Config>
    {
        public static Plugin Singleton { get; set; } = null!;
        private static readonly HttpClient httpClient = new HttpClient();
        // Plugin Info
        public override string Name => "Slafight_Plugin_EXILED";
        public override string Author => "Slaviaaa_2";
        public override string Prefix => "Slafight_Plugin_EXILED";
        public override Version Version => new Version(1,4,3);
        public override Version RequiredExiledVersion { get; } = new Version(9, 11, 3);

        public Harmony HarmonyInstance { get; private set; }
        
        public EventHandler EventHandler { get; set; }
        public SpecialEventsHandler SpecialEventsHandler { get; set; }
        public CustomMap CustomMap { get; set; }
        public CustomRolesHandler CustomRolesHandler { get; set; }
        public LabApiHandler LabApiHandler { get; set; }
        public EasterEggsHandler EasterEggsHandler { get; set; }
        public PlayerHUD PlayerHUD { get; set; }
        public CandyChanges CandyChanges { get; set; }
        public ActivateHandler ProximityChatActiveHandler { get; set; }
        public RPNameSetter RolePlayNameSetter { get; set; }

        public HIDTurret _HIDTurret;
        public KeycardFifthist _KeycardFifthist;
        public ArmorInfantry _ArmorInfantry;
        public GunN7CR _GunN7CR;
        public GunGoCRailgun _GunGoCRailgun;
        public ArmorVip _ArmorVip;
        public MagicMissile _MagicMissile;
        public DummyRoad _DummyRoad;
        public FakeGrenade _FakeGrenade;
        
        public KeycardOld_Cadet _KeycardOld_Cadet;
        public KeycardOld_Commander _KeycardOld_Commander;
        public KeycardOld_ContainmentEngineer _KeycardOld_ContainmentEngineer;
        public KeycardOld_FacilityManager _KeycardOld_FacilityManager;
        public KeycardOld_Guard _KeycardOld_Guard;
        public KeycardOld_Janitor _KeycardOld_Janitor;
        public KeycardOld_Lieutenant _KeycardOld_Lieutenant;
        public KeycardOld_O5 _KeycardOld_O5;
        public KeycardOld_ResearchSupervisor _KeycardOld_ResearchSupervisor;
        public KeycardOld_Scientist _KeycardOld_Scientist;
        public KeycardOld_ZoneManager _KeycardOld_ZoneManager;

        public HdInfantry CR_HdInfantry { get; set; }
        public HdCommander CR_HdCommander { get; set; }
        public NtfAide CR_NtfAide { get; set; }
        public Scp3114Role CRScp3114Role { get; set; }
        public Scp966Role CR_Scp966Role { get; set; }
        public ZoneManager CR_ZoneManager { get; set; }
        public FacilityManager CR_FacilityManager { get; set; }
        public EvacuationGuard CR_ESGuard { get; set; }
        public Janitor CR_Janitor { get; set; }
        
        public SpawnSystem SpawnSystem { get; set; }
        public EscapeHandler EscapeHandler { get; set; }
        
        public OperationBlackout OperationBlackout { get; set; }
        // Enable & Disable
        public override void OnEnabled()
        {
            Singleton = this;
            EventHandler = new EventHandler();
            SpecialEventsHandler = new SpecialEventsHandler();
            CustomMap = new CustomMap();
            CustomRolesHandler = new CustomRolesHandler();
            LabApiHandler = new();
            EasterEggsHandler = new();
            PlayerHUD = new();
            EscapeHandler = new();
            CandyChanges = new();
            OperationBlackout = new();
            ProximityChat.Handler.RegisterEvents();
            ProximityChatActiveHandler = new();
            RolePlayNameSetter = new();
            CustomHandlersManager.RegisterEventsHandler(LabApiHandler);
            CustomHandlersManager.RegisterEventsHandler(CustomMap);
            
            _HIDTurret = new();
            _KeycardFifthist = new();
            _ArmorInfantry = new();
            _GunN7CR = new();
            _GunGoCRailgun = new();
            _ArmorVip = new();
            _MagicMissile = new();
            _DummyRoad = new();
            _FakeGrenade = new();
            
            _KeycardOld_Cadet = new();
            _KeycardOld_Commander = new();
            _KeycardOld_ContainmentEngineer = new();
            _KeycardOld_FacilityManager = new();
            _KeycardOld_Guard = new();
            _KeycardOld_Janitor = new();
            _KeycardOld_Lieutenant = new();
            _KeycardOld_O5 = new();
            _KeycardOld_ResearchSupervisor = new();
            _KeycardOld_Scientist = new();
            _KeycardOld_ZoneManager = new();
            
            Config.HidTurretConfig.Register();
            Config.KeycardFifthistConfig.Register();
            Config.KeycardFifthistPriestConfig.Register();
            Config.ArmorInfantryConfig.Register();
            Config.GunN7CRConfig.Register();
            Config.GunGoCRailgunConfig.Register();
            Config.ArmorVipConfig.Register();
            Config.MagicMissileConfig.Register();
            Config.DummyRoadConfig.Register();
            Config.FakeGrenadeConfig.Register();
            
            Config.KeycardOld_CadetConfig.Register();
            Config.KeycardOld_CommanderConfig.Register();
            Config.KeycardOld_ContainmentEngineerConfig.Register();
            Config.KeycardOld_FacilityManagerConfig.Register();
            Config.KeycardOld_GuardConfig.Register();
            Config.KeycardOld_JanitorConfig.Register();
            Config.KeycardOld_LieutenantConfig.Register();
            Config.KeycardOld_O5Config.Register();
            Config.KeycardOld_ResearchSupervisorConfig.Register();
            Config.KeycardOld_ScientistConfig.Register();
            Config.KeycardOld_ZoneManagerConfig.Register();

            
            
            CR_HdInfantry = new();
            CR_HdCommander = new();
            CR_NtfAide = new();
            CRScp3114Role = new();
            CR_Scp966Role = new();
            CR_ZoneManager = new();
            CR_FacilityManager = new();
            CR_ESGuard = new();
            CR_Janitor = new();

            CustomRole.RegisterRoles(false);
            
            SpawnSystem = new();

            var Settings = ServerSpecifics.Settings();
            var a = Settings.ToList();
            ServerSpecificSettingsSync.DefinedSettings = a.ToArray();
            ServerSpecificSettingsSync.SendToAll();
            Log.Debug($"Settings List: \n{ServerSpecificSettingsSync.DefinedSettings}");
            
            Slafight_Plugin_EXILED.Plugin.Singleton.EasterEggsHandler.loadClips();
            
            HarmonyInstance = new Harmony(this.Name);
            HarmonyInstance.PatchAll();  // 全HarmonyPatch属性を自動適用

            _ = SendPlayerCountLoop();
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Singleton = null!;
            ProximityChat.Handler.UnregisterEvents();
            CustomHandlersManager.UnregisterEventsHandler(LabApiHandler);
            CustomHandlersManager.UnregisterEventsHandler(CustomMap);
            
            LabApiHandler = null;
            
            _HIDTurret = null;
            _KeycardFifthist = null;
            _ArmorInfantry = null;
            CustomItem.UnregisterItems();
            CustomRole.UnregisterRoles();
            
            HarmonyInstance.UnpatchAll(this.Name);
            HarmonyInstance = null;
            
            ServerSpecificSettingsSync.DefinedSettings = [];
            ServerSpecificSettingsSync.SendToAll();
            
            base.OnDisabled();
        }
        
        private async Task SendPlayerCountLoop()
        {
            while (true)
            {
                await SendPlayerCountAsync(Player.List.Count);
                await Task.Delay(60000); // 1分
            }
        }

        private async Task SendPlayerCountAsync(int count)
        {
            try
            {
                var data = new
                {
                    server = "シャープ鯖",
                    count = count,
                    timestamp = DateTime.UtcNow
                };

                string json = JsonSerializer.Serialize(data);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                await httpClient.PostAsync("http://localhost:5000/playercount", content);
            }
            catch (Exception ex)
            {
                Log.Error($"SendPlayerCountAsync error: {ex}");
            }
        }
    }
}