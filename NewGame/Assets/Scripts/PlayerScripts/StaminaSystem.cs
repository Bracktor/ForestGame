using UnityEngine;
using UnityEngine.Events;
 
public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
 
    [Header("Drain")]
    public float sprintDrainRate = 20f;     // stamina per second while sprinting
    public float jumpCost = 10f;            // flat cost per jump
 
    [Header("Regeneration")]
    public float regenRate = 15f;           // stamina per second while not sprinting
    public float regenDelay = 1.5f;         // seconds after last drain before regen starts
 
    [Header("Exhaustion")]
    public float exhaustionThreshold = 10f; // below this, player can't sprint until fully recharged
    private bool isExhausted = false;
 
    [Header("Events")]
    public UnityEvent onExhausted;
    public UnityEvent onRecovered;
    public UnityEvent<float> onStaminaChanged;  // passes currentStamina (0–maxStamina)
 
    private float regenTimer = 0f;
    private PlayerMovement playerMovement;
 
    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
            Debug.LogWarning("StaminaSystem: No PlayerMovement found on this GameObject.");
    }
 
    private void Update()
    {
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && playerMovement != null && playerMovement.canMove;
        bool isMoving = playerMovement != null &&
                        (Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f ||
                         Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f);
 
        bool isSprinting = wantsToSprint && isMoving && !isExhausted && currentStamina > 0f;
 
        // Tell PlayerMovement whether sprinting is actually allowed
        if (playerMovement != null)
            playerMovement.canSprint = !isExhausted && currentStamina > 0f;
 
        if (isSprinting)
        {
            DrainStamina(sprintDrainRate * Time.deltaTime);
        }
        else
        {
            regenTimer -= Time.deltaTime;
            if (regenTimer <= 0f)
            {
                RegenerateStamina(regenRate * Time.deltaTime);
            }
        }
 
        // Exhaustion: can't sprint again until stamina is fully restored
        if (isExhausted && currentStamina >= maxStamina)
        {
            isExhausted = false;
            onRecovered?.Invoke();
            Debug.Log("Stamina recovered.");
        }
    }
 
    public void DrainStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina - amount, 0f, maxStamina);
        regenTimer = regenDelay;
        onStaminaChanged?.Invoke(currentStamina);
 
        if (currentStamina <= 0f && !isExhausted)
        {
            isExhausted = true;
            onExhausted?.Invoke();
            Debug.Log("Player is exhausted!");
        }
    }
 
    public void UseJumpStamina()
    {
        if (currentStamina >= jumpCost)
        {
            DrainStamina(jumpCost);
        }
    }
 
    private void RegenerateStamina(float amount)
    {
        if (currentStamina >= maxStamina) return;
        currentStamina = Mathf.Clamp(currentStamina + amount, 0f, maxStamina);
        onStaminaChanged?.Invoke(currentStamina);
    }
 
    public float GetStaminaPercent() => currentStamina / maxStamina;
 
    public bool IsExhausted() => isExhausted;
    public bool CanSprint() => !isExhausted && currentStamina > 0f;
}