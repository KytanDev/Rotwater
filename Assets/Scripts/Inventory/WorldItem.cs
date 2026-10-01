using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemData itemData;

    [SerializeField] private int quantity = 1;

    public ItemData ItemData => itemData;
    public int Quantity => quantity;

    public void Pickup()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("No InventoryManager exists!");
            return;
        }

        bool added = InventoryManager.Instance.AddItem(itemData, quantity);

        if (added)
        {
            Debug.Log("Picked up: " + itemData.itemName + " x" + quantity);
            Destroy(gameObject);
        }
        else { Debug.Log("Inventory full!"); }
    }
}
