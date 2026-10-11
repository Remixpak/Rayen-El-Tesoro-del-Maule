using UnityEngine;
using TMPro;
public class GameUI: MonoBehaviour
{
    [SerializeField] private CurrentAreaData areaData;
    [SerializeField] private TextMeshProUGUI AreaName;

    private void OnEnable()
    {
        if(areaData != null)
        {
            areaData.OnAreaChanged += ShowAreaName;
        }
        ShowAreaName();
    }

    private void OnDisable()
    {
        if(areaData != null)
        {
            areaData.OnAreaChanged -= ShowAreaName;
        }
        
    }

    public void ShowAreaName()
    {
        AreaName.text = areaData.areaKey;
    }
}