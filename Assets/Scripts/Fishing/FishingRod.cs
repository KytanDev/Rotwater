
using System;
using System.Collections;
using System.Security;
using UnityEngine;

public class FishingRod : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rodTip;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private GameObject flyPrefab;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private FishingLine fishingLine;

    [Header("Casting")]
    [SerializeField] private float minCastDistance = 3f;
    [SerializeField] private float maxCastDistance = 20f;
    [SerializeField] private float castDuration = 0.6f;
    [SerializeField] private float arcHeight = 2f;

    [Header("Retrieval")]
    [SerializeField] private float reelSpeed = 5f;
    [SerializeField] private float retrievalDistance = 0.5f;

    public GameObject ActiveFly { get; private set; }

    public IEnumerator Cast(float castPower, Action<bool> onComplete)
    {
        float distance = Mathf.Lerp(minCastDistance, maxCastDistance, Mathf.Clamp01(castPower));
        Vector3 startPosition = rodTip.position;

        Vector3 direction = playerCamera.forward;
        direction.y = 0f;
        direction.Normalize();

        Vector3 targetPosition = startPosition + direction * distance;

        Vector3 rayOrigin = targetPosition + Vector3.up * 20f;

        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 50f, waterLayer, QueryTriggerInteraction.Collide))
        {
            Debug.Log("Cast missed the water.");

            onComplete?.Invoke(false);
            yield break;
        }

        targetPosition = hit.point;
        ClearFly();

        ActiveFly = Instantiate(flyPrefab, startPosition, Quaternion.identity);

        fishingLine.AttachFly(ActiveFly.transform);

        float elapsed = 0f;

        while (elapsed < castDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / castDuration);

            Vector3 position = Vector3.Lerp(startPosition, targetPosition, t);

            position.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

            ActiveFly.transform.position = position;

            yield return null;
        }

        ActiveFly.transform.position = targetPosition;

        onComplete?.Invoke(true);
    }

    public bool RetrieveFly()
    {
        if (ActiveFly == null) { return true; }

        ActiveFly.transform.position = Vector3.MoveTowards(ActiveFly.transform.position, rodTip.position, reelSpeed * Time.deltaTime);

        float distance = Vector3.Distance(ActiveFly.transform.position, rodTip.position);

        if (distance <= retrievalDistance)
        {
            ClearFly();
            return true;
        }

        return false;
    }

    public void ClearFly()
    {
        fishingLine.DetachFly();

        if (ActiveFly == null) { return; }

        Destroy(ActiveFly);
        ActiveFly = null;
    }

    public void FollowFish(Transform fish)
    {
        if (ActiveFly == null || fish == null) { return; }

        ActiveFly.transform.position = fish.position;
    }
}
