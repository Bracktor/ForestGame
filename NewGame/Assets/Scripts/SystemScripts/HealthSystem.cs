using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public int currentHealth = 100;

    public void Heal(int amount)
    {
        currentHealth += amount;
        Debug.Log("Player healed! Current Health: " + currentHealth);
    }
}