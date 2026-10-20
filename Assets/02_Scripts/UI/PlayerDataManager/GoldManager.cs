using UnityEngine;

public class GoldManager : MonoBehaviour
{


   public static GoldManager Instance {  get; private set; }

    public int Gold { get; set; } = 1000;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void InitGold()
    {
        Gold = 1000;
    }
}
