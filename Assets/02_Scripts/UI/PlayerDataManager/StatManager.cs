using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    [Header("Level Stats")]
    public int Level { get; set; }

    [Header("Exp Stats")]
    public int AccumulationExp { get; set; }

    [Header("Base Stats")]
    public float MaxHP { get; set; }
    public float MaxStamina { get; set; }
    public float BaseATK { get; set; }
    public float BaseDEF { get; set; }
    public float BaseLUCK { get; set; }

    [Header("Stat Point")]
    public int StatResetCount { get; set; }
    public int StatPoint { get; set; }

    [Header("Stat Level")]
    public int HPLevel { get; set; }
    public int StaminaLevel { get; set; }
    public int ATKLevel { get; set; }
    public int DEFLevel { get; set; }
    public int LUCKLevel { get; set; }

    [Header("Initial Value")]
    [SerializeField] private int _initialLevel = 1;
    [SerializeField] private int _initialExp = 0;

    [SerializeField] private float _initialMaxHP = 100f;
    [SerializeField] private float _initialMaxStamina = 100f;
    [SerializeField] private float _initialBaseATK = 25f;
    [SerializeField] private float _initialBaseDEF = 25f;
    [SerializeField] private float _initialBaseLUCK = 0f;

    [SerializeField] private int _initialStatResetCount = 1;
    [SerializeField] private int _initialStatPoint = 0;

    [SerializeField] private int _initialHPLevel = 1;
    [SerializeField] private int _initialStaminaLevel = 1;
    [SerializeField] private int _initialATKLevel = 1;
    [SerializeField] private int _initialDEFLevel = 1;
    [SerializeField] private int _initialLUCKLevel = 1;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitStats();
    }

    public void InitStats()
    {
        Level = _initialLevel;
        AccumulationExp = _initialExp;

        MaxHP = _initialMaxHP;
        MaxStamina = _initialMaxStamina;
        BaseATK = _initialBaseATK;
        BaseDEF = _initialBaseDEF;
        BaseLUCK = _initialBaseLUCK;

        StatResetCount = _initialStatResetCount;
        StatPoint = _initialStatPoint;

        HPLevel = _initialHPLevel;
        StaminaLevel = _initialStaminaLevel;
        ATKLevel = _initialATKLevel;
        DEFLevel = _initialDEFLevel;
        LUCKLevel = _initialLUCKLevel;
    }
}