using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Image fillImage;         // Image con Image Type = Filled
    [SerializeField] private float fillSpeed = 1.5f;  // fraccion de barra por segundo (0 = instantanea)

    private float targetFill;
    private float currentFill;

    private void Start()
    {
        if (stats == null || fillImage == null)
        {
            Debug.LogError("HealthBarUI: faltan referencias (Stats o Fill Image).", this);
            enabled = false;
            return;
        }

        if (fillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning("HealthBarUI: Fill Image debe tener Image Type = Filled (y un Source Image asignado).", this);
        }

        stats.HealthChanged += OnHealthChanged;

        targetFill = GetFraction(stats.Health, stats.MaxHealth);
        currentFill = targetFill;
        fillImage.fillAmount = currentFill;
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.HealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        targetFill = GetFraction(current, max);
    }

    private void Update()
    {
        if (Mathf.Approximately(currentFill, targetFill))
        {
            return;
        }

        if (fillSpeed <= 0f)
        {
            currentFill = targetFill;
        }
        else
        {
            currentFill = Mathf.MoveTowards(currentFill, targetFill, fillSpeed * Time.unscaledDeltaTime);
        }

        fillImage.fillAmount = currentFill;
    }

    private static float GetFraction(int current, int max)
    {
        if (max <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp01((float)current / max);
    }
}