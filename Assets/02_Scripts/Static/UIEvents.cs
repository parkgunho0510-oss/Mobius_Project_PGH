using System;
using UnityEngine;

public static class UIEvents
{
    public static event Action OnInteractedEvent;
    public static void RaiseInteractedEvent()
    {
        OnInteractedEvent?.Invoke();
    }

    public static event Action<float, float> OnHPChanged;
    public static void RaiseHPChanged(float currentHP, float maxHP)
    {
        OnHPChanged?.Invoke(currentHP, maxHP);
    }


    public static event Action<float, float> OnStaminaChanged;
    public static void RaiseStaminaChanged(float currentMP, float maxMP)
    {
        OnStaminaChanged?.Invoke(currentMP, maxMP);
    }

    public static event Action<int> OnLevelUpEvent;
    public static void RaiseLevelUpEvent(int nextLevel)
    {
        OnLevelUpEvent?.Invoke(nextLevel);
    }
    public static event Action<int> OnLevelSetEvent;
    public static void RaiseLevelSetEvent(int level)
    {
        OnLevelSetEvent?.Invoke(level);
    }
    public static event Action<int> OnExpChangedEvent;
    public static void RaiseExpChangedEvent(int Exp)
    {
        OnExpChangedEvent?.Invoke(Exp);
    }


    public static event Action<int> OnGoldChanged;

    public static void RaiseGoldChanged(int Gold)
    {
        OnGoldChanged?.Invoke(Gold);
    }
    public static event Action<int> OnStatPointChanged;
    public static void RaiseStatPointChanged(int StatPoint)
    {
        OnStatPointChanged?.Invoke(StatPoint);
    }
    public static event Action<float> OnMaxHPChanged;
    public static void RaiseMaxHPChanged(float currentHPStats)
    {
        OnMaxHPChanged?.Invoke(currentHPStats);
    }
    public static event Action<float> OnMaxStaminaChanged;

    public static void RaiseMaxStaminaChanged(float currentHPStats)
    {
        OnMaxStaminaChanged?.Invoke(currentHPStats);
    }
    public static event Action<float> OnATKStatChanged;

    public static void RaiseATKStatChanged(float currnetATKStat)
    {
        OnATKStatChanged?.Invoke(currnetATKStat);
    }
    public static event Action<float> OnDEFStatChanged;

    public static void RaiseDEFStatChanged(float currnetATKStat)
    {
        OnDEFStatChanged?.Invoke(currnetATKStat);
    }

    public static event Action<float> OnLUCKStatChanged;

    public static void RaiseLUCKStatChanged(float currnetATKStat)
    {
        OnLUCKStatChanged?.Invoke(currnetATKStat);
    }
    public static event Action<StatType, int> OnStatLevelChanged;
    public static void RaiseStatLevelChanged(StatType statType, int level)
    {
        OnStatLevelChanged?.Invoke(statType, level);
    }
    public static event Action<int> OnStatResetCountChanged;
    public static void RaiseStatResetCountChanged(int count)
    {
        OnStatResetCountChanged?.Invoke(count);
    }

    public static event Action OnInventoryOpenedEvent;
    public static void RaiseInventoryOpened()
    {
        OnInventoryOpenedEvent?.Invoke();
    }
    public static event Action OnInventoryClosedEvent;
    public static void RaiseInventoryClosed()
    {
        OnInventoryClosedEvent?.Invoke();
    }

    public static event Action CursorShowedEvent;
    public static void RaiseCursorShowed()
    {
        CursorShowedEvent?.Invoke();
    }
    public static event Action CursorHideEvent;
    public static void RaiseCursorHide()
    {
        CursorHideEvent?.Invoke();
    }

    public static event Action<NpcData, NpcRunTimeState> OnNpcInteractedEvent;
    public static void RaiseNpcInteracted(NpcData data, NpcRunTimeState runTimeState)
    {
        OnNpcInteractedEvent?.Invoke(data, runTimeState);
    }

    public static event Action<string> OnDialogueEndedEvent;
    public static void RaiseDialogueEnded(string id)
    {
        OnDialogueEndedEvent?.Invoke(id);   
    }
    public static event Action OnStatusOpenedEvent;
    public static void RaiseStatusOpened()
    {
        OnStatusOpenedEvent?.Invoke();
    }
    public static event Action OnStatusClosedEvent;
    public static void RaiseStatusClosed()
    {
        OnStatusClosedEvent?.Invoke();
    }

    // RŰ ����Ī
    public static event Action<EquipmentData, EquipmentData> OnWeaponSwitched;

    public static void RaiseWeaponSwitched(
        EquipmentData currentWeapon,
        EquipmentData subWeapon)
    {
        OnWeaponSwitched?.Invoke(currentWeapon, subWeapon);
    }

    // �κ��丮 ���� ����
    public static event Action<EquipmentData, EquipmentData> OnWeaponEquipped;

    public static void RaiseWeaponEquipped(
        EquipmentData weapon1,
        EquipmentData weapon2)
    {
        OnWeaponEquipped?.Invoke(weapon1, weapon2);
    }

    public static event Action OnPlayerDead;

    public static void RaisePlayerDead()
    {
        OnPlayerDead?.Invoke();
    }

    public static event Action UIOpened;
    public static void RaiseUIOpened()
    {
        UIOpened?.Invoke(); 
    }

    public static event Action UIClosed;
    public static void RaiseUIClosed()
    {
        UIClosed?.Invoke();
    }
}