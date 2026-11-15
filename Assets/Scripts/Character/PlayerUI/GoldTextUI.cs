using TMPro;
using UnityEngine;

public class GoldTextUI : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private string format = "{0}"; // por ejemplo "Oro: {0}" (debe conservar el {0})

    private bool isInitialized;

    private void Start()
    {
        TryInitialize();
    }

    private void Update()
    {
        if (!isInitialized)
        {
            TryInitialize();
        }
    }

    private void TryInitialize()
    {
        if (isInitialized) return;

        if (stats == null && GameManager.Instance != null && GameManager.Instance.player != null)
        {
            stats = GameManager.Instance.player.GetComponent<PlayerStats>();
        }

        if (stats == null || goldText == null)
        {
            return;
        }

        stats.GoldChanged += Refresh;
        Refresh(stats.Gold);
        isInitialized = true;
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