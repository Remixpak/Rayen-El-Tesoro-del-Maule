using UnityEngine;
public class AreaInit: MonoBehaviour
{
    /*
    El areaInit tiene como única responsabilidad cambiar la info del scriptable object 
    le di este enfoque pq al tener un scriptableObj todos los sistemas apuntan a un solo archivo global
    por ejemplo el sistema que dibuje el punto de la ubicacion en el mapa o el sistema que muestre el nombre del area en la UI 
    apunta al scriptableObj Current area data.
    eso es mejor que preguntar al script de level manager de cada zona por que cada level manager es un script distinto
    */
    [SerializeField] private CurrentAreaData currentArea;//data del area como scriptableobj para que todos los sistemas apunten a un solo archivo
    [SerializeField] private string areaKey;//nombre del area (se lo asignamos en el inspector)

    private void Awake()//cada que se cargue una escena se sobreescribe la data
    {
        if(currentArea != null)
        {
            currentArea.SetCurrentArea(areaKey);
        }
    }
}