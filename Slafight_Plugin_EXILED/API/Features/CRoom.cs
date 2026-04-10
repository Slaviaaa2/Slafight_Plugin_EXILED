using System;
using Slafight_Plugin_EXILED.API.Interface;
using UnityEngine;

namespace Slafight_Plugin_EXILED.API.Features;

public class CRoom : IControllableObject, ISpawnableObject
{
    // FOR INHERITANCE
    public virtual CRoomType CRoomType { get; }
    
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }

    public void Create()
    {
        
    }

    public void Destroy()
    {
        
    }
    // FOR GLOBAL MANAGEMENT
    public static CRoom? Create(CRoomType CRoomType)
    {
        CRoom? ret = CRoomType switch
        {
            CRoomType.None => null,
            CRoomType.LczExClassD => null,
            CRoomType.HczEx => null,
            _ => throw new ArgumentOutOfRangeException(nameof(CRoomType), CRoomType, null)
        };
        return ret;
    }
}