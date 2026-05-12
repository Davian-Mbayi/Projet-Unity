using UnityEngine;

// A coller sur le Joueur (tag "Player")
// Systeme de 3 coeurs
public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;
    public float invulnerabilityTime = 1.5f;

    private int currentLives;
    private float lastHitTime = -999f;
    private Renderer rend;
    private Color baseColor;

    void Start()
    {
        currentLives = maxLives;
        rend = GetComponentInChildren<Renderer>();
        if (rend != null) baseColor = rend.material.color;

        if (GameManager.Instance != null)
            GameManager.Instance.UpdateLivesUI(currentLives, maxLives);
    }

    void Update()
    {
        // Effet "clignote rouge" pendant l'invulnerabilite
        if (rend != null)
        {
            bool invul = Time.time - lastHitTime < invulnerabilityTime;
            if (invul)
            {
                bool blink = Mathf.FloorToInt(Time.time * 10f) % 2 == 0;
                rend.material.color = blink ? Color.red : baseColor;
            }
            else
            {
                rend.material.color = baseColor;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (Time.time - lastHitTime < invulnerabilityTime) return;
        lastHitTime = Time.time;

        currentLives -= amount;
        if (currentLives < 0) currentLives = 0;

        if (GameManager.Instance != null)
            GameManager.Instance.UpdateLivesUI(currentLives, maxLives);

        if (currentLives <= 0)
        {
            GameManager.Instance.GameOver();
            gameObject.SetActive(false);
        }
    }
}
