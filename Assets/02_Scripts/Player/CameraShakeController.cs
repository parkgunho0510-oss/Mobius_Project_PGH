// CameraShakeController.cs
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShakeController : MonoBehaviour
{
    //[Header("ShakeData (float intensity¿ë fallback)")]
    //[SerializeField] private ShakeData _criticalHitData;

    private CinemachineImpulseSource _impulseSource;
    [SerializeField] private StressReceiver _stressReceiver;

    private void Awake()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        if (_stressReceiver == null) _stressReceiver = FindFirstObjectByType<StressReceiver>();
    }

    private void OnEnable()
    {
        CameraEvents.OnShakeRequested += HandleShake;

    }

    private void OnDisable()
    {
        CameraEvents.OnShakeRequested -= HandleShake;

    }

    private void HandleShake(ShakeData data, Vector3 direction)
    {
        if (data == null) return;

        if (_impulseSource != null)
        {
            _impulseSource.ImpulseDefinition.ImpulseDuration = data.Duration;
            _impulseSource.DefaultVelocity = Vector3.one * data.Frequency;

            Vector3 impulseVec = BuildImpulseVector(data, direction) * data.Amplitude;
            _impulseSource.GenerateImpulseWithVelocity(impulseVec);
        }

        if (_stressReceiver != null)
        {
            float trauma = 0.85f;
            //Mathf.Clamp01(data.Amplitude * 0.08f);

            
            _stressReceiver.SetNoiseFrequency(data.Frequency);
            _stressReceiver.SetTraumaDecay(data.Decay);
            _stressReceiver.InduceStress(trauma);
        }

        /*if (_stressReceiver != null && data.Amplitude > 2.0f)
        {

            _stressReceiver.InduceStress(data.Amplitude * 0.2f);
        }*/

        if (data.DirectionType == ShakeDirectionType.VERTICAL)
        {
            _stressReceiver.InduceStress(0.4f);
        }

        StopAllCoroutines();
        StartCoroutine(HitStopCoroutine(data.HitStopDuration));
    }
    private IEnumerator HitStopCoroutine(float duration)
    {
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        float elapsed = 0f;
        float recoverTime = 0.1f;
        while (elapsed < recoverTime)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(0.05f, 1f, elapsed / recoverTime);
            yield return null;
        }
        Time.timeScale = 1f;
    }

    private Vector3 BuildImpulseVector(ShakeData data, Vector3 direction)
    {
        switch (data.DirectionType)
        {
            case ShakeDirectionType.RADIAL:
                // for attack
                return direction == Vector3.zero
                    ? Vector3.right
                    : new Vector3(direction.x, direction.y * 0.3f, 0f).normalized;

            case ShakeDirectionType.VERTICAL:
                // for Slam Attack
                return Vector3.down;

            case ShakeDirectionType.HORIZONTAL:
                // for DashAttack
                return direction == Vector3.zero
                    ? Vector3.right
                    : new Vector3(direction.x, 0f, 0f).normalized;

            case ShakeDirectionType.ALLAXIS:
                // for AOE Damage
                return new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-0.5f, 0.5f),
                    0f).normalized;

            case ShakeDirectionType.NOISE:
            default:
                // For Parrying
                return new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.05f, 0.05f), 0f).normalized;
        }
    }
}