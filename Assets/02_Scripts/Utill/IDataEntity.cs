using UnityEngine;

public interface IDataEntity
{
    string ID { get; }

    // 로드가 끝난 후, 데이터 간의 관계를 맺기 위한 함수
    public void LinkData(DataManager manager) { }
}
