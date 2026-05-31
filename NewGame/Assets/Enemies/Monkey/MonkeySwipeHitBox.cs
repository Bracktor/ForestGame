using UnityEngine;

/// <summary>
/// Attach to the child GameObject that holds the swipe trigger collider.
/// MonkeyStalker enables/disables this collider during the swipe window.
///
/// Setup:
///   1. Create a child GameObject on the Monkey named "SwipeHitbox"
///   2. Add a Collider set to Is Trigger
///   3. Add this script
///   4. Assign the damage amount (or leave at default — MonkeyStalker.swipeDamage is preferred)
///   5. Drag this child's Collider into MonkeyStalker.swipeHitbox in the Inspector
/// </summary>
public class MonkeySwipeHitbox : MonoBehaviour
{
    [Tooltip("Damage dealt per swipe. Should match MonkeyStalker.swipeDamage.")]
    public int swipeDamage = 20;

    [Tooltip("Knockback force applied to the player on hit (set 0 to disable).")]
    public float knockbackForce = 5f;

    private bool hasHitThisSwipe = false;   // prevent multiple hits per swing

    private void OnEnable()
    {
        // Reset hit flag each time the hitbox is armed
        hasHitThisSwipe = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHitThisSwipe) return;
        if (!other.CompareTag("Player")) return;

        hasHitThisSwipe = true;

        // Deal damage
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.TakeDamage(swipeDamage);
            Debug.Log($"[MonkeySwipeHitbox] Swiped player for {swipeDamage} damage.");
        }

        // Knockback (requires Rigidbody on player, or CharacterController workaround)
        Rigidbody playerRb = other.GetComponent<Rigidbody>();
        if (playerRb != null && knockbackForce > 0f)
        {
            Vector3 knockDir = (other.transform.position - transform.position).normalized;
            knockDir.y = 0.4f; // slight upward lift
            playerRb.AddForce(knockDir.normalized * knockbackForce, ForceMode.Impulse);
        }
    }
}