using System.Collections;
using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [SerializeField] private CurrentAreaData areaData;
    [SerializeField] private TextMeshProUGUI AreaName;
    [SerializeField] private GameObject Panel;

    private Coroutine hidePanelCoroutine;

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
        // Actualizamos el texto si las referencias son válidas
        if (areaData != null && AreaName != null)
        {
            AreaName.text = areaData.areaKey;
        }

        // Si el panel está asignado, lo activamos y gestionamos el tiempo
        if (Panel != null)
        {
            Panel.SetActive(true);

            // Si ya hay una corrutina corriendo, la detenemos para reiniciar el contador de 2 segundos
            if (hidePanelCoroutine != null)
            {
                StopCoroutine(hidePanelCoroutine);
            }

            hidePanelCoroutine = StartCoroutine(HidePanelAfterDelay(2f));
        }
    }

    private IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (Panel != null)
        {
            Panel.SetActive(false);
        }

        hidePanelCoroutine = null;
    }
}