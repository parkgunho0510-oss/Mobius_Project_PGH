using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/*
    [기능 - 에디터 전용]
    - 게임 시작 시 동적으로 생성될 슬롯 미리보기(개수, 크기 미리보기 가능)
    [��� - ������ ����]
    - ���� ���� �� �������� ������ ���� �̸�����(����, ũ�� �̸����� ����)
    

    [기능 - 유저 인터페이스]
    - 슬롯에 마우스 올리기
      - 사용 가능 슬롯 : 하이라이트 이미지 표시
      - 아이템 존재 슬롯 : 아이템 정보 툴팁 표시

    - 드래그 앤 드롭
      - 아이템 존재 슬롯 -> 아이템 존재 슬롯 : 두 아이템 위치 교환
      - 아이템 존재 슬롯 -> 아이템 미존재 슬롯 : 아이템 위치 변경
        - Shift 또는 Ctrl 누른 상태일 경우 : 셀 수 있는 아이템 수량 나누기
      - 아이템 존재 슬롯 -> UI 바깥 : 아이템 버리기

    - 슬롯 우클릭
      - 사용 가능한 아이템일 경우 : 아이템 사용

    - 기능 버튼(좌측 상단)
      - Trim : 앞에서부터 빈 칸 없이 아이템 채우기
      - Sort : 정해진 가중치대로 아이템 정렬

*/




public class InventoryUI : MonoBehaviour
{
    /***********************************************************************
    *                               Option Fields
    ***********************************************************************/
    #region .
    [Header("Options")]
    [Range(0, 10)]
    [SerializeField] private int _horizontalSlotCount = 8;  // 슬롯 가로 개수
    [Range(0, 10)]
    [SerializeField] private int _verticalSlotCount = 8;      // 슬롯 세로 개수
    [SerializeField] private float _slotMargin = 8f;          // 한 슬롯의 상하좌우 여백
    [SerializeField] private float _contentAreaPadding = 20f; // 인벤토리 영역의 내부 여백
    [Range(32, 128)]
    [SerializeField] private float _slotSize = 64f;      // 각 슬롯의 크기

    [Space]
    [SerializeField] private bool _showTooltip = true;
    [SerializeField] private bool _showHighlight = true;
    [SerializeField] private bool _showRemovingPopup = true;

    [Header("Connected Objects")]
    [SerializeField] private RectTransform _contentAreaRT; // 슬롯들이 위치할 영역
    [SerializeField] private GameObject _slotUiPrefab;     // 슬롯의 원본 프리팹
    [SerializeField] private ItemTooltipUI _itemTooltip;   // 아이템 정보를 보여줄 툴팁 UI
    [SerializeField] private ItemTooltipUI _equippedItemTooltip;
    [SerializeField] private InventoryPopupUI _popup;      // 팝업 UI 관리 객체
    [SerializeField] private GameObject _root;


    [Header("Buttons")]
    [SerializeField] private Button _trimButton;
    [SerializeField] private Button _sortTypeButton;
    [SerializeField] private Button _sortGradeButton;
    [SerializeField] private Button _sortNameButton;

    [Header("Popup Buttons")]
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _closeButton;

   

    [Space(16)]
    

    [SerializeField] private GameObject _selectItemPopup;
    private ItemSlotUI _selectedSlot;


    [SerializeField] private ItemSlotUI _usingPotionItemSlot1;
    [SerializeField] private ItemSlotUI _usingPotionItemSlot2;
    [SerializeField] private ItemSlotUI _usingWeaponItemSlot1;
    [SerializeField] private ItemSlotUI _usingWeaponItemSlot2;
    [SerializeField] private ItemSlotUI _usingArmorItemSlot1;
    [SerializeField] private ItemSlotUI _usingArmorItemSlot2;


    [SerializeField] private float _horizontalOffset = 10f;

    [SerializeField] private CurrentUsingItems _currentUsingItems;

    [SerializeField] private PotionHandler _healPotionHandler;

    [SerializeField] private PlayerStatus _playerStatus;


    [SerializeField] private WeaponSwitcher _weaponSwitcher;

    [SerializeField] private GameObject _currentTooltip;
    [SerializeField] private GameObject _shopPanel;


    private ItemData _usingPotionItemData1;
    private ItemData _usingPotionItemData2;
    private ItemData _usingWeaponItemData1;
    private ItemData _usingWeaponItemData2;
    private ItemData _usingArmorItemData1;
    private ItemData _usingArmorItemData2;

    private int _usingPotionAmount1;
    private int _usingPotionAmount2;

    private int _lastSwappedWeaponSlot = 2;
    private int _lastSwappedPotionSlot = 1;
    private int _lastSwappedArmorSlot = 1;

    [Header("Sell Item")]
    [SerializeField] private GameObject _shopRoot;
    [SerializeField] private Wallet _wallet;
    [SerializeField] private ShopService _shopService;
    [SerializeField] private ShopUI _shopUI;
    [SerializeField] private GameObject _sellPopup;

