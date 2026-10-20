using System;
using UnityEngine;

public static class SoundEvents
{
    public static event Action<float> SetMasterVolumeEvent;
    public static void RaiseSetMasterVolumeEvent(float volume)
    {
        SetMasterVolumeEvent?.Invoke(volume);
    }

    public static event Action<float> SetBGMVolumeEvent;
    public static void RaiseSetBGMVolumeEvent(float volume)
    {
        SetBGMVolumeEvent?.Invoke(volume);
    }

    public static event Action<float> SetSFXVolumeEvent;
    public static void RaiseSetSFXVolumeEvent(float volume)
    {
        SetSFXVolumeEvent?.Invoke(volume);
    }

    public static event Action<string> SFXPlayedEvent;
    public static void RaiseSFXPlayedEvent(string name)
    {
        SFXPlayedEvent?.Invoke(name);
    }

    public static event Action<string> BGMChangeEvent;
    public static void RaiseBGMChangeEvent(string name)
    {
        BGMChangeEvent?.Invoke(name);
    }
}
