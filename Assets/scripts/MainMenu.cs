using UnityEngine;
using UnityEngine.SceneManagement;

// A coller sur un GameObject "MainMenu" dans la scene MainMenu
public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "Game";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitter (ne fonctionne qu'en build)");
    }
}
