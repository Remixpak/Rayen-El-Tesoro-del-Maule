using System;
using UnityEngine;

// permite al jugador interactuar con NPCs (hablar) o cofres (abrir)
// busca el objetivo mas cercano o el fijado con lock on, y actua al pulsar interactuar
//la mejor solucion fue colocarla dentro del mismo script total solo interactuas con dos cosas :p
//en caso de que hagamos carteles, solo le ponemos el tag de npc y listo, aplicamos el show dialog para que lea la info
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private string npcTag = "NPC"; // tag de los NPC
    [SerializeField] private string chestTag = "Chest"; // tag de los cofres
    [SerializeField] private float interactRange = 2.5f; // distancia maxima
    [SerializeField] private LayerMask interactMask = ~0; // capas donde busca

    [Header("Debug")]
    [SerializeField] private bool logAvailability = true;

    [Header("Dialogo")]
    [SerializeField] private DialogManager dialogManager; // sistema de dialogo

    private PlayerInputHandler inputs;
    private TargetLock targetLock;
    private GameObject currentTarget; // objetivo actual (NPC o cofre)
    private GameObject lastLoggedTarget;
    private readonly Collider[] buffer = new Collider[32];

    public bool CanInteract => currentTarget != null; // true si hay algo cerca
    public GameObject CurrentTarget => currentTarget; // devuelve el objetivo actual
    public bool CurrentIsChest => currentTarget != null && currentTarget.CompareTag(chestTag); // true si es cofre

    public event Action<GameObject> TalkedToNpc;

    private void Awake()
    {
        inputs = GetComponent<PlayerInputHandler>();
        targetLock = GetComponent<TargetLock>(); // opcional
    }

    private void Update()
    {
        currentTarget = FindTarget();
        LogAvailability();

        if (!inputs.interact) return;

        inputs.interact = false; // consume el input

        if (currentTarget != null)
        {
            Interact(currentTarget);
        }
    }

    // busca el objetivo mas cercano o el fijado con lock on
    private GameObject FindTarget()
    {
        // 1) prioridad al lock on
        if (targetLock != null && targetLock.IsLocked)
        {
            Collider locked = targetLock.CurrentTarget;

            if (locked != null && IsInteractable(locked))
            {
                if (DistanceTo(locked) <= interactRange) return locked.gameObject;
                return null;
            }
        }

        // 2) si no, el mas cercano en rango
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, interactRange, buffer, interactMask, QueryTriggerInteraction.Ignore);

        GameObject best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = buffer[i];
            if (!IsInteractable(col)) continue;

            float distance = DistanceTo(col);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = col.gameObject;
            }
        }

        return best;
    }

    // es interactuable si es un NPC o un cofre abrible
    private bool IsInteractable(Collider col)
    {
        if (col.CompareTag(npcTag)) return true;

        if (col.CompareTag(chestTag))
        {
            Chest chest = col.GetComponentInParent<Chest>();
            return chest != null && chest.CanOpen;
        }

        return false;
    }

    // calcula la distancia al borde del collider
    private float DistanceTo(Collider col)
    {
        Vector3 closest = col.bounds.ClosestPoint(transform.position);
        return Vector3.Distance(transform.position, closest);
    }

    // actua segun sea cofre o NPC
    private void Interact(GameObject target)
    {
        if (target.CompareTag(chestTag))
        {
            Chest chest = target.GetComponentInParent<Chest>();
            if (chest != null) chest.Open();
            return;
        }

        Talk(target);
    }

    // dispara el evento de dialogo
    private void Talk(GameObject npc)
    {
        TalkedToNpc?.Invoke(npc);

        NpcDialog dialog = npc.GetComponentInParent<NpcDialog>();

        if (dialog == null || dialogManager == null) return;

        dialogManager.getConversation(dialog.Conversation);
    }

    // logea cuando cambia el objetivo
    private void LogAvailability()
    {
        if (!logAvailability || currentTarget == lastLoggedTarget) return;
        lastLoggedTarget = currentTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}