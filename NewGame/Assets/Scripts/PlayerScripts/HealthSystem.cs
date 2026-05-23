using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("Regeneration")]
    public bool regenEnabled = false;
    public float regenRate = 5f;          // HP per second
    public float regenDelay = 5f;         // seconds after last damage before regen kicks in

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<int> onHealthChanged;   // passes currentHealth

    private float regenTimer;

    private void Awake()
    {
        regenTimer = regenDelay; // don't regen immediately on scene start
    }
    private bool isDead = false;

    private void Update()
    {
        if (!regenEnabled || isDead || currentHealth >= maxHealth) return;

        regenTimer -= Time.deltaTime;
        if (regenTimer <= 0f)
        {
            int regenAmount = Mathf.RoundToInt(regenRate * Time.deltaTime);
            if (regenAmount > 0) Heal(regenAmount);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        regenTimer = regenDelay;

        Debug.Log($"Player took {amount} damage. Current Health: {currentHealth}");
        onHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        Debug.Log($"Player healed {amount} HP. Current Health: {currentHealth}");
        onHealthChanged?.Invoke(currentHealth);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player has died.");
        onDeath?.Invoke();
    }

    public float GetHealthPercent() => (float)currentHealth / maxHealth;

    public bool IsDead() => isDead;

    public void Revive(int healthAmount)
    {
        isDead = false;
        currentHealth = Mathf.Clamp(healthAmount, 1, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }
}