using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Slafight_Plugin_EXILED.API.Interface;
using UnityEngine;

namespace Slafight_Plugin_EXILED.API.Features;

public abstract class ObjectPrefab : IObjectPrefab
{
    // ==== Main Prefab Data ==== //
    private string _objectInstanceID = string.Empty;
    public string ObjectInstanceID 
    { 
        get => _objectInstanceID; 
        set 
        {
            if (!string.IsNullOrEmpty(ObjectInstanceID))
                throw new InvalidOperationException("ObjectInstanceID can only be set once.");
            _objectInstanceID = value[..Math.Min(5, value.Length)];  // 自動5文字制限
        } 
    }
    public virtual Vector3 Position { get; set; } = Vector3.zero;
    public virtual Quaternion Rotation { get; set; } = Quaternion.identity;
    public virtual Vector3 Scale { get; set; } = Vector3.one;
    
    public virtual ObjectPrefab Create()
    {
        ObjectInstanceID = Guid.NewGuid().ToString("N")[..5];  // C#8以降のスライスで安全に5文字
        InstanceManager.Register(this);
        OnCreate();
        return this;
    }
    
    public virtual void Destroy()
    {
        InstanceManager.Unregister(ObjectInstanceID);
        ObjectInstanceID = string.Empty;
        OnDestroy();
    }

    protected virtual void OnCreate() { }
    protected virtual void OnDestroy() { }
}

// ==== 静的インスタンス管理クラス ==== //
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
        return _instances.Values.ToList();  // コピーして返す（スレッドセーフではないがUnityメインスレッド想定）
    }

    public static void ClearAll()
    {
        foreach (var prefab in _instances.Values.ToList())
        {
            prefab.Destroy();
        }
        _instances.Clear();
    }

    // 全インスタンスを破棄
    public static void DestroyAll()
    {
        var instances = GetAll().ToList();
        foreach (var instance in instances)
        {
            instance.Destroy();
        }
    }
}
