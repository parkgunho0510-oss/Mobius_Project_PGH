using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] PlayerSenses _playerSenses;
    [SerializeField] Inventory _inventory;
    void Awake()
    {
        if(_playerSenses == null) _playerSenses = GetComponent<PlayerSenses>();
        if (_inventory == null) _inventory = FindFirstObjectByType<Inventory>();
    }
    void ItemPickUp(WorldItem item)
    {
        if (_inventory.Add(item.Data, 1) == 0)
        {
            QuestEvents.RaiseCollectItem(item.Data, 1);
            Destroy(item.gameObject);
        }
    }
    public void TryPickUp()
    {
        WorldItem item = _playerSenses.GetNearestWorldItem();
        if (item == null) return;
        ItemPickUp(item);
    }
    public bool TryInteract()
    {
        IInteractable _interactable = _playerSenses.GetNearestInteractable();
        if(_interactable == null)
        {
            Debug.Log("_interactable is null");
            return false;
        }
        return _interactable.Interact();
    }
}
