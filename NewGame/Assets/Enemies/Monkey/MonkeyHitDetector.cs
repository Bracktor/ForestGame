using UnityEngine;

/// <summary>
/// Attach to the Monkey's root GameObject (or any collider on it).
/// Listens for collision with any object tagged "Thrown" and calls
/// MonkeyStalker.Provoke() to trigger the attack sequence.
///
/// To make a throwable object:
///   1. Tag it "Thrown" in the Inspector
///   2. Give it a Rigidbody + Collider
///   3. On throw, set the Rigidbody velocity and optionally flip a
///      flag on it so it doesn't trigger when it first spawns in the
///      player's hand. See ThrownObjectMarker for that.
/// </summary>
public class MonkeyHitDetector : MonoBehaviour
{
    [Header("References")]
    public MonkeyStalker monkeyStalker;

    [Header("Settings")]
    [Tooltip("Tag that thrown objects must have to provoke the monkey.")]
    public string thrownTag = "Thrown";

    [Tooltip("Minimum speed (m/s) the object must be travelling to count as a hit.")]
    public float minimumImpactSpeed = 2f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(thrownTag)) return;

        // Ignore very slow-moving objects (e.g. items rolling along the ground)
        if (collision.relativeVelocity.magnitude < minimumImpactSpeed) return;

        // Only provoke from objects that are actually in flight
        ThrownObjectMarker marker = collision.gameObject.GetComponent<ThrownObjectMarker>();
        if (marker != null && !marker.isInFlight) return;

        Debug.Log($"[MonkeyHitDetector] Hit by {collision.gameObject.name} — provoking monkey!");
        monkeyStalker?.Provoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Support trigger-based hit detection as well (for physics layers that use triggers)
        if (!other.CompareTag(thrownTag)) return;

        ThrownObjectMarker marker = other.GetComponent<ThrownObjectMarker>();
        if (marker != null && !marker.isInFlight) return;

        Debug.Log($"[MonkeyHitDetector] Trigger hit by {other.gameObject.name} — provoking monkey!");
        monkeyStalker?.Provoke();
    }
}