    [SerializeField] private GameObject _popupRoot;


    private float _lastClickTime;
    private int _lastClickSlotIndex = -1;
    private const float DOUBLE_CLICK_TIME = 0.3f;



    private ItemData _sellItemData;
    private int _sellAmount;


    //���콺 ��� �и���
    [SerializeField] private InventoryMouseController _mouseController;



    #endregion
    /***********************************************************************
    *                               Private Fields
    ***********************************************************************/
    #region .

    /// <summary> 연결된 인벤토리 </summary>
    private Inventory _inventory;

    private List<ItemSlotUI> _slotUIList = new List<ItemSlotUI>();
    private GraphicRaycaster _gr;

    private ItemSlotUI _pointerOverSlot; // 현재 포인터가 위치한 곳의 슬롯
    private ItemSlotUI _beginDragSlot; // 현재 드래그를 시작한 슬롯
    private Transform _beginDragIconTransform; // 해당 슬롯의 아이콘 트랜스폼

    private Vector3 _beginDragIconPoint;   // 드래그 시작 시 슬롯의 위치
    private Vector3 _beginDragCursorPoint; // 드래그 시작 시 커서의 위치
    private int _beginDragSlotSiblingIndex;

    
  

    #endregion
    /***********************************************************************
    *                               Unity Events
    ***********************************************************************/
    #region .
    private void Awake()
    {
        _confirmButton.onClick.AddListener(ConfirmSelectItem);
        _closeButton.onClick.AddListener(ClosePopup);

        _root.SetActive(false);
        UIEvents.OnInventoryOpenedEvent += HandleInventoryOpened;
        UIEvents.OnInventoryClosedEvent += HandleInventoryClosed;

        if (_currentUsingItems == null)
        {
            _currentUsingItems = FindFirstObjectByType<CurrentUsingItems>();
        }

        InitSlots();
    }
    void OnDestroy()
    {
        _confirmButton.onClick.RemoveListener(ConfirmSelectItem);
        _closeButton.onClick.RemoveListener(ClosePopup);

        UIEvents.OnInventoryOpenedEvent -= HandleInventoryOpened;
        UIEvents.OnInventoryClosedEvent -= HandleInventoryClosed;
        if (_currentUsingItems != null)
            _currentUsingItems.OnItemsInitialized -= InitializeUsingItemsFromEvent;
    }
    private void Update()
    {
        if (Mouse.current == null) return;

        
        _mouseController.UpdateMousePosition(Mouse.current.position.ReadValue());

        OnPointerEnterAndExit();
        if (_showTooltip) ShowOrHideItemTooltip();
        OnPointerDown();
        OnPointerDrag();
        OnPointerUp();
    }

    #endregion
    /***********************************************************************
    *                               Init Methods
    ***********************************************************************/
    #region .
    private void Init()
    {
        TryGetComponent(out _gr);
        if (_gr == null)
            _gr = gameObject.AddComponent<GraphicRaycaster>();

        //// Graphic Raycaster
        //_ped = new PointerEventData(EventSystem.current);
        //_rrList = new List<RaycastResult>(10);
        _mouseController.Init(_gr);

        // Item Tooltip UI
        if (_itemTooltip == null)
        {
            _itemTooltip = GetComponentInChildren<ItemTooltipUI>();
        }

        if (_root == null) _root = transform.GetChild(0).gameObject;

    }

