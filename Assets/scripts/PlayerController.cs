using UnityEngine;

// A coller sur le Joueur (Capsule avec Rigidbody)
// Deplacement vue du dessus (top-down)
public class PlayerController : MonoBehaviour
{
    [Header("Deplacement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 12f;

    private Rigidbody rb;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // On bloque la rotation X et Z pour ne pas que le joueur tombe
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // ZQSD ou Fleches
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(h, 0, v).normalized;
    }

    void FixedUpdate()
    {
        // Le joueur regarde dans la direction du deplacement
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }

        // Deplacement physique pour ne pas traverser les murs
        Vector3 newPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }
}
