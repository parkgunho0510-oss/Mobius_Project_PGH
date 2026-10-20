using UnityEngine;

public class StressReceiver : MonoBehaviour 
{

    private float _traumaDecay = 0.6f;
    private float _trauma;

    private Vector3 _lastPosition;
    private Vector3 _lastRotation;

    private Vector3 _originPosition;
    private Quaternion _originRotation;

    // 이전 프레임 쉐이크 오프셋 저장
    //private Vector3 _prevPositionOffset;
    //private Vector3 _prevRotationOffset;

    private float _noiseFrequency = 10f;

    [Header("Shake Settings")]
    public float TraumaExponent = 2f;

    [Header("Rotation Shake")]
    public Vector3 MaximumAngularShake =
        new Vector3(2.5f, 2.5f, 1f);

    [Header("Position Shake")]
    public Vector3 MaximumTranslationShake =
        new Vector3(0.03f, 0.03f, 0f);

    private void Awake()
    {
        _originPosition = transform.localPosition;
        _originRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        float shake = Mathf.Pow(_trauma, TraumaExponent);

        if (shake > 0f)
        {
            // 위치 쉐이크 계산
            _lastPosition = new Vector3(
                MaximumTranslationShake.x *
                (Mathf.PerlinNoise(0, Time.time * _noiseFrequency) * 2f - 1f),

                MaximumTranslationShake.y *
                (Mathf.PerlinNoise(1, Time.time * _noiseFrequency) * 2f - 1f),

                MaximumTranslationShake.z *
                (Mathf.PerlinNoise(2, Time.time * _noiseFrequency) * 2f - 1f)

            ) * shake;

            // 회전 쉐이크 계산

            /*_lastRotation = new Vector3(
                    Random.Range(-1f, 1f) * MaximumAngularShake.x,
                    Random.Range(-1f, 1f) * MaximumAngularShake.y,
                    Random.Range(-0.5f, 0.5f) * MaximumAngularShake.z
             ) * shake; */

            _lastRotation = new Vector3(
                MaximumAngularShake.x *
                (Mathf.PerlinNoise(3, Time.time * _noiseFrequency) * 2f - 1f),

                MaximumAngularShake.y *
                (Mathf.PerlinNoise(4, Time.time * _noiseFrequency) * 2f - 1f),

                MaximumAngularShake.z *
                (Mathf.PerlinNoise(5, Time.time * _noiseFrequency) * 2f - 1f)

            ) * shake;

            transform.localPosition = _originPosition + _lastPosition;

            transform.localRotation =
                _originRotation * Quaternion.Euler(_lastRotation);

            

            // 이전 쉐이크 제거 후 새 쉐이크 적용
            /*transform.localPosition =
                transform.localPosition
                - _prevPositionOffset
                + _lastPosition;

            transform.localRotation =
                transform.localRotation
                * Quaternion.Inverse(Quaternion.Euler(_prevRotationOffset))
                * Quaternion.Euler(_lastRotation);

            _prevPositionOffset = _lastPosition;
            _prevRotationOffset = _lastRotation;
            */
            // trauma 감소
            _trauma = Mathf.Clamp01(
                _trauma - Time.deltaTime * _traumaDecay);
        }
        else
        {
            transform.localPosition = _originPosition;
            transform.localRotation = _originRotation;

            // 남아있는 쉐이크 제거
            /*transform.localPosition -= _prevPositionOffset;

            transform.localRotation =
                transform.localRotation *
                Quaternion.Inverse(
                    Quaternion.Euler(_prevRotationOffset));

            _prevPositionOffset = Vector3.zero;
            _prevRotationOffset = Vector3.zero;

            _lastPosition = Vector3.zero;
            _lastRotation = Vector3.zero;*/
        }
    }

    public void InduceStress(float stress)
    {
        _trauma = Mathf.Clamp01(_trauma + stress);
    }

    public void SetNoiseFrequency(float frequency)
    {
        _noiseFrequency = frequency;
    }

