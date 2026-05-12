using UnityEngine;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 90f;

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // On utilise le Singleton pour appeler le GameManager directement
            GameManager.Instance.AddCoin();
            
            Destroy(gameObject);
        }
    }
}