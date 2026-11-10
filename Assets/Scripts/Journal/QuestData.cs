using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Rotwater/Quest")]
public class QuestData : ScriptableObject
{
    public string questID;
    public string questTitle;

    [TextArea(3, 6)]
    public string description;

    public List<QuestObjective> objectives = new();
}

[Serializable]
public class QuestObjective
{
    public string objectiveID;
    public string description;

    public ItemData requiredItem;
    public int requiredAmount = 1;
}

