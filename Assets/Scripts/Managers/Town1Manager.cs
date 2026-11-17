using UnityEngine;

public class Town1Manager : LevelManager
{
    [SerializeField] private GameObject criaPihuchen;
    
    protected override void Start()
    {
        base.Start();

        if (GameManager.Instance.MisionesState == Misiones.BusquedaPihuchen && !GameManager.Instance.cria2Destruida)
        {
            criaPihuchen.SetActive(true);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
