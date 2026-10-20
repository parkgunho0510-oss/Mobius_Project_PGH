using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;


public class QuestDetailPopup : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] TMP_Text _title;
    [SerializeField] TMP_Text _subTitle;
    [SerializeField] TMP_Text _progress;
    [SerializeField] TMP_Text _reward;
    void Awake()
    {
        if (_root == null) _root = transform.GetChild(0).gameObject;
        if (_title == null) Debug.Log("Title Text is NULL");
        if (_subTitle == null) Debug.Log("SubTitle Text is NULL");
        if (_progress == null) Debug.Log("Progress Text is NULL");
        if (_reward == null) Debug.Log("Reward Text is NULL");
    }
    void OnEnable()
    {
        Hide();
    }
    void Start()
    {

    }
    public void RefreshUI(QuestData data)
    {
        _title.SetText(data.QuestName);
        
        QuestRunTimeState runTimeState = QuestManager.Instance.QuestRunTimeStateTable[data.Id];

        _subTitle.SetText("");
        _progress.SetText("");
        _reward.SetText("");
        for (int i = 0; i < data.Objectives.Count; i++)
        {
            _subTitle.text += $"{data.Objectives[i].Description}\n";
            _progress.text += $"{runTimeState.ObjectiveRunTimeStateList[i].CurrentAmount} / {data.Objectives[i].RequireAmount}\n";
        }

        for (int i = 0; i < data.Rewards.Count; i++)
        {
            _reward.text += $"{data.Rewards[i].Description}\n";
        }

    }
    public void Show()
    {
        _root.SetActive(true);
    }
    public void Hide()
    {
        _root.SetActive(false);
    }
}
