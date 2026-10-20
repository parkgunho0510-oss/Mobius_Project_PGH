using System;
using UnityEngine;

public static class CameraEvents
{
    
    public static event Action<ShakeData, Vector3> OnShakeRequested;
    public static void RaiseShake(ShakeData data, Vector3 direction = default)
    { 
        OnShakeRequested?.Invoke(data, direction); 
    }


    /*public static event Action<float, Vector3> OnCriticalHitShake;

    public static void RaiseCriticalHitShake(float intensity, Vector3 hitDirection = default)
    {
        OnCriticalHitShake?.Invoke(intensity, hitDirection);
    }*/
}
