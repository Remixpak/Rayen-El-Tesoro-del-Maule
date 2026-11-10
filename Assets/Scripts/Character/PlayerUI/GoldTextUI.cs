using TMPro;
using UnityEngine;

public class GoldTextUI : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private string format = "{0}"; // por ejemplo "Oro: {0}" (debe conservar el {0})

    private void Start()
    {
        if (stats == null || goldText == null)
        {
            Debug.LogError("GoldTextUI: faltan referencias (Stats o Gold Text).", this);
            enabled = false;
            return;
        }

        stats.GoldChanged += Refresh;
        Refresh(stats.Gold);
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.GoldChanged -= Refresh;
        }
    }

    private void Refresh(int gold)
    {
        goldText.text = string.Format(format, gold);
    }
}