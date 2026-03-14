using System;
using System.Text.Json.Serialization;
using Exiled.API.Enums;
using UnityEngine;

[Serializable]
public class PrefabSaveData
{
    public string PrefabType;

    // JSON 上は文字列
    public string RoomTypeName;

    [JsonIgnore]
    public RoomType RoomType
    {
        get
        {
            if (Enum.TryParse<RoomType>(RoomTypeName, out var rt))
                return rt;
            return RoomType.Unknown;
        }
        set => RoomTypeName = value.ToString();
    }

    public Vector3 LocalPosition;
    public Vector3 LocalRotationEuler;
    public Vector3 Scale;
    public int MaxRooms = 1;
    public float AutoDestroyTime;
    public bool AutoDestroyEnabled;
}