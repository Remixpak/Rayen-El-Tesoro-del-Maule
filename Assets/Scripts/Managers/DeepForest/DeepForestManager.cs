using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DeepForestManager : LevelManager
{
    public GameObject Reja;
    public float velocidad = 3f;

    private Coroutine moverRejaCoroutine;

    [Header("Spawn Pihuchen")]
    public GameObject prefabAInstanciar;
    public Transform puntoDeSpawn;

   

    protected override void Start()
    {
        base.Start();
        DayNightController DiaNoche = FindAnyObjectByType<DayNightController>();
        if(DiaNoche != null)
        {
            Destroy(DiaNoche.gameObject);
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
        Quaternion rotacion = puntoDeSpawn != null ? puntoDeSpawn.rotation : Quaternion.identity;

        Instantiate(prefabAInstanciar, posicion, rotacion);*/
        Debug.Log("BESTIAAAAAAAAAAA");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
}