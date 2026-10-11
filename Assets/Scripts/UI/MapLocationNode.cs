using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapLocationNode : MonoBehaviour
{
    /*
    Cada locacion tiene este script que cambia su color dependiendo si estan en esta zona o no
    
    
    */
    [Header("Identificador de esta Ubicación")]
    public string areaKey; 

    [Header("Componentes Visuales")]
    [SerializeField] private Image locationIcon;   
    [SerializeField] private TextMeshProUGUI locationText; 

    [Header("Colores")]
    [SerializeField] private Color defaultColor = Color.green; 
    [SerializeField] private Color activeColor = Color.cyan;   

    // Método que activa o desactiva el resaltado de este nodo
    public void SetHighlight(bool isActive)
    {
        Color targetColor = isActive ? activeColor : defaultColor;

        if (locationIcon != null)
        {
            locationIcon.color = targetColor;
        }

        if (locationText != null)
        {
            locationText.color = targetColor;
        }
    }
}