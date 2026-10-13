
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private FishingRod fishingRod;
    [SerializeField] private FishingLine fishingLine;
    [SerializeField] private FishBehaviour testFish;
    [SerializeField] private Transform playerCamera;

    [Header("Casting")]
    [SerializeField] private float chargeTime = 1.5f;

    [Header("Bite Settings")]
    [SerializeField] private float timeUntilBite = 3f;
    [SerializeField] private float hookWindow = 2.5f;

    [Header("Fight Settings")]
    [SerializeField] private float fishTensionGain = 12f;
    [SerializeField] private float reelTensionGain = 25f;
    [SerializeField] private float rodTensionGain = 15f;
    [SerializeField] private float counterTensionGain = 20f;
    [SerializeField] private float minReelTension = 30f;

    private float biteTimer;
    private float hookTimer;
    private FishBehaviour hookedFish;

    [Header("State")]
    [SerializeField]
    private FishingState currentState = FishingState.Inactive;

    private float castPower;
    private bool isReeling;
    private bool isRodRaised;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        switch (currentState)
        {
            case FishingState.Charging:
                UpdateCharging();
                break;

            case FishingState.Waiting:
                UpdateWaiting();
                break;

            case FishingState.Bite:
                UpdateBite();
                break;

            case FishingState.Fighting:
                UpdateFighting();
                break;
        }
    }

    public void OnCast(InputValue value)
    {
        bool pressed = value.isPressed;

        if (pressed)
        {
            switch (currentState)
            {
                case FishingState.Ready:
                    StartCharging();
                    break;

                case FishingState.Waiting:
                    isReeling = true;
                    break;

                case FishingState.Fighting:
                    isReeling = true;
                    break;
            }
        }
        else
        {
            if (currentState == FishingState.Charging)
            {
                StartCast();
            }
            else if (currentState == FishingState.Waiting)
            {
                isReeling = false;
            }
            else if (currentState == FishingState.Fighting)
            {
                isReeling = false;
            }
        }
    }

    public void OnReel(InputValue value)
    {
        if (currentState == FishingState.Bite && value.isPressed)
        {
            SetHook();
        }
        else if (currentState == FishingState.Fighting)
        {
            isRodRaised = value.isPressed;
        }
    }

    public void OnLook(InputValue value)
    {

    }

    public void OnExitFishing(InputValue value)
    {
        if (value.isPressed)
        {
            ExitFishingMode();
        }
    }

    public void EnterFishingMode()
    {
        if (currentState != FishingState.Inactive) { return; }

        playerInput.SwitchCurrentActionMap("Fishing");

        ChangeState(FishingState.Ready);
    }

    public void ExitFishingMode()
    {
        if (currentState == FishingState.Inactive) { return; }

        StopAllCoroutines();

        fishingRod.ClearFly();

        castPower = 0f;

        isReeling = false;
        isRodRaised = false;
        if (hookedFish != null)
        {
            hookedFish.StopFight();
            hookedFish = null;
        }
        fishingLine.ResetTension();
        playerInput.SwitchCurrentActionMap("Player");

        ChangeState(FishingState.Inactive);
    }

    private void StartCharging()
    {
        castPower = 0f;

        ChangeState(FishingState.Charging);
    }

    private void UpdateCharging()
    {
        castPower += Time.deltaTime / chargeTime;

        castPower = Mathf.Clamp01(castPower);
    }

    private void UpdateWaiting()
    {
        if (isReeling)
        {
            if (fishingRod.RetrieveFly())
            {
                isReeling = false;
                ChangeState(FishingState.Ready);
            }

            return;
        }

        biteTimer += Time.deltaTime;

        if (biteTimer >= timeUntilBite)
        {
            hookTimer = 0f;

            ChangeState(FishingState.Bite);

            Debug.Log("BITE! Press RMB to set the hook!");
        }
    }

    private void UpdateBite()
    {
        hookTimer += Time.deltaTime;

        if (hookTimer >= hookWindow)
        {
            Debug.Log("Missed the bite!");

            biteTimer = 0f;

            ChangeState(FishingState.Waiting);
        }
    }

    private void UpdateFighting()
    {
        if (hookedFish == null) { return; }

        float tensionGain = fishTensionGain * hookedFish.Strength;

        if (isReeling)
        {
            tensionGain += reelTensionGain;

            if (fishingLine.CurrentTension >= minReelTension) // Add stamina check for exhaustion
            {
                hookedFish.ReelTowardsPlayer();
            }
        }

        if (isRodRaised)
        {
            float counterStrength = CalculateCounterStrength();

            tensionGain += rodTensionGain + counterTensionGain;

            Debug.Log($"Counter Strength: {counterStrength:F2}");
        }

        fishingRod.FollowFish(hookedFish.transform);

        if (fishingLine.UpdateTenstion(tensionGain))
        {
            Debug.Log("LINE BROKEN!");
        }
    }

    private void StartCast()
    {
        ChangeState(FishingState.Casting);

        StartCoroutine(fishingRod.Cast(castPower, OnCastComplete));
    }

    private void OnCastComplete(bool successful)
    {
        if (successful)
        {
            biteTimer = 0f;
            ChangeState(FishingState.Waiting);
            Debug.Log("Fly landed on water.");
        }
        else
        {
            ChangeState(FishingState.Ready);
        }
    }

    private void SetHook()
    {
        if (currentState != FishingState.Bite) { return; }

        if (testFish == null)
        {
            Debug.LogWarning("No fish assigned for hooksetting!");
            return;
        }

        hookedFish = testFish;
        hookedFish.StartFight(transform, fishingRod.ActiveFly.transform.position);
        Debug.Log("Fish hooked!");
        Debug.Log($"Hooked {hookedFish.name} | " + $"Strength: {hookedFish.Strength} | " + $"Speed: {hookedFish.SwimSpeed}");
        isRodRaised = false;
        isReeling = false;
        fishingLine.ResetTension();
        ChangeState(FishingState.Fighting);
    }

    private float CalculateCounterStrength()
    {
        if (hookedFish == null) { return 0f; }

        Vector3 cameraDirection = playerCamera.forward;
        cameraDirection.y = 0f;
        cameraDirection.Normalize();

        Vector3 oppositeFishDirection = -hookedFish.RunDirection;

        float alignment = Vector3.Dot(cameraDirection, oppositeFishDirection);

        return Mathf.Clamp01(alignment);
    }

    private void ChangeState(FishingState newState)
    {
        Debug.Log($"Fishing: {currentState} -> {newState}");

        currentState = newState;
    }

    [ContextMenu("Test Enter Fishing Mode")]
    private void TestEnterFishingMode()
    {
        EnterFishingMode();
    }
}
