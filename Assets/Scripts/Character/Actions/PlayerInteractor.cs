using System;
using UnityEngine;

// este script se encarga de que el jugador pueda hablar con los NPC: busca al NPC mas cercano
// (o al que este fijado con lock on), y cuando el jugador pulsa el boton de interactuar, avisa
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private string npcTag = "NPC"; // tag que deben tener los NPC para poder hablar con ellos
    [SerializeField] private float interactRange = 2.5f; // distancia maxima para hablar con un NPC
    [SerializeField] private LayerMask interactMask = ~0; // capas donde busca NPC (por defecto todas)

    [Header("Debug")]
    [SerializeField] private bool logAvailability = true; // escribe en consola cuando aparece la opcion de hablar

    private PlayerInputHandler inputs; // referencia al input del jugador
    private TargetLock targetLock; // referencia al lock on (opcional): si hay un NPC fijado, tiene prioridad
    private GameObject currentNpc; // NPC con el que se puede hablar ahora mismo (o null)
    private GameObject lastLoggedNpc; // ultimo NPC que se logueo (para no spamear la consola)
    private readonly Collider[] buffer = new Collider[32]; // buffer reutilizable para no generar basura

    // true cuando hay un NPC con el que se puede hablar ahora mismo (para mostrar la opcion en la UI mas adelante)
    public bool CanInteract
    {
        get { return currentNpc != null; }
    }

    // devuelve el NPC con el que se puede hablar ahora mismo (o null)
    public GameObject CurrentNpc
    {
        get { return currentNpc; }
    }

    // para conectar el sistema de dialogo: interactor.TalkedToNpc += MiMetodo;
    public event Action<GameObject> TalkedToNpc;

    private void Awake()
    {
        inputs = GetComponent<PlayerInputHandler>();
        targetLock = GetComponent<TargetLock>(); // opcional: sin el solo funciona por cercania
    }

    private void Update()
    {
        currentNpc = FindNpc(); // buscamos el NPC actual cada frame
        LogAvailability(); // logueamos si cambio el NPC disponible

        // si no se ha pulsado interactuar, no hacemos nada mas
        if (!inputs.interact)
        {
            return;
        }

        inputs.interact = false; // se consume siempre, para que no quede pendiente

        // si hay un NPC cerca, hablamos con el
        if (currentNpc != null)
        {
            Talk(currentNpc);
        }
    }

    // busca el NPC con el que se puede hablar ahora mismo
    private GameObject FindNpc()
    {
        // 1) si hay un NPC fijado con lock on, tiene prioridad y es el unico con el que se puede hablar
        if (targetLock != null && targetLock.IsLocked)
        {
            Collider locked = targetLock.CurrentTarget;

            // si el objetivo fijado es un NPC y esta dentro del rango, lo devolvemos
            if (locked != null && locked.CompareTag(npcTag))
            {
                if (DistanceTo(locked) <= interactRange)
                {
                    return locked.gameObject;
                }

                return null; // si esta fuera de rango, no hay NPC disponible
            }
        }

        // 2) si no hay lock on (o el objetivo no es un NPC), buscamos el NPC mas cercano dentro del rango
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, interactRange, buffer, interactMask, QueryTriggerInteraction.Ignore);

        GameObject best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = buffer[i];

            // si no tiene el tag de NPC, lo saltamos
            if (!col.CompareTag(npcTag))
            {
                continue;
            }

            // nos quedamos con el NPC mas cercano
            float distance = DistanceTo(col);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = col.gameObject;
            }
        }

        return best;
    }

    // calcula la distancia desde el jugador hasta el punto mas cercano del collider del NPC
    private float DistanceTo(Collider col)
    {
        Vector3 closest = col.bounds.ClosestPoint(transform.position);
        return Vector3.Distance(transform.position, closest);
    }

    // se llama cuando el jugador habla con un NPC
    private void Talk(GameObject npc)
    {
        Debug.Log("Se ha hablado con el NPC (" + npc.name + ")", npc);
        TalkedToNpc?.Invoke(npc); // avisamos a quien escuche (sistema de dialogo)
    }

    // escribe en consola cuando aparece o desaparece la opcion de hablar (solo si logAvailability esta activo)
    private void LogAvailability()
    {
        // si no esta activado el log o el NPC no ha cambiado, no hacemos nada
        if (!logAvailability || currentNpc == lastLoggedNpc)
        {
            return;
        }

        lastLoggedNpc = currentNpc;

        // si hay un NPC disponible, lo logueamos
        if (currentNpc != null)
        {
            Debug.Log("Se puede hablar con " + currentNpc.name + " (E / B)", currentNpc);
        }
    }

    // dibuja el rango de interaccion en el editor (solo para depurar)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}