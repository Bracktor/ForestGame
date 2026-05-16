using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite image;
    [TextArea] public string description;
    public ItemType itemType;
    public bool stackable;

    public virtual void Use(GameObject user)
    {
        Debug.Log($"Used {itemName}");
    }
}

public enum ItemType { Weapon, Consumable, KeyItem, Equipment }