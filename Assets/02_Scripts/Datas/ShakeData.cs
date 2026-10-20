using UnityEngine;
using Unity.Cinemachine;

[CreateAssetMenu(fileName = "ShakeData", menuName = "Scriptable Objects/ShakeData")]
public class ShakeData : ScriptableObject
{
    
    [Header("Shake Intensity")]
    public float Amplitude = 1.5f;   
    public float Frequency = 2f;   
    public float Duration = 0.3f;

    [Header("Stress")]
    public float Decay = 1.5f;

    [Header("HitStop Settings")]
    public float HitStopDuration = 0.05f; 

    [Header("Direction")]
    public ShakeDirectionType DirectionType;

    [Header("Cinemachine Quality")]
    public NoiseSettings NoiseSettings;

}
