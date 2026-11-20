using UnityEngine;
using UnityEngine.Events;
public class SpawnPihuchenTrigger : MonoBehaviour
{
    [Tooltip("Tag del objeto que activa el trigger (normalmente Player)")]
    public string targetTag = "Player";

    [Tooltip("Se activa solo una vez para no instanciar varias veces")]
    public bool triggerOnce = true;

    public UnityEvent onTriggered;

    private bool yaActivado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && yaActivado) return;

        if (other.CompareTag(targetTag))
        {
            yaActivado = true;
            onTriggered?.Invoke();
        }
    }
}
