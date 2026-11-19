using TMPro;
using UnityEngine;

//visual de las llaves en la UI
public class KeyUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private KeyInventory keyInventory;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private string format = "x{0}";

    private int lastKeyCount = -1;
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
            return;
        }

        // Si el numero de llaves cambio, actualizamos el texto
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

        if (keyInventory == null && GameManager.Instance != null && GameManager.Instance.player != null)
        {
            keyInventory = GameManager.Instance.player.GetComponent<KeyInventory>();
        }

        if (keyInventory == null || keyText == null)
        {
            return;
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