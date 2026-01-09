using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChanger : MonoBehaviour
{

    public void LoadSpecficScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
} 
