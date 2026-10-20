using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerFootIK : MonoBehaviour
{
    private Animator _animator;

    [Header("Foot IK Settings")]
    [Range(0, 1f)] public float IKWeight = 1f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _footOffset = 0.1f;
    [SerializeField] private float _raycastDistance = 1.0f;

    [Header("Weight Settings")]
    [SerializeField] private bool _useDynamicWeight = true;
    [SerializeField] private float _runIKWeight = 0.15f; // 달릴 때의 IK 반영률
    private float _currentDynamicWeight = 1f;

    [Header("Smoothing Settings")]
    [SerializeField] private float _positionLerpSpeed = 15f;
    [SerializeField] private float _rotationLerpSpeed = 15f;

    [Header("Knee Hint Settings")]
    [SerializeField] private float _kneeForwardOffset = 0.5f;

    [Header("Body(Pelvis) Settings")]
    [SerializeField] private float _pelvisDownOffset = 0.15f;

    [Header("Rotation Limits")]
    [Range(0, 1f)] public float SideBendingWeight = 0f;

    // 보간을 위한 상태 캐싱
    private Vector3 _lastLeftFootPosition;
    private Vector3 _lastRightFootPosition;
    private Quaternion _lastLeftFootRotation;
    private Quaternion _lastRightFootRotation;
    private bool _isInitialized = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator == null || IKWeight <= 0f) return;
        CalculateDynamicWeight();

        float finalWeight = IKWeight * _currentDynamicWeight;

        if (finalWeight <= 0.01f) return;

        InitializeStateIfNeeded();
        AdjustPelvisPosition(finalWeight);
        ApplyIKWeights(finalWeight);

        // 양발 각각 처리
        ProcessFootIK(AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee, ref _lastLeftFootPosition, ref _lastLeftFootRotation);
        ProcessFootIK(AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee, ref _lastRightFootPosition, ref _lastRightFootRotation);
    }

    private void CalculateDynamicWeight()
    {
        if (!_useDynamicWeight)
        {
            _currentDynamicWeight = 1f;
            return;
        }

        float forward = Mathf.Abs(_animator.GetFloat("Forward"));
        float side = Mathf.Abs(_animator.GetFloat("Side"));

        // 이동 벡터의 크기를 구함 (대기=0, 걷기=1 근처, 뛰기=2 근처)
        float moveMagnitude = new Vector2(forward, side).magnitude;

        // magnitude가 1(걷기)에서 2(뛰기)로 커질수록 가중치를 _runIKWeight로 보간
        // 0~1 구간은 1.0f
        float t = Mathf.Clamp01(moveMagnitude - 1f);
        _currentDynamicWeight = Mathf.Lerp(1f, _runIKWeight, t);
    }

    #region IK 초기화 및 기본 설정
    // 첫 프레임 좌표 스내핑 방지를 위한 초기화
    private void InitializeStateIfNeeded()
    {
        if (_isInitialized) return;

        _lastLeftFootPosition = _animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        _lastRightFootPosition = _animator.GetIKPosition(AvatarIKGoal.RightFoot);
        _lastLeftFootRotation = _animator.GetIKRotation(AvatarIKGoal.LeftFoot);
        _lastRightFootRotation = _animator.GetIKRotation(AvatarIKGoal.RightFoot);

        _isInitialized = true;
    }

    // 골반(몸통)을 내려 무릎이 굽혀지도록 유도
    private void AdjustPelvisPosition(float weight)
    {
        Vector3 bodyPos = _animator.bodyPosition;
        bodyPos.y -= _pelvisDownOffset * weight;
        _animator.bodyPosition = bodyPos;
    }

    // IK 가중치 일괄 적용
    private void ApplyIKWeights(float weight)
    {
        _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, weight);
        _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, weight);
        _animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, weight);

        _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, weight);
        _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, weight);
        _animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, weight);
    }
    #endregion

    #region 발바닥 및 무릎 계산 로직
    // 단일 발과 무릎의 IK 연산을 종합적으로 처리
    private void ProcessFootIK(AvatarIKGoal foot, AvatarIKHint knee, ref Vector3 lastPos, ref Quaternion lastRot)
    {
        Vector3 targetIKPosition = _animator.GetIKPosition(foot);
        Vector3 rayOrigin = targetIKPosition + Vector3.up * 0.5f;

        // 레이캐스트를 통해 지형 판별 후 분기 처리
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _raycastDistance, _groundLayer))
        {
            ApplyFootGrounding(foot, hit, ref lastPos, ref lastRot);
        }
        else
        {
            ApplyAirborneFallback(foot, targetIKPosition, ref lastPos, ref lastRot);
        }

        ApplyKneeHint(knee);
    }

    // 바닥에 닿았을 때의 위치/회전 보간 적용
    private void ApplyFootGrounding(AvatarIKGoal foot, RaycastHit hit, ref Vector3 lastPos, ref Quaternion lastRot)
    {
        // 발바닥 위치 처리
        Vector3 targetFootPosition = hit.point;
        targetFootPosition.y += _footOffset;
        lastPos = Vector3.Lerp(lastPos, targetFootPosition, Time.deltaTime * _positionLerpSpeed);
        _animator.SetIKPosition(foot, lastPos);

        // 발목 회전 처리
        Quaternion currentRotation = _animator.GetIKRotation(foot);
        Vector3 footRight = currentRotation * Vector3.right;
        Vector3 forwardOnlyNormal = Vector3.ProjectOnPlane(hit.normal, footRight).normalized;
        Vector3 finalNormal = Vector3.Lerp(forwardOnlyNormal, hit.normal, SideBendingWeight).normalized;

        Quaternion targetFootRotation = Quaternion.FromToRotation(Vector3.up, finalNormal) * currentRotation;
        lastRot = Quaternion.Slerp(lastRot, targetFootRotation, Time.deltaTime * _rotationLerpSpeed);
        _animator.SetIKRotation(foot, lastRot);
    }

    // 공중 상태일 때 원래 애니메이션 자세로 복귀
    private void ApplyAirborneFallback(AvatarIKGoal foot, Vector3 originalIKPosition, ref Vector3 lastPos, ref Quaternion lastRot)
    {
        lastPos = Vector3.Lerp(lastPos, originalIKPosition, Time.deltaTime * _positionLerpSpeed);
        _animator.SetIKPosition(foot, lastPos);

        Quaternion origRot = _animator.GetIKRotation(foot);
        lastRot = Quaternion.Slerp(lastRot, origRot, Time.deltaTime * _rotationLerpSpeed);
        _animator.SetIKRotation(foot, lastRot);
    }

    // 무릎 방향 지정 (게다리 현상 방지)
    private void ApplyKneeHint(AvatarIKHint knee)
    {
        Vector3 currentKneePos = _animator.GetIKHintPosition(knee);
        Vector3 kneeHintPos = currentKneePos + transform.forward * _kneeForwardOffset;
        _animator.SetIKHintPosition(knee, kneeHintPos);
    }
    #endregion
}