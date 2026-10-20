using UnityEngine;

public class BossRoomEnterTrigger : MonoBehaviour
{
    [Header("Boss HP UI")]
    [SerializeField] private GameObject _bossHpUI;

    [Header("Door")]
    [SerializeField] private GameObject _bossDoor;

    [Header("BGM")]
    [SerializeField] private string _bossBgmName;

    private bool _isTriggered;

    private void OnEnable()
    {
        UIEvents.OnPlayerDead += BossRoomResetTrigger;
    }

    private void OnDisable()
    {
        UIEvents.OnPlayerDead -= BossRoomResetTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isTriggered) return;
        if (!other.CompareTag("Player")) return;

        _isTriggered = true;

        if (_bossHpUI != null)
            _bossHpUI.SetActive(true);

        if (_bossDoor != null)
            _bossDoor.SetActive(true);

        
         SoundEvents.RaiseBGMChangeEvent(_bossBgmName);
              
        
    }

    private void BossRoomResetTrigger()
    {
        _isTriggered = false;

        if (_bossHpUI != null)
            _bossHpUI.SetActive(false);

        if (_bossDoor != null)
            _bossDoor.SetActive(false);
    }
}