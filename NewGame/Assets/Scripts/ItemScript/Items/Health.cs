using UnityEngine;

[CreateAssetMenu(fileName = "NewPotion", menuName = "Inventory/Potion")]
public class PotionItem : Item
{
    public int healAmount = 50;

    public override void Use(GameObject user)
    {
        var health = user.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.Heal(healAmount);
            Debug.Log($"{itemName} healed {healAmount} HP");
        }
        else
        {
            Debug.LogWarning($"{itemName}: No HealthSystem found on {user.name}!");
        }
    }
}