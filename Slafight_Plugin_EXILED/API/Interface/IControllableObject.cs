using UnityEngine;

namespace Slafight_Plugin_EXILED.API.Interface;

public interface IControllableObject
{
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }
}