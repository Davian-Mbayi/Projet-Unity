using UnityEngine;
using TMPro;

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
        if (welcomePanel != null) welcomePanel.SetActive(true);
        if (startButton != null) startButton.SetActive(true);
        if (introText != null) introText.gameObject.SetActive(false);
        
        Time.timeScale = 0f;
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
    // 1. Cacher d'abord le panel (qui contient IntroText)
    if (startButton != null) startButton.SetActive(false);
    if (welcomePanel != null) welcomePanel.SetActive(false);

    // 2. Afficher IntroText ensuite (il échappe au SetActive du parent)
    if (introText != null) introText.gameObject.SetActive(true);

    Time.timeScale = 1f;
    gameStarted = true;
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
                
                if (alertMessageText != null)
                {
                    alertMessageText.text = "La sortie est ouverte ! Félicitations !";
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
}