using UnityEngine;

public class DebugDamage : MonoBehaviour
{
    public HealthSystem healthSystem;
    public int damageAmount = 10;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            healthSystem.TakeDamage(damageAmount);
            Debug.Log($"Debug: dealt {damageAmount} damage. HP now {healthSystem.currentHealth}");
        }
    }
}