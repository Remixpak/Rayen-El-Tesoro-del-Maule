using UnityEngine;

public class KeyInventory : MonoBehaviour
{

    public int KeysAdquired = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
