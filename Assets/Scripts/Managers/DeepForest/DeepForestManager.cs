using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DeepForestManager : LevelManager
{
    public GameObject Reja;
    public float velocidad = 3f;

    private Coroutine moverRejaCoroutine;

    [Header("Spawn Pihuchen")]
    public GameObject pihuchen;
    public Transform puntoDeSpawn;

    [Header("Pantalla de Victoria")]
    [SerializeField] private GameObject victoryScreen;

    private Pihuchen pihuchenBoss;
    private bool isSubscribedToBoss;

    protected override void Start()
    {
        base.Start();
        Debug.Log("Start Del Deep");

        if (victoryScreen != null)
        {
            victoryScreen.SetActive(false);
        }

        TrySubscribeToPihuchen();

        if (DayNightController.Instance != null)
        {
            Destroy(DayNightController.Instance.gameObject);
            Debug.Log("DestruyendoLaNoche (Instance)");
        }
        else
        {
            DayNightController DiaNoche = FindAnyObjectByType<DayNightController>();
            if (DiaNoche != null)
            {
                Destroy(DiaNoche.gameObject);
                Debug.Log("DestruyendoLaNoche (Find)");
            }
            else
            {
                Debug.Log("No hay noche");
            }
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromPihuchen();
    }

    private void TrySubscribeToPihuchen()
    {
        if (isSubscribedToBoss) return;

        Pihuchen targetBoss = pihuchen != null
            ? pihuchen.GetComponentInChildren<Pihuchen>()
            : FindAnyObjectByType<Pihuchen>(FindObjectsInactive.Include);

        if (targetBoss != null)
        {
            pihuchenBoss = targetBoss;
            pihuchenBoss.OnDefeated += HandlePihuchenDefeated;
            isSubscribedToBoss = true;
            Debug.Log($"[DeepForestManager] Suscrito a OnDefeated en '{pihuchenBoss.name}'.", this);
        }
        else
        {
            Debug.LogWarning("[DeepForestManager] No se encontró ningún componente Pihuchen para suscribirse.", this);
        }
    }

    private void UnsubscribeFromPihuchen()
    {
        if (pihuchenBoss != null && isSubscribedToBoss)
        {
            pihuchenBoss.OnDefeated -= HandlePihuchenDefeated;
        }

        isSubscribedToBoss = false;
        pihuchenBoss = null;
    }

    private void HandlePihuchenDefeated()
    {
        Debug.Log("Se LLamó al Handle");
        UnsubscribeFromPihuchen();

        if (victoryScreen != null)
        {
            victoryScreen.SetActive(true);
            Debug.Log("[DeepForestManager] VictoryScreen activada.", this);
        }
        else
        {
            Debug.LogWarning("[DeepForestManager] No se asignó 'victoryScreen' en el Inspector.", this);
        }
    }

    public void TestSwitch()
    {
        if (Reja == null) return;

        if (moverRejaCoroutine != null)
        {
            StopCoroutine(moverRejaCoroutine);
        }

        moverRejaCoroutine = StartCoroutine(MoverRejaHastaY(-11f));
    }

    private IEnumerator MoverRejaHastaY(float targetY)
    {
        Vector3 posicionActual = Reja.transform.position;
        Vector3 posicionDestino = new Vector3(posicionActual.x, targetY, posicionActual.z);

        while (!Mathf.Approximately(Reja.transform.position.y, targetY))
        {
            Reja.transform.position = Vector3.MoveTowards(
                Reja.transform.position,
                posicionDestino,
                velocidad * Time.deltaTime
            );
            yield return null;
        }

        Reja.transform.position = posicionDestino;
        moverRejaCoroutine = null;
    }

    public void InstanciarPihuchen()
    {
        /*if (prefabAInstanciar == null) return;

        Vector3 posicion = puntoDeSpawn != null ? puntoDeSpawn.position : transform.position;
        Debug.Log("Punto de spawn X: " + posicion.x + " Y: " + posicion.y + " Z: " + posicion.z);

        Quaternion rotacion = puntoDeSpawn != null ? puntoDeSpawn.rotation : Quaternion.identity;

        GameObject pihuchenInstanciado = Instantiate(prefabAInstanciar, posicion, rotacion);
        Debug.Log("Instanciado en X: " + pihuchenInstanciado.transform.position.x +
                " Y: " + pihuchenInstanciado.transform.position.y +
                " Z: " + pihuchenInstanciado.transform.position.z);*/
        if (pihuchen != null)
        {
            pihuchen.SetActive(true);
            TrySubscribeToPihuchen();
        }
        Debug.Log("BESTIAAAAAAAAAAA");
    }
}