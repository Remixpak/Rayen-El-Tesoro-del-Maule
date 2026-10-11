using UnityEngine;

public class MapUI : MonoBehaviour
{
    [Header("Referencia al Estado del Área")]
    [SerializeField] private CurrentAreaData currentArea;

    [Header("Lista de Nodos en el Mapa")]
    [SerializeField] private MapLocationNode[] locationNodes;

    private void Awake()
    {
        
        if (locationNodes == null || locationNodes.Length == 0)
        {
            locationNodes = GetComponentsInChildren<MapLocationNode>(true);
        }
    }

    private void OnEnable()
    {
        if (currentArea != null)
        {
            currentArea.OnAreaChanged += HighlightCurrentLocation;
        }

       
        HighlightCurrentLocation();
    }

    private void OnDisable()
    {
        if (currentArea != null)
        {
            currentArea.OnAreaChanged -= HighlightCurrentLocation;
        }
    }

    public void HighlightCurrentLocation()
    {
        if (currentArea == null) return;

        string currentKey = currentArea.areaKey;

        
        foreach (var node in locationNodes)
        {
            if (node != null)
            {
                bool isCurrentArea = node.areaKey.Equals(currentKey, System.StringComparison.OrdinalIgnoreCase);
                node.SetHighlight(isCurrentArea);
            }
        }
    }
}