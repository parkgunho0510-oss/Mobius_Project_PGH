using UnityEngine;

public class FireBreath : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _destroyTime = 5f;

    [Header("Burn Settings")]
    [SerializeField] private float _burnDuration = 5f;

    private Rigidbody _rb;
    private bool _hasAppliedBurn = false;
    private float _damage;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;

        Destroy(gameObject, _destroyTime);
    }

    public void Breath(Vector3 direction, float damage)
    {
        _damage = damage;
        _rb.linearVelocity = direction.normalized * _speed;

        transform.rotation = Quaternion.LookRotation(direction);
    }

  
    private void OnTriggerEnter(Collider other)
    {
        if (!_hasAppliedBurn && other.CompareTag("Player"))
        {
            PlayerStatus status = other.GetComponent<PlayerStatus>();

            if (status != null)
            {
                // 몬스터 데미지 기반 화상 데미지 계산
                float burnTickDamage = _damage * 0.2f;

                status.ApplyBurn(_burnDuration, burnTickDamage);

                _hasAppliedBurn = true;
            }
        }

        //if (!other.isTrigger && !other.CompareTag("Enemy"))
        //if (other.CompareTag("Wall"))
        //{
        //    Destroy(gameObject);
        //}
    }

    /*private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Time.time < _nextTickTime)
            return;

        _nextTickTime = Time.time + _tickInterval;

        var status = other.GetComponent<PlayerStatus>();

        Vector3 hitNormal = (other.transform.position - transform.position).normalized;

        if (status != null)
        {
            status.TakeDamage(_damage, gameObject, false, default, hitNormal, null, false);
        }
    }*/
}