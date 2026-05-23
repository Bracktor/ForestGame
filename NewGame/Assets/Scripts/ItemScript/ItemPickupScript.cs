using UnityEngine;

public class ItemPickupScript : MonoBehaviour
{
    public Item item;

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("The item was touched by: " + other.gameObject.name + " | Tag: " + other.tag);
        
        if (other.CompareTag("Player"))
        {
            // Find the InventoryManager in the scene
            InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();

            if (inventoryManager != null)
            {
                bool added = inventoryManager.AddItem(item);
                if (added)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Inventory full!");
                }
            }
        }
    }
}