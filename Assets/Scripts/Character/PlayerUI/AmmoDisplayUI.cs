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

    private void Start()
    {
        if (stats == null || container == null || bulletPrefab == null)
        {
            Debug.LogError("AmmoDisplayUI: faltan referencias (Stats, Container o Bullet Prefab).", this);
            enabled = false;
            return;
        }

        fullColor = bulletPrefab.color;
        stats.AmmoChanged += Refresh;
        Refresh(stats.Ammo, stats.MaxAmmo);
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