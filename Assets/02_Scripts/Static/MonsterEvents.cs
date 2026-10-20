using System;
using UnityEngine;

public static class MonsterEvents
{
    public static event Action<string, GameObject> MonsterReturnedEvent;

    public static void RaiseMonsterReturnedEvent(string Id, GameObject obj)
    {
        MonsterReturnedEvent?.Invoke(Id, obj);
    }

    public static event Action MonsterDefeatedEvent;
    public static void RaiseMonsterDefeatedEvent()
    {
        MonsterDefeatedEvent?.Invoke();
    }
}