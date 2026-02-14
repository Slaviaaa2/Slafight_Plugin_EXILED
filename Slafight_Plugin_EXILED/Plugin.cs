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
using System.Threading;
using HarmonyLib;
using MEC;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Changes;
using Slafight_Plugin_EXILED.CustomRoles.Scientist;
using Slafight_Plugin_EXILED.CustomRoles.SCPs;
using Slafight_Plugin_EXILED.Hints;
using Slafight_Plugin_EXILED.MainHandlers;
using Slafight_Plugin_EXILED.MapExtensions;
using Slafight_Plugin_EXILED.ProximityChat;
using Slafight_Plugin_EXILED.SpecialEvents.Events;
using UserSettings.ServerSpecific;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED
{
    using Exiled.API.Features;
    public class Plugin : Plugin<Config>
    {
        public static Plugin Singleton { get; set; } = null!;
        // Plugin クラスのフィールド宣言部（既存の public EventHandler たちの近く）に追加
        private CancellationTokenSource _playerCountCts;
        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10) // デフォルトより短く/長く調整
        };
        // Plugin Info
        public override string Name => "Slafight_Plugin_EXILED";
        public override string Author => "Slaviaaa_2";
        public override string Prefix => "Slafight_Plugin_EXILED";
        public override Version Version => new Version(1,6,0,5);
        
        public override Version RequiredExiledVersion { get; } = new Version(9, 13, 0);

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
        public FirstRolesHandler FirstRolesHandler { get; set; }
        public ChristmasChanges ChristmasChanges { get; set; }
        
        public SpawnSystem SpawnSystem { get; set; }
        public SpawningHandler SpawningHandler { get; set; }
        public EscapeHandler EscapeHandler { get; set; }
        public AbilityInputHandler AbilityInputHandler { get; set; }
        public Sinkhole Sinkhole { get; set; }
        public PDEx PDEx { get; set; }
        public Engineer EngineerRole { get; private set; }
        public MapGuardHandler MapGuardHandler { get; set; }
        public Scp1509Handler Scp1509Handler { get; set; }
        public Scp012_033 Scp012_033 { get; set; }
        public TerminalRiftLabHandler TerminalRiftLabHandler { get; set; }
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
            ProximityChat.Handler.RegisterEvents();
            ProximityChatActiveHandler = new();
            RolePlayNameSetter = new();
            FirstRolesHandler = new();
            ChristmasChanges = new();
            AbilityInputHandler = new();
            Sinkhole = new();
            PDEx = new();
            MapGuardHandler = new();
            Scp1509Handler = new();
            Scp012_033 = new();
            CustomHandlersManager.RegisterEventsHandler(LabApiHandler);
            CustomHandlersManager.RegisterEventsHandler(CustomMap);

            TerminalRiftLabHandler = new();
            CustomHandlersManager.RegisterEventsHandler(TerminalRiftLabHandler);
            TerminalRift.Register();

            EngineerRole = new Engineer();
            EngineerRole.RegisterEvents();
            CRole.OverrideRoleInstance(EngineerRole.UniqueRoleName, EngineerRole);
            
            CRole.RegisterAllEvents();
            CItem.RegisterAllEvents();
            AbilityBase.RegisterEvents();
            AbilityManager.RegisterEvents();
            CustomRole.RegisterRoles(false);
            CustomItem.RegisterItems(skipReflection: false, overrideClass: Config);
            
            SpawnSystem = new();
            SpawningHandler = new();

            var Settings = ServerSpecifics.Settings();
            var a = Settings.ToList();
            ServerSpecificSettingsSync.DefinedSettings = a.ToArray();
            ServerSpecificSettingsSync.SendToAll();
            Log.Debug($"Settings List: \n{ServerSpecificSettingsSync.DefinedSettings}");
            
            Plugin.Singleton.EasterEggsHandler.loadClips();
            
            HarmonyInstance = new Harmony(this.Name);
            HarmonyInstance.PatchAll();

            // ここから差し替え
            _playerCountCts?.Cancel();                     // 念のため前回のを殺す
            _playerCountCts = new CancellationTokenSource();
            _ = SendPlayerCountLoop(_playerCountCts.Token);
            // ここまで差し替え

            base.OnEnabled();

        }

        public override void OnDisabled()
        {
            Singleton = null!;
            try
            {
                _playerCountCts?.Cancel();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to cancel player count loop: {ex}");
            }
            finally
            {
                _playerCountCts = null;
            }
            
            ProximityChat.Handler.UnregisterEvents();
            CustomHandlersManager.UnregisterEventsHandler(LabApiHandler);
            CustomHandlersManager.UnregisterEventsHandler(CustomMap);
            
            CustomHandlersManager.UnregisterEventsHandler(TerminalRiftLabHandler);
            TerminalRift.Unregister();
            
            EngineerRole.UnregisterEvents();
            
            CRole.UnregisterAllEvents();
            CItem.UnregisterAllEvents();
            AbilityBase.UnregisterEvents();
            AbilityManager.UnregisterEvents();
            CustomItem.UnregisterItems();
            CustomRole.UnregisterRoles();
            
            HarmonyInstance.UnpatchAll(this.Name);
            HarmonyInstance = null;
            
            ServerSpecificSettingsSync.DefinedSettings = [];
            ServerSpecificSettingsSync.SendToAll();
            
            base.OnDisabled();
        }
        
        private async Task SendPlayerCountLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await SendPlayerCountAsync(Player.List.Count);
                }
                catch (Exception ex)
                {
                    Log.Error($"SendPlayerCountLoop error: {ex}");
                }

                try
                {
                    await Task.Delay(60000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
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
                await HttpClient.PostAsync("http://localhost:5000/playercount", content);
            }
            catch (TaskCanceledException tce)
            {
                // 一時的なタイムアウトとして扱う
                Log.Debug($"SendPlayerCountAsync timeout: {tce.Message}");
            }
            catch (HttpRequestException hre)
            {
                Log.Debug($"SendPlayerCountAsync failure: {hre.Message}");
            }
            catch (Exception ex)
            {
                Log.Error($"SendPlayerCountAsync error: {ex}");
            }
        }
    }
}