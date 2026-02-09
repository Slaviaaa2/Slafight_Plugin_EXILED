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
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

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
        // 【ログ1】全ダメージイベントを記録（RoomType? → string変換で解決）
        string roomStr = ev.Player?.CurrentRoom?.Type.ToString() ?? "null";
        Log.Debug($"[CancelDeath] Player={ev.Player?.Nickname ?? "null"}, Damage={ev.DamageHandler.Type}, Attacker={ev.Attacker?.Nickname ?? "null"}, Amount={ev.Amount}, Room={roomStr}");
    
        // 【厳密チェック1】プレイヤー存在確認
        if (ev.Player == null)
        {
            Log.Debug("[CancelDeath] SKIP: Player is null");
            return;
        }
    
        // 【厳密チェック2】Crushedダメージのみ
        if (ev.DamageHandler.Type != DamageType.Crushed)
        {
            Log.Debug($"[CancelDeath] SKIP: Not Crushed (is {ev.DamageHandler.Type})");
            return;
        }
    
        // 【厳密チェック3】攻撃者なしのみ
        if (ev.Attacker != null)
        {
            Log.Debug($"[CancelDeath] SKIP: Has Attacker ({ev.Attacker.Nickname})");
            return;
        }
    
        Log.Debug("[CancelDeath] PASSED: Crushed + No Attacker");
    
        // 【厳密チェック4】部屋判定（?.ToString() ?? "null" で安全）
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
