using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public InventoryManager inventoryManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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