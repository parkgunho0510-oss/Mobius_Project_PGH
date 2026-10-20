using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// [REFACTORING]
/// 1. 객체 재사용을 통한 GC 최적화
/// - LinkData 호출 시마다 List를 새로 할당(new)하던 기존 방식 대신, Clear() 후 AddRange() 방식을 채택.
/// - 힙(Heap) 메모리 할당을 최소화하여 가비지 컬렉터(GC) 부하를 줄이고 프레임 드랍(Stuttering) 방지.
/// 
/// 2. 데이터 안전성 및 예외 처리 강화
/// - DataManager로부터 가져온 데이터에 대해 null 체크 로직을 추가.
/// - 데이터가 없는 경우에도 리스트를 null이 아닌 빈 상태(Count=0)로 유지하여 참조 에러(NullReferenceException) 사전 차단
/// 
/// </summary>

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject, IDataEntity
{
    public string Id;
    //public string QuestID;
    public string ID => Id;

    public string QuestName;
    public QuestType QuestType;
    
    [TextArea]
    public string Description;

    [Header("Repeat")]
    public RepeatRuleType RepeatRule;

    [Header("NPC")]
    public string StartNpcID;
    public string CompleteNpcID;

    [Header("Prerequisite")]
    public List<string> RequiredQuestIds = new();

    public List<QuestObjectiveData> Objectives = new();
    public List<QuestRewardData> Rewards = new();

    public void LinkData(DataManager dataManager)
    {
        var objec = dataManager.GetQuestObjectives(ID);
        var rws = dataManager.GetQuestRewards(ID);

        Objectives.Clear();
        if (objec != null && objec.Count > 0) Objectives.AddRange(objec);

        Rewards.Clear();
        if (rws != null && rws.Count > 0) Rewards.AddRange(rws);

        //Objectives = dataManager.GetQuestObjectives(ID);
        //Rewards = dataManager.GetQuestRewards(ID);

    }
}
