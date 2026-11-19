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
    }

}