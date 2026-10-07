using UnityEngine;

public class GameManager : MonoBehaviour
{
    /*
     el game manager es el que se encarga de gestionar gran parte del juego,
     reconoce la escena, a cual debe pasar etc. 
    controla variables globales como el oro o eventos completados
    por ejemplo cuando se haya terminado el evento del toro o matado a una bestia la instancia del game manager lo sabe.
    De ese modo si necesitamos comprobar algo hacemos GameManager.Instance.("nombre del evento") y nos devuelve true o false.
    eso a grandes rasgos
     */
    public static GameManager Instance { get; private set; }

    //variables de eventos

    private bool NgurifiluCompleted { get; set; }
    private bool PihuchenCompleted { get; set; }
    private bool JhonElderCompleted { get; set; }
    //añadir más según lo planificado pq ahora no me acuerdo, pero aqui se hace

    //variables de control
    private string currentSceneName { get; set; }
    /*
     cada vez que se cargue una escena se actualiza el nombre de la escena actual, de ese modo podemos saber en que escena estamos y que hacer en consecuencia.
    debe hacerse en el respectivo manager de la escena.
    podemos usarla para saber en que parte dibujar la ubicación del jugador en el mapa, o para saber que eventos mostrar en la escena.
     */
    private int gold { get; set; }//cantidad de oro que tiene el jugador, se puede modificar desde cualquier parte del juego y se mantiene entre escenas.




    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    
}
