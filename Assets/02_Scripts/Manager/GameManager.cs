using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] Inventory _inventory;
    

    void Awake()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Init") return;
        if (scene.name == "StartScene_LCS") return;
        

        if (_inventory == null) _inventory = FindFirstObjectByType<Inventory>();
    }
}
