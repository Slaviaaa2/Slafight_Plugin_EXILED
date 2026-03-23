using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MEC;
using ProjectMER.Events.Arguments;
using ProjectMER.Features;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Light = LabApi.Features.Wrappers.LightSourceToy;
using Logger = LabApi.Features.Console.Logger;
using Player = LabApi.Features.Wrappers.Player;

namespace Slafight_Plugin_EXILED.MainHandlers;

public class LabApiHandler : CustomEventsHandler
{
    public LabApiHandler()
    {
        LabApi.Events.Handlers.ServerEvents.RoundStarted += PickupSetup;
        Exiled.Events.Handlers.Player.Dying += DiedCassie;
        LabApi.Events.Handlers.PlayerEvents.SearchedToy += InteractionEvent;
        LabApi.Events.Handlers.ServerEvents.RoundStarted += Init;
        LabApi.Events.Handlers.ServerEvents.RoundStarted += Init;
    }

    ~LabApiHandler()
    {
        LabApi.Events.Handlers.ServerEvents.RoundStarted -= PickupSetup;
        Exiled.Events.Handlers.Player.Dying -= DiedCassie;
        LabApi.Events.Handlers.PlayerEvents.SearchedToy -= InteractionEvent;
        LabApi.Events.Handlers.ServerEvents.RoundStarted -= Init;
        LabApi.Events.Handlers.ServerEvents.RoundStarted -= Init;
    }

    public bool activatedAntiMemeProtocol = false;
    public bool _activatedAntiMemeProtocolInPast = false;

    public void Init()
    {
        activatedAntiMemeProtocol = false;
        _activatedAntiMemeProtocolInPast = false;
    }

    public void InteractionEvent(PlayerSearchedToyEventArgs ev)
    {
        if (ev.Interactable.Position != new Vector3(107.921f, 296.313f, -68.748f))
            return;

        if (!activatedAntiMemeProtocol)
        {
            foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
            {
                if (player.GetCustomRole() == CRoleTypeId.Scp3005)
                {
                    if (!_activatedAntiMemeProtocolInPast)
                        player.Health = 10000;

                    player.EnableEffect(EffectType.Poisoned, 255);
                    player.EnableEffect(EffectType.Decontaminating, 255);
                }
            }

            int count = 0;
            foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
            {
                if (player.GetCustomRole() != CRoleTypeId.Scp3005)
                    continue;

                count++;
                if (!_activatedAntiMemeProtocolInPast)
                {
                    Exiled.API.Features.Cassie.MessageTranslated(
                        "By order of Facility Manager Control Room , $pitch_.85 Anti- $pitch_1 Me mu Protocol Activated .",
                        "<color=#ff0087>施設管理者制御室</color>からの命令により、<color=#ff00fa>アンチミームプロトコロル</color>が有効化されました。エージェントにより反ミーム性物体の非活性化が開始されます。",
                        true,
                        false);
                    _activatedAntiMemeProtocolInPast = true;
                }
                else
                {
                    Exiled.API.Features.Cassie.MessageTranslated(
                        "$pitch_.85 Anti- $pitch_1 Me mu Protocol Resumed .",
                        "<color=#ff00fa>アンチミームプロトコル</color>が再開されました。",
                        false,
                        false);
                }

                activatedAntiMemeProtocol = true;
                break;
            }

            if (count <= 0)
                ev.Player.SendHint("<size=26>※対象が見つかりませんでした</size>", 3.5f);
        }
        else
        {
            foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
            {
                if (player.GetCustomRole() == CRoleTypeId.Scp3005)
                {
                    player.DisableEffect(EffectType.Poisoned);
                    player.DisableEffect(EffectType.Decontaminating);
                }
            }

            foreach (Exiled.API.Features.Player _ in Exiled.API.Features.Player.List)
            {
                Exiled.API.Features.Cassie.MessageTranslated(
                    "$pitch_.85 Anti- $pitch_1 Me mu Protocol Stopped .",
                    "<color=#ff00fa>アンチミームプロトコル</color>が停止されました。",
                    false,
                    false);
                activatedAntiMemeProtocol = false;
                break;
            }
        }
    }

