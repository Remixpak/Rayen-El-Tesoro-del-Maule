using UnityEngine;
public class GraveYardManager : LevelManager
{
    [SerializeField] private GameObject criaPihuchen;
    
    protected override void Start()
    {
        base.Start();
        if(GameManager.Instance.MisionesState == Misiones.BusquedaPihuchen && !GameManager.Instance.cria1Destruida)
        {
            criaPihuchen.SetActive(true);
        }
    }
}