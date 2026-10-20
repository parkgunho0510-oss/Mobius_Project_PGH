using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [SerializeField] GameObject _root;
    void Awake()
    {
        UIEvents.OnInteractedEvent += HandleInteracted;
    }
    void OnDestroy()
    {
        UIEvents.OnInteractedEvent -= HandleInteracted;
    }
    void Start()
    {
        if (_root == null) _root = transform.GetChild(0).gameObject;
        _root.SetActive(false);
    }

    void HandleInteracted()
    {
        _root.SetActive(!_root.activeSelf);
    }
}
