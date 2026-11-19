using TMPro;
using UnityEngine;

public class KeyHUD : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private KeyInventory keyInventory;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private string format = "x{0}"; // Por ejemplo "Llaves: {0}" o "x{0}"

    private int lastKeyCount = -1;
    private bool isInitialized;

    private void Start()
    {
        TryInitialize();
    }

    private void Update()
    {
        // Si aún no se ha inicializado (porque el jugador apareció tarde), lo intentamos de nuevo
        if (!isInitialized)
        {
            TryInitialize();
            return;
        }

        // Si el número de llaves cambió, actualizamos el texto
        int currentKeys = keyInventory.KeysAdquired;

        if (currentKeys != lastKeyCount)
        {
            lastKeyCount = currentKeys;
            RefreshText(currentKeys);
        }
    }

    private void TryInitialize()
    {
        if (isInitialized) return;

        // Si no tenemos la referencia en el Inspector, la buscamos en el jugador
        if (keyInventory == null && GameManager.Instance != null && GameManager.Instance.player != null)
        {
            keyInventory = GameManager.Instance.player.GetComponent<KeyInventory>();
        }

        if (keyInventory == null || keyText == null)
        {
            return; // Faltan referencias, esperamos al siguiente frame
        }

        // Inicializamos con el valor actual
        lastKeyCount = keyInventory.KeysAdquired;
        RefreshText(lastKeyCount);

        isInitialized = true;
    }

    private void RefreshText(int keys)
    {
        keyText.text = string.Format(format, keys);
    }
}