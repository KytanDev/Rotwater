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

    private WorldItem currentItem;
    private WorldItem inspectedItem;
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

        lookAction = playerInput.currentActionMap.FindAction("Look");
        moveAction = playerInput.currentActionMap.FindAction("Move");
    }

    private void Update()
    {
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
        if (!value.isPressed)
            return;

        // If already inspecting, take the item
        if (isInspecting)
        {
            AddInspectedItemToInventory();
            return;
        }

        // Otherwise inspect the item we're looking at
        if (currentItem != null)
        {
            BeginInspection(currentItem);
        }
    }

    public void OnCancelAction(InputValue value)
    {
        if (!value.isPressed)
            return;

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
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            currentItem = hit.collider.GetComponentInParent<WorldItem>();
        }
        else
        {
            currentItem = null;
        }
    }

    public void BeginInspection(WorldItem item)
    {
        inspectedItem = item;

        // Remember original world state
        originalPosition = item.transform.position;
        originalRotation = item.transform.rotation;
        originalParent = item.transform.parent;

        // Disable normal player controls that conflict with inspection
        lookAction.Disable();
        moveAction.Disable();

        isInspecting = true;

        // Disable physics while we're holding the item
        inspectedRigidbody = item.GetComponent<Rigidbody>();

        if (inspectedRigidbody != null)
        {
            inspectedRigidbody.isKinematic = true;
        }

        // Move item to inspection point
        item.transform.SetParent(inspectionPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        Debug.Log(item.ItemData.itemName);
        Debug.Log(item.ItemData.description);
        Debug.Log("Press E to take | Press X to put back");
    }

    private void AddInspectedItemToInventory()
    {
        if (inspectedItem == null)
            return;

        WorldItem itemToPickup = inspectedItem;

        inspectedItem = null;
        currentItem = null;

        ExitInspectionMode();

        itemToPickup.Pickup();
    }

    private void CancelInspection()
    {
        if (inspectedItem == null)
            return;

        // Restore hierarchy first
        inspectedItem.transform.SetParent(originalParent);

        // Then restore original world transform
        inspectedItem.transform.position = originalPosition;
        inspectedItem.transform.rotation = originalRotation;

        // Restore physics
        if (inspectedRigidbody != null)
        {
            inspectedRigidbody.isKinematic = false;
        }

        // Fish - Delete Object?
        //if () // Tag? Could work
        //{
            // Delete fish Object
        //}

        inspectedItem = null;
        currentItem = null;
        inspectedRigidbody = null;

        ExitInspectionMode();

        Debug.Log("Inspection Cancelled.");
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
}