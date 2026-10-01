using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Basic Information")]
    public string itemName;
    public string itemID;
    [TextArea(2, 4)]
    public string description;

    [Header("Stacking")]
    public bool isStackable = true;
    public int maxStackSize = 12;

    [Header("Classification")]
    public ItemType itemType;
}

public enum ItemType { Consumable, Equipment, Material, Quest, Key}
