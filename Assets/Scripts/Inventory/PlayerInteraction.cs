using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;

    [Header("Inspection")]
    [SerializeField] private Transform inspectionPoint;
    private WorldItem currentItem;
    private WorldItem inspectedItem;
    private bool isInspecting = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private Rigidbody inspectedRigidbody;

    private void Update()
    {
        if (!isInspecting)
        {
            CheckForItem();
        }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        // if inspecting already add to inventory
        if (isInspecting)
        {
            AddInspectedItemToInventory();
            return;
        }

        if (currentItem != null)
        {
            BeginInspection(currentItem);
        }
    }

    public void OnCancelInspect(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (isInspecting)
        {
            CancelInspection();
        }
    }

    private void CheckForItem()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            currentItem = hit.collider.GetComponentInParent<WorldItem>();
        }
        else { currentItem = null; }
    }

    private void BeginInspection(WorldItem item)
    {
        inspectedItem = item;
        isInspecting = true;

        // Remember position
        originalPosition = item.transform.position;
        originalRotation = item.transform.rotation;
        originalParent = item.transform.parent;

        // Deal with Rigidbody
        inspectedRigidbody = item.GetComponent<Rigidbody>();
        if (inspectedRigidbody != null) { inspectedRigidbody.isKinematic = true; }

        // Move to inspection point
        item.transform.SetParent(inspectionPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        Debug.Log("Press E to take | Press X to put back");
    }

    private void AddInspectedItemToInventory()
    {
        if (inspectedItem == null) { return; }

        WorldItem itemToPickup = inspectedItem;

        inspectedItem = null;
        currentItem = null;
        isInspecting = false;

        itemToPickup.Pickup();
    }

    private void CancelInspection()
    {
        if (inspectedItem == null) { return; }

        inspectedItem.transform.position = originalPosition;
        inspectedItem.transform.rotation = originalRotation;

        if (inspectedRigidbody != null)
        {
            inspectedRigidbody.isKinematic = false;
        }

        inspectedItem = null;
        currentItem = null;
        inspectedRigidbody = null;

        isInspecting = false;

        Debug.Log("Inspection Cancelled.");
    }

}
