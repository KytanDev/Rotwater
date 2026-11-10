using System;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private QuestData activeQuest;
    public QuestData ActiveQuest => activeQuest;

    public event Action OnQuestUpdated;

    private InventoryManager inventory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        inventory = InventoryManager.Instance;

        if (inventory == null)
        {
            Debug.LogError("QuestManager: InventoryManager missing!");
            return;
        }

        inventory.OnInventoryChanged += HandleInventoryChanged;
        OnQuestUpdated?.Invoke();
    }

    private void OnDestroy()
    {
        if (inventory != null) { inventory.OnInventoryChanged -= HandleInventoryChanged; }
        if (Instance == this) { Instance = null; }
    }

    private void HandleInventoryChanged()
    {
        OnQuestUpdated?.Invoke();
    }

    public int GetProgress(QuestObjective objective)
    {
        if (inventory == null || objective.requiredItem == null) { return 0; }

        return Mathf.Min(inventory.GetItemQuantity(objective.requiredItem), Mathf.Max(1, objective.requiredAmount));
    }

    public bool IsObjectiveComplete(QuestObjective objective)
    {
        return GetProgress(objective) >= Mathf.Max(1, objective.requiredAmount);
    }

    public bool IsQuestComplete()
    {
        if (activeQuest == null || activeQuest.objectives.Count == 0) { return false; }

        foreach (QuestObjective objective in activeQuest.objectives)
        {
            if (!IsObjectiveComplete(objective)) { return false; }
        }

        return true;
    }
}
