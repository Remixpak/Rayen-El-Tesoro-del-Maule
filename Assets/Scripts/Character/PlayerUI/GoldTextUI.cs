using System.ComponentModel;
using TMPro;
using UnityEngine;

public class GoldTextUI : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private string format = "{0}"; // por ejemplo "Oro: {0}" (debe conservar el {0})

    private bool isInitialized;

    private PlayerStats boundStats;

    private void Start()
    {
        TryInitialize();
    }

    private void Update()
    {
        TryInitialize();
    }

    private void TryInitialize()
    {
        PlayerStats targetStats = stats;
        //if (isInitialized) return;

        if (stats == null && GameManager.Instance != null && GameManager.Instance.player != null)
        {
            PlayerStats gmstats = GameManager.Instance.player.GetComponent<PlayerStats>();
            if(gmstats != null)
            {
                targetStats = gmstats;
            }
        }
        if(targetStats != boundStats)
        {
            if(boundStats != null)
            {
                boundStats.GoldChanged -= Refresh;
                
            }
            stats = targetStats;
            boundStats = targetStats;
            if(boundStats != null)
            {
                boundStats.GoldChanged += Refresh;
                isInitialized = false;
            }
        }

        if(isInitialized) return;
        if(stats == null) return;

        Refresh(stats.Gold);
        isInitialized = true;


        /*if (stats == null || goldText == null)
        {
            return;
        }

        stats.GoldChanged += Refresh;
        Refresh(stats.Gold);
        isInitialized = true;*/
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