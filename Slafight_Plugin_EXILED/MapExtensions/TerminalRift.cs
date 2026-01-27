using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MEC;
using ProjectMER.Events.Arguments;
using ProjectMER.Features.Objects;
using LabApi.Events.Handlers;
using UnityEngine;

namespace Slafight_Plugin_EXILED.MapExtensions;

public static class TerminalRift
{
    private static bool _registered = false;
    private static CoroutineHandle _animCoroutineHandle;
    
    public static void Register()
    {
        if (_registered) return;
        ProjectMER.Events.Handlers.Schematic.SchematicSpawned += SchematicsSetup;
        Exiled.Events.Handlers.Server.RoundStarted += Setup;
        Exiled.Events.Handlers.Server.RestartingRound += Cleanup;
        Exiled.Events.Handlers.Player.Hurting += CancelDeath;
        _registered = true;
    }

    public static void Unregister()
    {
        if (!_registered) return;
        Timing.KillCoroutines(_animCoroutineHandle);
        ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= SchematicsSetup;
        Exiled.Events.Handlers.Server.RoundStarted -= Setup;
        Exiled.Events.Handlers.Server.RestartingRound -= Cleanup;
        Exiled.Events.Handlers.Player.Hurting -= CancelDeath;
        _registered = false;
    }
    
    public const float PositionTolerance = 2.25f;
    
    public static SchematicObject RiftObject;
    public static Vector3 RiftObjectPosition;
    public static readonly List<SchematicObject> ControlObjects = new();

    public static bool Invoking { get; private set; } = false;
    static Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio = EventHandler.CreateAndPlayAudio;

    private static void SchematicsSetup(SchematicSpawnedEventArgs ev)
    {
        switch (ev.Schematic.Name)
        {
            case "Rift":
                RiftObject = ev.Schematic;
                RiftObjectPosition = ev.Schematic.Position;
                break;
            case "TerminalControl":
                ControlObjects.Add(ev.Schematic);
                break;
        }
    }
    
    private static void Setup()
    {
        Invoking = false;
        Timing.KillCoroutines(_animCoroutineHandle);
    }

    private static void Cleanup()
    {
        ControlObjects.Clear();
        Invoking = false;
        Timing.KillCoroutines(_animCoroutineHandle);
    }

    public static void TryInvoke()
    {
        Log.Debug($"TryInvoke: Invoking={Invoking}");
        if (Invoking) return;
        
        Invoking = true;
        Timing.KillCoroutines(_animCoroutineHandle);

        if (RiftObject == null)
        {
            Log.Warn("TryInvoke: RiftObject null");
            Invoking = false;
            return;
        }
        
        CreateAndPlayAudio("Moving.ogg","RiftElevator",RiftObjectPosition,true,null,false,30f,0);
        _animCoroutineHandle = Timing.RunCoroutine(AnimSet());
        
        Timing.CallDelayed(50f, () => ForceReset("timeout"));
    }

    private static void ForceReset(string reason)
    {
        if (Invoking) {
            Log.Warn($"TerminalRift ForceReset ({reason})");
            Invoking = false;
            Timing.KillCoroutines(_animCoroutineHandle);
        }
    }

    private static void CancelDeath(HurtingEventArgs ev)
    {
        if ((ev.DamageHandler.Type == DamageType.Crushed || !ev.IsInstantKill) && 
            ev.Player?.CurrentRoom.Type == RoomType.HczTestRoom)  // TestRoom限定に変更
        {
            ev.IsAllowed = false;
        }
    }

    private static IEnumerator<float> AnimSet()
    {
        Log.Debug("AnimSet start");
        if (RiftObject?.gameObject == null || !ControlObjects.Any()) {
            ForceReset("no rift/controls");
            yield break;
        }
        
        yield return Timing.WaitUntilDone(Anim(RiftObjectPosition, new Vector3(0f, -28.5f, 0f), 15f));
        Log.Debug("Anim down complete");
        yield return Timing.WaitForSeconds(0.2f);
        CreateAndPlayAudio("Beep.ogg","RiftElevator",RiftObjectPosition,true,null,false,30f,0);
        yield return Timing.WaitForSeconds(10f);
        
        CreateAndPlayAudio("Moving.ogg","RiftElevator",RiftObjectPosition,true,null,false,30f,0);
        yield return Timing.WaitUntilDone(AnimToPosition(RiftObject.Position, RiftObjectPosition, 15f));
        Log.Debug("Anim up complete");
        yield return Timing.WaitForSeconds(0.2f);
        CreateAndPlayAudio("Beep.ogg","RiftElevator",RiftObjectPosition,true,null,false,30f,0);
        
        Log.Debug("AnimSet complete");
        Invoking = false;
    }
    
    private static IEnumerator<float> Anim(Vector3 startpos, Vector3 offset, float duration)
    {
        Vector3 startPos = startpos;
        Vector3 endPos = startPos + offset;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            Vector3 targetPos = Vector3.Lerp(startPos, endPos, progress);
            
            if (RiftObject?.gameObject != null)
                RiftObject.gameObject.transform.position = targetPos;
            else {
                Log.Warn("Rift invalid during down anim");
                yield break;
            }
            
            yield return 0f;
        }
        
        if (RiftObject?.gameObject != null)
            RiftObject.gameObject.transform.position = endPos;
    }
    
    private static IEnumerator<float> AnimToPosition(Vector3 startpos, Vector3 endpos, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            Vector3 targetPos = Vector3.Lerp(startpos, endpos, progress);
            
            if (RiftObject?.gameObject != null)
                RiftObject.gameObject.transform.position = targetPos;
            else yield break;
            
            yield return 0f;
        }
        
        if (RiftObject?.gameObject != null)
            RiftObject.gameObject.transform.position = endpos;
    }
}

// ★同じファイルにLabApiハンドラー
public class TerminalRiftLabHandler : CustomEventsHandler
{
    public TerminalRiftLabHandler()
    {
        LabApi.Events.Handlers.PlayerEvents.SearchedToy += OnSearchedToy;
    }

    ~TerminalRiftLabHandler()
    {
        LabApi.Events.Handlers.PlayerEvents.SearchedToy -= OnSearchedToy;
    }
    private void OnSearchedToy(PlayerSearchedToyEventArgs ev)
    {
        if (TerminalRift.ControlObjects.Any(control => 
            Vector3.SqrMagnitude(ev.Interactable.Position - control.Position) <= TerminalRift.PositionTolerance * TerminalRift.PositionTolerance))
        {
            TerminalRift.TryInvoke();
        }
    }
}
