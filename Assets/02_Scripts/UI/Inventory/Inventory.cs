using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Inventory : MonoBehaviour
{

    public int Capacity { get; private set; }


    [SerializeField, Range(8, 64)] private int _initalCapacity = 32;

    
    [SerializeField, Range(8, 64)]  private int _maxCapacity = 64;

    [SerializeField] private InventoryUI _inventoryUI; 


    [SerializeField] private InventoryItem[] _items;

    [SerializeField] private CurrentUsingItems _currentUsingItems;

   
    private readonly HashSet<int> _indexSetForUpdate = new HashSet<int>();





    private class TypeComparer : IComparer<InventoryItem>
    {
        public int Compare(InventoryItem a, InventoryItem b)
        {
            int result = a.Data.Type.CompareTo(b.Data.Type);

            if (result == 0)
            {
                result = string.Compare(a.Data.ID, b.Data.ID);
            }

            return result;
        }
    }

    private static readonly TypeComparer _typeComparer = new TypeComparer();
    private class GradeComparer : IComparer<InventoryItem>
    {
        public int Compare(InventoryItem a, InventoryItem b)
        {
            EquipmentData equipmentA = a.Data as EquipmentData;
            EquipmentData equipmentB = b.Data as EquipmentData;

            int gradeA = equipmentA != null ? (int)equipmentA.GradeType : -1;
            int gradeB = equipmentB != null ? (int)equipmentB.GradeType : -1;

            int result = gradeB.CompareTo(gradeA);

            if (result == 0)
            {
                result = string.Compare(a.Data.ID, b.Data.ID);
            }

            return result;
        }
    }

    private static readonly GradeComparer _gradeComparer = new GradeComparer();

    private class NameComparer : IComparer<InventoryItem>
    {
        public int Compare(InventoryItem a, InventoryItem b)
        {
            return string.Compare(a.Data.ID, b.Data.ID);
        }
    }

    private static readonly NameComparer _nameComparer = new NameComparer();

 

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_initalCapacity > _maxCapacity)
            _initalCapacity = _maxCapacity;
    }
