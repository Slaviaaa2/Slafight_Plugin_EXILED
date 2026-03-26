using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using Newtonsoft.Json;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.Commands.DevTools;

// .sl objprefab ...
public class SpawnObjectPrefab : ICommand
{
    public string Command => "objprefab";
    public string[] Aliases { get; } = ["opf"];
    public string Description => "RoomTypeローカル座標でObjectPrefabを編集・保存・ロードする開発用ツール";

    private static readonly Dictionary<Player, ObjectPrefab> Grabbing = new();
    private static readonly Dictionary<Player, float> GrabDistance = new();
    private static readonly Dictionary<Player, Quaternion> GrabRotationOffset = new();
    private static readonly Dictionary<Player, bool> GrabLockRotation = new();
    private static readonly Dictionary<Player, CoroutineHandle> GrabCoroutines = new();

    private static readonly Dictionary<Player, ObjectPrefab> Selected = new();

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"slperm.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: slperm.{Command}";
            return false;
        }

        var player = Player.Get(sender);
        if (player is null)
        {
            response = "Player not found.";
            return false;
        }

        if (arguments.Count == 0)
        {
            response = GetUsage();
            return false;
        }

        string sub = arguments.At(0).ToLower();

        switch (sub)
        {
            case "spawn":   return Spawn(arguments, player, out response);
            case "save":    return Save(arguments, player, out response);
            case "load":    return Load(arguments, player, out response);
            case "list":    return List(arguments, player, out response);
            case "remove":  return Remove(arguments, player, out response);
            case "clear":   return Clear(arguments, player, out response);
            case "tp":      return Tp(arguments, player, out response);

            case "move":    return Move(arguments, player, out response);
            case "rot":     return Rotate(arguments, player, out response);
            case "setpos":  return SetPos(arguments, player, out response);
            case "setrot":  return SetRot(arguments, player, out response);

            case "grab":    return Grab(arguments, player, out response);
            case "grabpos": return GrabPos(arguments, player, out response);
            case "ungrab":  return Ungrab(arguments, player, out response);
            case "offset":  return Offset(arguments, player, out response);
            case "grot":    return GrabRotate(arguments, player, out response);
            case "bring":   return Bring(arguments, player, out response);
            case "bringpos":return BringPos(arguments, player, out response);

            case "max":     return SetMaxRooms(arguments, player, out response);

            case "sel":     return Select(arguments, player, out response);
            case "mod":     return Mod(arguments, player, out response);

            default:
                response = GetUsage();
                return false;
        }
    }

    private string GetUsage() =>
        "Usage:\n" +
        "  .sl objprefab spawn <PrefabClass> [AutoDestroySeconds]\n" +
        "  .sl objprefab save <MapName>\n" +
        "  .sl objprefab load <MapName>\n" +
        "  .sl objprefab list\n" +
        "  .sl objprefab remove <InstanceID>\n" +
        "  .sl objprefab clear\n" +
        "  .sl objprefab tp <InstanceID>\n" +
        "  .sl objprefab move <InstanceID> <dx> <dy> <dz>\n" +
        "  .sl objprefab rot <InstanceID> <pitch> <yaw> <roll>\n" +
        "  .sl objprefab setpos <InstanceID> <x> <y> <z>\n" +
        "  .sl objprefab setrot <InstanceID> <pitch> <yaw> <roll>\n" +
        "  .sl objprefab grab <InstanceID>\n" +
        "  .sl objprefab grabpos <InstanceID>\n" +
        "  .sl objprefab ungrab\n" +
        "  .sl objprefab offset <distanceDelta>\n" +
        "  .sl objprefab grot <pitch> <yaw> <roll>\n" +
        "  .sl objprefab bring <InstanceID>\n" +
        "  .sl objprefab bringpos <InstanceID>\n" +
        "  .sl objprefab max <InstanceID> <count>\n" +
        "  .sl objprefab sel [InstanceID|none]\n" +
        "  .sl objprefab mod <info|pos|addpos|rot|max|autodestroy|bring>\n";

    // ===== spawn =====
    private bool Spawn(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 2)
        {
            response = "Usage: .sl objprefab spawn <PrefabClass> [AutoDestroySeconds]";
            return false;
        }

        string prefabTypeName = args.At(1);

        Type prefabType = Assembly.GetExecutingAssembly().GetTypes()
            .FirstOrDefault(t => t.IsSubclassOf(typeof(ObjectPrefab)) &&
                                 (t.Name.Equals(prefabTypeName, StringComparison.OrdinalIgnoreCase) ||
                                  t.FullName?.EndsWith("." + prefabTypeName, StringComparison.OrdinalIgnoreCase) == true));

        if (prefabType == null)
        {
            response = $"Prefab class '{prefabTypeName}' not found.";
            return false;
        }

        float autoDestroyTime = -1f;
        if (args.Count > 2 && !float.TryParse(args.At(2), out autoDestroyTime))
        {
            response = "AutoDestroySeconds must be a number.";
            return false;
        }

        var prefab = (ObjectPrefab)Activator.CreateInstance(prefabType)!;

        prefab.Position = player.Position + Vector3.up * 1.5f;
        prefab.Rotation = player.Rotation;
        prefab.Scale = Vector3.one;
        prefab.AutoDestroyEnabled = args.Count > 2 && autoDestroyTime > 0f;
        prefab.AutoDestroyTime = autoDestroyTime;
        prefab.MaxRooms = 1;

        prefab.Create();

        response = $"Spawned prefab '{prefabType.Name}' with ID {prefab.ObjectInstanceID}.";
        return true;
    }

    // ===== save =====
    private bool Save(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 2)
        {
            response = "Usage: .sl objprefab save <MapName>";
            return false;
        }

        string mapName = args.At(1);

        var prefabs = InstanceManager.GetAll().ToList();
        if (!prefabs.Any())
        {
            response = "No ObjectPrefab instances to save.";
            return false;
        }

        var cfg = new ObjectPrefabConfig();

        foreach (var p in prefabs)
        {
            var closestRoom = Room.List
                .OrderBy(r => Vector3.Distance(r.Position, p.Position))
                .FirstOrDefault();

            if (closestRoom == null)
            {
                Log.Warn($"[Save] Skipping prefab {p.ObjectInstanceID}: No closest room found");
                continue;
            }

            var room = closestRoom;
            var roomType = room.Type;

            Quaternion inv = Quaternion.Inverse(room.Rotation);
            Vector3 localPos = inv * (p.Position - room.Position);
            Quaternion localRot = inv * p.Rotation;

            var op = p as ObjectPrefab;

            cfg.Prefabs.Add(new PrefabSaveData
            {
                PrefabType = p.GetType().FullName,
                RoomType = roomType,
                LocalPosition = localPos,
                LocalRotationEuler = localRot.eulerAngles,
                Scale = p.Scale,
                MaxRooms = op?.MaxRooms ?? 1,
                AutoDestroyTime = p.AutoDestroyTime,
                AutoDestroyEnabled = p.AutoDestroyEnabled,
                Options = p.CollectOptions(),
            });
        }

        cfg.Save(mapName);

        response = $"Saved {cfg.Prefabs.Count} prefabs to map '{mapName}'.";
        return true;
    }

    // ===== load =====
    private bool Load(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 2)
        {
            response = "Usage: .sl objprefab load <MapName>";
            return false;
        }

        string mapName = args.At(1);
        int count = ObjectPrefabLoader.LoadMap(mapName);

        response = $"Loaded {count} prefabs from map '{mapName}'.";
        return true;
    }

    // ===== list =====
    private bool List(ArraySegment<string> args, Player player, out string response)
    {
        var all = InstanceManager.GetAll()
            .Select(p =>
            {
                var closestRoom = Room.List
                    .OrderBy(r => Vector3.Distance(r.Position, p.Position))
                    .FirstOrDefault();
                var roomName = closestRoom?.Name ?? "Unknown";
                var op = p as ObjectPrefab;
                return $"[{p.ObjectInstanceID}] {p.GetType().Name} @ {roomName} " +
                       $"Pos({p.Position.x:F1},{p.Position.y:F1},{p.Position.z:F1}) MaxRooms:{op?.MaxRooms ?? 1}";
            })
            .ToList();

        response = all.Any()
            ? string.Join("\n", all.Take(50))
            : "No ObjectPrefab instances.";
        return true;
    }

    // ===== remove =====
    private bool Remove(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 2)
        {
            response = "Usage: .sl objprefab remove <InstanceID>";
            return false;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id);

        if (prefab is null)
        {
            response = $"Prefab {id} not found.";
            return false;
        }

        prefab.Destroy();
        response = $"Removed prefab {id}.";
        return true;
    }

    // ===== clear =====
    private bool Clear(ArraySegment<string> args, Player player, out string response)
    {
        InstanceManager.ClearAll();
        response = "Cleared all ObjectPrefab instances.";
        return true;
    }

    // ===== tp =====
    private bool Tp(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 2)
        {
            response = "Usage: .sl objprefab tp <InstanceID>";
            return false;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id);

        if (prefab is null)
        {
            response = $"Prefab {id} not found.";
            return false;
        }

        player.Position = prefab.Position + Vector3.up * 1.2f;
        response = $"Teleported to prefab {id}.";
        return true;
    }

    // ===== move =====
    private bool Move(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 5)
        {
            response = "Usage: .sl objprefab move <InstanceID> <dx> <dy> <dz>";
            return false;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id);
        if (prefab is null)
        {
            response = $"Prefab {id} not found.";
            return false;
        }

        if (!float.TryParse(args.At(2), out float dx) ||
            !float.TryParse(args.At(3), out float dy) ||
            !float.TryParse(args.At(4), out float dz))
        {
            response = "dx, dy, dz must be numbers.";
            return false;
        }

        prefab.Position += new Vector3(dx, dy, dz);
        response = $"Moved prefab {id} by ({dx}, {dy}, {dz}).";
        return true;
    }

    // ===== setpos =====
    private bool SetPos(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 5)
        {
            response = "Usage: .sl objprefab setpos <InstanceID> <x> <y> <z>";
            return false;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id);
        if (prefab is null)
        {
            response = $"Prefab {id} not found.";
            return false;
        }

        if (!float.TryParse(args.At(2), out float x) ||
            !float.TryParse(args.At(3), out float y) ||
            !float.TryParse(args.At(4), out float z))
        {
            response = "x, y, z must be numbers.";
            return false;
        }

        prefab.Position = new Vector3(x, y, z);
        response = $"Set prefab {id} position to ({x}, {y}, {z}).";
        return true;
    }

    // ===== rot =====
    private bool Rotate(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 5)
        {
            response = "Usage: .sl objprefab rot <InstanceID> <pitch> <yaw> <roll>";
            return false;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id);
        if (prefab is null)
        {
            response = $"Prefab {id} not found.";
            return false;
        }

        if (!float.TryParse(args.At(2), out float pitch) ||
            !float.TryParse(args.At(3), out float yaw) ||
            !float.TryParse(args.At(4), out float roll))
        {
            response = "pitch, yaw, roll must be numbers.";
            return false;
        }

        var currentEuler = prefab.Rotation.eulerAngles;
        var newEuler = currentEuler + new Vector3(pitch, yaw, roll);
        prefab.Rotation = Quaternion.Euler(newEuler);

        response = $"Rotated prefab {id} by ({pitch}, {yaw}, {roll}).";
        return true;
    }

    // ===== setrot =====
    private bool SetRot(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 5)
        {
            response = "Usage: .sl objprefab setrot <InstanceID> <pitch> <yaw> <roll>";
            return false;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id);
        if (prefab is null)
        {
            response = $"Prefab {id} not found.";
            return false;
        }

        if (!float.TryParse(args.At(2), out float pitch) ||
            !float.TryParse(args.At(3), out float yaw) ||
            !float.TryParse(args.At(4), out float roll))
        {
            response = "pitch, yaw, roll must be numbers.";
            return false;
        }

        prefab.Rotation = Quaternion.Euler(pitch, yaw, roll);
        response = $"Set prefab {id} rotation to ({pitch}, {yaw}, {roll}).";
        return true;
    }

    // ===== grab =====
    private bool Grab(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 2)
        {
            response = "Usage: .sl objprefab grab <InstanceID>";
            return false;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id) as ObjectPrefab;
        if (prefab is null)
        {
            response = $"Prefab {id} not found or not ObjectPrefab.";
            return false;
        }

        if (Grabbing.TryGetValue(player, out _))
            UngrabInternal(player);

        Grabbing[player] = prefab;
        float dist = Vector3.Distance(player.CameraTransform.position, prefab.Position);
        GrabDistance[player] = dist > 0.5f ? dist : 2f;
        GrabRotationOffset[player] = Quaternion.Inverse(Quaternion.Euler(0, player.CameraTransform.rotation.eulerAngles.y, 0)) * prefab.Rotation;
        GrabLockRotation[player] = false;

        var handle = Timing.RunCoroutine(GrabFollowCoroutine(player));
        GrabCoroutines[player] = handle;

        response = $"Now grabbing prefab {id}.";
        return true;
    }

    // ===== grabpos (位置だけ追従) =====
    private bool GrabPos(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 2)
        {
            response = "Usage: .sl objprefab grabpos <InstanceID>";
            return false;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id) as ObjectPrefab;
        if (prefab is null)
        {
            response = $"Prefab {id} not found or not ObjectPrefab.";
            return false;
        }

        if (Grabbing.TryGetValue(player, out _))
            UngrabInternal(player);

        Grabbing[player] = prefab;
        float dist = Vector3.Distance(player.CameraTransform.position, prefab.Position);
        GrabDistance[player] = dist > 0.5f ? dist : 2f;
        GrabRotationOffset[player] = Quaternion.identity;
        GrabLockRotation[player] = true;

        var handle = Timing.RunCoroutine(GrabFollowCoroutine(player));
        GrabCoroutines[player] = handle;

        response = $"Now grabbing (position only) prefab {id}.";
        return true;
    }

    private bool Ungrab(ArraySegment<string> args, Player player, out string response)
    {
        if (!Grabbing.ContainsKey(player))
        {
            response = "You are not grabbing any prefab.";
            return false;
        }

        UngrabInternal(player);
        response = "Released grabbed prefab.";
        return true;
    }

    private void UngrabInternal(Player player)
    {
        if (GrabCoroutines.TryGetValue(player, out var handle))
        {
            Timing.KillCoroutines(handle);
            GrabCoroutines.Remove(player);
        }

        Grabbing.Remove(player);
        GrabDistance.Remove(player);
        GrabRotationOffset.Remove(player);
        GrabLockRotation.Remove(player);
    }

    private IEnumerator<float> GrabFollowCoroutine(Player player)
    {
        while (Grabbing.TryGetValue(player, out var prefab))
        {
            if (!player.IsConnected || prefab == null)
                break;

            var dist = GrabDistance.TryGetValue(player, out var d) ? d : 2f;
            var rotOffset = GrabRotationOffset.TryGetValue(player, out var ro) ? ro : Quaternion.identity;
            var lockRot = GrabLockRotation.TryGetValue(player, out var lr) && lr;

            prefab.Position = player.CameraTransform.position + player.CameraTransform.forward * dist;

            if (!lockRot)
            {
                Quaternion playerYaw = Quaternion.Euler(0, player.CameraTransform.rotation.eulerAngles.y, 0);
                prefab.Rotation = playerYaw * rotOffset;
            }

            yield return Timing.WaitForSeconds(0.05f);
        }

        UngrabInternal(player);
    }

    // ===== offset =====
    private bool Offset(ArraySegment<string> args, Player player, out string response)
    {
        if (!Grabbing.ContainsKey(player))
        {
            response = "You are not grabbing any prefab.";
            return false;
        }

        if (args.Count < 2)
        {
            response = "Usage: .sl objprefab offset <distanceDelta>";
            return false;
        }

        if (!float.TryParse(args.At(1), out var delta))
        {
            response = "distanceDelta must be a number.";
            return false;
        }
        
        if (GrabDistance.TryGetValue(player, out var currDist))
        {
            GrabDistance[player] = Mathf.Max(0.5f, currDist + delta);
        }

        response = $"Added {delta} to grab distance. Current: {GrabDistance[player]:F1}";
        return true;
    }

    // ===== grot =====
    private bool GrabRotate(ArraySegment<string> args, Player player, out string response)
    {
        if (!Grabbing.ContainsKey(player))
        {
            response = "You are not grabbing any prefab.";
            return false;
        }

        if (args.Count < 4)
        {
            response = "Usage: .sl objprefab grot <pitch> <yaw> <roll>";
            return false;
        }

        if (!float.TryParse(args.At(1), out var pitch) ||
            !float.TryParse(args.At(2), out var yaw) ||
            !float.TryParse(args.At(3), out var roll))
        {
            response = "pitch, yaw, roll must be numbers.";
            return false;
        }

        GrabRotationOffset[player] = Quaternion.Euler(pitch, yaw, roll);
        GrabLockRotation[player] = false;
        response = $"Set grab rotation offset to ({pitch}, {yaw}, {roll}).";
        return true;
    }

    // ===== bring (位置+回転) =====
    private bool Bring(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 2)
        {
            response = "Usage: .sl objprefab bring <InstanceID>";
            return false;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id);
        if (prefab is null)
        {
            response = $"Prefab {id} not found.";
            return false;
        }

        prefab.Position = player.CameraTransform.position + player.CameraTransform.forward * 2f;
        prefab.Rotation = Quaternion.Euler(0, player.CameraTransform.rotation.eulerAngles.y, 0);

        response = $"Brought prefab {id} to your position.";
        return true;
    }

    // ===== bringpos (位置だけ) =====
    private bool BringPos(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 2)
        {
            response = "Usage: .sl objprefab bringpos <InstanceID>";
            return false;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id);
        if (prefab is null)
        {
            response = $"Prefab {id} not found.";
            return false;
        }

        prefab.Position = player.CameraTransform.position + player.CameraTransform.forward * 2f;
        response = $"Brought (position only) prefab {id} to your position.";
        return true;
    }

    // ===== max (ID指定) =====
    private bool SetMaxRooms(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count < 3)
        {
            response = "Usage: .sl objprefab max <InstanceID> <count>";
            return false;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id) as ObjectPrefab;
        if (prefab is null)
        {
            response = $"Prefab {id} not found or not ObjectPrefab.";
            return false;
        }

        if (!int.TryParse(args.At(2), out var count) || count < 0)
        {
            response = "count must be >= 0 integer.";
            return false;
        }

        prefab.MaxRooms = count == 0 ? 1 : count;
        response = $"Set prefab {id} MaxRooms to {prefab.MaxRooms}.";
        return true;
    }

    // ===== sel =====
    private bool Select(ArraySegment<string> args, Player player, out string response)
    {
        if (args.Count == 1)
        {
            if (Selected.TryGetValue(player, out var current))
            {
                response = $"Selected prefab: [{current.ObjectInstanceID}] {current.GetType().Name} at {current.Position}";
                return true;
            }

            response = "No prefab selected. Usage: .sl objprefab sel <InstanceID>";
            return false;
        }

        if (args.At(1).Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            Selected.Remove(player);
            response = "Cleared selected prefab.";
            return true;
        }

        string id = args.At(1);
        var prefab = InstanceManager.Get(id) as ObjectPrefab;
        if (prefab is null)
        {
            response = $"Prefab {id} not found or not ObjectPrefab.";
            return false;
        }

        Selected[player] = prefab;
        response = $"Selected prefab [{prefab.ObjectInstanceID}] {prefab.GetType().Name}.";
        return true;
    }

    // ===== mod =====
    private bool Mod(ArraySegment<string> args, Player player, out string response)
    {
        if (!Selected.TryGetValue(player, out var prefab))
        {
            response = "No prefab selected. Use '.sl objprefab sel <InstanceID>' first.";
            return false;
        }

        if (args.Count < 2)
        {
            response = "Usage: .sl objprefab mod <info|pos|addpos|rot|max|autodestroy|bring>";
            return false;
        }

        string sub = args.At(1).ToLower();
        switch (sub)
        {
            case "info":
                response =
                    $"[{prefab.ObjectInstanceID}] {prefab.GetType().Name}\n" +
                    $" Pos: {prefab.Position}\n" +
                    $" Rot: {prefab.Rotation.eulerAngles}\n" +
                    $" Scale: {prefab.Scale}\n" +
                    $" MaxRooms: {prefab.MaxRooms}\n" +
                    $" AutoDestroy: {(prefab.AutoDestroyEnabled ? prefab.AutoDestroyTime.ToString() : "disabled")}";
                var options = prefab.CollectOptions();
                if (options.Count > 0)
                    response += "\n Options: " + string.Join(", ", options.Select(kv => $"{kv.Key}={kv.Value}"));
                return true;

            case "pos":
                return ModSetPos(args, prefab, out response);

            case "addpos":
                return ModAddPos(args, prefab, out response);

            case "rot":
                return ModSetRot(args, prefab, out response);

            case "max":
                return ModSetMaxRooms(args, prefab, out response);

            case "autodestroy":
                return ModSetAutoDestroy(args, prefab, out response);

            case "bring":
                return ModBring(args, player, prefab, out response);

            default:
                // サブクラス固有のmodサブコマンドを試行
                if (prefab.HandleModCommand(args, out response))
                    return true;
                response = "Unknown subcommand. Use: info / pos / addpos / rot / max / autodestroy / bring";
                return false;
        }
    }

    private bool ModSetPos(ArraySegment<string> args, ObjectPrefab prefab, out string response)
    {
        if (args.Count < 5)
        {
            response = "Usage: .sl objprefab mod pos <x> <y> <z>";
            return false;
        }

        if (!float.TryParse(args.At(2), out var x) ||
            !float.TryParse(args.At(3), out var y) ||
            !float.TryParse(args.At(4), out var z))
        {
            response = "x, y, z must be numbers.";
            return false;
        }

        prefab.Position = new Vector3(x, y, z);
        response = $"Set position to ({x}, {y}, {z}).";
        return true;
    }

    private bool ModAddPos(ArraySegment<string> args, ObjectPrefab prefab, out string response)
    {
        if (args.Count < 5)
        {
            response = "Usage: .sl objprefab mod addpos <dx> <dy> <dz>";
            return false;
        }

        if (!float.TryParse(args.At(2), out var dx) ||
            !float.TryParse(args.At(3), out var dy) ||
            !float.TryParse(args.At(4), out var dz))
        {
            response = "dx, dy, dz must be numbers.";
            return false;
        }

        prefab.Position += new Vector3(dx, dy, dz);
        response = $"Added position ({dx}, {dy}, {dz}). Now: {prefab.Position}";
        return true;
    }

    private bool ModSetRot(ArraySegment<string> args, ObjectPrefab prefab, out string response)
    {
        if (args.Count < 5)
        {
            response = "Usage: .sl objprefab mod rot <pitch> <yaw> <roll>";
            return false;
        }

        if (!float.TryParse(args.At(2), out var pitch) ||
            !float.TryParse(args.At(3), out var yaw) ||
            !float.TryParse(args.At(4), out var roll))
        {
            response = "pitch, yaw, roll must be numbers.";
            return false;
        }

        prefab.Rotation = Quaternion.Euler(pitch, yaw, roll);
        response = $"Set rotation to ({pitch}, {yaw}, {roll}).";
        return true;
    }

    private bool ModSetMaxRooms(ArraySegment<string> args, ObjectPrefab prefab, out string response)
    {
        if (args.Count < 3)
        {
            response = "Usage: .sl objprefab mod max <count>";
            return false;
        }

        if (!int.TryParse(args.At(2), out var count) || count < 0)
        {
            response = "count must be >= 0 integer.";
            return false;
        }

        prefab.MaxRooms = count == 0 ? 1 : count;
        response = $"Set MaxRooms to {prefab.MaxRooms}.";
        return true;
    }

    private bool ModSetAutoDestroy(ArraySegment<string> args, ObjectPrefab prefab, out string response)
    {
        if (args.Count < 3)
        {
            response = "Usage: .sl objprefab mod autodestroy <seconds|-1>";
            return false;
        }

        if (!float.TryParse(args.At(2), out var sec))
        {
            response = "seconds must be a number (or -1 to disable).";
            return false;
        }

        if (sec <= 0f)
        {
            prefab.AutoDestroyEnabled = false;
            prefab.AutoDestroyTime = -1f;
            response = "AutoDestroy disabled.";
            return true;
        }

        prefab.AutoDestroyEnabled = true;
        prefab.AutoDestroyTime = sec;
        response = $"AutoDestroy enabled: {sec} seconds.";
        return true;
    }

    private bool ModBring(ArraySegment<string> args, Player player, ObjectPrefab prefab, out string response)
    {
        prefab.Position = player.CameraTransform.position + player.CameraTransform.forward * 2f;
        response = "Brought selected prefab to your front (position only).";
        return true;
    }
}

