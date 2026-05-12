using UnityEngine;

// A coller sur le prefab Coin (Cylindre jaune, IsTrigger coche)
public class Coin : MonoBehaviour
{
    public int value = 10;
    public float rotationSpeed = 180f;
    public float bobAmount = 0.2f;
    public float bobSpeed = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Rotation
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        // Mouvement de haut en bas
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobAmount;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddScore(value);
            Destroy(gameObject);
        }
    }
}
