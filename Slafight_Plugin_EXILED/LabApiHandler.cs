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
using UnityEngine;
using Light = LabApi.Features.Wrappers.LightSourceToy;
using Logger = LabApi.Features.Console.Logger;
using Player = LabApi.Features.Wrappers.Player;

namespace Slafight_Plugin_EXILED;

public class LabApiHandler : CustomEventsHandler
{
    public LabApiHandler()
    {
        LabApi.Events.Handlers.ServerEvents.WaitingForPlayers += PickupSetup;

        Exiled.Events.Handlers.Player.Dying += DiedCassieAnnounce;

        LabApi.Events.Handlers.PlayerEvents.SearchedToy += InteractionEvent;

        LabApi.Events.Handlers.ServerEvents.RoundStarted += init;
    }
    ~LabApiHandler()
    {
        LabApi.Events.Handlers.ServerEvents.WaitingForPlayers -= PickupSetup;

        Exiled.Events.Handlers.Player.Dying -= DiedCassieAnnounce;

        LabApi.Events.Handlers.PlayerEvents.SearchedToy -= InteractionEvent;
        LabApi.Events.Handlers.ServerEvents.RoundStarted -= init;
    }
    public bool activatedAntiMemeProtocol = false;
    public bool _activatedAntiMemeProtocolInPast = false;

    public void init()
    {
        activatedAntiMemeProtocol = false;
        _activatedAntiMemeProtocolInPast = false;
    }

    public void InteractionEvent(PlayerSearchedToyEventArgs ev)
    {
        if (ev.Interactable.Position == new Vector3(107.921f, 296.313f, -68.748f))
        {
            if (!activatedAntiMemeProtocol)
            {
                foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
                {
                    if (player.UniqueRole == "SCP-3005")
                    {
                        if (!_activatedAntiMemeProtocolInPast)
                        {
                            player.Health = 10000;
                        }
                        player.EnableEffect(EffectType.Poisoned, 255);
                        player.EnableEffect(EffectType.Decontaminating,255);
                    }
                }
                foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
                {
                    if (player.UniqueRole == "SCP-3005")
                    {
                        if (!_activatedAntiMemeProtocolInPast)
                        {
                            Cassie.MessageTranslated("By order of Facility Manager Control Room , Anti me mu Protocol Activated .",
                                "<color=#ff0087>施設管理者制御室</color>からの命令により、<color=#ff00fa>アンチミームプロトコロル</color>が有効化されました。エージェントによりミーム性物体の非活性化が開始されます。",true,false);
                            _activatedAntiMemeProtocolInPast = true;
                        }
                        else
                        {
                            Cassie.MessageTranslated("Anti me mu Protocol Resumed .",
                                "<color=#ff00fa>アンチミームプロトコル</color>が再開されました。",false,false);
                        }
                        activatedAntiMemeProtocol = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
                {
                    if (player.UniqueRole == "SCP-3005")
                    {
                        player.DisableEffect(EffectType.Poisoned);
                        player.DisableEffect(EffectType.Decontaminating);
                    }
                }
                foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
                {
                    if (player.UniqueRole == "SCP-3005")
                    {
                        Cassie.MessageTranslated("Anti me mu Protocol Stopped .",
                            "<color=#ff00fa>アンチミームプロトコル</color>が停止されました。",false,false);
                        activatedAntiMemeProtocol = false;
                        break;
                    }
                }
            }
        }
    }

    public void PickupSetup()
    {
        Logger.Info("LabApi Loader: Green");

        Timing.CallDelayed(1.05f, () =>
        {
            var HIDTurret = CustomItem.TrySpawn(1,new Vector3(134.94f,300.65f,-65f),out var pickup);
        });
    }

    public void Schem3005(Player player)
    {
        Timing.CallDelayed(1.5f, () =>
        {
            
            SchematicObject schematicObject;
            try
            {
                schematicObject = ObjectSpawner.SpawnSchematic("SCP3005",Vector3.zero);
            }
            catch (Exception ex)
            {
                Logger.Error("error schem");
                schematicObject = null;
                return;
            }
            
            player.Scale = new Vector3(0.001f,1f,0.001f);
            schematicObject.transform.SetParent(player.GameObject.transform);
            Timing.CallDelayed(0.5f, () =>
            {
                //schematicObject.transform.GetChild(0).localScale = new Vector3(1000f,1f,1000f); // 沼みたいで何かに使えそう
                schematicObject.transform.GetChild(0).localScale = new Vector3(1f,1f,1f);
                schematicObject.transform.position = player.GameObject.transform.position + new Vector3(0f, 0f, 0f);
                var light = Light.Create(Vector3.zero);
                light.Position = schematicObject.transform.position + new Vector3(0f, -0.08f, 0f);
                light.Transform.parent = schematicObject.transform;
                light.Scale = new Vector3(1f,1f,1f);
                light.Range = 10f;
                light.Intensity = 1.25f;
                light.Color = Color.magenta;

                Timing.RunCoroutine(DestroyCoroutine(schematicObject, player));
            });
        });
    }

    public void DiedCassieAnnounce(DyingEventArgs ev)
    {
        if (ev.Player.UniqueRole == "SCP-3005")
        {
            SchematicObject schematicObject = ObjectSpawner.SpawnSchematic("SCP3005",ev.Player.Position,ev.Player.Rotation,Vector3.one);
        }
    }

    private IEnumerator<float> DestroyCoroutine(SchematicObject schem, Exiled.API.Features.Player player)
    {
        for (;;)
        {
            yield return Timing.WaitForSeconds(1);
            if (player.UniqueRole == null)
            {
                schem.Destroy();
                yield break;
            }

            if (player.UniqueRole == "FIFTHIST")
            {
                schem.Destroy();
                yield break;
            }
            if (player.UniqueRole == "F_Priest")
            {
                schem.Destroy();
                yield break;
            }
            if (player.UniqueRole == "Scp096_Anger")
            {
                schem.Destroy();
                yield break;
            }
            if (player.UniqueRole == "CI_Commando")
            {
                schem.Destroy();
                yield break;
            }
            if (player.UniqueRole == "HdInfantry")
            {
                schem.Destroy();
                yield break;
            }
            if (player.UniqueRole == "HdCommando")
            {
                schem.Destroy();
                yield break;
            }
        }
    }
}