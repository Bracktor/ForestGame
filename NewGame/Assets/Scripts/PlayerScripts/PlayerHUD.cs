using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    public HealthSystem healthSystem;
    public StaminaSystem staminaSystem;

    [Header("Health Bar")]
    public Image healthBarFill;
    public Text healthText;             // optional: shows "75 / 100"

    [Header("Stamina Bar")]
    public Image staminaBarFill;
    public Text staminaText;            // optional: shows "80 / 100"

    [Header("Exhaustion Flash")]
    public Image staminaBarBackground;  // optional: flashes red when exhausted
    public Color exhaustedColor = new Color(0.6f, 0f, 0f, 1f);
    public Color normalColor   = new Color(0.2f, 0.2f, 0.2f, 1f);

    void Update()
    {
        if (healthSystem != null)
        {
            float hp = healthSystem.GetHealthPercent();
            if (healthBarFill != null) healthBarFill.fillAmount = hp;
            if (healthText != null)
                healthText.text = $"{healthSystem.currentHealth} / {healthSystem.maxHealth}";
        }

        if (staminaSystem != null)
        {
            float st = staminaSystem.GetStaminaPercent();
            if (staminaBarFill != null) staminaBarFill.fillAmount = st;
            if (staminaText != null)
                staminaText.text = $"{Mathf.RoundToInt(staminaSystem.currentStamina)} / {Mathf.RoundToInt(staminaSystem.maxStamina)}";

            if (staminaBarBackground != null)
                staminaBarBackground.color = staminaSystem.IsExhausted() ? exhaustedColor : normalColor;
        }
    }
}