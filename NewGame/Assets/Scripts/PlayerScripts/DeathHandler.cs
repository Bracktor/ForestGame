using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DeathHandler : MonoBehaviour
{
    [Header("References")]
    public HealthSystem healthSystem;
    public PlayerMovement playerMovement;
    public StaminaSystem staminaSystem;

    [Header("Death Screen")]
    public GameObject deathScreenUI;    // optional: a "You Died" panel
    public float deathDelay = 2f;       // seconds before scene reloads

    private void Start()
    {
        if (healthSystem != null)
            healthSystem.onDeath.AddListener(OnPlayerDied);

        if (deathScreenUI != null)
            deathScreenUI.SetActive(false);
    }

    private void OnPlayerDied()
    {
        Debug.Log("Player died - reloading scene...");

        // Instantly kill all control
        if (playerMovement != null) playerMovement.enabled = false;
        if (staminaSystem != null) staminaSystem.enabled = false;
 
        // Unlock cursor so it doesn't feel stuck
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (deathScreenUI != null)
            deathScreenUI.SetActive(true);

        StartCoroutine(ReloadAfterDelay());
    }

    private IEnumerator ReloadAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
            healthSystem.onDeath.RemoveListener(OnPlayerDied);
    }
}