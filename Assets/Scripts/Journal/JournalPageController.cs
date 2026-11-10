using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalPageController : MonoBehaviour
{
    [Header("Page Spreads")]
    [SerializeField] private GameObject[] leftPages;
    [SerializeField] private GameObject[] rightPages;

    [Header("Navigation")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text pageNumberText;

    private int currentSpread = 0;

    private void OnEnable()
    {
        ShowSpread(currentSpread);
    }

    public void NextPage()
    {
        ShowSpread(currentSpread + 1);
    }

    public void PreviousPage()
    {
        ShowSpread(currentSpread - 1);
    }

    public void ShowSpread(int index)
    {
        int totalSpreads = Mathf.Min(leftPages.Length, rightPages.Length);

        if (totalSpreads == 0) { return; }

        currentSpread = Mathf.Clamp(index, 0, totalSpreads - 1);

        for (int i = 0; i < leftPages.Length; i++)
        {
            if (leftPages[i] != null) { leftPages[i].SetActive(i == currentSpread); }
        }

        for (int i = 0; i < rightPages.Length; ++i)
        {
            if (rightPages[i] != null) { rightPages[i].SetActive(i == currentSpread); }
        }

        if (previousButton != null) { previousButton.interactable = currentSpread > 0; }

        if (nextButton != null) { nextButton.interactable = currentSpread < totalSpreads - 1; }

        if (pageNumberText != null)
        {
            int leftNumber = currentSpread * 2 + 1;
            int rightNumber = leftNumber + 1;

            pageNumberText.text = $"{leftNumber} - {rightNumber}";
        }
    }
}
