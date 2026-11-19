using UnityEngine;

// logica de inventario de llaves del jugador
public class KeyInventory : MonoBehaviour
{
    public int KeysAdquired
    {
        get { return GameManager.Data.keys; }
        set { GameManager.Data.keys = Mathf.Max(0, value); } //nos aseguramos que no tengamos llaves en cantidades negativas
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddKey()
    {
        KeysAdquired++;
    }

    public void RemoveKey()
    {
        KeysAdquired--;
    }

    public bool HasKey()
    {
        return KeysAdquired > 0;
    }
}