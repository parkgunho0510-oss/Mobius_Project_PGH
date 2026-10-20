using System;
using UnityEngine;
public static class EffectEvents
{
    public static event Action<EffectType, GameObject> EffectReturnedEvent;
    public static void RaiseEffectReturnedEvent(EffectType type, GameObject obj)
    {
        EffectReturnedEvent?.Invoke(type, obj);
    }

    public static event Action<EffectType, Vector3, Quaternion> EffectPlayedEvent;
    public static void RaiseEffectPlayedEvent(EffectType type, Vector3 position, Quaternion rotation)
    {
        EffectPlayedEvent?.Invoke(type, position, rotation);
    }
}