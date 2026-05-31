using System.Collections;
using UnityEngine;

/// <summary>
/// Optional companion to MonkeyStalker.
/// Drives a secondary 3D AudioSource — separate from the main monkeyAudioSource —
/// to produce creepy ambient cues (breathing, branch creak, leaves rustling) that
/// the player hears as positional 3D audio. 
///
/// Because this uses a dedicated AudioSource with Spatial Blend = 1 (full 3D),
/// the player can hear the monkey moving through the trees even without seeing it.
///
/// Setup:
///   1. Add an AudioSource to the Monkey's root (or a child)
///   2. Set Spatial Blend to 1.0, Min Distance ~5, Max Distance ~30
///   3. Assign the AudioSource and clip arrays in the Inspector
/// </summary>
public class MonkeyAmbientSounds : MonoBehaviour
{
    [Header("References")]
    public MonkeyStalker monkeyStalker;
    public AudioSource ambientSource;       // 3D spatial audio source

    [Header("Ambient Clips")]
    [Tooltip("Very subtle — branch creaks, rustling. Play constantly while stalking.")]
    public AudioClip[] ambientStalkClips;
    public float minAmbientInterval = 4f;
    public float maxAmbientInterval = 12f;

    [Tooltip("A breath or subtle grunt. Plays when stationary and close to the player.")]
    public AudioClip[] closeProximityClips;
    public float closeProximityDistance = 10f;
    public float minBreathInterval = 6f;
    public float maxBreathInterval = 15f;

    [Tooltip("A distant hoot — plays occasionally when far from the player.")]
    public AudioClip[] distantHoots;
    public float distantHootDistance = 20f;
    public float minHootInterval = 15f;
    public float maxHootInterval = 35f;

    private float ambientTimer;
    private float breathTimer;
    private float hootTimer;

    private void Start()
    {
        ResetTimer(ref ambientTimer, minAmbientInterval, maxAmbientInterval);
        ResetTimer(ref breathTimer, minBreathInterval, maxBreathInterval);
        ResetTimer(ref hootTimer, minHootInterval, maxHootInterval);
    }

    private void Update()
    {
        if (monkeyStalker == null) return;

        MonkeyStalker.MonkeyState state = monkeyStalker.currentState;
        bool isStalking = state == MonkeyStalker.MonkeyState.Stalk ||
                          state == MonkeyStalker.MonkeyState.Reposition;

        if (!isStalking) return;

        float dist = Vector3.Distance(
            transform.position,
            monkeyStalker.playerTransform.position);

        // Ambient rustling — always while stalking
        ambientTimer -= Time.deltaTime;
        if (ambientTimer <= 0f)
        {
            PlayRandom(ambientStalkClips, 0.3f, 0.6f);
            ResetTimer(ref ambientTimer, minAmbientInterval, maxAmbientInterval);
        }

        // Close proximity breathing — only when near
        if (dist <= closeProximityDistance)
        {
            breathTimer -= Time.deltaTime;
            if (breathTimer <= 0f)
            {
                PlayRandom(closeProximityClips, 0.4f, 0.75f);
                ResetTimer(ref breathTimer, minBreathInterval, maxBreathInterval);
            }
        }

        // Distant hoot — only when far
        if (dist >= distantHootDistance)
        {
            hootTimer -= Time.deltaTime;
            if (hootTimer <= 0f)
            {
                PlayRandom(distantHoots, 0.5f, 0.9f);
                ResetTimer(ref hootTimer, minHootInterval, maxHootInterval);
            }
        }
    }

    private void PlayRandom(AudioClip[] clips, float minVol, float maxVol)
    {
        if (clips == null || clips.Length == 0 || ambientSource == null) return;
        ambientSource.PlayOneShot(
            clips[Random.Range(0, clips.Length)],
            Random.Range(minVol, maxVol));
    }

    private void ResetTimer(ref float timer, float min, float max)
        => timer = Random.Range(min, max);
}