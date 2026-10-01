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

    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item.isStackable)
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].item == item && Slots[i].quantity < item.maxStackSize)
                {
                    int space = item.maxStackSize - Slots[i].quantity;
                    int toAdd = Mathf.Min(quantity, space);
                    Slots[i].quantity += toAdd;
                    quantity -= toAdd;
                    if (quantity <= 0) { OnInventoryChanged?.Invoke(); return true; }
                }
            }
        }

        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i].item == null)
            {
                int toAdd = item.isStackable ? Mathf.Min(quantity, item.maxStackSize) : 1;
                Slots[i] = new InventorySlotData(item, toAdd);
                quantity -= toAdd;
                if (quantity <= 0) { OnInventoryChanged?.Invoke(); return true; }

            }
        }

        OnInventoryChanged?.Invoke();
        return quantity <= 0;
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

