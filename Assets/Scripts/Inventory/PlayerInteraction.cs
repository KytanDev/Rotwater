using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;

    [Header("Inspection")]
    [SerializeField] private Transform inspectionPoint;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float zoomSpeed = 0.02f;
    [SerializeField] private float movementSpeed = 1f;

    [Header("Inspection Limits")]
    [SerializeField] private float maxHorizontalMove = 0.75f;
    [SerializeField] private float maxVerticalMove = 0.5f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 2f;

    [SerializeField] private CrowbarMinigame crowbarMinigame;

    private WorldItem currentItem;
    private CacheInteractable currentCache;
    private RadioTuner currentRadio;
    private RadioTuner activeRadio;
    private WorldItem inspectedItem;
    private FishingController fishingController;
    private bool inspectingFish;
    private bool isInspecting = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private Rigidbody inspectedRigidbody;

    private Vector2 inspectionMovement;

    private PlayerInput playerInput;
    private InputAction lookAction;
    private InputAction moveAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        fishingController = GetComponent<FishingController>();

        lookAction = playerInput.currentActionMap.FindAction("Look");
        moveAction = playerInput.currentActionMap.FindAction("Move");
    }

    private void Update()
    {
        if (crowbarMinigame != null && crowbarMinigame.IsActive) { return; }
        if (activeRadio != null) { return; }

        if (!isInspecting)
        {
            CheckForItem();
        }
        else
        {
            MoveInspectionPoint();
        }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (crowbarMinigame != null && crowbarMinigame.IsActive) { return; }

        if (activeRadio != null)
        {
            EndRadioTuning();
            return;
        }

        // If already inspecting, take the item
        if (isInspecting)
        {
            AddInspectedItemToInventory();
            return;
        }

        // Radios take priority when looking at an object with a RadioTuner.
        if (currentRadio != null)
        {
            BeginRadioTuning(currentRadio);
            return;
        }

        // Otherwise inspect the item we're looking at.
        if (currentItem != null)
        {
            BeginInspection(currentItem);
            return;
        }

        if (currentCache != null)
        {
            currentCache.Interact();
        }
    }

    public void OnCancelAction(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (activeRadio != null)
        {
            EndRadioTuning();
            return;
        }

        if (isInspecting)
        {
            CancelInspection();
        }
    }

    public void OnInspectRotate(InputValue value)
    {
        if (!isInspecting || inspectedItem == null)
            return;

        Vector2 mouseDelta = value.Get<Vector2>();

        float rotationX = mouseDelta.x * rotationSpeed;
        float rotationY = mouseDelta.y * rotationSpeed;

        inspectedItem.transform.Rotate(rotationY, rotationX, 0);
    }

    public void OnInspectZoom(InputValue value)
    {
        if (activeRadio != null)
        {
            float scrollY = value.Get<Vector2>().y;
            if (Mathf.Abs(scrollY) > 0.01f)
                activeRadio.TuneStep(scrollY > 0f ? 1 : -1);
            return;
        }

        if (!isInspecting || inspectedItem == null)
            return;

        Vector2 scroll = value.Get<Vector2>();

        Vector3 position = inspectionPoint.localPosition;

        position.z += scroll.y * zoomSpeed;
        position.z = Mathf.Clamp(position.z, minZoom, maxZoom);

        inspectionPoint.localPosition = position;
    }

    public void OnInspectMovement(InputValue value)
    {
        if (!isInspecting)
            return;

        inspectionMovement = value.Get<Vector2>();
    }

    private void MoveInspectionPoint()
    {
        Vector3 position = inspectionPoint.localPosition;

        position.x += inspectionMovement.x * movementSpeed * Time.deltaTime;
        position.y += inspectionMovement.y * movementSpeed * Time.deltaTime;

        position.x = Mathf.Clamp(
            position.x,
            -maxHorizontalMove,
            maxHorizontalMove
        );

        position.y = Mathf.Clamp(
            position.y,
            -maxVerticalMove,
            maxVerticalMove
        );

        inspectionPoint.localPosition = position;
    }

    private void CheckForItem()
    {
        if (crowbarMinigame != null && crowbarMinigame.IsActive)
        {
            currentItem = null;
            currentCache = null;
            currentRadio = null;
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            currentItem = hit.collider.GetComponentInParent<WorldItem>();
            currentCache = hit.collider.GetComponentInParent<CacheInteractable>();
            currentRadio = hit.collider.GetComponentInParent<RadioTuner>();
        }
        else
        {
            currentItem = null;
            currentCache = null;
            currentRadio = null;
        }
    }

    private void BeginRadioTuning(RadioTuner radio)
    {
        if (radio == null || isInspecting || activeRadio != null)
            return;

        if (!radio.BeginTuning(inspectionPoint))
            return;

        activeRadio = radio;
        lookAction.Disable();
        moveAction.Disable();
    }

    private void EndRadioTuning()
    {
        if (activeRadio == null)
            return;

        activeRadio.StopTuning();
        activeRadio = null;
        currentRadio = null;
        lookAction.Enable();
        moveAction.Enable();
    }

    public void BeginInspection(WorldItem item)
    {
        if (item == null)
        {
            Debug.LogError("Cannot inspect a null WorldItem!");
            return;
        }

        if (item.ItemData == null)
        {
            Debug.LogError("WorldItem has no ItemData!");
            return;
        }
        if (isInspecting) { return; }

        inspectedItem = item;
        inspectingFish = item.GetComponent<FishBehaviour>() != null;

        // Remember original world state
        originalPosition = item.transform.position;
        originalRotation = item.transform.rotation;
        originalParent = item.transform.parent;

        if (inspectedRigidbody != null)
        {
            inspectedRigidbody.isKinematic = true;
        }

        // Disable normal player controls that conflict with inspection
        lookAction.Disable();
        moveAction.Disable();

        isInspecting = true;

        // Disable physics while we're holding the item
        inspectedRigidbody = item.GetComponent<Rigidbody>();

        // Move item to inspection point
        item.transform.SetParent(inspectionPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        Debug.Log(item.ItemData.itemName);
        Debug.Log(item.ItemData.description);
        if (inspectingFish)
        {
            Debug.Log("Press E to keep fish | Q to release fish");
        }
        else
        {
            Debug.Log("Press E to take | Press Q to put back");
        }
    }

    private void AddInspectedItemToInventory()
    {
        if (inspectedItem == null)
            return;

        WorldItem itemToPickup = inspectedItem;
        bool wasFish = inspectingFish;

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager missing!");
            return;
        }

        bool added = InventoryManager.Instance.AddItem(itemToPickup.ItemData, itemToPickup.Quantity);

        if (!added)
        {
            Debug.Log("Inventory Full! Cannot keep item.");
            return;
        }

        inspectedItem = null;
        currentItem = null;
        inspectedRigidbody = null;
        inspectingFish = false;

        ExitInspectionMode();

        Destroy(itemToPickup.gameObject); // Remove if deletes if not chosen to pick up. //(-14.96222 -4.82 -2.5834815

        if (wasFish && fishingController != null)
        {
            fishingController.OnFishInspectionFinished();
        }

        Debug.Log("Item added to inventory.");
    }

    private void CancelInspection()
    {
        if (inspectedItem == null)
            return;

        bool wasFish = inspectingFish;
        WorldItem itemToRelease = inspectedItem;

        // Restore hierarchy first
        itemToRelease.transform.SetParent(originalParent);

        // Then restore original world transform
        itemToRelease.transform.position = originalPosition;
        itemToRelease.transform.rotation = originalRotation;

        // Restore physics
        if (inspectedRigidbody != null)
        {
            inspectedRigidbody.isKinematic = false;
        }

        inspectedItem = null;
        currentItem = null;
        inspectedRigidbody = null;
        inspectingFish = false;

        ExitInspectionMode();

        if (wasFish)
        {
            Destroy(itemToRelease.gameObject);

            if (fishingController != null) { fishingController.OnFishInspectionFinished(); }

            Debug.Log("Fish Released.");

        }
        else
        {
            Debug.Log("Inspection Cancelled.");
        }

    }

    private void ExitInspectionMode()
    {
        isInspecting = false;

        // Clear any held WASD input
        inspectionMovement = Vector2.zero;

        // Give control back to FPS controller
        lookAction.Enable();
        moveAction.Enable();
    }

    public void SetCrowbarControls(bool enabled)
    {
        if (enabled)
        {
            moveAction.Enable();
            lookAction.Enable();
        }
        else
        {
            moveAction.Disable();
            lookAction.Disable();
        }
    }
}