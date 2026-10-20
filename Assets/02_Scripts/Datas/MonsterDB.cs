using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterDB", menuName = "Scriptable Objects/MonsterDB")]
public class MonsterDB : ScriptableObject, IDataEntity
{
    // CSV Header : ID
    [SerializeField] private string id;
    public string ID => id;

    public string MonsterID;
    public string MonsterName;
    public MonsterType MonsterType;

    [Header("Monster Stats")]
    public float Hp;
    public float HpScaling;
    public float Damage;
    public float DamageScaling;
    public float Defense;
    public float MoveSpeed;
    public float DetectRange;
    public float AttackRange;

    [Header("Reward")]
    public int DropExp;
    public int MinGold;
    public int MaxGold;


    public MonsterDB EvolveData;

    // CSV Header: DropTableID (Link to DropItemTable)
    public string DropTableID;

    // List to be populated by DataManager using DropTableID at runtime
    [HideInInspector] public List<DropItemData> DropTable = new();

    public void LinkData(DataManager manager)
    {

    }

}
