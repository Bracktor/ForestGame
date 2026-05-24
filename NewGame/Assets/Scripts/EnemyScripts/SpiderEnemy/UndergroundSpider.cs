using UnityEngine;
using System.Collections;

public class UndergroundSpider : MonoBehaviour
{
    [Header("Detection")]
    public float captureRadius = 1.5f;      // how close player must be to get grabbed
    public LayerMask playerLayer;

    [Header("Repositioning")]
    public float minMoveInterval = 4f;      // min seconds before spider moves
    public float maxMoveInterval = 10f;     // max seconds before spider moves
    public float moveRadius = 8f;           // how far it can reposition from origin

    [Header("References")]
    public Transform facePoint;             // attach child transform here for camera lock
    public SpiderCaptureSystem captureSystem;

    [Header("Cooldown")]
    public float escapeCooldown = 2f;       // seconds before spider can capture again

    private float cooldownTimer = 0f;
    private bool hasPlayer = false;
    private bool isMoving = false;
    private Vector3 originPoint;

    private void Start()
    {
        originPoint = transform.position;

        if (facePoint == null)
            Debug.LogWarning("UndergroundSpider: No FacePoint assigned! Camera won't know where to look.");

        StartCoroutine(RepositionLoop());
    }

    private void Update()
    {
        if (hasPlayer || isMoving) return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // Check if player stepped on us
        Collider[] hits = Physics.OverlapSphere(transform.position, captureRadius, playerLayer);
        if (hits.Length > 0)
        {
            GameObject player = hits[0].gameObject;
            hasPlayer = true;
            captureSystem.StartCapture(player, this);
        }
    }

    private IEnumerator RepositionLoop()
    {
        while (true)
        {
            float wait = Random.Range(minMoveInterval, maxMoveInterval);
            yield return new WaitForSeconds(wait);

            if (!hasPlayer)
            {
                yield return StartCoroutine(MoveToNewPosition());
            }
        }
    }

    private IEnumerator MoveToNewPosition()
    {
        isMoving = true;

        // Pick a random point within moveRadius of origin
        Vector2 randomCircle = Random.insideUnitCircle * moveRadius;
        Vector3 newPos = originPoint + new Vector3(randomCircle.x, 0f, randomCircle.y);

        // Snap to ground
        if (Physics.Raycast(newPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
            newPos.y = hit.point.y;

        yield return null;
        transform.position = newPos;
        Debug.Log($"Spider repositioned to {newPos}");

        isMoving = false;
    }

    public void ReleasePlayer()
    {
        hasPlayer = false;
        cooldownTimer = escapeCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, captureRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? originPoint : transform.position, moveRadius);
    }
}