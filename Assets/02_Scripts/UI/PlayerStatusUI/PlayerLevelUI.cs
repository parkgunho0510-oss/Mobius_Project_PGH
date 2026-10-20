using UnityEngine;
using TMPro;
public class PlayerLevelUI : MonoBehaviour
{
    [SerializeField] TMP_Text _levelText;
    [SerializeField] TMP_Text _expText;

    int _expPercent = 0;

    void Awake()
    {
        if (_levelText == null) Debug.Log("Level Text Is NULL");
        if (_expText == null) Debug.Log("Exp Text Is NULL");

        UIEvents.OnLevelSetEvent += HandleLevelSet;
        UIEvents.OnExpChangedEvent += HandleExpChanged;
        
    }
    void OnDestroy()
    {
        UIEvents.OnLevelSetEvent -= HandleLevelSet;
        UIEvents.OnExpChangedEvent -= HandleExpChanged;
    }


    void RefreshLevelUI(float level)
    {
        _levelText.SetText($"Lv. {level}");
    }
    void RefreshExpUI()
    {
        _expText.SetText($"Exp: {_expPercent}%");
    }

    void HandleLevelSet(int level)
    {
        RefreshLevelUI(level);
    }
    void HandleExpChanged(int currentExpPercent)
    {
        if(currentExpPercent != _expPercent)
        {
            _expPercent = currentExpPercent;
            RefreshExpUI();
        }
    }
}
