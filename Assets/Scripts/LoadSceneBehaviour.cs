/// <summary>
/// author: Unity Oficial
/// </summary>
using UnityEngine;
using Utilities.Inspector;

public class LoadSceneBehaviour : MonoBehaviour
{
    public SceneField sceneTop;
    public SceneField sceneTest;
    public SceneField sceneMinigame; 

    public void LoadSceneTop()
    {
        NextSceneLoader sceneLoader = new NextSceneLoader();
        sceneLoader.LoadNextScene(sceneTop);
    }

    public void LoadSceneTest()
    {
        NextSceneLoader sceneLoader = new NextSceneLoader();
        sceneLoader.LoadNextScene(sceneTest);
    }

    public void LoadSceneMinigame()
    {
        NextSceneLoader sceneLoader = new NextSceneLoader();
        sceneLoader.LoadNextScene(sceneMinigame);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