#endif
    private void Awake()
    {
        if (_currentUsingItems ==null)
        {
            _currentUsingItems = GetComponent<CurrentUsingItems>();
        }
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
    private void Start()
    {
        _items = InventoryManager.Instance.Items;
        Capacity = InventoryManager.Instance.Capacity;
    }

   
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < Capacity;
    }

    
    private int FindEmptySlotIndex(int startIndex = 0)
    {
        for (int i = startIndex; i < Capacity; i++)
            if (_items[i] == null)
                return i;
        return -1;
    }

  
    private int FindCountableItemSlotIndex(ConsumableData target, int startIndex = 0)
    {
        for (int i = startIndex; i < Capacity; i++)
        {
            var current = _items[i];
            if (current == null)
                continue;

            ConsumableItem consumable = current as ConsumableItem;
            if (consumable == null)
                continue;

            
            if (consumable.Data == target)
            {
               
                if (consumable.Amount < target.MaxStack)
                    return i;
            }
        }

        return -1;
    }


   
    private void UpdateSlot(int index)
    {
        if (!IsValidIndex(index)) return;

        InventoryItem item = _items[index];

        
        if (item != null)
        {
            
            _inventoryUI.SetItemIcon(index, item.Data.Icon);

            
            if (item is ConsumableItem ci)
            {
                
                if (ci.IsEmpty)
                {
                    _items[index] = null;
                    RemoveIcon();
                    return;
                }
               
                else
                {
                    _inventoryUI.SetItemAmountText(index, ci.Amount);
                }
            }
           
            else
            {
                _inventoryUI.HideItemAmountText(index);
            }

            
            
        }
        
        else
        {
            RemoveIcon();
        }

        
        void RemoveIcon()
        {
            _inventoryUI.RemoveItem(index);
            _inventoryUI.HideItemAmountText(index); 
        }
    }

    
    private void UpdateSlot(params int[] indices)
    {
        foreach (var i in indices)
        {
            UpdateSlot(i);
        }
    }

    
    private void UpdateAllSlot()
    {
        for (int i = 0; i < Capacity; i++)
        {
            UpdateSlot(i);
        }
    }

  
    public bool HasItem(int index)
    {
        return IsValidIndex(index) && _items[index] != null;
    }

   
    public bool IsCountableItem(int index)
    {
        return HasItem(index) && _items[index] is ConsumableItem;
    }


    public int GetCurrentAmount(int index)
    {
        if (!IsValidIndex(index)) return -1;
        if (_items[index] == null) return 0;

        ConsumableItem ci = _items[index] as ConsumableItem;
        if (ci == null)
            return 1;

        return ci.Amount;
    }

    public void SetCurrentAmount(int index, int amount)
    {
        if (!IsValidIndex(index)) return;
        if (_items[index] == null) return;

        ConsumableItem ci = _items[index] as ConsumableItem;
        if (ci == null) return;

        ci.SetAmount(amount);
    }

   
    public ItemData GetItemData(int index)
    {
        if (!IsValidIndex(index)) return null;
        if (_items[index] == null) return null;

        return _items[index].Data;
    }

   
    public string GetItemName(int index)
    {
        if (!IsValidIndex(index)) return "";
        if (_items[index] == null) return "";

        return _items[index].Data.ItemName;
    }

 
    public void ConnectUI(InventoryUI inventoryUI)
    {
        _inventoryUI = inventoryUI;
        _inventoryUI.SetInventoryReference(this);
    }

  
    public int Add(ItemData itemData, int amount = 1)
    {
        int index;

        if (itemData is ConsumableData ciData)
        {
            bool findNextCountable = true;
            index = -1;

            while (amount > 0)
            {
                
                if (findNextCountable)
                {
                    index = FindCountableItemSlotIndex(ciData, index + 1);

                    
                    if (index == -1)
                    {
                        findNextCountable = false;
                    }
                    
                    else
                    {
                        ConsumableItem ci = _items[index] as ConsumableItem;
                        amount = ci.AddAmountAndGetExcess(amount);

                        UpdateSlot(index);
                        _inventoryUI.MarkSlotAsNewItem(index);
                    }
                }
               
                else
                {
                    index = FindEmptySlotIndex(index + 1);

                   
                    if (index == -1)
                    {
                        break;
                    }
                    
                    else
                    {
                       
                        ConsumableItem ci = ciData.CreateItem() as ConsumableItem;
                        ci.SetAmount(amount);

                        
                        _items[index] = ci;

                        
                        amount = (amount > ciData.MaxStack) ? (amount - ciData.MaxStack) : 0;

                        UpdateSlot(index);
                        _inventoryUI.MarkSlotAsNewItem(index);
                    }
                }
            }
        }
        
        else
        {
           
            if (amount == 1)
            {
                index = FindEmptySlotIndex();
                if (index != -1)
                {
                   
                    _items[index] = itemData.CreateItem();
                    amount = 0;

                    UpdateSlot(index);
                    _inventoryUI.MarkSlotAsNewItem(index);
                }
            }

            
            index = -1;
            for (; amount > 0; amount--)
            {
                
                index = FindEmptySlotIndex(index + 1);

                
                if (index == -1)
                {
                    break;
                }

                
                _items[index] = itemData.CreateItem();

                UpdateSlot(index);
                _inventoryUI.MarkSlotAsNewItem(index);

            }
        }

        return amount;
    }


    public int AddByID(string itemId, int amount = 1)
    {
        ItemData itemData = DataManager.Instance.GetItem(itemId);

        if (itemData == null)
        {
            Debug.LogError($"아이템 없음 : {itemId}");
            return amount;
        }

        return Add(itemData, amount);
    }

    
    public void Remove(int index)
    {
        if (!IsValidIndex(index)) return;

        _items[index] = null;
        _inventoryUI.RemoveItem(index);
    }

    public void Remove(int index, int removeAmount)
    {
        if (!IsValidIndex(index)) return;

        if (removeAmount <= 0) return;

        if (_items[index] == null) return;

        int currentAmount = GetCurrentAmount(index);

        if (currentAmount < removeAmount) return;

        int resultAmount = currentAmount - removeAmount;

        if(resultAmount > 0)
        {
            SetCurrentAmount(index, resultAmount);
        }
        else
        {
            Remove(index);
        }
        UpdateSlot(index);
    }

   
    public void SetItemData(int index, ItemData itemData, int amount = 1)
    {
        if (!IsValidIndex(index)) return;

       
        if (itemData == null)
        {
            _items[index] = null;
            UpdateSlot(index);
            return;
        }

      
        if (itemData is ConsumableData ciData)
        {
            ConsumableItem ci = ciData.CreateItem() as ConsumableItem;
            ci.SetAmount(amount);
            _items[index] = ci;
        }
        
        else
        {
            _items[index] = itemData.CreateItem();
        }

        UpdateSlot(index);
    }

    
    public void Swap(int indexA, int indexB)
    {
        if (!IsValidIndex(indexA)) return;
        if (!IsValidIndex(indexB)) return;

        InventoryItem itemA = _items[indexA];
        InventoryItem itemB = _items[indexB];

       
        if (itemA != null && itemB != null &&
            itemA.Data == itemB.Data &&
            itemA is ConsumableItem ciA && itemB is ConsumableItem ciB)
        {
            int maxAmount = ciB.MaxAmount;
            int sum = ciA.Amount + ciB.Amount;

            if (sum <= maxAmount)
            {
                ciA.SetAmount(0);
                ciB.SetAmount(sum);
            }
            else
            {
                ciA.SetAmount(sum - maxAmount);
                ciB.SetAmount(maxAmount);
            }
        }
        
        else
        {
            _items[indexA] = itemB;
            _items[indexB] = itemA;
        }

        
        UpdateSlot(indexA, indexB);
    }

   
    public void SeparateAmount(int indexA, int indexB, int amount)
    {
        

        if (!IsValidIndex(indexA)) return;
        if (!IsValidIndex(indexB)) return;

        InventoryItem _itemA = _items[indexA];
        InventoryItem _itemB = _items[indexB];

        ConsumableItem _ciA = _itemA as ConsumableItem;

        
        if (_ciA != null && _itemB == null)
        {
            _items[indexB] = _ciA.SeparateAndClone(amount);

            UpdateSlot(indexA, indexB);
        }
    }

   
        
    public int FindIndex(string id)
    {
        for(int i = 0; i < _items.Length; i++)
        {
            InventoryItem item = _items[i];
            if(item == null) continue;
            if(item.Data.ID == id) 
                return i;
        }

        return -1;
    }

    public InventorySaveData CreateSaveData()
    {
        InventorySaveData data = new();

        for (int i = 0; i < _items.Length; i++)
        {
            ItemSaveData itemSaveData = new();
            if (_items[i] == null)
            {
                itemSaveData.ItemID = "";
                itemSaveData.Amount = 0;
                itemSaveData.SlotIndex = i;
            }
            else
            {
                itemSaveData.ItemID = _items[i].Data.ID;
                itemSaveData.Amount = GetCurrentAmount(i);
                itemSaveData.SlotIndex = i;
            }
                

            data.ItemSaveDatas.Add(itemSaveData);
        }

        return data;
    }
    
    public void LoadSaveData(InventorySaveData data)
    {
        for(int i = 0; i < data.ItemSaveDatas.Count; i++)
        {
            string id = data.ItemSaveDatas[i].ItemID;
            int index = data.ItemSaveDatas[i].SlotIndex;
            int amount = data.ItemSaveDatas[i].Amount;

            if(string.IsNullOrEmpty(id))
            {
                Remove(index);
            }

            ItemData itemData = DataManager.Instance.GetItem(id);

            if(itemData is ConsumableData consumable)
            {
                ConsumableItem item = consumable.CreateItem() as ConsumableItem;
                item.SetAmount(amount);
                _items[index] = item;
                UpdateSlot(index);
            }
            else if(itemData is EquipmentData equipment)
            {
                EquipmentItem item = equipment.CreateItem() as EquipmentItem;
                _items[index] = item;
                UpdateSlot(index);
            }
        }
    }

    public void UpdateAccessibleStatesAll()
    {
        _inventoryUI.SetAccessibleSlotRange(Capacity);
    }

    
    public void TrimAll()
    {
       

        _indexSetForUpdate.Clear();

        int i = -1;
        while (_items[++i] != null) ;
        int j = i;

        while (true)
        {
            while (++j < Capacity && _items[j] == null) ;

            if (j == Capacity)
                break;

            _indexSetForUpdate.Add(i);
            _indexSetForUpdate.Add(j);

            _items[i] = _items[j];
            _items[j] = null;
            i++;
        }

        foreach (var index in _indexSetForUpdate)
        {
            UpdateSlot(index);
        }
        
    }

   
    public void SortAllByType()
    { int itemCount = TrimForSort();

    Array.Sort(_items, 0, itemCount, _typeComparer);

    UpdateAllSlot();
    
    }
    public void SortAllByGrade()
    {
        int itemCount = TrimForSort();

        Array.Sort(_items, 0, itemCount, _gradeComparer);

        UpdateAllSlot();
        
    }

    public void SortAllByName()
    {
        int itemCount = TrimForSort();

        Array.Sort(_items, 0, itemCount, _nameComparer);

        UpdateAllSlot();
        
    }
    private int TrimForSort()
    {
        int i = -1;
        while (_items[++i] != null) ;

        int j = i;

        while (true)
        {
            while (++j < Capacity && _items[j] == null) ;

            if (j == Capacity)
                break;

            _items[i] = _items[j];
            _items[j] = null;
            i++;
        }

        return i;
    }



    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Init") return;
        if (scene.name == "StartScene_LCS") return;

        if (_inventoryUI == null) _inventoryUI = FindFirstObjectByType<InventoryUI>();
        _inventoryUI.SetInventoryReference(this);
        UpdateAccessibleStatesAll();

        for (int i = 0; i < Capacity; i++)
        {
            UpdateSlot(i);
        }
    }
   

}