    /// <summary> 지정된 개수만큼 슬롯 영역 내에 슬롯들 동적 생성 </summary>
    private void InitSlots()
    {
        if (_horizontalSlotCount <= 0 || _verticalSlotCount <= 0)
        {
            Debug.LogError($"슬롯 개수 설정이 잘못됨. horizontal={_horizontalSlotCount}, vertical={_verticalSlotCount}");
            return;
        }

        if (_slotUiPrefab == null)
        {
            Debug.LogError("_slotUiPrefab이 null");
            return;
        }

        if (_contentAreaRT == null)
        {
            Debug.LogError("_contentAreaRT가 null");
            return;
        }
        // 슬롯 프리팹 설정
        _slotUiPrefab.TryGetComponent(out RectTransform slotRect);
        slotRect.sizeDelta = new Vector2(_slotSize, _slotSize);

        _slotUiPrefab.TryGetComponent(out ItemSlotUI itemSlot);
        if (itemSlot == null)
            _slotUiPrefab.AddComponent<ItemSlotUI>();

        _slotUiPrefab.SetActive(false);

        Vector2 beginPos = new Vector2(_contentAreaPadding + _horizontalOffset, -_contentAreaPadding);
        Vector2 curPos = beginPos;

        _slotUIList = new List<ItemSlotUI>(_verticalSlotCount * _horizontalSlotCount);

        // 슬롯들 동적 생성
        for (int j = 0; j < _verticalSlotCount; j++)
        {
            for (int i = 0; i < _horizontalSlotCount; i++)
            {
                int slotIndex = (_horizontalSlotCount * j) + i;

                var slotRT = CloneSlot();
                slotRT.pivot = new Vector2(0f, 1f); // Left Top
                slotRT.anchoredPosition = curPos;
                slotRT.gameObject.SetActive(true);
                slotRT.gameObject.name = $"Item Slot [{slotIndex}]";

                var slotUI = slotRT.GetComponent<ItemSlotUI>();
                slotUI.SetSlotIndex(slotIndex);
                _slotUIList.Add(slotUI);

                // Next X
                curPos.x += (_slotMargin + _slotSize);
            }

            // Next Line
            curPos.x = beginPos.x;
            curPos.y -= (_slotMargin + _slotSize);
        }

        // 슬롯 프리팹 - 프리팹이 아닌 경우 파괴
        if (_slotUiPrefab.scene.rootCount != 0)
            Destroy(_slotUiPrefab);

        // -- Local Method --
        RectTransform CloneSlot()
        {
            GameObject slotGo = Instantiate(_slotUiPrefab);
            RectTransform rt = slotGo.GetComponent<RectTransform>();
            rt.SetParent(_contentAreaRT);

            return rt;
        }

        float contentWidth =
       (_horizontalSlotCount * _slotSize) +
       ((_horizontalSlotCount - 1) * _slotMargin) +
       (_contentAreaPadding * 2);

        float contentHeight =
            (_verticalSlotCount * _slotSize) +
            ((_verticalSlotCount - 1) * _slotMargin) +
            (_contentAreaPadding * 2);

        _contentAreaRT.sizeDelta = new Vector2(contentWidth, contentHeight);
    }
    private void InitButtonEvents()
    {
        _trimButton.onClick.AddListener(() => _inventory.TrimAll());
        _sortTypeButton.onClick.AddListener(() => _inventory.SortAllByType());
        _sortGradeButton.onClick.AddListener(() => _inventory.SortAllByGrade());

        _sortNameButton.onClick.AddListener(() => _inventory.SortAllByName());
    }

    
    /// <summary> 슬롯에 포인터가 올라가는 경우, 슬롯에서 포인터가 빠져나가는 경우 </summary>
    /// <summary> ���Կ� �����Ͱ� �ö󰡴� ���, ���Կ��� �����Ͱ� ���������� ��� </summary>

    #endregion
    /***********************************************************************
    *                               Mouse Event Methods
    ***********************************************************************/
    #region .
   
    /// <summary> ���Կ� �����Ͱ� �ö󰡴� ���, ���Կ��� �����Ͱ� ���������� ��� </summary>
    private void OnPointerEnterAndExit()
    {
        // 이전 프레임의 슬롯
        var prevSlot = _pointerOverSlot;

        // 현재 프레임의 슬롯
        var curSlot = _pointerOverSlot = _mouseController.RaycastAndGetFirstComponent<ItemSlotUI>();

        if (prevSlot == null)
        {
            // Enter
            if (curSlot != null)
            {
                OnCurrentEnter();
            }
        }
        else
        {
            // Exit
            if (curSlot == null)
            {
                OnPrevExit();
            }

            // Change
            else if (prevSlot != curSlot)
            {
                OnPrevExit();
                OnCurrentEnter();
            }
        }

        // ===================== Local Methods ===============================
        void OnCurrentEnter()
        {
            if (_showHighlight)
                curSlot.Highlight(true);
        }
        void OnPrevExit()
        {
            prevSlot.Highlight(false);
        }
    }
    /// <summary> 아이템 정보 툴팁 보여주거나 감추기 </summary>
    private void ShowOrHideItemTooltip()
    {
        // 마우스가 유효한 아이템 아이콘 위에 올라와 있다면 툴팁 보여주기
        bool isValid =
            _pointerOverSlot != null && _pointerOverSlot.HasItem && _pointerOverSlot.IsAccessible
            && (_pointerOverSlot != _beginDragSlot); // 드래그 시작한 슬롯이면 보여주지 않기

        if (isValid)
        {
            UpdateTooltipUI(_pointerOverSlot);
            //_itemTooltip.Show();
            
            //_equippedItemTooltip.Show();
        }
        else
        {
            _itemTooltip.Hide();
            _equippedItemTooltip.Hide();
        }

    }
    /// <summary> ���Կ� Ŭ���ϴ� ��� </summary>
    private void OnPointerDown()
    {
        // Left Click : Begin Drag
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            _beginDragSlot = _mouseController.RaycastAndGetFirstComponent<ItemSlotUI>();

            if (_beginDragSlot != null && _beginDragSlot.HasItem && _beginDragSlot.IsAccessible)
            {
                // ������ �������� ���� ����Ŭ�� �Ǹ� �˾�
                if (_shopRoot != null && _shopRoot.activeSelf)
                {
                    int index = _beginDragSlot.Index;

                    if (_lastClickSlotIndex == index &&
                        Time.time - _lastClickTime <= DOUBLE_CLICK_TIME)
                    {
                        ItemData itemData = _inventory.GetItemData(index);
                        int amount = _inventory.GetCurrentAmount(index);

                        if (itemData != null)
                        {
                            _popup.OpenSellPopup(index, itemData, amount);
                        }

                        _lastClickSlotIndex = -1;
                        _lastClickTime = 0f;
                        _beginDragSlot = null;
                        return;
                    }

                    _lastClickSlotIndex = index;
                    _lastClickTime = Time.time;
                }

                // ���� �巡�� ���� ����
                _beginDragIconTransform = _beginDragSlot.IconRect.transform;
                _beginDragIconPoint = _beginDragIconTransform.position;
                _beginDragCursorPoint = Mouse.current.position.ReadValue();

                _beginDragSlotSiblingIndex = _beginDragSlot.transform.GetSiblingIndex();
                _beginDragSlot.transform.SetAsLastSibling();

                _beginDragSlot.SetHighlightOnTop(false);
            }
            else
            {
                _beginDragSlot = null;
            }
        }

