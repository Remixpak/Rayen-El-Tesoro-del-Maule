using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInputHandler))]

public class PlayerAttack : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private float damage = 10f; // Cantidad de daño que el ataque inflige
    [SerializeField] private float cooldown = 0.1f;// Tiempo de enfriamiento entre ataques
    [SerializeField] private LayerMask hitMask = ~0; // LayerMask para determinar qué capas pueden ser golpeadas por el ataque

    [Header("Zona de golpe")]
    [SerializeField] private Transform hitbox; // transform que representa la zona del golpe

    [Header("Timing (version prototipo pq faltan animaciones dou)")]
    [SerializeField] private bool useAnimationEvents = false; 
    [SerializeField] private float windupTime = 0.15f; // Tiempo de preparación antes de que el ataque se active
    [SerializeField] private float activeTime = 0.15f; // Tiempo durante el cual el ataque puede golpear a los enemigos
    [SerializeField] private float recoveryTime = 0.25f; // Tiempo de recuperación después de que el ataque ha terminado

    [Header("Animator")]
    [SerializeField] private Animator animator; // Referencia al Animator para controlar las animaciones del ataque
    [SerializeField] private string attackTrigger = "Attack"; // Nombre del trigger en el Animator que inicia la animación de ataque

    private enum State { Idle, Windup, Active, Recovery } // Estados del ataque: Idle (inactivo), Windup (preparación), Active (activo), Recovery (recuperación)

    private PlayerInputHandler inputs; 
    private State state = State.Idle;
    private float StateTimer = 0f; // el timer para pasar de un estado a otro
    private float nextAttackTime = 0f;
    private int triggerHash;

    private readonly Collider[] buffer = new Collider[16]; // Buffer para almacenar los colliders golpeados
    private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>(); // Conjunto para almacenar los enemigos que ya han sido golpeados en este ataque

    public bool isAttacking => state != State.Idle;

    private void Awake()
    {
        inputs = GetComponent<PlayerInputHandler>();
        triggerHash = Animator.StringToHash(attackTrigger);

        if(hitbox!=null)
        {
            hitbox.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (inputs.attack)
        {
            inputs.attack = false; // Resetear el input de ataque para evitar ataques continuos
            if (state == State.Idle && Time.time >= nextAttackTime)
                            {
                StartAttack();
            }
        }
        if (state != State.Idle && !useAnimationEvents)
        {
            TickTimers();
        }

        //solo comprobamos golpes mientras la hitbox este activa 
        if (state == State.Active)
        {
            DoHitCheck();
        }
    }

    //funcion que se encarga de iniciar un nuevo ataque 
    private void StartAttack()
    {
        state = State.Windup;
        StateTimer = 0f;
        alreadyHit.Clear();

        if(animator !=null)
        {
            animator.SetTrigger(triggerHash);
        }
    }

    private void TickTimers()
    {
        StateTimer += Time.deltaTime;

        switch (state)
        {
            case State.Windup when StateTimer >= windupTime: // termina la prepracion y empieza el golpe
                AttackHitStart();
                break;
            case State.Active when StateTimer >= activeTime:// termina el golpe y pasa a la recuperacion
                AttackHitEnd();
                break;
            case State.Recovery when StateTimer >= recoveryTime://termina la recuperacion y vuelve a iddle 
                AttackEnd();
                break;
        }
    }

    //funcion que empioeza la fase de golpe 
    public void AttackHitStart()
    {
        state = State.Active;
        StateTimer = 0f;
        if (hitbox != null) // activamos el hitbox para golpear 
        {
            hitbox.gameObject.SetActive(true);
        }
    }


    //funcion que se llama al terminar la fase del golpe 
    public void AttackHitEnd()
    {
        state = State.Recovery;
        StateTimer = 0f;
        if (hitbox != null) // se desactiva la hitbox para que deje de golpear
        {
            hitbox.gameObject.SetActive(false);
        }
    }


    //funcion para manejar el cambio de estado para verificar que el ataque termina 
    public void AttackEnd()
    {
        state = State.Idle;
        nextAttackTime = Time.time + cooldown;
    }


    //funcion que comprueba si hay enemigos dentro del hitbox y les aplica el daño
    private void DoHitCheck()
    {
        if (hitbox == null) {
            return;
        }

        // el overlap llena el buffer sin generar basura visual
        int count = Physics.OverlapBoxNonAlloc(
            hitbox.position, hitbox.lossyScale * 0.5f, buffer, hitbox.rotation,
            hitMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            //buscamos el IDamageable en el padre por si el collider esta en un hijo
            IDamageable target = buffer[i].GetComponentInParent<IDamageable>();

            // si no es dañable el objetivo o ya lo hemos golpeado lo saltamos 
            if (target == null || !alreadyHit.Add(target)) 
            {
                continue; // un golpe por ataque y objetivo
            }


            //calculamos la direccion del golpe desde el jugador 
            Vector3 dir = buffer[i].transform.position - transform.position;
            dir.y = 0f;

            //aplicamos el daño al enemigo 
            target.TakeDamage(damage, dir.normalized);
        }
    }

    // dibuja la hitbox en el editor cuando el objeto está seleccionado (solo para depurar)
    private void OnDrawGizmosSelected()
    {
        if (hitbox == null) {
            return;
        } 
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(hitbox.position, hitbox.rotation, hitbox.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }




}
