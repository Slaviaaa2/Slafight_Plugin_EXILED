using Exiled.API.Features;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Events.Handlers;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.LabApiBridgeHandlers;

/// <summary>
/// This Handler is wrapper for ObjectPrefabs event receiving.
/// </summary>
public class ObjectPrefabHandler : CustomEventsHandler
{
    public ObjectPrefabHandler()
    {
        PlayerEvents.SearchingToy += OnSearchingToy;
        PlayerEvents.SearchedToy += OnSearchedToy;

        ServerEvents.RoundStarted += OnRoundStarted;
        ServerEvents.RoundRestarted += OnRoundRestarting;
    }

    ~ObjectPrefabHandler()
    {
        PlayerEvents.SearchingToy -= OnSearchingToy;
        PlayerEvents.SearchedToy -= OnSearchedToy;
        
        ServerEvents.RoundStarted -= OnRoundStarted;
        ServerEvents.RoundRestarted -= OnRoundRestarting;
    }
    
    /// <summary>
    /// This method for prefab triggering. Get Near and Invoke that's invoked event.
    /// but it's for SearchingToy.
    /// </summary>
    /// <param name="ev"><seealso cref="PlayerSearchingToyEventArgs"/></param>
    private static void OnSearchingToy(PlayerSearchingToyEventArgs ev)
    {
        var exiledPlayer = Player.Get(ev.Player);
        if (exiledPlayer == null)
            return;

        // LabApi 側が渡してくる「実際に調べられたオブジェクト」の位置を優先
        // Interactable が null のときだけ Player の位置を fallback にする
        var toyPos = ev.Interactable != null
            ? ev.Interactable.Position
            : ev.Player.Position;

        foreach (var prefab in InstanceManager.GetAll())
        {
            if (prefab == null || prefab.ToySearchRadius <= 0f)
                continue;

            float dist = Vector3.Distance(prefab.Position, toyPos);
            if (dist <= prefab.ToySearchRadius)
            {
                prefab.InvokeToySearchingNearby(ev);
            }
        }
    }

    /// <summary>
    /// This method for prefab triggering. Get Near and Invoke that's invoked event.
    /// </summary>
    /// <param name="ev"><seealso cref="PlayerSearchedToyEventArgs"/></param>
    private static void OnSearchedToy(PlayerSearchedToyEventArgs ev)
    {
        var exiledPlayer = Player.Get(ev.Player);
        if (exiledPlayer == null)
            return;

        // LabApi 側が渡してくる「実際に調べられたオブジェクト」の位置を優先
        // Interactable が null のときだけ Player の位置を fallback にする
        var toyPos = ev.Interactable != null
            ? ev.Interactable.Position
            : ev.Player.Position;

        foreach (var prefab in InstanceManager.GetAll())
        {
            if (prefab == null || prefab.ToySearchRadius <= 0f)
                continue;

            float dist = Vector3.Distance(prefab.Position, toyPos);
            if (dist <= prefab.ToySearchRadius)
            {
                prefab.InvokeToySearchedNearby(ev);
            }
        }
    }

    // ==== Pure Triggering Wrappers ==== //
    
    private static void OnRoundStarted()
    {
        foreach (var prefab in InstanceManager.GetAll())
        {
            prefab?.InvokeRoundStarted();
        }
    }

    private static void OnRoundRestarting()
    {
        foreach (var prefab in InstanceManager.GetAll())
        {
            prefab?.InvokeRoundRestarting();
        }
    }
}