using UnityEngine;

public class DropItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _dropItemUI;

    private void Start()
    {
        if (_dropItemUI != null)
            _dropItemUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _dropItemUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _dropItemUI.SetActive(false);
        }
    }
}