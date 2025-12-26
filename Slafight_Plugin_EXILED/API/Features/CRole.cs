using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using InventorySystem;
using MEC;
using PlayerRoles;
using ProjectMER.Commands.Utility;
using Slafight_Plugin_EXILED.API.Enums;
using UnityEngine;

namespace Slafight_Plugin_EXILED.API.Features;

public abstract class CRole
{
    // 全インスタンスを追跡（重複登録防止）
    private static readonly HashSet<CRole> RegisteredInstances = new();
    
    // 全Roleタイプをキャッシュ
    private static readonly List<Type> RoleTypes;
    
    static CRole()
    {
        RoleTypes = typeof(CRole).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(CRole)) && !t.IsAbstract)
            .ToList();
    }
    
    // 各子クラスがoverride（オプション）
    public virtual void RegisterEvents() { }
    public virtual void UnregisterEvents() { }
    
    // Pluginで呼ぶ：全Role自動登録
    public static void RegisterAllEvents()
    {
        foreach (var type in RoleTypes)
        {
            try
            {
                var instance = (CRole)Activator.CreateInstance(type);
                instance.InternalRegisterEvents();
            }
            catch (Exception ex)
            {
                Log.Error($"CRole.RegisterAllEvents failed for {type.Name}: {ex}");
            }
        }
    }
    
    public static void UnregisterAllEvents()
    {
        foreach (var instance in RegisteredInstances.ToList())
        {
            instance.InternalUnregisterEvents();
        }
        RegisteredInstances.Clear();
    }
    
    // 内部実装：重複防止付き
    private void InternalRegisterEvents()
    {
        if (RegisteredInstances.Add(this))
        {
            RegisterEvents();  // overrideされたメソッド実行
            Log.Debug($"CRole registered: {GetType().Name}");
        }
    }
    
    private void InternalUnregisterEvents()
    {
        if (RegisteredInstances.Remove(this))
        {
            UnregisterEvents();  // overrideされたメソッド実行
            Log.Debug($"CRole unregistered: {GetType().Name}");
        }
    }
    
    public virtual void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        if (player == null)
        {
            Log.Error($"CRole: SpawnRole failed in {player.Nickname}. Reason: Player is Null");
            return;
        }
        if (roleSpawnFlags == RoleSpawnFlags.None)
        {
            Vector3 savePosition = player.Position + new Vector3(0f,0.1f,0f);
            var items = player.Items.ToList(); 
            var ammos = player.Ammo.ToList();
            Timing.CallDelayed(1f, () =>
            {
                player.Position = savePosition;
                player.ClearInventory();
                foreach (var item in items)
                {
                    player.AddItem(item);
                }
                foreach (var ammo in ammos)
                {
                    player.AddAmmo((AmmoType)ammo.Key, ammo.Value);
                }
            });
        }
        else if (roleSpawnFlags == RoleSpawnFlags.AssignInventory)
        {
            Vector3 savePosition = player.Position + new Vector3(0f,0.1f,0f);
            Timing.CallDelayed(1f, () =>
            {
                player.Position = savePosition;
            });
        }
        else if (roleSpawnFlags == RoleSpawnFlags.UseSpawnpoint)
        {
            var items = player.Items.ToList(); 
            var ammos = player.Ammo.ToList();
            Timing.CallDelayed(1f, () =>
            {
                player.ClearInventory();
                foreach (var item in items)
                {
                    player.AddItem(item);
                }
                foreach (var ammo in ammos)
                {
                    player.AddAmmo((AmmoType)ammo.Key, ammo.Value);
                }
            });
        }
    }
}