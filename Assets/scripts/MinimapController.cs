using UnityEngine;

public class MinimapController : MonoBehaviour
{
    public Transform playerTransform;
    public float height = 30f;

    void LateUpdate()
    {
        if (playerTransform == null) return;
        // La caméra suit le joueur en X/Z, fixe en Y
        transform.position = new Vector3(
            playerTransform.position.x,
            playerTransform.position.y + height,
            playerTransform.position.z
        );
    }
}