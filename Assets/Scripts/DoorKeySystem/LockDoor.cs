using UnityEngine;

public class LockDoor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    KeyInventory playerKeyInventory;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            playerKeyInventory = other.GetComponent<KeyInventory>();
            if(playerKeyInventory.HasKey())
            {
                playerKeyInventory.RemoveKey();
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Player")
        {
            playerKeyInventory = collision.collider.GetComponent<KeyInventory>();
            if (playerKeyInventory.HasKey())
            {
                playerKeyInventory.RemoveKey();
                Destroy(gameObject);
            }
        }
    }
}
