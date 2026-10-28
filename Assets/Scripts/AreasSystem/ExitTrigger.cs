using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.NotifyExitTriggered(this.gameObject);
            }
            else
            {
                Debug.LogError("No se encontró ningún ZoneManager activo en esta escena.");
            }
        }
    }
}