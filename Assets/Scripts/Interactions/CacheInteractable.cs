
using UnityEngine;

public class CacheInteractable : MonoBehaviour
{
    [Header("Cache")]
    [SerializeField] private GameObject contentsPrefab;
    [SerializeField] private Transform contentsSpawnPoint;
    [SerializeField] private CrowbarMinigame crowbarMinigame;

    private bool isOpen;

    public bool IsOpen => isOpen;

    public void Interact()
    {
        if (isOpen || crowbarMinigame == null)
            return;

        if (crowbarMinigame.IsActive || crowbarMinigame.IsBroken)
            return;

        crowbarMinigame.Begin(OpenCache, OnMinigameFinished);
    }

    private void OpenCache()
    {
        if (isOpen)
            return;

        isOpen = true;

        Debug.Log("Cache successfully opened!");

        if (contentsPrefab != null && contentsSpawnPoint != null)
        {
            Instantiate(
                contentsPrefab,
                contentsSpawnPoint.position,
                contentsSpawnPoint.rotation
            );
        }
    }

    private void OnMinigameFinished()
    {
        Debug.Log("Crowbar interaction finished.");
    }
}
