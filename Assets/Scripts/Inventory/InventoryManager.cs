using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance {  get; private set; }

    [SerializeField] private int slotCount = 24;

    public InventorySlotData[] Slots { get; private set; }
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return;  }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Slots = new InventorySlotData[slotCount];
    }
    private void DebugInventory()
    {
        Debug.Log("----- INVENTORY -----");

        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i].item != null)
            {
                Debug.Log(
                    "Slot " + i +
                    ": " + Slots[i].item.itemName +
                    " x" + Slots[i].quantity
                );
            }
            else
            {
                Debug.Log("Slot " + i + ": EMPTY");
            }
        }

        Debug.Log("---------------------");
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
            return false;

        int remainingCapacity = 0;

        int stackSize = item.isStackable
            ? Mathf.Max(1, item.maxStackSize)
            : 1;

        // Calculate available capacity.
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i].item == null)
            {
                remainingCapacity += stackSize;
            }
            else if (item.isStackable && Slots[i].item == item)
            {
                remainingCapacity += Mathf.Max(
                    0,
                    stackSize - Slots[i].quantity
                );
            }
        }

        // Refuse the entire pickup if it cannot fit.
        if (remainingCapacity < quantity)
        {
            Debug.Log("Not enough inventory space!");
            return false;
        }

        // Fill existing stacks.
        if (item.isStackable)
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].item != item)
                    continue;

                int space = stackSize - Slots[i].quantity;
                int toAdd = Mathf.Min(quantity, space);

                Slots[i].quantity += toAdd;
                quantity -= toAdd;

                if (quantity <= 0)
                    break;
            }
        }

        // Fill empty slots.
        for (int i = 0; i < Slots.Length && quantity > 0; i++)
        {
            if (Slots[i].item != null)
                continue;

            int toAdd = Mathf.Min(quantity, stackSize);

            Slots[i] = new InventorySlotData(item, toAdd);

            quantity -= toAdd;
        }

        OnInventoryChanged?.Invoke();

        DebugInventory();

        return true;
    }

    public bool HasItem(ItemData item)
    {
        if (item == null) { return false; }

        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i].item == item && Slots[i].quantity > 0)
            {
                return true;
            }
        }

        return false;
    }

    public int GetItemQuantity(ItemData item)
    {
        int total = 0;

        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i].item == item)
            {
                total += Slots[i].quantity;
            }
        }

        return total;
    }

    public void RemoveItemAt(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= Slots.Length) return;
        Slots[slotIndex].quantity -= quantity;
        if (Slots[slotIndex].quantity <= 0)
            Slots[slotIndex] = default;
        OnInventoryChanged?.Invoke();
    }

    public void SwapSlots(int indexA, int indexB)
    {
        (Slots[indexA], Slots[indexB]) = (Slots[indexB], Slots[indexA]);
        OnInventoryChanged?.Invoke();
    }
}

[Serializable]
public struct InventorySlotData
{
    public ItemData item;
    public int quantity;
    public InventorySlotData(ItemData item, int quantity)
    { this.item = item; this.quantity = quantity;}
}

