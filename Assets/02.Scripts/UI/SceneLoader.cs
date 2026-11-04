using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private const string LoadingScene = "LoadingScene";

    public static string NextSceneName { get; private set; }

    public static void Load(string sceneName)
    {
        NextSceneName = sceneName;

        SceneManager.LoadScene(LoadingScene);
    }
}