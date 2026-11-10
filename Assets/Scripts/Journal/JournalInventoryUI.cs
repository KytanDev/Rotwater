using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class JournalInventoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text inventoryText;

    private InventoryManager inventory;

    private void Start()
    {
        inventory = InventoryManager.Instance;

        if (inventory == null)
        {
            Debug.LogError("JournalInventoryUI: InventoryManager missing!");
            return;
        }

        inventory.OnInventoryChanged += RefreshInventory;
        RefreshInventory();
    }

    private void OnDestroy()
    {
        if (inventory != null) { inventory.OnInventoryChanged -= RefreshInventory; }
    }

    public void RefreshInventory()
    {
        if (inventory == null) { return; }

        Dictionary<ItemData, int> items = new();

        foreach (InventorySlotData slot in inventory.Slots)
        {
            if (slot.item == null || slot.quantity <= 0) { continue; }

            if (!items.ContainsKey(slot.item)) { items.Add(slot.item, 0); }

            items[slot.item] += slot.quantity;
        }

        StringBuilder builder = new();

        if (items.Count == 0)
        {
            builder.AppendLine("No items collected.");
        }
        else
        {
            foreach (var entry in items)
            {
                builder.AppendLine($"{entry.Key.itemName}   x{entry.Value}");
            }
        }

        inventoryText.text = builder.ToString();
    }

}
