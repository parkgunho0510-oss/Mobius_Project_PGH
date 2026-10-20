using UnityEngine;

public class PlayerFootStep : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _parentAnimator;
    [SerializeField] private PlayerController _playerController;

    [Header("Auto Tilt Settings")]
    [SerializeField] private float _maxTiltAngle = 1.5f;
    [SerializeField] private float _bobbingAmount = 0.08f;

    [Space]
    [Tooltip("걷기 애니메이션은 발을 적게 들므로 민감도를 크게 높여야 합니다")]
    [SerializeField] private float _walkHeightMultiplier = 15f;
    [Tooltip("뛰기 애니메이션은 발을 높이 들므로 기존 민감도를 유지합니다")]
    [SerializeField] private float _runHeightMultiplier = 5f;

    [Space]
    [SerializeField] private float _smoothness = 12f;

    private Vector3 _initialLocalPosition;
    private Quaternion _initialLocalRotation;

    private Transform _leftFoot;
    private Transform _rightFoot;

    private void Awake()
    {
        // 처음에 설정된 기본 위치와 회전 방향을 기억해둡니다
        _initialLocalPosition = transform.localPosition;
        _initialLocalRotation = transform.localRotation;

        // 부모 객체로부터 필요한 컴포넌트들을 가져옵니다
        if (_parentAnimator == null) _parentAnimator = GetComponentInParent<Animator>();
        if (_playerController == null) _playerController = GetComponentInParent<PlayerController>();

        // 애니메이터에서 사람 형태의 양쪽 발 뼈대 위치를 찾아옵니다
        _leftFoot = _parentAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _rightFoot = _parentAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
    }

    private void LateUpdate()
    {
        // 목표로 하는 기울기와 상하 흔들림 값을 영으로 맞춥니다
        float targetTilt = 0f;
        float currentBobbing = 0f;

        // 캐릭터가 걷거나 뛰는 상태일 때만 작동하도록 확인합니다
        if (_playerController.StateMachine.CurrentState == _playerController.WalkState)
        {
            if (_leftFoot != null && _rightFoot != null)
            {
                // 뛰는 상태인지 확인하여 알맞은 민감도 값을 고릅니다
                float currentMultiplier = _playerController.IsRun ? _runHeightMultiplier : _walkHeightMultiplier;

                // 캐릭터의 중심을 기준으로 양쪽 발의 수직 높이를 구합니다
                float leftY = transform.InverseTransformPoint(_leftFoot.position).y;
                float rightY = transform.InverseTransformPoint(_rightFoot.position).y;

                // 두 발의 높이 차이에 민감도를 곱해서 기울기를 정합니다
                float heightDifference = (leftY - rightY) * currentMultiplier;
                targetTilt = Mathf.Clamp(heightDifference, -1f, 1f);

                // 기울기 값의 절대값을 활용해 몸을 아래로 내리는 무게감을 만듭니다
                currentBobbing = Mathf.Abs(targetTilt) * -_bobbingAmount;
            }
        }

        // 계산된 기울기를 바탕으로 최종 회전 목표를 만듭니다
        Quaternion targetRotation = _initialLocalRotation * Quaternion.Euler(0, 0, -targetTilt * _maxTiltAngle);
        // 계산된 상하 흔들림을 바탕으로 최종 위치 목표를 만듭니다
        Vector3 targetPosition = _initialLocalPosition + new Vector3(0, currentBobbing, 0);

        // 현재 상태에서 목표 상태로 아주 부드럽게 이동하고 회전하도록 섞어줍니다
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * _smoothness);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * _smoothness);
    }
}