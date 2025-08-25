using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryUI;    // Assign in inspector
    public PlayerMovement playerMovement;
    public InventoryManager inventoryManager;
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
        Item receivedItem = inventoryManager.GetSelectedItem(true);
        Debug.Log("No item received!");
        if (receivedItem != null)
        {
            Debug.Log("Received item: " + receivedItem);

        }
        else
        {
            Debug.Log("No item received!");
        }

    }
    
    
}