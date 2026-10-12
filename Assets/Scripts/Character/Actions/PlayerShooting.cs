using System.Collections;
using UnityEngine;


//basicamente este script se encarga del manejo del disparo del arma del jugador con su logica 
public class PlayerShooting : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] private float damage = 5f; // daño que hace el arma
    [SerializeField] private float range = 40f; // rango del arma
    [SerializeField] private float fireCooldown = 0.25f; //cadencia de disparo
    [SerializeField] private LayerMask hitMask = ~0; //capas que puede golpear
    [SerializeField] private Transform muzzle; // punta del arma 

    [Header("Disparo visual")]
    [SerializeField] private LineRenderer shoot; //linea de disparo que se dibuja 
    [SerializeField] private float shootDuration = 0.05f; // en cuanto tiempo se va el disparo

    [Header("Animacion(no tenemos aun)")]
    [SerializeField] private bool useAnimationEvents = false; // si es true se ejecuta la animacion
    [SerializeField] private Animator animator;
    [SerializeField] private string shootTrigger = "Shoot";


    private TargetLock targetLock; // si hay un enemigo fijado el disparo va hacia el
    private float nextFireTime;// momento en el que se puede volver a disparar 
    private int triggerHash; // hash del trigger animator 
    private Coroutine shotRoutine; //referencia a la corrutina que oculta la linea de disparo



    private void Awake()
    {
        targetLock = GetComponent<TargetLock>(); //referencia al target para apuntar el disparo
        triggerHash = Animator.StringToHash(shootTrigger);//calculamos el hash del disparo una sola vez 


        //configuramos el line renderer para que dibuje en las coordenadas del mundo el disparo
        if (shoot != null)
        {
            shoot.useWorldSpace = true;
            shoot.positionCount = 2;
            shoot.enabled = false; //empieza oculta hasta que se dispare 
        }
    }

    //funcion que se llama desde playerweapon cuando el jugador ataca con el disparo
    public void TryShoot()
    {
        //si aun no pasa el cooldwown no se dispara 
        if(Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + fireCooldown; // actualizamos el momento para el proximo disparo

        if(animator != null)
        {
            animator.SetTrigger(triggerHash);
        }

        if (!useAnimationEvents)
        {
            FireShot();
        }
    }

    //funcion que hace que el disparo se dispare 
    public void FireShot()
    {
        //si no hay muzzle (mira que esta en la punta del arma) no se dispara 
        if(muzzle == null)
        {
            Debug.LogError("Falta asignar el Muzzle en PlayerShooting.", this);
            return;
        }

        Vector3 origin = muzzle.position;//punto desde donde se deispara 
        Vector3 direction = GetAimDirection(origin); //direccion de disparo
        Vector3 endPoint = origin + direction * range;// punto final del disparo

        //lanzamos un raycast para ver si el disparo golpea algo 
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;

            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null) target.TakeDamage(damage, direction);
        }
        ShowShoot(origin, endPoint);//dibuamos la linea de disparo
    }

    //funcion para ver la direccion del disparo
    private Vector3 GetAimDirection(Vector3 origin)
    {
        if (targetLock != null && targetLock.IsLocked) // si hay un enemigo fijado el disparo se va hacia el
            return (targetLock.AimPoint - origin).normalized;

        return muzzle.forward; // si no se dispara desde el muzzle 
    }

    //funcion que dibuja la linea del disparo 
    private void ShowShoot(Vector3 start, Vector3 end)
    {
        if (shoot == null) return;

        //colocamos los dos puntos de la linea de disparo
        shoot.SetPosition(0, start);
        shoot.SetPosition(1, end);
        shoot.enabled = true;

        if (shotRoutine != null) StopCoroutine(shotRoutine);
        shotRoutine = StartCoroutine(HideShoot());
    }

    //corrutina que oculta la linea de disparo luego de ser ejecutado
    private IEnumerator HideShoot()
    {
        yield return new WaitForSeconds(shootDuration);
        shoot.enabled = false;
    }

    //dibuja el rango de disparo en el editor 
    private void OnDrawGizmosSelected()
    {
        if (muzzle == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(muzzle.position, muzzle.forward * range);
    }


}
