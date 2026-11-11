using UnityEngine;
using TMPro;

public class RadioTuner : MonoBehaviour
{
    [Header("Radio Display")]
    [SerializeField] private TMP_Text frequencyText;

    [Header("Tuning (MHz)")]
    [SerializeField] private float minimumFrequencyMHz = 150f;
    [SerializeField] private float maximumFrequencyMHz = 160f;
    [SerializeField] private float startingFrequencyMHz = 152f;

    [Header("Position when held")]
    [Tooltip("Adjust this if not centered")]
    [SerializeField] private Vector3 heldLocalPosition = Vector3.zero;
    [Tooltip("Adjust this to face radio towards player")]
    [SerializeField] private Vector3 heldLocalEulerAngles = Vector3.zero;

    private int currentFrequencyTenths;
    private int minFrequencyTenths;
    private int maxFrequencyTenths;
    private bool isTuning;

    // Save radio previous state
    private Transform previousParent;
    private Vector3 previousLocalPosition;
    private Quaternion previousLocalRotation;
    private Vector3 previousLocalScale;
    private Rigidbody radioRigidbody;
    private bool previousIsKinematic;
    private bool previousUseGravity;
    private Collider[] colliders;
    private bool[] previousColliderStates;

    public bool IsTuning => isTuning;
    public float CurrentFrequencyMHz => currentFrequencyTenths / 10f;

    private void Awake()
    {
        minFrequencyTenths = Mathf.RoundToInt(minimumFrequencyMHz * 10f);
        maxFrequencyTenths = Mathf.Max(minFrequencyTenths, Mathf.RoundToInt(maximumFrequencyMHz * 10f));
        currentFrequencyTenths = Mathf.Clamp(Mathf.RoundToInt(startingFrequencyMHz * 10), minFrequencyTenths, maxFrequencyTenths);

        RefreshDisplay();
    }

    private void Start()
    {
        if (CommunicationManager.Instance == null)
        {
            Debug.LogError("RadioTuner: CommunicationManager is missing from the scene!", this);
            return;
        }

        CommunicationManager.Instance.RegisterRadio(this, currentFrequencyTenths);
    }

    // Called from PlayerInteraction when E is pressed while looking at the radio.
    public bool BeginTuning(Transform inspectionPoint)
    {
        if (isTuning || inspectionPoint == null)
        {
            if (inspectionPoint == null)
                Debug.LogError("RadioTuner: PlayerInteraction has no InspectionPoint assigned.", this);
            return false;
        }

        // Remember the scene state before attaching this radio to the player's view.
        previousParent = transform.parent;
        previousLocalPosition = transform.localPosition;
        previousLocalRotation = transform.localRotation;
        previousLocalScale = transform.localScale;

        radioRigidbody = GetComponent<Rigidbody>();
        if (radioRigidbody != null)
        {
            previousIsKinematic = radioRigidbody.isKinematic;
            previousUseGravity = radioRigidbody.useGravity;
            radioRigidbody.isKinematic = true;
            radioRigidbody.useGravity = false;
        }

        // The held radio must not collide with the player or block their camera.
        colliders = GetComponentsInChildren<Collider>(true);
        previousColliderStates = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            previousColliderStates[i] = colliders[i].enabled;
            colliders[i].enabled = false;
        }

        transform.SetParent(inspectionPoint, true);
        transform.localPosition = heldLocalPosition;
        transform.localRotation = Quaternion.Euler(heldLocalEulerAngles);

        isTuning = true;
        Debug.Log("Radio in hand: mouse wheel to tune, E or Q to put back.");
        CommunicationManager.Instance?.TryConnect(this);
        return true;
    }

    public void StopTuning()
    {
        if (!isTuning)
            return;

        // Restore its original parent, position, rotation and scale.
        transform.SetParent(previousParent, false);
        transform.localPosition = previousLocalPosition;
        transform.localRotation = previousLocalRotation;
        transform.localScale = previousLocalScale;

        if (radioRigidbody != null)
        {
            radioRigidbody.isKinematic = previousIsKinematic;
            radioRigidbody.useGravity = previousUseGravity;
        }

        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = previousColliderStates[i];
            }
        }

        isTuning = false;
    }

    public void TuneStep(int direction)
    {
        if (!isTuning || direction == 0)
            return;

        int newFrequency = Mathf.Clamp(
            currentFrequencyTenths + (direction > 0 ? 1 : -1),
            minFrequencyTenths,
            maxFrequencyTenths);

        if (newFrequency == currentFrequencyTenths)
            return;

        currentFrequencyTenths = newFrequency;
        Debug.Log($"Radio frequency: {currentFrequencyTenths}");
        RefreshDisplay();
        CommunicationManager.Instance?.UpdateRadioFrequency(this, currentFrequencyTenths);
    }

    private void RefreshDisplay()
    {
        if (frequencyText != null)
            frequencyText.text = $"{CurrentFrequencyMHz:F1} MHz";
    }

    private void OnDestroy()
    {
        CommunicationManager.Instance?.UnregisterRadio(this);
    }
}

