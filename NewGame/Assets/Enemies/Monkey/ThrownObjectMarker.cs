using System.Collections;
using UnityEngine;

/// <summary>
/// Add this to any item the player can throw.
/// 
/// Call Throw() from your throw logic — it arms the marker so the
/// MonkeyHitDetector knows the object is genuinely airborne, not just
/// sitting in the world or being held by the player.
/// 
/// The marker auto-disarms after the object has been grounded for
/// groundedDisarmDelay seconds, so it won't re-provoke the monkey if it
/// rolls into it later.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ThrownObjectMarker : MonoBehaviour
{
    [HideInInspector] public bool isInFlight = false;

    [Tooltip("Seconds after landing before the 'in flight' flag clears.")]
    public float groundedDisarmDelay = 0.5f;

    private Rigidbody rb;
    private Coroutine disarmCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Call this from your throw input handler right before adding force to the Rigidbody.
    /// </summary>
    public void Throw()
    {
        isInFlight = true;

        if (disarmCoroutine != null)
            StopCoroutine(disarmCoroutine);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Don't disarm on monkey hits — MonkeyHitDetector handles that trigger.
        // Disarm after hitting anything solid (ground, trees, walls).
        if (!isInFlight) return;

        if (disarmCoroutine != null)
            StopCoroutine(disarmCoroutine);

        disarmCoroutine = StartCoroutine(DisarmAfterDelay());
    }

    private IEnumerator DisarmAfterDelay()
    {
        yield return new WaitForSeconds(groundedDisarmDelay);
        isInFlight = false;
        Debug.Log($"[ThrownObjectMarker] {name} disarmed.");
    }
}