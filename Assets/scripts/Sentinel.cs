using UnityEngine;

// A coller sur le prefab Sentinel (Cylindre rouge avec tag "Sentinel")
// Tourelle fixe : detecte le joueur a vue, tire si pas de mur entre eux
public class Sentinel : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 12f;
    public float fieldOfView = 90f;       // angle de vision en degres
    public LayerMask obstacleMask;        // mettre la layer "Wall" ici

    [Header("Tir")]
    public GameObject bulletPrefab;
    public Transform firePoint;           // un Empty place devant la tourelle
    public float fireRate = 1f;           // tirs par seconde
    public float rotateSpeedToTarget = 5f;

    private Transform player;
    private float fireTimer;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        if (PlayerVisible())
        {
            // Tourne vers le joueur
            Vector3 dir = (player.position - transform.position);
            dir.y = 0;
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeedToTarget * Time.deltaTime);

            // Tire au rythme defini
            fireTimer += Time.deltaTime;
            if (fireTimer >= 1f / fireRate)
            {
                fireTimer = 0f;
                Shoot();
            }
        }
        else
        {
            fireTimer = 0f;
        }
    }

    bool PlayerVisible()
    {
        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist > detectionRange) return false;

        // Verifie l'angle (champ de vision)
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > fieldOfView * 0.5f) return false;

        // Verifie qu'aucun mur ne bloque la vue
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, toPlayer.normalized, dist, obstacleMask))
        {
            return false;
        }
        return true;
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    // Aide visuelle dans l'editeur Unity (Gizmos)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Vector3 left = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward * detectionRange;
        Vector3 right = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);
    }
}
