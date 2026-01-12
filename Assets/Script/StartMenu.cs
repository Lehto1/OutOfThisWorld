using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [Header("Scene to load")]
    [SerializeField] private string firstLevelSceneName;
    public void StartGame()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }

    // Update is called once per frame
   public void QuitGame()
    {
        Debug.Log("Game Quit"); // Works in editor
        Application.Quit();     // Works in build
    }
}
