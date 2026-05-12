using UnityEngine;

// A coller sur la Main Camera
// Camera placee au-dessus du joueur (vue du dessus)
public class TopDownCamera : MonoBehaviour
{
    public Transform target;             // glisser le joueur ici
    public Vector3 offset = new Vector3(0, 15, -6); // hauteur et leger recul
    public float followSpeed = 5f;

    void Start()
    {
        // Incline la camera vers le bas pour bien voir le labyrinthe
        transform.rotation = Quaternion.Euler(65f, 0f, 0f);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
    }
}
