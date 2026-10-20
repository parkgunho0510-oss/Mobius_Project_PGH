using UnityEngine;
using UnityEngine.UI;
public class DeleteDataTabUI : MonoBehaviour
{
    [Header("Confirm Popup")]
    [SerializeField] private ConfirmPopupUI _confirmPopupUI;

    [Header("Buttons")]
    [SerializeField] private Button _deleteSaveButton;

    private void Awake()
    {
        _deleteSaveButton.onClick.AddListener(OnClickDeleteSave);
    }

    private void OnClickDeleteSave()
    {
        if (_confirmPopupUI == null)
        {
            Debug.LogError("ConfirmPopupUI가 연결되지 않았습니다.");
            return;
        }

        _confirmPopupUI.Open(
            onApply: DeleteSaveData,
            onDiscard: null
        );
    }

    private void DeleteSaveData()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager가 없습니다.");
            return;
        }

        SaveManager.Instance.Delete();
        //InventoryManager.Instance.ResetToDefaultData();

        Debug.Log("저장 데이터 삭제 완료");
    }
}