    private void PickupSetup()
    {
        Logger.Info("LabApi Loader: Green");

        Timing.CallDelayed(1.05f, () =>
        {
            CustomItemExtensions.TrySpawn<HIDTurret>(new Vector3(134.94f,300.65f,-65f),out _);
            CustomItemExtensions.TrySpawn<NvgNormal>(Room.Get(RoomType.Hcz939).WorldPosition(Vector3.up*1.5f), out _);
        });
        Timing.CallDelayed(2.0f, PickupSetupTriggerPoints);
    }
       
    public void PickupSetupTriggerPoints()
    {
        var points = TriggerPointManager.GetAll();
        foreach (var point in points)
        {
            if (point.Base is not SerializableCustomTriggerPoint trig || string.IsNullOrEmpty(trig.Tag))
                continue;

            var pos = TriggerPointManager.GetWorldPosition(point);

            try
            {
                switch (trig.Tag)
                {
                    case "CISR_GoCRailgun":
                        CustomItemExtensions.TrySpawn<GunGoCRailgun>(pos, out _);
                        break;

                    case "CISR_OldPrivateCard":
                        if (CustomItemExtensions.TrySpawn<KeycardOld_Cadet>(pos, out var privateCard))
                            privateCard?.Rotation *= Quaternion.Euler(180f, 0f, 0f);
                        break;

                    case "CISR_OldCECard":
                        if (CustomItemExtensions.TrySpawn<KeycardOld_ContainmentEngineer>(pos, out var ceCard))
                            ceCard?.Rotation *= Quaternion.Euler(180f, 0f, 0f);
                        break;

                    case "CISR_Scp1425":
                        if (CustomItemExtensions.TrySpawn<Scp1425>(pos, out var scp1425))
                            scp1425?.Rotation *= Quaternion.Euler(180f, 0f, 0f);
                        break;

                    case "CISR_SNAV300":
                        CustomItemExtensions.TrySpawn<SNAV300>(pos, out _);
                        break;

                    case "CISR_MFP":
                        CustomItemExtensions.TrySpawn<ClassXMemoryForcePil>(pos, out _);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error($"[PickupSetupTriggerPoints] Error while spawning CustomItem at trigger point {trig.Tag}: {e}");
            }
        }
    }

    /// <summary>
    /// SCP-3005 用 schematic 生成（見た目は昔のまま、ロール監視は WearsHandler に任せる）
    /// </summary>
    public void Schem3005(Player labPlayer)
    {
        Timing.CallDelayed(1.5f, () =>
        {
            var exiledPlayer = Exiled.API.Features.Player.Get(labPlayer.NetworkId);
            Logger.Info($"[DEBUG] Schem3005 ExiledPlayer={(exiledPlayer != null ? exiledPlayer.Nickname : "null")}");
            if (exiledPlayer == null)
            {
                Logger.Warn("[LabApiHandler] Schem3005: Exiled player not found.");
                return;
            }

            SchematicObject schem;
            try
            {
                schem = ObjectSpawner.SpawnSchematic("SCP3005", Vector3.zero);
            }
            catch (Exception ex)
            {
                Logger.Error("[LabApiHandler] Schem3005: Spawn error " + ex);
                return;
            }

            labPlayer.Scale = new Vector3(0.001f, 1f, 0.001f);
            schem.transform.SetParent(labPlayer.GameObject.transform);

            // ロール監視用に WearsHandler に登録
            WearsHandler.RegisterExternal(exiledPlayer, schem);

            Timing.CallDelayed(0.5f, () =>
            {
                if (schem == null || schem.transform == null)
                    return;

                schem.transform.GetChild(0).localScale = Vector3.one;
                schem.transform.position = labPlayer.GameObject.transform.position;

                var light = Light.Create(Vector3.zero);
                light.Position = schem.transform.position + new Vector3(0f, -0.08f, 0f);
                light.Transform.parent = schem.transform;
                light.Scale = Vector3.one;
                light.Range = 10f;
                light.Intensity = 1.25f;
                light.Color = Color.magenta;
            });
        });
    }

    public void Schem999(Player labPlayer)
    {
        Timing.CallDelayed(1.5f, () =>
        {
            var exiledPlayer = Exiled.API.Features.Player.Get(labPlayer.NetworkId);
            Logger.Info($"[DEBUG] S999 ExiledPlayer={(exiledPlayer != null ? exiledPlayer.Nickname : "null")}");
            if (exiledPlayer == null)
            {
                Logger.Warn("[LabApiHandler] Schem999: Exiled player not found.");
                return;
            }

            SchematicObject schem;
            try
            {
                schem = ObjectSpawner.SpawnSchematic("Scp999Model", Vector3.zero);
            }
            catch (Exception ex)
            {
                Logger.Error("[LabApiHandler] Schem999: Spawn error " + ex);
                return;
            }

            labPlayer.Scale = new Vector3(0.35f, 0.2f, 0.35f);
            schem.transform.SetParent(labPlayer.GameObject.transform);

            WearsHandler.RegisterExternal(exiledPlayer, schem);

            Timing.CallDelayed(0.5f, () =>
            {
                if (schem == null || schem.transform == null)
                    return;

                schem.transform.position = labPlayer.GameObject.transform.position + new Vector3(0f, 0f, 0.05f);

                for (int i = 0; i < schem.transform.childCount; i++)
                {
                    var child = schem.transform.GetChild(i);
                    Logger.Info($"[Scp999Model] Child {i}: {child.name}, localScale={child.localScale}");
                }
                    
                labPlayer.DestroySchematic(schem);
            });
        });
    }

    public void SchemSnowWarrier(Player labPlayer)
    {
        Timing.CallDelayed(1.5f, () =>
        {
            var exiledPlayer = Exiled.API.Features.Player.Get(labPlayer.NetworkId);
            Logger.Info($"[DEBUG] SSW ExiledPlayer={(exiledPlayer != null ? exiledPlayer.Nickname : "null")}");
            if (exiledPlayer == null)
            {
                Logger.Warn("[LabApiHandler] SchemSnowWarrier: Exiled player not found.");
                return;
            }

            SchematicObject schem;
            try
            {
                schem = ObjectSpawner.SpawnSchematic("SnowWarrier", Vector3.zero);
            }
            catch (Exception ex)
            {
                Logger.Error("[LabApiHandler] SchemSnowWarrier: Spawn error " + ex);
                return;
            }

            schem.transform.SetParent(labPlayer.GameObject.transform);

            WearsHandler.RegisterExternal(exiledPlayer, schem);

            Timing.CallDelayed(0.5f, () =>
            {
                if (schem == null || schem.transform == null)
                    return;

                schem.transform.position = labPlayer.GameObject.transform.position;

                var light = Light.Create(Vector3.zero);
                light.Position = schem.transform.position + new Vector3(0f, -0.08f, 0f);
                light.Transform.parent = schem.transform;
                light.Scale = Vector3.one;
                light.Range = 10f;
                light.Intensity = 1.25f;
                light.Color = Color.white;
            });
        });
    }

    public void DiedCassie(Exiled.Events.EventArgs.Player.DyingEventArgs ev)
    {
        if (ev.Player.GetCustomRole() == CRoleTypeId.Scp3005)
        {
            ObjectSpawner.SpawnSchematic("SCP3005", ev.Player.Position, ev.Player.Rotation, Vector3.one);
        }
        if (ev.Player.GetCustomRole() == CRoleTypeId.Scp999)
        {
            ObjectSpawner.SpawnSchematic("Scp999Model", ev.Player.Position, ev.Player.Rotation, Vector3.one);
        }
    }
}