using TMPro;
using UnityEngine;

public class JournalUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private TMP_Text questTitle;
    [SerializeField] private TMP_Text questDescription;
    [SerializeField] private Transform objectiveContainer;
    [SerializeField] private TMP_Text objectivePrefab;

    private QuestManager questManager;
    private bool isOpen;

    private void Start()
    {
        questManager = QuestManager.Instance;

        if (questManager == null)
        {
            Debug.LogError("JournalUI: QuestManager missing!");
            return;
        }

        questManager.OnQuestUpdated += RefreshJournal;

        journalPanel.SetActive(false);
        isOpen = false;
    }

    private void OnDestroy()
    {
        if (questManager != null) { questManager.OnQuestUpdated -= RefreshJournal; }
    }

    public void ToggleJournal()
    {
        isOpen = !isOpen;
        journalPanel.SetActive(isOpen);

        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            RefreshJournal();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void RefreshJournal()
    {
        if (questManager == null) { return; }

        QuestData quest = questManager.ActiveQuest;

        if (quest == null) { return; }

        questTitle.text = quest.questTitle;
        questDescription.text = quest.description;

        foreach (Transform child in objectiveContainer) { Destroy(child.gameObject); }

        foreach (QuestObjective objective in quest.objectives)
        {
            TMP_Text row = Instantiate(objectivePrefab, objectiveContainer);

            int progress = questManager.GetProgress(objective);
            int required = Mathf.Max(1, objective.requiredAmount);
            bool complete = questManager.IsObjectiveComplete(objective);

            string marker = complete ? "[X]" : "[ ]";

            row.text = $"{marker} {objective.description} " + $"({progress}/{required})";

            if (complete) { row.color = new Color(0.45f, 0.55f, 0.45f); }
            else { row.color = new Color(0.85f, 0.8f, 0.7f); }
        }
    }
}
