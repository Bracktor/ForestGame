using UnityEngine;
using System.Collections;

public class SpiderCaptureSystem : MonoBehaviour
{
    [Header("Capture Settings")]
    public float damagePerSecond = 10f;
    public float cameraTransitionSpeed = 5f;
    public float captureDelay = 1.5f;       // seconds for camera to reach spider before damage/QTE start

    [Header("QTE Settings")]
    public KeyCode[] keySequence = { KeyCode.E, KeyCode.R, KeyCode.Q };
    public float inputWindow = 1.5f;            // seconds player has to hit each key

    [Header("References")]
    public Camera playerCamera;
    public QTEDisplay qteDisplay;

    // Runtime
    private GameObject capturedPlayer;
    private UndergroundSpider spider;
    private HealthSystem playerHealth;
    private PlayerMovement playerMovement;
    private StaminaSystem playerStamina;

    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;

    private bool isCaptured = false;
    private Coroutine damageCoroutine;
    private Coroutine qteCoroutine;
    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;
    private Transform originalCameraParent;

    public void StartCapture(GameObject player, UndergroundSpider spiderRef)
    {
        if (isCaptured) return;

        isCaptured = true;
        capturedPlayer = player;
        spider = spiderRef;

        playerHealth = player.GetComponent<HealthSystem>();
        playerMovement = player.GetComponent<PlayerMovement>();
        playerStamina = player.GetComponent<StaminaSystem>();

        // Lock player controls
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerStamina != null) playerStamina.enabled = false;

        // Store exact local position before detaching
        originalCameraPos = playerCamera.transform.localPosition;
        originalCameraRot = playerCamera.transform.localRotation;
        originalCameraParent = playerCamera.transform.parent;

        StartCoroutine(TransitionCameraToSpider());

        // Wait for camera to arrive before starting damage and QTE
        StartCoroutine(BeginCaptureSequence());

        Debug.Log("Player captured by spider!");
    }

    private IEnumerator BeginCaptureSequence()
    {
        // Let camera travel to spider face first
        yield return new WaitForSeconds(captureDelay);

        if (!isCaptured) yield break; // player died during transition

        damageCoroutine = StartCoroutine(DamageLoop());
        qteCoroutine = StartCoroutine(RunQTE());
    }

    private IEnumerator TransitionCameraToSpider()
    {
        // Detach camera from player
        playerCamera.transform.SetParent(null);

        // Disable near clip culling so underground doesn't get clipped
        float originalNearClip = playerCamera.nearClipPlane;
        playerCamera.nearClipPlane = 0.01f;

        Transform face = spider.facePoint;
        float t = 0f;
        Vector3 startPos = playerCamera.transform.position;
        Quaternion startRot = playerCamera.transform.rotation;

        // Hard transition over captureDelay seconds
        while (t < 1f && isCaptured)
        {
            t += Time.deltaTime * cameraTransitionSpeed;
            playerCamera.transform.position = Vector3.Lerp(startPos, face.position, t);
            playerCamera.transform.rotation = Quaternion.Lerp(startRot, face.rotation, t);
            yield return null;
        }

        // Snap exactly to face and lock there
        while (isCaptured)
        {
            playerCamera.transform.position = face.position;
            playerCamera.transform.rotation = face.rotation;
            yield return null;
        }

        playerCamera.nearClipPlane = originalNearClip;
    }

    private float damageAccumulator = 0f;

    private IEnumerator DamageLoop()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("SpiderCaptureSystem: No HealthSystem found on captured player!");
            yield break;
        }

        damageAccumulator = 0f;

        while (isCaptured)
        {
            damageAccumulator += damagePerSecond * Time.deltaTime;

            if (damageAccumulator >= 1f)
            {
                int damage = Mathf.FloorToInt(damageAccumulator);
                playerHealth.TakeDamage(damage);
                damageAccumulator -= damage;
            }

            yield return null;
        }
    }

    private IEnumerator RunQTE()
    {
        if (qteDisplay != null)
            qteDisplay.ShowSequence(keySequence);

        int currentStep = 0;
        float stepTimer = inputWindow;

        while (currentStep < keySequence.Length)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                // Timed out on this key — restart sequence
                currentStep = 0;
                stepTimer = inputWindow;
                if (qteDisplay != null) qteDisplay.ResetSequence();
                Debug.Log("QTE timed out, restarting sequence");
            }

            if (Input.GetKeyDown(keySequence[currentStep]))
            {
                if (qteDisplay != null) qteDisplay.HighlightStep(currentStep);
                currentStep++;
                stepTimer = inputWindow; // reset timer on correct key
                Debug.Log($"QTE step {currentStep}/{keySequence.Length} correct!");
            }

            yield return null;
        }

        // Sequence complete — release player
        ReleasePlayer();
    }

    private void ReleasePlayer()
    {
        if (!isCaptured) return;
        isCaptured = false;

        if (damageCoroutine != null) StopCoroutine(damageCoroutine);

        // Return camera to player
        playerCamera.transform.SetParent(originalCameraParent);
        StartCoroutine(TransitionCameraBack());

        // Restore controls
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerStamina != null) playerStamina.enabled = true;

        if (qteDisplay != null) qteDisplay.Hide();

        spider.ReleasePlayer();

        Debug.Log("Player escaped the spider!");
    }

    private IEnumerator TransitionCameraBack()
    {
        float t = 0f;
        Vector3 startPos = playerCamera.transform.position;
        Quaternion startRot = playerCamera.transform.rotation;

        // Reparent first so localPosition math works correctly
        playerCamera.transform.SetParent(originalCameraParent);

        while (t < 1f)
        {
            t += Time.deltaTime * cameraTransitionSpeed;
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition, originalCameraPos, t);
            playerCamera.transform.localRotation = Quaternion.Lerp(
                playerCamera.transform.localRotation, originalCameraRot, t);
            yield return null;
        }

        // Snap exactly back to original
        playerCamera.transform.localPosition = originalCameraPos;
        playerCamera.transform.localRotation = originalCameraRot;
    }
}