using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplayUI : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Transform container;   // objeto con Horizontal Layout Group donde se crean las balas
    [SerializeField] private Image bulletPrefab;    // prefab de una bala (Image con su sprite)

    [Header("Opciones")]
    [SerializeField] private bool showEmptySlots = false;                         // false = las balas gastadas desaparecen; true = quedan atenuadas
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.25f);     // color de las balas gastadas si showEmptySlots esta activo

    private readonly List<Image> bullets = new List<Image>();
    private Color fullColor = Color.white;
    private bool isInitialized;

    

    private void Start()
    {
        TryInitialize();
    }

    private void Update()
    {
        TryInitialize();
    }
    private PlayerStats boundStats;
    private void TryInitialize()
    {
        PlayerStats targetStats = stats;

        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            PlayerStats gmStats = GameManager.Instance.player.GetComponent<PlayerStats>();
            if (gmStats != null)
            {
                targetStats = gmStats;
            }
        }

        if (targetStats != boundStats)
        {
            if (boundStats != null)
            {
                boundStats.AmmoChanged -= Refresh;
            }

            stats = targetStats;
            boundStats = targetStats;

            if (boundStats != null)
            {
                boundStats.AmmoChanged += Refresh;
                isInitialized = false;
            }
        }

        if (isInitialized) return;
        if (stats == null || container == null || bulletPrefab == null) return;

        fullColor = bulletPrefab.color;
        Refresh(stats.Ammo, stats.MaxAmmo);
        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.AmmoChanged -= Refresh;
        }
    }

    private void Refresh(int current, int max)
    {
        // si cambia la capacidad maxima, se vuelven a crear las casillas
        if (bullets.Count != max)
        {
            Rebuild(max);
        }

        for (int i = 0; i < bullets.Count; i++)
        {
            bool filled = i < current;

            if (showEmptySlots)
            {
                bullets[i].gameObject.SetActive(true);
                bullets[i].color = filled ? fullColor : emptyColor;
            }
            else
            {
                bullets[i].gameObject.SetActive(filled); // las balas gastadas desaparecen de derecha a izquierda
                bullets[i].color = fullColor;
            }
        }
    }

    private void Rebuild(int count)
    {
        foreach (Image bullet in bullets)
        {
            bullet.gameObject.SetActive(false); // deja de contar en el layout al instante
            Destroy(bullet.gameObject);
        }

        bullets.Clear();

        for (int i = 0; i < count; i++)
        {
            bullets.Add(Instantiate(bulletPrefab, container));
        }
    }
}