using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class DontDestroyObjects : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        StartCoroutine(Co_LoadScene());
    }
    IEnumerator Co_LoadScene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartScene_LCS");
    }
}