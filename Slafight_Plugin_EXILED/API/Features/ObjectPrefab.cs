using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using LabApi.Events.Arguments.PlayerEvents;
using MEC;
using Slafight_Plugin_EXILED.API.Interface;
using UnityEngine;

namespace Slafight_Plugin_EXILED.API.Features;

public abstract class ObjectPrefab : IObjectPrefab
{
    private string _objectInstanceID = string.Empty;
    public string ObjectInstanceID
    {
        get => _objectInstanceID;
        set
        {
            if (!string.IsNullOrEmpty(_objectInstanceID))
                throw new InvalidOperationException("ObjectInstanceID can only be set once.");
            _objectInstanceID = value[..Math.Min(5, value.Length)];
        }
    }

    public virtual Vector3 Position { get; set; } = Vector3.zero;
    public virtual Quaternion Rotation { get; set; } = Quaternion.identity;
    public virtual Vector3 Scale { get; set; } = Vector3.one;
    public virtual bool AutoDestroyEnabled { get; set; } = false;
    public virtual float AutoDestroyTime { get; set; } = -1f;
    public virtual CoroutineHandle AutoDestroyCoroutineHandle { get; set; } = new CoroutineHandle();

    /// <summary>
    /// Save/Load 用の MaxRooms。デフォルト 1。
    /// </summary>
    public virtual int MaxRooms { get; set; } = 1;

    public virtual float ToySearchRadius { get; set; } = 0f;

    public virtual ObjectPrefab Create()
    {
        Log.Debug("[ObjectPrefab]Create Invoked.");
        ObjectInstanceID = Guid.NewGuid().ToString("N")[..5];
        InstanceManager.Register(this);
        if (AutoDestroyEnabled)
            AutoDestroyCoroutineHandle = Timing.RunCoroutine(AutoDestroy());
        OnCreate();
        return this;
    }

    public virtual void Destroy()
    {
        Log.Debug("[ObjectPrefab]Destroy Invoked.");

        if (string.IsNullOrEmpty(ObjectInstanceID))
            return;

        InstanceManager.Unregister(ObjectInstanceID);
        Timing.KillCoroutines(AutoDestroyCoroutineHandle);
        OnDestroy();
    }

    protected virtual void OnCreate() { }
    protected virtual void OnDestroy() { }

    protected virtual IEnumerator<float> AutoDestroy()
    {
        yield return Timing.WaitForSeconds(AutoDestroyTime);
        Destroy();
    }

    // Invoke Prefab Events.
    protected virtual void OnToySearchingNearby(PlayerSearchingToyEventArgs eventArgs) { }

    public void InvokeToySearchingNearby(PlayerSearchingToyEventArgs eventArgs)
        => OnToySearchingNearby(eventArgs);
    
    protected virtual void OnToySearchedNearby(PlayerSearchedToyEventArgs eventArgs) { }

    public void InvokeToySearchedNearby(PlayerSearchedToyEventArgs eventArgs)
        => OnToySearchedNearby(eventArgs);
    
    protected virtual void OnRoundStarted() { }
    public void InvokeRoundStarted()
        => OnRoundStarted();

    protected virtual void OnRoundRestarting()
    {
        Destroy();
    }
    public void InvokeRoundRestarting()
        => OnRoundRestarting();
}

public static class InstanceManager
{
    private static readonly Dictionary<string, ObjectPrefab> _instances = new();

    public static void Register(ObjectPrefab prefab)
    {
        if (string.IsNullOrEmpty(prefab.ObjectInstanceID))
            throw new ArgumentException("ObjectInstanceID must be set before registering.");

        _instances[prefab.ObjectInstanceID] = prefab;
    }

    public static void Unregister(string objectInstanceID)
    {
        _instances.Remove(objectInstanceID);
    }

    public static ObjectPrefab? Get(string objectInstanceID)
    {
        return _instances.TryGetValue(objectInstanceID, out var prefab) ? prefab : null;
    }

    public static IEnumerable<ObjectPrefab> GetAll()
    {
        return _instances.Values.ToList();
    }

    public static void ClearAll()
    {
        foreach (var prefab in _instances.Values.ToList())
            prefab.Destroy();

        _instances.Clear();
    }

    public static void DestroyAll()
    {
        var instances = GetAll().ToList();
        foreach (var instance in instances)
            instance.Destroy();
    }
}