using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FishingLine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rodTip;

    private LineRenderer lineRenderer;
    private Transform flyTransform;

    [Header("Tension")]
    [SerializeField] private float maxTension = 100f;
    [SerializeField] private float tensionRecovery = 20f;

    [SerializeField] private float currentTension;

    public float CurrentTension => currentTension;
    public float MaxTension => maxTension;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;

        lineRenderer.useWorldSpace = true;

        lineRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        if (flyTransform == null) {  return; }

        lineRenderer.SetPosition(0, rodTip.position);
        lineRenderer.SetPosition(1, flyTransform.position);
    }

    public void AttachFly(Transform fly)
    {
        flyTransform = fly;
        lineRenderer.enabled = true;
    }

    public void DetachFly()
    {
        flyTransform = null;
        lineRenderer.enabled = false;
    }

    public bool UpdateTenstion(float tensionGain)
    {
        currentTension -= tensionRecovery * Time.deltaTime;

        currentTension += tensionGain * Time.deltaTime;

        currentTension = Mathf.Clamp(currentTension, 0f, maxTension);

        return currentTension >= maxTension;
    }

    public void ResetTension()
    {
        currentTension = 0f;
    }

}
