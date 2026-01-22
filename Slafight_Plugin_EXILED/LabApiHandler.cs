using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using LabApi.Features;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using MEC;
using PlayerRoles;
using ProjectMER.Events.Arguments;
using ProjectMER.Features.Extensions;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Light = LabApi.Features.Wrappers.LightSourceToy;
using Logger = LabApi.Features.Console.Logger;
using Player = LabApi.Features.Wrappers.Player;

namespace Slafight_Plugin_EXILED
{
    public class LabApiHandler : CustomEventsHandler
    {
        public LabApiHandler()
        {
            LabApi.Events.Handlers.ServerEvents.RoundStarted += PickupSetup;
            Exiled.Events.Handlers.Player.Dying += DiedCassie;
            LabApi.Events.Handlers.PlayerEvents.SearchedToy += InteractionEvent;
            LabApi.Events.Handlers.ServerEvents.RoundStarted += Init;
            ProjectMER.Events.Handlers.Schematic.SchematicSpawned += PickupSetupBySchemPoint;
        }

        ~LabApiHandler()
        {
            LabApi.Events.Handlers.ServerEvents.RoundStarted -= PickupSetup;
            Exiled.Events.Handlers.Player.Dying -= DiedCassie;
            LabApi.Events.Handlers.PlayerEvents.SearchedToy -= InteractionEvent;
            LabApi.Events.Handlers.ServerEvents.RoundStarted -= Init;
            ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= PickupSetupBySchemPoint;
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
                    if (player.UniqueRole == "SCP-3005")
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

        public void PickupSetup()
        {
            Logger.Info("LabApi Loader: Green");

            Timing.CallDelayed(1.05f, () =>
            {
                CustomItem.TrySpawn(1, new Vector3(134.94f, 300.65f, -65f), out Pickup _);
                CustomItem.TrySpawn(2015, new Vector3(-31.42325f, 253f, -102.171f), out Pickup _);
            });
        }

        public void PickupSetupBySchemPoint(SchematicSpawnedEventArgs ev)
        {
            Vector3 pos;
            switch (ev.Schematic.Name)
            {
                case "CISR_GoCRailgun":
                {
                    pos = ev.Schematic.gameObject.transform.position;
                    ev.Schematic.Destroy();
                    CustomItem.TrySpawn(50, pos, out Pickup _);
                    break;
                }
                case "CISR_OldPrivateCard":
                {
                    pos = ev.Schematic.gameObject.transform.position;
                    ev.Schematic.Destroy();
                    if (CustomItem.TrySpawn(104, pos, out Pickup pickup))
                        pickup.Rotation *= Quaternion.Euler(180f, 0f, 0f);
                    break;
                }
                case "CISR_OldCECard":
                {
                    pos = ev.Schematic.gameObject.transform.position;
                    ev.Schematic.Destroy();
                    if (CustomItem.TrySpawn(100, pos, out Pickup pickup))
                        pickup.Rotation *= Quaternion.Euler(180f, 0f, 0f);
                    break;
                }
                case "CISR_Scp1425":
                {
                    pos = ev.Schematic.gameObject.transform.position;
                    ev.Schematic.Destroy();
                    if (CustomItem.TrySpawn(1102, pos, out Pickup pickup))
                        pickup.Rotation *= Quaternion.Euler(180f, 0f, 0f);
                    break;
                }
                case "CISR_SNAV300":
                    pos = ev.Schematic.gameObject.transform.position;
                    ev.Schematic.Destroy();
                    if (CustomItem.TrySpawn(2012, pos, out Pickup _)) ;
                    break;
            }
        }

        /// <summary>
        /// SCP-3005 用 schematic 生成のみ。FakeScale は使わない。
        /// </summary>
        public void Schem3005(Player player)
        {
            Timing.CallDelayed(1.5f, () =>
            {
                SchematicObject schematicObject;
                try
                {
                    schematicObject = ObjectSpawner.SpawnSchematic("SCP3005", Vector3.zero);
                }
                catch (Exception ex)
                {
                    Logger.Error("error schem");
                    return;
                }

                player.Scale = new Vector3(0.001f, 1f, 0.001f);
                schematicObject.transform.SetParent(player.GameObject.transform);

                Timing.CallDelayed(0.5f, () =>
                {
                    schematicObject.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
                    schematicObject.transform.position = player.GameObject.transform.position;

                    var light = Light.Create(Vector3.zero);
                    light.Position = schematicObject.transform.position + new Vector3(0f, -0.08f, 0f);
                    light.Transform.parent = schematicObject.transform;
                    light.Scale = new Vector3(1f, 1f, 1f);
                    light.Range = 10f;
                    light.Intensity = 1.25f;
                    light.Color = Color.magenta;

                    player.DestroySchematic(schematicObject);
                    var exiledPlayer = Exiled.API.Features.Player.Get(player.NetworkId);
                    Timing.RunCoroutine(DestroyCoroutine(schematicObject, exiledPlayer));
                });
            });
        }
        
        public void Schem999(Player player)
        {
            Timing.CallDelayed(1.5f, () =>
            {
                SchematicObject schematicObject;
                try
                {
                    schematicObject = ObjectSpawner.SpawnSchematic("Scp999Model", Vector3.zero);
                }
                catch (Exception ex)
                {
                    Logger.Error("error schem");
                    return;
                }

                player.Scale = new Vector3(0.35f, 0.2f, 0.35f);
                schematicObject.transform.SetParent(player.GameObject.transform);

                Timing.CallDelayed(0.5f, () =>
                {
                    //schematicObject.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
                    schematicObject.transform.position = player.GameObject.transform.position + new Vector3(0f,0f,0.05f);
                    for (int i = 0; i < schematicObject.transform.childCount; i++)
                    {
                        var child = schematicObject.transform.GetChild(i);
                        Logger.Info($"[Scp999Model] Child {i}: {child.name}, localScale={child.localScale}");
                    }

                    player.DestroySchematic(schematicObject);
                    var exiledPlayer = Exiled.API.Features.Player.Get(player.NetworkId);
                    Timing.RunCoroutine(DestroyCoroutine(schematicObject, exiledPlayer));
                });
            });
        }

        public void SchemSnowWarrier(Player player)
        {
            Timing.CallDelayed(1.5f, () =>
            {
                SchematicObject schematicObject;
                try
                {
                    schematicObject = ObjectSpawner.SpawnSchematic("SnowWarrier", Vector3.zero);
                }
                catch (Exception ex)
                {
                    Logger.Error("SnowWarrier schem spawn error: " + ex);
                    return;
                }

                schematicObject.transform.SetParent(player.GameObject.transform);

                Timing.CallDelayed(0.5f, () =>
                {
                    schematicObject.transform.position = player.GameObject.transform.position;
                    var light = Light.Create(Vector3.zero);
                    light.Position = schematicObject.transform.position + new Vector3(0f, -0.08f, 0f);
                    light.Transform.parent = schematicObject.transform;
                    light.Scale = new Vector3(1f, 1f, 1f);
                    light.Range = 10f;
                    light.Intensity = 1.25f;
                    light.Color = Color.white;

                    player.DestroySchematic(schematicObject);
                    var exiledPlayer = Exiled.API.Features.Player.Get(player.NetworkId);
                    Timing.RunCoroutine(DestroyCoroutine(schematicObject, exiledPlayer));
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

        private readonly List<CRoleTypeId> HasModels = new()
        {
            CRoleTypeId.Scp3005,
            CRoleTypeId.Scp999,
            CRoleTypeId.SnowWarrier
        };

        private IEnumerator<float> DestroyCoroutine(SchematicObject schem, Exiled.API.Features.Player player)
        {
            yield return Timing.WaitForSeconds(1f);
            for (;;)
            {
                yield return Timing.WaitForSeconds(1f);

                if (player == null || !player.IsConnected || !HasModels.Contains(player.GetCustomRole()))
                {
                    schem.Destroy();
                    yield break;
                }
            }
        }
    }
}