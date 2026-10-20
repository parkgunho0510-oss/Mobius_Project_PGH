using System.Collections;
using UnityEngine;

public class BossWeakPoint : MonoBehaviour, IDamageable
{
    private BossBase _boss;
    private SliceableObject _sliceable;
    private int _hitCount = 0;
    private const int MAX_HIT = 4;

    [Header("Hit Feedback")]
    [SerializeField] private Renderer _renderer;
    [SerializeField, ColorUsage(true, true)] private Color _flashColor = Color.yellow * 2f; // HDR 지원 (빛나도록 강도 증폭)
    [SerializeField] private float _flashDuration = 0.1f; // 번쩍이는 시간

    private MaterialPropertyBlock _propBlock;
    private Coroutine _flashCoroutine;

    // URP/HDRP 기본 Lit 쉐이더의 Emission 속성 ID
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        _boss = GetComponentInParent<BossBase>();
        _sliceable = GetComponent<SliceableObject>();

        if (_renderer == null) _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    public void TakeDamage(float damage, GameObject attacker = null, bool isParryable = false, Vector3 hitPoint = default, Vector3 hitNormal = default, ShakeData shakeData = default)
    {
        // 보스가 존재하고, 오직 "그로기" 상태일 때만 타격 허용
        if (_boss != null && _boss.IsGroggy)
        {
            _hitCount++;

            // 타격 시전 시 피드백 코루틴 실행
            TriggerFlashEffect();

            if (_hitCount >= MAX_HIT)
            {
                BreakPart(hitPoint, hitNormal);
            }
        }
    }

    private void TriggerFlashEffect()
    {
        // 이미 번쩍이고 있다면 중단하고 처음부터 다시 시작 (연타 대비)
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }
        _flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        // 현재 상태를 가져와서 노란색 Emission(발광) 적용
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(EmissionColorID, _flashColor);
        _renderer.SetPropertyBlock(_propBlock);

        // 설정한 시간만큼 대기
        yield return new WaitForSeconds(_flashDuration);

        // Emission을 검은색(Color.black)으로 꺼서 원래 상태로 복귀
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(EmissionColorID, Color.black);
        _renderer.SetPropertyBlock(_propBlock);
    }

    private void BreakPart(Vector3 point, Vector3 normal)
    {
        // 절단 실행 후, 성공적으로 절단된 경우에만 보스 방어력 감소
        if (_sliceable != null)
        {
            bool isSliced = _sliceable.Slice(point, normal);
            if (isSliced)
            {
                _boss.ReduceDefenseFromPartBreak();
            }
        }
    }
}