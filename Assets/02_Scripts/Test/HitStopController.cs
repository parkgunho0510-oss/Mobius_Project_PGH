using System.Collections;
using UnityEngine;

public class HitStopController : MonoBehaviour
{
    private Animator _animator;
    private Coroutine _hitStopCoroutine;
    private bool _isStopped = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // 타격 성공 시 호출
    public void PlayHitStop(float duration = 0.1f)
    {
        // 이미 역경직 중이라면 무시 (다중 타격 시 코루틴 겹침 방지)
        if (_isStopped) return; 

        _hitStopCoroutine = StartCoroutine(Co_HitStop(duration));
    }

    private IEnumerator Co_HitStop(float duration)
    {
        _isStopped = true;
        _animator.speed = 0f; // 애니메이션 정지

        yield return new WaitForSecondsRealtime(duration);

        RestoreAnimatorSpeed();
    }

    public void RestoreAnimatorSpeed()
    {
        if (_hitStopCoroutine != null)
        {
            StopCoroutine(_hitStopCoroutine);
            _hitStopCoroutine = null;
        }
        
        if (_animator != null)
        {
            _animator.speed = 1f; // 애니메이션 속도 원상복구
        }
        _isStopped = false;
    }
}