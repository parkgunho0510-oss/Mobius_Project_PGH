//using System.Collections;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class InitSceneController : MonoBehaviour
//{
//    private void Start()
//    {
//        StartCoroutine(Co_LoadScene());
//    }

//    private IEnumerator Co_LoadScene()
//    {
//        yield return new WaitForSeconds(3f);

//        SceneManager.LoadScene(SceneLoadContext.NextSceneName);

//        SceneLoadContext.NextSceneName = "StartScene_LCS";
//    }
//}

//public static class SceneLoadContext
//{
//    public static string NextSceneName = "StartScene_LCS";
//}