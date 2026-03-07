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
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.CustomMaps;

public static class TerminalRift
{
    private static bool _registered = false;

    private static CoroutineHandle _animCoroutineHandle;
    private static CoroutineHandle _timeoutHandle;
    
    public const float PositionTolerance = 2.25f;
    
    public static SchematicObject RiftObject;
    public static Vector3 RiftObjectPosition;
    public static readonly List<SchematicObject> ControlObjects = new();

    public static bool Invoking { get; private set; } = false;
    private static readonly Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio
        = EventHandler.CreateAndPlayAudio;

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

        KillAllCoroutines();

        ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= SchematicsSetup;
        Exiled.Events.Handlers.Server.RoundStarted -= Setup;
        Exiled.Events.Handlers.Server.RestartingRound -= Cleanup;
        Exiled.Events.Handlers.Player.Hurting -= CancelDeath;

        _registered = false;
    }

    private static void KillAllCoroutines()
    {
        if (_animCoroutineHandle.IsRunning)
            Timing.KillCoroutines(_animCoroutineHandle);
        if (_timeoutHandle.IsRunning)
            Timing.KillCoroutines(_timeoutHandle);

        _animCoroutineHandle = default;
        _timeoutHandle = default;
    }

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
        ControlObjects.Clear();   // ← 追加
        RiftObject = null;        // ← 追加（念のため）
        RiftObjectPosition = Vector3.zero;  // ← 追加
        Invoking = false;
        KillAllCoroutines();
    }

    private static void Cleanup()
    {
        ControlObjects.Clear();
        Invoking = false;
        KillAllCoroutines();
        RiftObject = null;
        RiftObjectPosition = Vector3.zero;
    }

    public static void TryInvoke()
    {
        Log.Debug($"TryInvoke: Invoking={Invoking}");
        if (Invoking) return;

        if (Round.IsLobby || Round.IsEnded)
        {
            Log.Debug("TryInvoke: round is lobby/ended, ignore.");
            return;
        }
        
        Invoking = true;
        KillAllCoroutines();

        if (RiftObject == null || RiftObject.gameObject == null)
        {
            Log.Warn("TryInvoke: RiftObject null or destroyed");
            Invoking = false;
            return;
        }

        if (!ControlObjects.Any())
        {
            Log.Warn("TryInvoke: no control objects");
            Invoking = false;
            return;
        }
        
        CreateAndPlayAudio("Moving.ogg", "RiftElevator", RiftObjectPosition, true, null, false, 30f, 0);

        _animCoroutineHandle = Timing.RunCoroutine(AnimSet());
        _timeoutHandle = Timing.CallDelayed(50f, () => ForceReset("timeout"));
    }

    private static void ForceReset(string reason)
    {
        if (!Invoking) return;

        Log.Warn($"TerminalRift ForceReset ({reason})");
        Invoking = false;
        KillAllCoroutines();
    }

    private static void CancelDeath(HurtingEventArgs ev)
    {
        string roomStr = ev.Player?.CurrentRoom?.Type.ToString() ?? "null";
        Log.Debug($"[CancelDeath] Player={ev.Player?.Nickname ?? "null"}, Damage={ev.DamageHandler.Type}, Attacker={ev.Attacker?.Nickname ?? "null"}, Amount={ev.Amount}, Room={roomStr}");
    
        if (ev.Player == null)
        {
            Log.Debug("[CancelDeath] SKIP: Player is null");
            return;
        }
    
        if (ev.DamageHandler.Type != DamageType.Crushed)
        {
            Log.Debug($"[CancelDeath] SKIP: Not Crushed (is {ev.DamageHandler.Type})");
            return;
        }
    
        if (ev.Attacker != null)
        {
            Log.Debug($"[CancelDeath] SKIP: Has Attacker ({ev.Attacker.Nickname})");
            return;
        }
    
        Log.Debug("[CancelDeath] PASSED: Crushed + No Attacker");
    
        string currentRoomType = ev.Player.CurrentRoom?.Type.ToString();
        Log.Debug($"[CancelDeath] Checking room: '{currentRoomType}'");
    
        if (currentRoomType == "HczTestRoom" ||
            currentRoomType == "Surface" ||
            string.IsNullOrEmpty(currentRoomType))
        {
            Log.Debug($"[CancelDeath] CANCELLED: Target room ({currentRoomType})");
            ev.IsAllowed = false;
        }
        else
        {
            Log.Debug($"[CancelDeath] ALLOWED: Non-target room ({currentRoomType})");
        }
    }

    private static IEnumerator<float> AnimSet()
    {
        Log.Debug("AnimSet start");

        if (RiftObject?.gameObject == null || !ControlObjects.Any())
        {
            ForceReset("no rift/controls at AnimSet start");
            yield break;
        }

        // ラウンドが終わっていないかチェック
        if (Round.IsLobby || Round.IsEnded)
        {
            ForceReset("round ended at AnimSet start");
            yield break;
        }
        
        yield return Timing.WaitUntilDone(Anim(RiftObjectPosition, new Vector3(0f, -28.5f, 0f), 15f));
        Log.Debug("Anim down complete");

        if (!Invoking) yield break;

        yield return Timing.WaitForSeconds(0.2f);
        CreateAndPlayAudio("Beep.ogg", "RiftElevator", RiftObjectPosition, true, null, false, 30f, 0);

        // ラウンド終了チェック
        if (Round.IsLobby || Round.IsEnded)
        {
            ForceReset("round ended after down anim");
            yield break;
        }

        yield return Timing.WaitForSeconds(10f);

        if (!Invoking) yield break;

        if (RiftObject?.gameObject == null)
        {
            ForceReset("rift destroyed before up anim");
            yield break;
        }

        CreateAndPlayAudio("Moving.ogg", "RiftElevator", RiftObjectPosition, true, null, false, 30f, 0);
        yield return Timing.WaitUntilDone(AnimToPosition(RiftObject.Position, RiftObjectPosition, 15f));
        Log.Debug("Anim up complete");

        if (!Invoking) yield break;

        yield return Timing.WaitForSeconds(0.2f);
        CreateAndPlayAudio("Beep.ogg", "RiftElevator", RiftObjectPosition, true, null, false, 30f, 0);
        
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
            if (Round.IsLobby || Round.IsEnded)
            {
                ForceReset("round ended during down anim");
                yield break;
            }

            if (RiftObject?.gameObject == null)
            {
                Log.Warn("Rift invalid during down anim");
                ForceReset("rift destroyed during down anim");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            Vector3 targetPos = Vector3.Lerp(startPos, endPos, progress);
            
            RiftObject.gameObject.transform.position = targetPos;
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
            if (Round.IsLobby || Round.IsEnded)
            {
                ForceReset("round ended during up anim");
                yield break;
            }

            if (RiftObject?.gameObject == null)
            {
                ForceReset("rift destroyed during up anim");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            Vector3 targetPos = Vector3.Lerp(startpos, endpos, progress);
            
            RiftObject.gameObject.transform.position = targetPos;
            yield return 0f;
        }
        
        if (RiftObject?.gameObject != null)
            RiftObject.gameObject.transform.position = endpos;
    }
}

// LabApi ハンドラーはそのままで OK（位置判定だけ）
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
                Vector3.SqrMagnitude(ev.Interactable.Position - control.Position)
                <= TerminalRift.PositionTolerance * TerminalRift.PositionTolerance))
        {
            TerminalRift.TryInvoke();
        }
    }
}
