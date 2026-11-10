using UnityEngine;
using UnityEngine.InputSystem;

public class JournalInputRelay : MonoBehaviour
{
    [SerializeField] private JournalUI journalUI;

    public void OnJournal(InputValue value)
    {
        if (!value.isPressed) { return; }

        journalUI.ToggleJournal();
    }
}
