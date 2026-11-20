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

    private void TryInitialize()
    {
        KeyInventory targetInventory = keyInventory;

        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            KeyInventory gmInventory = GameManager.Instance.player.GetComponent<KeyInventory>();
            if (gmInventory != null)
            {
                targetInventory = gmInventory;
            }
        }

        if (targetInventory != keyInventory)
        {
            keyInventory = targetInventory;
            lastKeyCount = -1;
            isInitialized = false;
        }

        if (isInitialized) return;
        if (keyInventory == null || keyText == null) return;

        lastKeyCount = keyInventory.KeysAdquired;
        RefreshText(lastKeyCount);
        isInitialized = true;
    }

    private void Update()
    {
        TryInitialize();

        if (!isInitialized || keyInventory == null)
        {
            return;
        }

        int currentKeys = keyInventory.KeysAdquired;
        if (currentKeys != lastKeyCount)
        {
            lastKeyCount = currentKeys;
            RefreshText(currentKeys);
        }
    }

    private void RefreshText(int keys)
    {
        keyText.text = string.Format(format, keys);
    }
}