using UnityEngine;

public class RiverManager : LevelManager
{
    [SerializeField] private GameObject novia;
    private bool NoviaisActive;
    
    private void Update()
    {
        
        if(!NoviaisActive && GameManager.Instance.currentTimeOfDay == TimeofDay.Night)
        {
            Debug.Log("De noche activando a la novia");
            novia.SetActive(true);
            NoviaisActive = true;
        }
        if(NoviaisActive && GameManager.Instance.currentTimeOfDay == TimeofDay.Day)
        {
            Debug.Log("De dia desactivando a la novia");
            novia.SetActive(false);
            NoviaisActive = false;
        }
    }

    void OnEnable()
    {
        
        novia.GetComponent<Novia>().UpdateConversation();
        Novia.OnHabladoConNovia += ProcesarDialogoNovia;
    }
    private void OnDisable()
    {
        // Aquí lo desconectas cuando se destruye o desactiva RiverManager
        Novia.OnHabladoConNovia -= ProcesarDialogoNovia;
    }

    private void ProcesarDialogoNovia(ConversationTemplate conversacion)
    {
        Debug.Log($"[RiverManager] ¡Evento recibido! Estado actual de misión: {GameManager.Instance.MisionesState}");
        switch (GameManager.Instance.MisionesState)
        {
            case Misiones.guadridaCuatreros:
                // Al terminar este diálogo, pasa a BusquedaPihuchen y avisa al GameManager/Diario
                GameManager.Instance.ActualizarEstadoMision(Misiones.BusquedaPihuchen);
                Debug.Log("LLamando para desbloquear la entrada 1");
                GameManager.Instance.DesbloquearEntradaDiario("1");
                break;

            case Misiones.BusquedaPihuchen:
                // Si hay una entrada intermedia mientras buscas el Pihuchen, puedes disparar un evento o flag aquí
                GameManager.Instance.ActualizarEstadoMision(Misiones.BusquedaPihuchen);
                GameManager.Instance.DesbloquearEntradaDiario("2");
                break;

            case Misiones.MisionPihuchen:
                // Ejemplo: entrada final del acceso al templo
                GameManager.Instance.ActualizarEstadoMision(Misiones.MisionPihuchen);
                GameManager.Instance.DesbloquearEntradaDiario("3");
                break;
        }

        novia.GetComponent<Novia>().UpdateConversation();
    }

}
