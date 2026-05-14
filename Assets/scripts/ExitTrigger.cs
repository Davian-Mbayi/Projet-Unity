using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // On vérifie que c'est bien le joueur qui traverse la zone
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                // Au lieu de NextLevel(), on appelle explicitement le niveau 2
                GameManager.Instance.LoadLevel2();
            }
        }
    }
}