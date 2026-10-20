using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemQuickSlotUI : MonoBehaviour
{
    [Header("1¹ø Äü½½·Ô")]
    [SerializeField] private Image _itemImage1;
    [SerializeField] private Slider _cooldownSlider1;
    [SerializeField] private Image _cooldownFillImage1;
    [SerializeField] private TextMeshProUGUI _countText1;

    [Header("2¹ø Äü½½·Ô")]
    [SerializeField] private Image _itemImage2;
    [SerializeField] private Slider _cooldownSlider2;
    [SerializeField] private Image _cooldownFillImage2;
    [SerializeField] private TextMeshProUGUI _countText2;

    private void Start()
    {
        InitSlot(1);
        InitSlot(2);
    }

    private void InitSlot(int slotIndex)
    {
        Slider slider = GetCooldownSlider(slotIndex);
        Image fillImage = GetCooldownFillImage(slotIndex);
        Image itemImage = GetItemImage(slotIndex);
        TextMeshProUGUI countText = GetCountText(slotIndex);

        slider.maxValue = 1f;
        slider.value = 0f;

        fillImage.gameObject.SetActive(false);

        itemImage.sprite = null;
        itemImage.gameObject.SetActive(false);

        countText.text = "0";
    }

    public void SetCount(int slotIndex, int count)
    {
        GetCountText(slotIndex).text = count.ToString();
    }

    public void SetItem(int slotIndex, Sprite icon)
    {
        Image itemImage = GetItemImage(slotIndex);

        if (icon == null)
        {
            itemImage.sprite = null;
            itemImage.gameObject.SetActive(false);
            return;
        }

        itemImage.sprite = icon;
        itemImage.gameObject.SetActive(true);
    }

    public void SetCooldown(int slotIndex, float currentTime, float maxTime)
    {
        Slider slider = GetCooldownSlider(slotIndex);
        Image fillImage = GetCooldownFillImage(slotIndex);

        if (maxTime <= 0f)
        {
            ClearCooldown(slotIndex);
            return;
        }

        fillImage.gameObject.SetActive(true);

        float ratio = currentTime / maxTime;
        slider.value = 1f - Mathf.Clamp01(ratio);

        if (slider.value <= 0f)
        {
            fillImage.gameObject.SetActive(false);
        }
    }

    public void ClearCooldown(int slotIndex)
    {
        Slider slider = GetCooldownSlider(slotIndex);
        Image fillImage = GetCooldownFillImage(slotIndex);

        slider.value = 0f;
        fillImage.gameObject.SetActive(false);
    }

    private Image GetItemImage(int slotIndex)
    {
        return slotIndex == 1 ? _itemImage1 : _itemImage2;
    }

    private Slider GetCooldownSlider(int slotIndex)
    {
        return slotIndex == 1 ? _cooldownSlider1 : _cooldownSlider2;
    }

    private Image GetCooldownFillImage(int slotIndex)
    {
        return slotIndex == 1 ? _cooldownFillImage1 : _cooldownFillImage2;
    }

    private TextMeshProUGUI GetCountText(int slotIndex)
    {
        return slotIndex == 1 ? _countText1 : _countText2;
    }
}