public static class ObjectPrefabLoader
{
    /// <summary>
    /// 指定マップ名のObjectPrefabマップファイルを読み込み、
    /// RoomType + Local座標 + MaxRooms に従ってPrefabをスポーンします。
    /// 既存のObjectPrefabは全クリアされます。
    /// </summary>
    public static int LoadMap(string mapName)
    {
        var cfg = ObjectPrefabConfig.Load(mapName);
        InstanceManager.ClearAll();
        int totalSpawned = 0;

        foreach (var data in cfg.Prefabs)
        {
            var type = Type.GetType(data.PrefabType) ??
                       Assembly.GetExecutingAssembly().GetTypes()
                           .FirstOrDefault(t => t.FullName == data.PrefabType || t.Name == data.PrefabType);

            if (type == null || !type.IsSubclassOf(typeof(ObjectPrefab)))
            {
                Log.Warn($"[ObjectPrefabLoader] Type '{data.PrefabType}' not found or not ObjectPrefab.");
                continue;
            }

            var roomsOfType = Room.List
                .Where(r => r.Type == data.RoomType)
                .ToList();

            if (!roomsOfType.Any())
            {
                Log.Warn($"[ObjectPrefabLoader] No rooms of type {data.RoomType} found for prefab '{data.PrefabType}'.");
                continue;
            }

            int maxRoomsFromData = data.MaxRooms;
            if (maxRoomsFromData <= 0)
                maxRoomsFromData = roomsOfType.Count;

            int maxRooms = Mathf.Min(maxRoomsFromData, roomsOfType.Count);

            roomsOfType = roomsOfType.OrderBy(_ => UnityEngine.Random.value).ToList();

            for (int i = 0; i < maxRooms; i++)
            {
                var room = roomsOfType[i];

                Quaternion roomRot = room.Rotation;
                Vector3 worldPos = room.Position + roomRot * data.LocalPosition;
                Quaternion worldRot = roomRot * Quaternion.Euler(data.LocalRotationEuler);

                var prefab = (ObjectPrefab)Activator.CreateInstance(type)!;
                prefab.Position = worldPos;
                prefab.Rotation = worldRot;
                prefab.Scale = data.Scale;
                prefab.AutoDestroyEnabled = data.AutoDestroyEnabled;
                prefab.AutoDestroyTime = data.AutoDestroyTime;
                prefab.MaxRooms = data.MaxRooms <= 0 ? 1 : data.MaxRooms;

                if (data.Options != null && data.Options.Count > 0)
                    prefab.ApplyOptions(data.Options);

                prefab.Create();
                totalSpawned++;
            }
        }

        Log.Info($"[ObjectPrefabLoader] Loaded map '{mapName}' ({totalSpawned} prefabs spawned).");
        return totalSpawned;
    }
}
public class ObjectPrefabConfig
{
    public List<PrefabSaveData> Prefabs { get; set; } = [];

    public static string DirectoryPath =>
        Path.Combine(Paths.Configs, "Slafight_Plugin_Exiled", "Maps");

    public static string GetFilePath(string mapName)
        => Path.Combine(DirectoryPath, $"{mapName}.json");

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        TypeNameHandling = TypeNameHandling.None,
    };

    public static ObjectPrefabConfig Load(string mapName)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            string path = GetFilePath(mapName);

            if (!File.Exists(path))
                return new ObjectPrefabConfig();

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ObjectPrefabConfig>(json, JsonSettings)
                   ?? new ObjectPrefabConfig();
        }
        catch (Exception e)
        {
            Log.Error($"[ObjectPrefabConfig] Load({mapName}) failed: {e}");
            return new ObjectPrefabConfig();
        }
    }

    public void Save(string mapName)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            string path = GetFilePath(mapName);

            var json = JsonConvert.SerializeObject(this, JsonSettings);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Log.Error($"[ObjectPrefabConfig] Save({mapName}) failed: {e}");
        }
    }
}