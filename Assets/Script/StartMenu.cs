using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [Header("Scene to load")]
    [SerializeField] private string firstLevelSceneName;
    public void StartGame()
    {
        SceneManager.LoadScene(firstLevelSceneName); //loadarin första scenen
    }

  
   public void QuitGame()
    {
        Debug.Log("Game Quit"); // skriver i debug logen "game quit"
        Application.Quit();     // lämmnar spelet 
    }
}
