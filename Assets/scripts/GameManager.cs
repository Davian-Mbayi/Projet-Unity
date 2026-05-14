using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // <-- Indispensable pour recharger le niveau

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Interface (UI)")]
    public TMP_Text coinCounterText;
    public TMP_Text alertMessageText;
    public TMP_Text introText;

    [Header("Menu Accueil")]
    public GameObject welcomePanel;
    public GameObject startButton;

    [Header("Timer & Niveaux")]
    public TMP_Text timerText;
    
    [Tooltip("Temps en secondes pour le Niveau 1")]
    public float timeLevel1 = 120f; // Ex: 120 secondes = 2 minutes
    [Tooltip("Temps en secondes pour le Niveau 2")]
    public float timeLevel2 = 60f;  // Ex: 60 secondes = 1 minute
    
    private float timeRemaining; // Le chrono actif (qui va diminuer)
    private bool timerRunning = false;// État du chrono

    [Header("Audio")]
    public AudioSource gameAudioSource;
    public AudioClip coinPickupSound;
    public AudioClip exitOpenSound;

    private int totalCoinsToFind;
    private int coinsCollected = 0;
    private GameObject exitWall;

    private bool gameStarted = false;
    private bool messageHidden = false;

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // On vérifie le nom de la scène actuelle
        if (SceneManager.GetActiveScene().name == "Level2")
        {
            // --- COMPORTEMENT POUR LE NIVEAU 2 ---
            timeRemaining = timeLevel2; // <--- ON APPLIQUE LE CHRONO DU NIVEAU 2 ICI

            if (welcomePanel != null) welcomePanel.SetActive(false);
            if (startButton != null) startButton.SetActive(false);
            if (introText != null) introText.gameObject.SetActive(false);
            
            Time.timeScale = 1f;
            gameStarted = true;
            timerRunning = true;
        }
        else
        {
            // --- COMPORTEMENT POUR LE NIVEAU 1 (Par défaut) ---
            timeRemaining = timeLevel1; // <--- ON APPLIQUE LE CHRONO DU NIVEAU 1 ICI

            if (welcomePanel != null) welcomePanel.SetActive(true);
            if (startButton != null) startButton.SetActive(true);
            if (introText != null) introText.gameObject.SetActive(false);
            
            Time.timeScale = 0f;
        }
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        // Gestion du chronomètre
        if (gameStarted && timerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else
            {
                GameOver();
            }
        }
    }

    // Appelée par PlayerController dès que le joueur bouge
    public void OnPlayerMoved()
    {
        if (gameStarted && !messageHidden)
        {
            if (introText != null) introText.gameObject.SetActive(false);
            messageHidden = true;
        }
    }

    public void StartGame()
    {
        // 1. Cacher d'abord le panel
        if (startButton != null) startButton.SetActive(false);
        if (welcomePanel != null) welcomePanel.SetActive(false);

        // 2. Afficher IntroText ensuite
        if (introText != null) introText.gameObject.SetActive(true);

        Time.timeScale = 1f;
        gameStarted = true;
        
        // 3. Lancer le chronomètre !
        timerRunning = true;
    }

    public void SetupLevel(int totalCoins, GameObject exitObject)
    {
        totalCoinsToFind = totalCoins;
        exitWall = exitObject;
        coinsCollected = 0;

        if (alertMessageText != null) alertMessageText.text = "";
        UpdateUI();
    }

    public void AddCoin()
    {
        coinsCollected++;
        UpdateUI();

        if (gameAudioSource != null && coinPickupSound != null)
        {
            gameAudioSource.PlayOneShot(coinPickupSound);
        }

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (coinsCollected >= totalCoinsToFind)
        {
            if (exitWall != null)
            {
                Destroy(exitWall);
                
                timerRunning = false; 

                if (alertMessageText != null)
                {
                    alertMessageText.text = "La sortie est ouverte ! Fuyez !";
                    
                    // --- NOUVEAU : On efface le message au bout de 3 secondes ---
                    Invoke("ClearAlertMessage", 3f); 
                }

                if (gameAudioSource != null && exitOpenSound != null)
                {
                    gameAudioSource.PlayOneShot(exitOpenSound);
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (coinCounterText != null)
        {
            coinCounterText.text = $"Pièces : {coinsCollected} / {totalCoinsToFind}";
        }
    }

    // --- NOUVELLES FONCTIONS : TIMER ET NIVEAU ---

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Formate le temps en Minutes:Secondes (ex: 01:45)
            float minutes = Mathf.FloorToInt(timeRemaining / 60);
            float seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            // Petit bonus de stress visuel : le texte devient rouge sous les 10 secondes
            if (timeRemaining <= 10f) timerText.color = Color.red;
        }
    }

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    private void GameOver()
    {
        timerRunning = false;
        timeRemaining = 0;
        UpdateTimerDisplay();

        // On active le panneau de défaite complet
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        Time.timeScale = 0f; // On fige le joueur et le monde
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level1"); 
    }

    public void NextLevel()
    {
        // Recharge la scène actuelle pour relancer une génération aléatoire
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void LoadLevel2()
{
    Time.timeScale = 1f; // Toujours remettre le temps à 1 avant de changer
    SceneManager.LoadScene("Level2");
}
private void ClearAlertMessage()
    {
        if (alertMessageText != null)
        {
            alertMessageText.text = "";
        }
    }
}