    public void SetTraumaDecay(float decay)
    {
        _traumaDecay = decay;
    }
    /*private float _trauma;

    private Vector3 _lastPosition;
    private Vector3 _lastRotation;

    // 이전 프레임 쉐이크 오프셋 저장
    private Vector3 _prevPositionOffset;
    private Vector3 _prevRotationOffset;

    // 카메라 원래 위치/회전 저장
    //private Vector3 _initialLocalPos;
    // private Quaternion _initialLocalRot;

    // 무기별 흔들림 속도
    private float _noiseFrequency = 25f;

    [Tooltip("Exponent for calculating the shake factor.")]
    public float TraumaExponent = 1f;

    [Tooltip("Maximum angle that the gameobject can shake.")]
    public Vector3 MaximumAngularShake =
        new Vector3(2f, 2f, 1f);

    [Tooltip("Maximum translation that the gameobject can receive.")]
    public Vector3 MaximumTranslationShake =
        new Vector3(0.03f, 0.03f, 0f);

    private void Awake()
    {
        // 초기 위치/회전 저장
        _initialLocalPos = transform.localPosition;
        _initialLocalRot = transform.localRotation;
    }

    private void Update()
    {
        float shake = Mathf.Pow(_trauma, TraumaExponent);

        if (shake > 0f)
        {
            // Position Shake
            _lastPosition = new Vector3(
                MaximumTranslationShake.x *
                (Mathf.PerlinNoise(0, Time.time * _noiseFrequency) * 2f - 1f),

                MaximumTranslationShake.y *
                (Mathf.PerlinNoise(1, Time.time * _noiseFrequency) * 2f - 1f),

                MaximumTranslationShake.z *
                (Mathf.PerlinNoise(2, Time.time * _noiseFrequency) * 2f - 1f)

            ) * shake;

            // Rotation Shake
            _lastRotation = new Vector3(
                MaximumAngularShake.x *
                (Mathf.PerlinNoise(3, Time.time * _noiseFrequency) * 2f - 1f),

                MaximumAngularShake.y *
                (Mathf.PerlinNoise(4, Time.time * _noiseFrequency) * 2f - 1f),

                MaximumAngularShake.z *
                (Mathf.PerlinNoise(5, Time.time * _noiseFrequency) * 2f - 1f)

            ) * shake;

            transform.localPosition = transform.localPosition - _prevPositionOffset + _lastPosition;

            transform.localRotation =
                transform.localRotation *
                Quaternion.Inverse(Quaternion.Euler(_prevRotationOffset)) *
                Quaternion.Euler(_lastRotation);

            _prevPositionOffset = _lastPosition;
            _prevRotationOffset = _lastRotation;

            // 초기값 기준으로 적용 (누적 X)
            transform.localPosition =
                _initialLocalPos + _lastPosition;

            transform.localRotation =
                _initialLocalRot *
                Quaternion.Euler(_lastRotation);

            // Trauma 감소
            _trauma = Mathf.Clamp01(
                _trauma - Time.deltaTime);
        }
        else
        {
            // 원래 상태 복구
            transform.localPosition = _initialLocalPos;
            transform.localRotation = _initialLocalRot;

            _lastPosition = Vector3.zero;
            _lastRotation = Vector3.zero;
        }
    }

    /// <summary>
    /// 카메라 흔들림 강도 추가
    /// </summary>
    public void InduceStress(float stress)
    {
        _trauma = Mathf.Clamp01(_trauma + stress);
    }

    /// <summary>
    /// 흔들림 속도 설정
    /// </summary>
    public void SetNoiseFrequency(float frequency)
    {
        _noiseFrequency = frequency;
    }*/

    /*private float _trauma;
    private Vector3 _lastPosition;
    private Vector3 _lastRotation;
    private float _noiseFrequency = 25f;

    [Tooltip("Exponent for calculating the shake factor. Useful for creating different effect fade outs")]
    public float TraumaExponent = 1;
    [Tooltip("Maximum angle that the gameobject can shake. In euler angles.")]
    public Vector3 MaximumAngularShake = new Vector3(2f, 2f, 1f);
    //public Vector3 MaximumAngularShake = Vector3.one * 5;
    [Tooltip("Maximum translation that the gameobject can receive when applying the shake effect.")]
    public Vector3 MaximumTranslationShake = new Vector3(0.03f, 0.03f, 0f);
    //public Vector3 MaximumTranslationShake = Vector3.one * .75f;
    */
    /*
    private void Update()
    {
        float shake = Mathf.Pow(_trauma, TraumaExponent);*/
    /* Only apply this when there is active trauma */
    /*if(shake > 0)
    {
        var previousRotation = _lastRotation;
        var previousPosition = _lastPosition;*/
    /* In order to avoid affecting the transform current position and rotation each frame we substract the previous translation and rotation */
    /* _lastPosition = new Vector3(
         MaximumTranslationShake.x * (Mathf.PerlinNoise(0, Time.time * 25) * 2 - 1),
         MaximumTranslationShake.y * (Mathf.PerlinNoise(1, Time.time * 25) * 2 - 1),
         MaximumTranslationShake.z * (Mathf.PerlinNoise(2, Time.time * 25) * 2 - 1)
     ) * shake;

     _lastRotation = new Vector3(
         MaximumAngularShake.x * (Mathf.PerlinNoise(3, Time.time * 25) * 2 - 1),
         MaximumAngularShake.y * (Mathf.PerlinNoise(4, Time.time * 25) * 2 - 1),
         MaximumAngularShake.z * (Mathf.PerlinNoise(5, Time.time * 25) * 2 - 1)
     ) * shake;

     transform.localPosition += _lastPosition - previousPosition;
     transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + _lastRotation - previousRotation);
     _trauma = Mathf.Clamp01(_trauma - Time.deltaTime);
 }
 else
 {
     if (_lastPosition == Vector3.zero && _lastRotation == Vector3.zero) return;*/
    /* Clear the transform of any left over translation and rotations */
    /*transform.localPosition -= _lastPosition;
    transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles - _lastRotation);
    _lastPosition = Vector3.zero;
    _lastRotation = Vector3.zero;
}
}

/// <summary>
///  Applies a stress value to the current object.
/// </summary>
/// <param name="Stress">[0,1] Amount of stress to apply to the object</param>
public void InduceStress(float Stress)
{
_trauma = Mathf.Clamp01(_trauma + Stress);
}*/
}