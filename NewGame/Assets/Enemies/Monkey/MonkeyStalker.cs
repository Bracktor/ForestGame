using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// The Monkey Stalker — a passive horror enemy that follows the player through the
/// tree canopy. It never attacks unless provoked (hit by a thrown object).
/// 
/// Behaviour loop:
///   Idle  →  (spots player)  →  Stalk  →  occasionally Reposition
///   If hit by thrown object  →  Enraged  →  Charge  →  Swipe  →  Retreat  →  Stalk
/// 
/// Wire up in Inspector:
///   • Assign playerTransform (the Player GameObject)
///   • Assign monkeyAudioSource (AudioSource on this GameObject)
///   • Populate the sound arrays in the Inspector
///   • Assign swipeHitbox (a child trigger collider, disabled by default)
///   • Assign monkeyAnimator if you have one
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MonkeyStalker : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector: References
    // ─────────────────────────────────────────────
    [Header("References")]
    public Transform playerTransform;
    public AudioSource monkeyAudioSource;
    public Animator monkeyAnimator;         // optional
    public Collider swipeHitbox;            // child trigger — enabled only during swipe

    // ─────────────────────────────────────────────
    //  Inspector: Detection
    // ─────────────────────────────────────────────
    [Header("Detection")]
    public float stalkerStartRadius  = 30f;  // how close player must be to start stalking
    public float stalkerStopRadius   = 50f;  // how far player must get before monkey gives up

    // ─────────────────────────────────────────────
    //  Inspector: Stalk Movement
    // ─────────────────────────────────────────────
    [Header("Stalk Movement")]
    public float minFollowDistance   = 8f;   // monkey tries to stay at least this far away
    public float maxFollowDistance   = 18f;  // …and no further than this
    public float repositionInterval  = 6f;   // seconds between picking a new perch
    public float repositionVariance  = 3f;   // ± randomness added to interval
    public float swingSpeed          = 4f;   // NavMeshAgent speed while stalking
    public float repositionRadius    = 12f;  // radius around player when picking a new spot

    // ─────────────────────────────────────────────
    //  Inspector: Enrage / Attack
    // ─────────────────────────────────────────────
    [Header("Enrage & Attack")]
    public float chargeSpeed         = 14f;
    public float swipeRange          = 2f;   // must be within this distance to swipe
    public int   swipeDamage         = 20;
    public float swipeDuration       = 0.4f; // hitbox active window
    public float postSwipeRetreat    = 5f;   // how far back to flee after swiping
    public float enrageChargTimeout  = 8f;   // give up charge if player not reached in time

    // ─────────────────────────────────────────────
    //  Inspector: Sounds
    // ─────────────────────────────────────────────
    [Header("Sounds — Idle Stalk")]
    public AudioClip[] distantChatterClips;  // quiet, eerie chimp sounds heard while stalking
    public float minChatterInterval  = 8f;
    public float maxChatterInterval  = 20f;

    [Header("Sounds — Spotted / Reposition")]
    public AudioClip[] repositionClips;      // rustling / branch creak as it moves

    [Header("Sounds — Enrage")]
    public AudioClip enrageScreechClip;      // plays the moment it's hit
    public AudioClip[] chargeClips;          // aggressive vocalisations while charging
    public AudioClip swipeWhooshClip;        // swipe attack sound

    [Header("Sounds — Retreat")]
    public AudioClip[] retreatClips;         // receding chimp howls after swiping

    // ─────────────────────────────────────────────
    //  State Machine
    // ─────────────────────────────────────────────
    public enum MonkeyState { Idle, Stalk, Reposition, Enraged, Charging, Swiping, Retreating }
    [Header("Debug — Read Only")]
    public MonkeyState currentState = MonkeyState.Idle;

    // ─────────────────────────────────────────────
    //  Private
    // ─────────────────────────────────────────────
    private NavMeshAgent    agent;
    private float           repositionTimer;
    private float           chatterTimer;
    private bool            isProvoked = false;     // set true when hit by thrown object
    private static readonly int AnimWalk    = Animator.StringToHash("isWalking");
    private static readonly int AnimRun     = Animator.StringToHash("isRunning");
    private static readonly int AnimSwipe   = Animator.StringToHash("swipe");

    // ─────────────────────────────────────────────
    //  Unity Messages
    // ─────────────────────────────────────────────
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = swingSpeed;
        agent.stoppingDistance = minFollowDistance;

        if (swipeHitbox != null)
            swipeHitbox.enabled = false;

        ResetRepositionTimer();
        ResetChatterTimer();
    }

    private void Update()
    {
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case MonkeyState.Idle:
                UpdateIdle(distToPlayer);
                break;

            case MonkeyState.Stalk:
                UpdateStalk(distToPlayer);
                break;

            case MonkeyState.Reposition:
                // Handled by coroutine — just wait
                break;

            // Enraged, Charging, Swiping, Retreating are all coroutine-driven
        }

        TickChatter();
        SyncAnimator();
    }

    // ─────────────────────────────────────────────
    //  State: Idle
    // ─────────────────────────────────────────────
    private void UpdateIdle(float dist)
    {
        if (dist <= stalkerStartRadius)
        {
            EnterStalk();
        }
    }

    // ─────────────────────────────────────────────
    //  State: Stalk
    // ─────────────────────────────────────────────
    private void EnterStalk()
    {
        currentState = MonkeyState.Stalk;
        agent.speed = swingSpeed;
        ResetRepositionTimer();
    }

    private void UpdateStalk(float dist)
    {
        // Give up if player too far away and not provoked
        if (dist > stalkerStopRadius && !isProvoked)
        {
            currentState = MonkeyState.Idle;
            agent.ResetPath();
            return;
        }

        // Reposition on a timer
        repositionTimer -= Time.deltaTime;
        if (repositionTimer <= 0f)
        {
            StartCoroutine(RepositionCoroutine());
            return;
        }

        // Keep loose follow — only move if outside the comfortable band
        if (dist > maxFollowDistance)
        {
            MoveTowardPlayerPerch();
        }
        else if (dist < minFollowDistance)
        {
            // Back away slightly
            Vector3 awayDir = (transform.position - playerTransform.position).normalized;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position + awayDir * minFollowDistance, out hit, 4f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
        else
        {
            // In the comfort zone — stop and lurk
            agent.ResetPath();
        }
    }

    private void MoveTowardPlayerPerch()
    {
        // Pick a point near the player but respecting minFollowDistance
        Vector3 direction = (transform.position - playerTransform.position).normalized;
        Vector3 targetPos = playerTransform.position + direction * (minFollowDistance + 1f);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 5f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // ─────────────────────────────────────────────
    //  Coroutine: Reposition (the creepy bit)
    // ─────────────────────────────────────────────
    private IEnumerator RepositionCoroutine()
    {
        currentState = MonkeyState.Reposition;
        agent.ResetPath();

        // Play rustling sound
        PlayRandomClip(repositionClips, 0.5f, 0.85f);

        // Brief pause — monkey crouches before leaping
        yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));

        // Pick a new perch: random point within repositionRadius of the player
        Vector3 newPerch = GetRandomNavMeshPointNearPlayer(repositionRadius);
        agent.speed = swingSpeed * 1.3f;   // slightly faster during a swing
        agent.SetDestination(newPerch);

        // Wait until we arrive (or timeout)
        float timeout = 6f;
        while (agent.pathPending || agent.remainingDistance > 1.5f)
        {
            timeout -= Time.deltaTime;
            if (timeout <= 0f) break;
            yield return null;
        }

        agent.speed = swingSpeed;
        ResetRepositionTimer();
        currentState = MonkeyState.Stalk;
    }

    // ─────────────────────────────────────────────
    //  Public: Provoke (called by MonkeyHitDetector)
    // ─────────────────────────────────────────────
    public void Provoke()
    {
        if (currentState == MonkeyState.Enraged  ||
            currentState == MonkeyState.Charging ||
            currentState == MonkeyState.Swiping)
            return;     // already in attack cycle

        StopAllCoroutines();
        StartCoroutine(EnrageCoroutine());
    }

    // ─────────────────────────────────────────────
    //  Coroutine: Enrage → Charge → Swipe → Retreat
    // ─────────────────────────────────────────────
    private IEnumerator EnrageCoroutine()
    {
        // ── Enrage ──
        currentState = MonkeyState.Enraged;
        isProvoked = true;
        agent.ResetPath();

        PlayClip(enrageScreechClip, 1f);

        // Face the player menacingly for a beat
        yield return StartCoroutine(FacePlayerCoroutine(1.2f));

        // ── Charge ──
        currentState = MonkeyState.Charging;
        agent.speed = chargeSpeed;

        float chargeTimer = 0f;
        PlayRandomClip(chargeClips, 0.9f);

        while (chargeTimer < enrageChargTimeout)
        {
            agent.SetDestination(playerTransform.position);
            float dist = Vector3.Distance(transform.position, playerTransform.position);

            if (dist <= swipeRange)
                break;

            chargeTimer += Time.deltaTime;
            yield return null;
        }

        // ── Swipe ──
        currentState = MonkeyState.Swiping;
        agent.ResetPath();

        PlayClip(swipeWhooshClip, 1f);
        SetAnimTrigger(AnimSwipe);

        if (swipeHitbox != null)
            swipeHitbox.enabled = true;

        yield return new WaitForSeconds(swipeDuration);

        if (swipeHitbox != null)
            swipeHitbox.enabled = false;

        // ── Retreat ──
        currentState = MonkeyState.Retreating;
        agent.speed = swingSpeed * 1.5f;

        Vector3 retreatPoint = GetRetreatPoint(postSwipeRetreat);
        agent.SetDestination(retreatPoint);
        PlayRandomClip(retreatClips, 0.8f);

        float retreatTimeout = 5f;
        while (agent.pathPending || agent.remainingDistance > 1.5f)
        {
            retreatTimeout -= Time.deltaTime;
            if (retreatTimeout <= 0f) break;
            yield return null;
        }

        // Settle back into stalk
        isProvoked = false;
        agent.speed = swingSpeed;
        EnterStalk();
    }

    // Smoothly rotates to face the player for a set duration
    private IEnumerator FacePlayerCoroutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Vector3 dir = (playerTransform.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // ─────────────────────────────────────────────
    //  Ambient Chatter (eerie idle sounds)
    // ─────────────────────────────────────────────
    private void TickChatter()
    {
        if (currentState != MonkeyState.Stalk && currentState != MonkeyState.Reposition) return;

        chatterTimer -= Time.deltaTime;
        if (chatterTimer <= 0f)
        {
            PlayRandomClip(distantChatterClips, Random.Range(0.2f, 0.55f));
            ResetChatterTimer();
        }
    }

    // ─────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────
    private void ResetRepositionTimer()
        => repositionTimer = repositionInterval + Random.Range(-repositionVariance, repositionVariance);

    private void ResetChatterTimer()
        => chatterTimer = Random.Range(minChatterInterval, maxChatterInterval);

    private Vector3 GetRandomNavMeshPointNearPlayer(float radius)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 rndCircle = Random.insideUnitCircle.normalized * Random.Range(minFollowDistance, radius);
            Vector3 candidate = playerTransform.position + new Vector3(rndCircle.x, 0f, rndCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, 5f, NavMesh.AllAreas))
                return hit.position;
        }
        // Fallback: just go near the player
        return playerTransform.position + transform.forward * minFollowDistance;
    }

    private Vector3 GetRetreatPoint(float distance)
    {
        Vector3 awayDir = (transform.position - playerTransform.position).normalized;
        Vector3 candidate = transform.position + awayDir * distance;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidate, out hit, 6f, NavMesh.AllAreas))
            return hit.position;
        return candidate;
    }

    // ─────────────────────────────────────────────
    //  Audio helpers
    // ─────────────────────────────────────────────
    private void PlayClip(AudioClip clip, float volume = 1f)
    {
        if (clip == null || monkeyAudioSource == null) return;
        monkeyAudioSource.PlayOneShot(clip, volume);
    }

    private void PlayRandomClip(AudioClip[] clips, float volume = 1f, float maxVolume = -1f)
    {
        if (clips == null || clips.Length == 0 || monkeyAudioSource == null) return;
        float vol = maxVolume < 0f ? volume : Random.Range(volume, maxVolume);
        monkeyAudioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], vol);
    }

    // ─────────────────────────────────────────────
    //  Animator helpers (gracefully optional)
    // ─────────────────────────────────────────────
    private void SyncAnimator()
    {
        if (monkeyAnimator == null) return;
        bool moving  = agent.velocity.sqrMagnitude > 0.1f;
        bool running = currentState == MonkeyState.Charging;
        monkeyAnimator.SetBool(AnimWalk, moving && !running);
        monkeyAnimator.SetBool(AnimRun, running);
    }

    private void SetAnimTrigger(int hash)
    {
        if (monkeyAnimator != null)
            monkeyAnimator.SetTrigger(hash);
    }

    // ─────────────────────────────────────────────
    //  Gizmos
    // ─────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        // Detection radius
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, stalkerStartRadius);

        // Comfort band
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(playerTransform.position, minFollowDistance);
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.15f);
        Gizmos.DrawWireSphere(playerTransform.position, maxFollowDistance);

        // Swipe range
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, swipeRange);
    }
}