using UnityEngine;

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
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(h, 0, v).normalized;

        // Notifie le GameManager dès que le joueur bouge
        if (moveInput.sqrMagnitude > 0.01f)
        {
            GameManager.Instance.OnPlayerMoved();
        }
    }

    void FixedUpdate()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }

        Vector3 newPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }
}