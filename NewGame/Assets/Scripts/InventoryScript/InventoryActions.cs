using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryActions : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Item[] itemsToPickup;
    

    public void PickupItem(int id)
    {
        bool result = inventoryManager.AddItem(itemsToPickup[id]);
        if (result == true)
        {
            Debug.Log("Item added");
        }
        else
        {
            Debug.Log("Item not addded Inventory Full!");
        }
    }


    public void UseSelectedItem()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Item receivedItem = inventoryManager.GetSelectedItem(true);
            Debug.Log("No item received!");
            if (receivedItem != null)
            {
                receivedItem.Use(gameObject);
                Debug.Log("Received item: " + receivedItem);

            }
            else
            {
                Debug.Log("No item received!");
            }
        }
    }
}