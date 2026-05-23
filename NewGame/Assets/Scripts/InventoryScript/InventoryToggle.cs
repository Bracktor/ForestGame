using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryUI;    // Assign in inspector
    public PlayerMovement playerMovement;
    public InventoryManager inventoryManager;
    public GameObject player;         // Assign the Player GameObject in inspector
    private bool isInventoryOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            toggleInventory();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            UseSelectedItem();
        }
    }

    public void toggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
            inventoryUI.SetActive(isInventoryOpen);

            if (isInventoryOpen)
            {
                playerMovement.canMove = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                playerMovement.canMove = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
    }

    public void UseSelectedItem()
    {
        Debug.Log("UseSelectedItem called");

        Item receivedItem = inventoryManager.GetSelectedItem(true);
        
        if (receivedItem != null)
        {
            GameObject target = player != null ? player : gameObject;
            Debug.Log($"Using item: {receivedItem.itemName} on {target.name}");
            receivedItem.Use(target);
        }
        else
        {
            Debug.LogWarning("No item in selected slot!");
        }
    }
    
    
}