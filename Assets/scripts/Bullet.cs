using UnityEngine;

// A coller sur le prefab Bullet (petite Sphere)
public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 4f;
    public int damage = 1;

    void Start()
    {
        // La balle se detruit automatiquement apres X secondes
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Avance tout droit
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // Touche le joueur
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);
            Destroy(gameObject);
        }
        // Touche un mur ou autre obstacle solide (pas une autre balle ni un trigger)
        else if (!other.isTrigger && !other.CompareTag("Sentinel"))
        {
            Destroy(gameObject);
        }
    }
}
