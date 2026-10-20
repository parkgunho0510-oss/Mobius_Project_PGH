using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;

public class CameraEffectController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume _globalVolume;
    [SerializeField] private CinemachineCamera _virtualCamera;

    [Header("Low HP Effect")]
    [SerializeField] private float _lowHpStartRatio = 1f;
    [SerializeField] private float _lowHpMaxIntensity = 0.55f;
    [SerializeField] private float _lowHpSmoothness = 0.75f;

    [Header("Sprint Effect")]
    [SerializeField] private float _sprintFOV = 72f;
    [SerializeField] private float _sprintMotionBlur = 0.35f;
    [SerializeField] private float _sprintLensDistortion = -0.12f;
    [SerializeField] private float _sprintTransitionSpeed = 8f;

    private bool _isSprinting;
    private bool _isPlayingHeavyAttackEffect;

    private Bloom _bloom;
    private LensDistortion _lens;
    private ChromaticAberration _chromatic;
    private MotionBlur _motionBlur;
    private Vignette _vignette;

    private float _defaultFOV;

    private void Awake()
    {
        _globalVolume.profile = Instantiate(_globalVolume.profile);

        _globalVolume.profile.TryGet(out _bloom);
        _globalVolume.profile.TryGet(out _lens);
        _globalVolume.profile.TryGet(out _chromatic);
        _globalVolume.profile.TryGet(out _motionBlur);
        _globalVolume.profile.TryGet(out _vignette);

        _defaultFOV = _virtualCamera.Lens.FieldOfView;

        if (_vignette != null)
        {
            _vignette.color.value = Color.red;
            _vignette.intensity.value = 0f;
            _vignette.smoothness.value = _lowHpSmoothness;
        }
    }
    private void OnEnable()
    {
        UIEvents.OnHPChanged += HandleHPChanged;
    }

    private void OnDisable()
    {
        UIEvents.OnHPChanged -= HandleHPChanged;
    }
    private void Update()
    {
        UpdateSprintEffect();
    }
    private void UpdateSprintEffect()
    {
        if (_isPlayingHeavyAttackEffect)
            return;

        if (_virtualCamera == null)
            return;

        float targetFOV = _isSprinting ? _sprintFOV : _defaultFOV;

        _virtualCamera.Lens.FieldOfView = Mathf.Lerp(
            _virtualCamera.Lens.FieldOfView,
            targetFOV,
            Time.deltaTime * _sprintTransitionSpeed
        );

        if (_motionBlur != null)
        {
            float targetMotionBlur = _isSprinting ? _sprintMotionBlur : 0f;

            _motionBlur.intensity.value = Mathf.Lerp(
                _motionBlur.intensity.value,
                targetMotionBlur,
                Time.deltaTime * _sprintTransitionSpeed
            );
        }

        if (_lens != null)
        {
            float targetLens = _isSprinting ? _sprintLensDistortion : 0f;

            _lens.intensity.value = Mathf.Lerp(
                _lens.intensity.value,
                targetLens,
                Time.deltaTime * _sprintTransitionSpeed
            );
        }
    }
    public void PlayHeavyAttackEffect()
    {
        StartCoroutine(Co_HeavyAttackEffect());
    }
    public void SetSprintEffect(bool isSprinting)
    {
        _isSprinting = isSprinting;
    }
    private IEnumerator Co_HeavyAttackEffect()
    {
        _isPlayingHeavyAttackEffect = true;
        
        // FOV 확대
        _virtualCamera.Lens.FieldOfView = 78f;

        // Bloom
        _bloom.intensity.value = 13f;

        // Lens Distortion
        _lens.intensity.value = -0.3f;

        // Chromatic Aberration
        _chromatic.intensity.value = 4.5f;

        // Motion Blur
        _motionBlur.intensity.value = 2.35f;

        yield return new WaitForSecondsRealtime(0.1f);

        // 원상복구
        _virtualCamera.Lens.FieldOfView = _defaultFOV;

        _bloom.intensity.value = 1f;

        _lens.intensity.value = 0f;

        _chromatic.intensity.value = 0f;

        _motionBlur.intensity.value = 0f;

        _isPlayingHeavyAttackEffect = false;
    }

    void HandleHPChanged(float currentHP, float maxHP)
    {
        if (_vignette == null)
            return;

        if (maxHP <= 0f)
            return;

        float hpRatio = currentHP / maxHP;

        if (hpRatio > _lowHpStartRatio)
        {
            _vignette.intensity.value = 0f;
            return;
        }

        float dangerRatio = 1f - hpRatio / _lowHpStartRatio;

        _vignette.color.value = Color.red;
        _vignette.smoothness.value = _lowHpSmoothness;
        _vignette.intensity.value = Mathf.Lerp(0f, _lowHpMaxIntensity, dangerRatio);
    }
}