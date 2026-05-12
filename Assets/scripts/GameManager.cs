using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// A coller sur un GameObject vide "GameManager"
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI HUD")]
    public TMP_Text scoreText;
    public TMP_Text livesText;          // affichera "Vies : 3"

    [Header("UI Game Over / Victoire")]
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public TMP_Text finalScoreText;

    [Header("Victoire")]
    public int totalCoinsToWin = 0;     // mis a jour automatiquement au Start

    private int score = 0;
    private int coinsCollected = 0;
    private bool isGameOver = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        // Compte automatiquement le nombre de pieces dans la scene
        totalCoinsToWin = GameObject.FindGameObjectsWithTag("Coin").Length;

        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        coinsCollected++;
        UpdateScoreUI();

        // Toutes les pieces ramassees = victoire
        if (totalCoinsToWin > 0 && coinsCollected >= totalCoinsToWin)
        {
            Win();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score : " + score;
    }

    public void UpdateLivesUI(int current, int max)
    {
        if (livesText != null)
        {
            string hearts = "";
            for (int i = 0; i < current; i++) hearts += "<3 ";
            livesText.text = "Vies : " + hearts;
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Score : " + score;

        Time.timeScale = 0f;
    }

    void Win()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (winPanel != null) winPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Score : " + score;

        Time.timeScale = 0f;
    }

    // Bouton "Rejouer"
    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Bouton "Menu"
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