        // Right Click : Use Item
        else if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            ItemSlotUI slot = _mouseController.RaycastAndGetFirstComponent<ItemSlotUI>();

            if (slot != null && slot.HasItem && slot.IsAccessible)
            {
                OpenPopupSelectItem(slot);
            }
        }
    }
    /// <summary> 드래그하는 도중 </summary>
    private void OnPointerDrag()
    {
        if (_beginDragSlot == null) return;
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            // 위치 이동
            _beginDragIconTransform.position =
                _beginDragIconPoint + ((Vector3)Mouse.current.position.ReadValue() - _beginDragCursorPoint);
        }
    }
    /// <summary> 클릭을 뗄 경우 </summary>
    private void OnPointerUp()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // End Drag
            if (_beginDragSlot != null)
            {
                // 위치 복원
                _beginDragIconTransform.position = _beginDragIconPoint;


                // UI 순서 복원
                _beginDragSlot.transform.SetSiblingIndex(_beginDragSlotSiblingIndex);

                // 드래그 완료 처리
                EndDrag();

                // 해당 슬롯의 하이라이트 이미지를 아이콘보다 앞에 위치시키기
                _beginDragSlot.SetHighlightOnTop(true);

                // 참조 제거
                _beginDragSlot = null;
                _beginDragIconTransform = null;
            }
        }
    }

    private void EndDrag()
    {
        ItemSlotUI endDragSlot = _mouseController.RaycastAndGetFirstComponent<ItemSlotUI>();

        // ������ ���Գ��� ������ ��ȯ �Ǵ� �̵�
        if (endDragSlot != null && endDragSlot.IsAccessible)
        {
            // ���� ������ ����
            // 1) ���콺 Ŭ�� ���� ���� ���� Ctrl �Ǵ� Shift Ű ����
            // 2) begin : �� �� �ִ� ������ / end : ����ִ� ����
            // 3) begin �������� ���� > 1
            bool isSeparatable =
     ((Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.leftShiftKey.isPressed) &&
     (_inventory.IsCountableItem(_beginDragSlot.Index) && !_inventory.HasItem(endDragSlot.Index)));

            // true : ���� ������, false : ��ȯ �Ǵ� �̵�
            bool isSeparation = false;
            int currentAmount = 0;

            // ���� ���� Ȯ��
            if (isSeparatable)
            {
                currentAmount = _inventory.GetCurrentAmount(_beginDragSlot.Index);
                if (currentAmount > 1)
                {
                    isSeparation = true;
                }
            }

            // 1. ���� ������
            if (isSeparation)
                TrySeparateAmount(_beginDragSlot.Index, endDragSlot.Index, currentAmount);
            // 2. ��ȯ �Ǵ� �̵�
            else
                TrySwapItems(_beginDragSlot, endDragSlot);

            // ���� ����
            UpdateTooltipUI(endDragSlot);
            return;
        }

        // ������(Ŀ���� UI ����ĳ��Ʈ Ÿ�� ���� ���� ���� ���)
        if (!_mouseController.IsOverUI())
        {
            // Ȯ�� �˾� ���� �ݹ� ����
            int index = _beginDragSlot.Index;
            string itemName = _inventory.GetItemName(index);
            int amount = _inventory.GetCurrentAmount(index);

            // �� �� �ִ� �������� ���, ���� ǥ��
            if (amount > 1)
                itemName += $" x{amount}";

            if (_showRemovingPopup)
                _popup.OpenConfirmationPopup(() => TryRemoveItem(index), itemName);
            else
                TryRemoveItem(index);
        }
        // ������ �ƴ� �ٸ� UI ���� ���� ���
        else
        {

        }
    }

    #endregion
    /***********************************************************************
    *                               Private Methods
    ***********************************************************************/
    #region .
    private void Start() // 새로 추가[cite: 6]
    {
        if (_mouseController == null)
        {
            _mouseController = GetComponent<InventoryMouseController>();
        }
        
        if (_healPotionHandler == null)
        {
            _healPotionHandler = FindFirstObjectByType<PotionHandler>();
        }
        if (_playerStatus == null)
        {
            _playerStatus = FindFirstObjectByType<PlayerStatus>();
        }
        if (_weaponSwitcher == null)
        {
            _weaponSwitcher = FindFirstObjectByType<WeaponSwitcher>();
        }
        if (_shopUI == null)
        {
            _shopUI = FindFirstObjectByType<ShopUI>();
        }
        if (_wallet == null)
        {
            _wallet = FindFirstObjectByType<Wallet>();
        }
        if (_shopService == null)
        {
            _shopService = FindFirstObjectByType<ShopService>();
        }

        Init();
        
        InitButtonEvents();
        

        if (_currentUsingItems != null)
        {
            if (_currentUsingItems.IsDataLoaded)
                InitializeUsingItemsFromEvent();
            else
                _currentUsingItems.OnItemsInitialized += InitializeUsingItemsFromEvent;
        }
    }
    private void InitializeUsingItemsFromEvent()
    {
        InitUsingItems(
    _currentUsingItems.Potion1,
    _currentUsingItems.Potion2,
    _currentUsingItems.Weapon1,
    _currentUsingItems.Weapon2,
    _currentUsingItems.Armor1,
    _currentUsingItems.Armor2,
    _currentUsingItems.PotionAmount1,
    _currentUsingItems.PotionAmount2
    );

        UpdateCurrentUsingItems();
    }
    /// <summary> UI 및 인벤토리에서 아이템 제거 </summary>
    private void TryRemoveItem(int index)
    {

        _inventory.Remove(index);
    }

    /// <summary> 아이템 사용 </summary>
    private void TryUseItem(int index)
    {

    }

    /// <summary> 두 슬롯의 아이템 교환 </summary>
    private void TrySwapItems(ItemSlotUI from, ItemSlotUI to)
    {
        if (from == to)
        {
            return;
        }


        from.SwapOrMoveIcon(to);
        _inventory.Swap(from.Index, to.Index);
    }

    /// <summary> 셀 수 있는 아이템 개수 나누기 </summary>
    private void TrySeparateAmount(int indexA, int indexB, int amount)
    {
        if (indexA == indexB)
        {
            return;
        }


        string itemName = $"{_inventory.GetItemName(indexA)} x{amount}";

        _popup.OpenAmountInputPopup(
            amt => _inventory.SeparateAmount(indexA, indexB, amt),
            amount, itemName
        );
    }

    /// <summary> 툴팁 UI의 슬롯 데이터 갱신 </summary>
    private void UpdateTooltipUI(ItemSlotUI slot)
    {
        if (slot == null)
            return;

        ItemData data = null;

        if (slot == _usingPotionItemSlot1)
        {
            data = _usingPotionItemData1;
        }
        else if (slot == _usingWeaponItemSlot1)
        {
            data = _usingWeaponItemData1;
        }
        else if (slot == _usingWeaponItemSlot2)
        {
            data = _usingWeaponItemData2;
        }
        else if (slot == _usingArmorItemSlot1)
        {
            data = _usingArmorItemData1;
        }
        else
        {
            data = _inventory.GetItemData(slot.Index);
        }

        if (data == null)
        {
            _itemTooltip.Hide();
            _equippedItemTooltip.Hide();
            return;
        }

        ItemData equippedData = null;

        if (data.Type == ItemType.CONSUMABLE)
        {
            equippedData = _usingPotionItemData1;
        }
        else if (data.Type == ItemType.EQUIPMENT)
        {
            EquipmentData equipData = data as EquipmentData;
            if (equipData == null) return;

            if (equipData.EquipmentType == EquipmentType.ARMOR)
            {
                equippedData = _usingArmorItemData1;
            }
            else
            {
                // 나머지는 전부 무기로 처리
                equippedData = _usingWeaponItemData1;
            }
        }
        // 상점같이 킬때는 장착아이템 툴팁끄기
        bool isShopOpen = _shopPanel != null && _shopPanel.activeSelf;

        if (equippedData != null && !isShopOpen)
        {
            //Debug.Log($"hover data : {data?.ItemName}");
            //Debug.Log($"equipped data : {equippedData?.ItemName}");

            _equippedItemTooltip.SetItemInfo(equippedData);
            _equippedItemTooltip.Show();
        }
        else
        {
            _equippedItemTooltip.Hide();
        }

        _itemTooltip.SetEquippedData(equippedData);
        _itemTooltip.SetItemInfo(data);
        _itemTooltip.SetStatInfo(data);
        _itemTooltip.Show();
    }

    public void InitUsingItems(
     ItemData potionData1,
     ItemData potionData2,
     ItemData weaponData1,
     ItemData weaponData2,
     ItemData armorData1,
     ItemData armorData2,
     int potionAmount1,
     int potionAmount2)
    {
        _usingPotionItemData1 = potionData1;
        _usingPotionItemData2 = potionData2;
        _usingWeaponItemData1 = weaponData1;
        _usingWeaponItemData2 = weaponData2;
        _usingArmorItemData1 = armorData1;
        _usingArmorItemData2 = armorData2;

        _usingPotionAmount1 = potionAmount1;
        _usingPotionAmount2 = potionAmount2;

        if (potionData1 is ConsumableData consumableData)
        {
            _usingPotionItemSlot1.SetItem(consumableData.Icon);
            _usingPotionItemSlot1.SetItemAmount(_usingPotionAmount1);
        }
        else
        {
            _usingPotionItemSlot1.RemoveItem();
            _usingPotionAmount1 = 0;
        }

        if (potionData2 is ConsumableData consumableData2)
        {
            _usingPotionItemSlot2.SetItem(consumableData2.Icon);
            _usingPotionItemSlot2.SetItemAmount(_usingPotionAmount2);
        }
        else
        {
            _usingPotionItemSlot2.RemoveItem();
            _usingPotionAmount2 = 0;
        }


        if (weaponData1 != null)
            _usingWeaponItemSlot1.SetItem(weaponData1.Icon);
        else
            _usingWeaponItemSlot1.RemoveItem();

        if (weaponData2 != null)
            _usingWeaponItemSlot2.SetItem(weaponData2.Icon);
        else
            _usingWeaponItemSlot2.RemoveItem();

        if (armorData1 != null)
            _usingArmorItemSlot1.SetItem(armorData1.Icon);
        else
            _usingArmorItemSlot1.RemoveItem();

        if (armorData2 != null)
            _usingArmorItemSlot2.SetItem(armorData2.Icon);
        else
            _usingArmorItemSlot2.RemoveItem();
    }




    #endregion
    /***********************************************************************
    *                               Public Methods
    ***********************************************************************/
    #region .

    /// <summary> 인벤토리 참조 등록 (인벤토리에서 직접 호출) </summary>
    public void SetInventoryReference(Inventory inventory)
    {
        _inventory = inventory;
    }

    /// <summary> 슬롯에 아이템 아이콘 등록 </summary>
    public void SetItemIcon(int index, Sprite icon)
    {

        _slotUIList[index].SetItem(icon);
    }

    /// <summary> 해당 슬롯의 아이템 개수 텍스트 지정 </summary>
    public void SetItemAmountText(int index, int amount)
    {

        // NOTE : amount가 1 이하일 경우 텍스트 미표시
        _slotUIList[index].SetItemAmount(amount);
    }

    /// <summary> 해당 슬롯의 아이템 개수 텍스트 지정 </summary>
    public void HideItemAmountText(int index)
    {

        _slotUIList[index].SetItemAmount(1);
    }

    /// <summary> 슬롯에서 아이템 아이콘 제거, 개수 텍스트 숨기기 </summary>
    public void RemoveItem(int index)
    {

        _slotUIList[index].RemoveItem();
    }

    /// <summary> 접근 가능한 슬롯 범위 설정 </summary>
    public void SetAccessibleSlotRange(int accessibleSlotCount)
    {
        for (int i = 0; i < _slotUIList.Count; i++)
        {
            _slotUIList[i].SetSlotAccessibleState(i < accessibleSlotCount);
        }
    }

    public void OpenPopupSelectItem(ItemSlotUI slot)
    {
        _selectedSlot = slot;
        _selectItemPopup.SetActive(true);

        // 확인용
        ItemData selectedData = _inventory.GetItemData(_selectedSlot.Index);

        if (selectedData != null)
        {
            Debug.Log($"선택한 아이템 데이터 : {selectedData.ItemName}");
        }
        else
        {
            Debug.Log("선택한 슬롯에 아이템 데이터가 없음");
        }
    }
    public void ConfirmSelectItem()
    {
        SwapUsingItem();
        ClosePopup();
    }


    public void ClosePopup()
    {
        _selectedSlot = null;
        _selectItemPopup.SetActive(false);
    }

    public ItemSlotUI[] GetAllUsingItemSlots()
    {
        return new ItemSlotUI[]
        {
        _usingPotionItemSlot1,
        _usingPotionItemSlot2,
        _usingWeaponItemSlot1,
        _usingWeaponItemSlot2,
        _usingArmorItemSlot1,
        _usingArmorItemSlot2
        };
    }


    public void SwapUsingItem()
    {
        if (_selectedSlot == null)
            return;

        ItemData selectedData = _inventory.GetItemData(_selectedSlot.Index);

        if (selectedData == null)
            return;

        int selectedAmount = _inventory.GetCurrentAmount(_selectedSlot.Index);

        ItemSlotUI targetSlot = null;

        ItemData tempData = null;
        int tempAmount = 0;

        // 소비 아이템
        if (selectedData.Type == ItemType.CONSUMABLE)
        {
            if (_usingPotionItemData1 == null)
            {
                targetSlot = _usingPotionItemSlot1;
                tempData = _usingPotionItemData1;
                tempAmount = _usingPotionAmount1;

                _usingPotionItemData1 = selectedData;
                _usingPotionAmount1 = selectedAmount;

                _lastSwappedPotionSlot = 1;
            }
            else if (_usingPotionItemData2 == null)
            {
                targetSlot = _usingPotionItemSlot2;
                tempData = _usingPotionItemData2;
                tempAmount = _usingPotionAmount2;

                _usingPotionItemData2 = selectedData;
                _usingPotionAmount2 = selectedAmount;

                _lastSwappedPotionSlot = 2;
            }
            else
            {
                if (_lastSwappedPotionSlot == 1)
                {
                    targetSlot = _usingPotionItemSlot2;
                    tempData = _usingPotionItemData2;
                    tempAmount = _usingPotionAmount2;

                    _usingPotionItemData2 = selectedData;
                    _usingPotionAmount2 = selectedAmount;

                    _lastSwappedPotionSlot = 2;
                }
                else
                {
                    targetSlot = _usingPotionItemSlot1;
                    tempData = _usingPotionItemData1;
                    tempAmount = _usingPotionAmount1;

                    _usingPotionItemData1 = selectedData;
                    _usingPotionAmount1 = selectedAmount;

                    _lastSwappedPotionSlot = 1;
                }
            }
        }
        // 장비 아이템
        else if (selectedData.Type == ItemType.EQUIPMENT)
        {
            EquipmentData equipData = selectedData as EquipmentData;

            if (equipData == null)
                return;

            // 방어구
            if (equipData.EquipmentType == EquipmentType.ARMOR)
            {
                if (_usingArmorItemData1 == null)
                {
                    targetSlot = _usingArmorItemSlot1;
                    tempData = _usingArmorItemData1;

                    _usingArmorItemData1 = selectedData;
                    _lastSwappedArmorSlot = 1;
                }
                else if (_usingArmorItemData2 == null)
                {
                    targetSlot = _usingArmorItemSlot2;
                    tempData = _usingArmorItemData2;

                    _usingArmorItemData2 = selectedData;
                    _lastSwappedArmorSlot = 2;
                }
                else
                {
                    if (_lastSwappedArmorSlot == 1)
                    {
                        targetSlot = _usingArmorItemSlot2;
                        tempData = _usingArmorItemData2;

                        _usingArmorItemData2 = selectedData;
                        _lastSwappedArmorSlot = 2;
                    }
                    else
                    {
                        targetSlot = _usingArmorItemSlot1;
                        tempData = _usingArmorItemData1;

                        _usingArmorItemData1 = selectedData;
                        _lastSwappedArmorSlot = 1;
                    }
                }
            }
            // 무기
            else
            {
                if (_usingWeaponItemData1 == null)
                {
                    targetSlot = _usingWeaponItemSlot1;
                    tempData = _usingWeaponItemData1;
                    _usingWeaponItemData1 = selectedData;

                    _lastSwappedWeaponSlot = 1;
                }
                else if (_usingWeaponItemData2 == null)
                {
                    targetSlot = _usingWeaponItemSlot2;
                    tempData = _usingWeaponItemData2;
                    _usingWeaponItemData2 = selectedData;

                    _lastSwappedWeaponSlot = 2;
                }
                else
                {
                    if (_lastSwappedWeaponSlot == 1)
                    {
                        targetSlot = _usingWeaponItemSlot2;
                        tempData = _usingWeaponItemData2;
                        _usingWeaponItemData2 = selectedData;

                        _lastSwappedWeaponSlot = 2;
                    }
                    else
                    {
                        targetSlot = _usingWeaponItemSlot1;
                        tempData = _usingWeaponItemData1;
                        _usingWeaponItemData1 = selectedData;

                        _lastSwappedWeaponSlot = 1;
                    }
                }
            }
        }
        else
        {
            Debug.Log("장착할 수 없는 아이템입니다.");
            return;
        }

        // 인벤토리 슬롯에 기존 장착템 되돌리기
        _inventory.SetItemData(_selectedSlot.Index, tempData);

        if (selectedData.Type == ItemType.CONSUMABLE)
        {
            _inventory.SetCurrentAmount(_selectedSlot.Index, tempAmount);
        }

        // 장착 슬롯 UI 갱신
        if (targetSlot != null)
        {
            targetSlot.SetItem(selectedData.Icon);

            if (selectedData.Type == ItemType.CONSUMABLE)
            {
                if (targetSlot == _usingPotionItemSlot1)
                    targetSlot.SetItemAmount(_usingPotionAmount1);
                else if (targetSlot == _usingPotionItemSlot2)
                    targetSlot.SetItemAmount(_usingPotionAmount2);
            }
            else
            {
                targetSlot.SetItemAmount(0);
            }
        }

        // 인벤토리 슬롯 UI 갱신
        if (tempData != null)
        {
            _selectedSlot.SetItem(tempData.Icon);

            if (tempData.Type == ItemType.CONSUMABLE)
            {
                _selectedSlot.SetItemAmount(tempAmount);
            }
            else
            {
                _selectedSlot.SetItemAmount(0);
            }
        }
        else
        {
            _selectedSlot.RemoveItem();
        }

        // CurrentUsingItems 반영
        CurrentUsingItems items = _currentUsingItems;

        items.Potion1 = _usingPotionItemData1 as ConsumableData;
        items.Potion2 = _usingPotionItemData2 as ConsumableData;

        items.PotionAmount1 = _usingPotionAmount1;
        items.PotionAmount2 = _usingPotionAmount2;

        items.Weapon1 = _usingWeaponItemData1 as EquipmentData;
        items.Weapon2 = _usingWeaponItemData2 as EquipmentData;

        items.Armor1 = _usingArmorItemData1 as EquipmentData;
        items.Armor2 = _usingArmorItemData2 as EquipmentData;

        items.CurrentWeapon = _usingWeaponItemData1 as EquipmentData;

        UpdateCurrentUsingItems();

        // 무기 프리팹 교체
        if (selectedData is EquipmentData weaponData &&
            weaponData.EquipmentType != EquipmentType.ARMOR)
        {
            if (targetSlot == _usingWeaponItemSlot1)
            {
                _weaponSwitcher.ChangeAndCreateMainWeapon(weaponData);
            }
            else if (targetSlot == _usingWeaponItemSlot2)
            {
                _weaponSwitcher.ChangeAndCreateSubWeapon(weaponData);
            }
        }

        _healPotionHandler.RefreshPotion();

        _playerStatus.ApplyEquipmentStat(_currentUsingItems);

        UIEvents.RaiseWeaponEquipped(
            _currentUsingItems.Weapon1,
            _currentUsingItems.Weapon2
        );

        ClosePopup();
    }

    public void UpdateCurrentUsingItems()
    {

        _currentUsingItems.SetUsingItems(
            _usingWeaponItemData1 as EquipmentData,
            _usingWeaponItemData2 as EquipmentData,
            _usingArmorItemData1 as EquipmentData,
            _usingArmorItemData2 as EquipmentData,

            _usingPotionItemData1 as ConsumableData,
            _usingPotionAmount1,
            _usingPotionItemData2 as ConsumableData,
            _usingPotionAmount2

        );
        _currentUsingItems.CurrentWeapon = _usingWeaponItemData1 as EquipmentData;
    }

    public void DecreaseUsingPotionAmount(int slotIndex)
    {
        if (slotIndex == 1)
        {
            _usingPotionAmount1--;

            if (_usingPotionAmount1 <= 0)
            {
                _usingPotionAmount1 = 0;
                _usingPotionItemData1 = null;
                _usingPotionItemSlot1.RemoveItem();
            }
            else
            {
                _usingPotionItemSlot1.SetItemAmount(_usingPotionAmount1);
            }
        }
        else if (slotIndex == 2)
        {
            _usingPotionAmount2--;

            if (_usingPotionAmount2 <= 0)
            {
                _usingPotionAmount2 = 0;
                _usingPotionItemData2 = null;
                _usingPotionItemSlot2.RemoveItem();
            }
            else
            {
                _usingPotionItemSlot2.SetItemAmount(_usingPotionAmount2);
            }
        }

        UpdateCurrentUsingItems();
    }




    public void SellItem(int index)
    {
        ItemData itemData = _inventory.GetItemData(index);
        int amount = _inventory.GetCurrentAmount(index);

        if (itemData == null)
            return;

        int sellPrice = itemData.BasePrice * amount;

        _wallet.AddGold(sellPrice);

        ShopEntryData resellData = CreateResellEntry(itemData, amount);
        _shopUI.AddResellSlot(resellData);

        _inventory.Remove(index);
    }

    private ShopEntryData CreateResellEntry(ItemData itemData, int amount)
    {
        ShopEntryData entry = ScriptableObject.CreateInstance<ShopEntryData>();

        entry.Item = itemData;
        entry.Price = itemData.BasePrice;
        entry.Stock = amount;
        entry.OverrideBasePrice = true;
        entry.InfiniteStock = false;

        return entry;
    }



    #endregion
    /***********************************************************************
    *                               Events
    ***********************************************************************/
    #region .
    void HandleInventoryOpened()
    {
        _root.SetActive(true);

        foreach (var slot in _slotUIList)
        {
            slot.OnInventoryOpened();
        }
    }
    void HandleInventoryClosed()
    {
        ClosePopup();
        _root.SetActive(false);
    }


    public void MarkSlotAsNewItem(int index)
    {
        if (index < 0 || index >= _slotUIList.Count)
            return;

        _slotUIList[index].MarkAsNewItem();
    }


    #endregion
}
    

    
   