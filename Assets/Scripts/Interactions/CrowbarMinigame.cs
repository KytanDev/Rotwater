using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CrowbarMinigame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private Slider pressureSlider;
    [SerializeField] private Slider integritySlider;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Pressure")]
    [SerializeField] private float pressurePerClick = 12f;
    [SerializeField] private float pressureDecay = 18f;
    [SerializeField] private float greenMin = 55f;
    [SerializeField] private float greenMax = 75f;

    [Header("Pull")]
    [SerializeField] private float requiredPullDistance = 100f;
    [SerializeField] private float maxPullDuration = 0.35f;
    [SerializeField] private int pullsRequired = 2;

    [Header("Damage")]
    [SerializeField] private float overpressureDamage = 25f;
    [SerializeField] private float failedPullDamage = 10f;

    [SerializeField] private PlayerInput playerInput;

    private string previousActionMap;
    private Vector2 mouseDelta;
    private InputAction pullAction;

    private float pressure;
    private float integrity = 100f;
    private float pullDistance;
    private float pullTimer;
    private bool pulling;
    private bool active;
    private int successfulPulls;

    private Action onSuccess;
    private Action onFinished;

    public bool IsActive => active;
    public bool IsBroken => integrity <= 0f;


    public void Begin(Action successCallback, Action finishedCallback)
    {
        if (active || IsBroken) { return; }

        onSuccess = successCallback;
        onFinished  = finishedCallback;

        pressure = 0f;
        pullDistance = 0f;
        pullTimer = 0f;
        pulling = false;
        successfulPulls = 0;
        active = true;

        previousActionMap = playerInput.currentActionMap.name;
        playerInput.SwitchCurrentActionMap("Crowbar");
        pullAction = playerInput.currentActionMap.FindAction("Pull", true);

        minigamePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        feedbackText.text = "";
        UpdateUI();

    }

    private void Update()
    {
        if (!active)
            return;

        // Pressure always decays
        pressure = Mathf.MoveTowards(
            pressure,
            0f,
            pressureDecay * Time.deltaTime
        );

        if (pulling)
        {
            pullTimer += Time.deltaTime;

            pullDistance += Mathf.Max(0f, -mouseDelta.y);

            // Evaluate when right-click is RELEASED
            if (pullAction.WasReleasedThisFrame())
            {
                pulling = false;

                Debug.Log(
                    $"Pull released! Distance: {pullDistance:F1}, " +
                    $"Time: {pullTimer:F2}, Pressure: {pressure:F1}"
                );

                AttemptPull();

                if (!active)
                    return;
            }
        }

        mouseDelta = Vector2.zero;

        UpdateUI();
    }

    private void AddPressure()
    {
        if (pulling) { return; } // Can't apply pressure while pulling

        pressure = Mathf.Min(100f, pressure + pressurePerClick);

        if (pressure > greenMax)
        {
            DamageCrowbar(overpressureDamage); // Damage if too much pressure
            feedbackText.text = "Too much pressure!";
        }

    }

    public void ApplyPressure()
    {
        if (!active || pulling) { return; }
        AddPressure();
    }

    public void SetPulling(bool isPressed)
    {
        if (!active) { return; }

        if (isPressed)
        {
            if (pulling) { return; }

            pulling = true;
            pullDistance = 0f;
            pullTimer = 0f;
            mouseDelta = Vector2.zero;

            Debug.Log("Pull Started");
        }
        else
        {
            if (!pulling) { return; }

            pulling = false;
            Debug.Log($"Pull released | Distance: {pullDistance:F1} |" + $"Time: {pullTimer:F2} | Pressure: {pressure:F1}");
            AttemptPull();
        }
        
    }

    public void SetMouseDelta(Vector2 delta)
    {
        if (!active || !pulling) { return; }

        mouseDelta = delta;
    }

    public void CancelMinigame()
    {
        if (active) { Finish(false); }
    }

    private void AttemptPull()
    {
        bool enoughDistance = pullDistance >= requiredPullDistance;
        bool fastEnough = pullTimer <= maxPullDuration;
        bool correctPressure = pressure >= greenMin && pressure <= greenMax;

        if (!enoughDistance || !fastEnough)
        {
            feedbackText.text = "Pull faster!";
            return;
        }

        if (correctPressure)
        {
            successfulPulls++;

            feedbackText.text = "Good pull!";

            pressure = Mathf.Max(0f, pressure - 35f);

            if (successfulPulls >= pullsRequired)
            {
                Finish(true);
                return;
            }
        }
        else if (pressure > greenMax)
        {
            DamageCrowbar(overpressureDamage);
            feedbackText.text = "Too much force!";
        }
        else
        {
            DamageCrowbar(failedPullDamage);
            feedbackText.text = "Not enough pressure!";
        }

        if (active)
            UpdateUI();
    }

    private void DamageCrowbar(float amount)
    {
        integrity = Mathf.Max(0f, integrity - amount);

        if (IsBroken)
        {
            feedbackText.text = "Crowbar Broken!";
            Finish(false);
        }
    }

    private void Finish(bool succeeded)
    {
        if (!active) { return; }

        active = false;
        pulling = false;
        mouseDelta = Vector2.zero;

        minigamePanel.SetActive(false);

        if (succeeded) { onSuccess?.Invoke(); }

        onFinished?.Invoke();

        playerInput.SwitchCurrentActionMap(previousActionMap);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        onSuccess = null;
        onFinished = null;
    }

    private void UpdateUI()
    {
        pressureSlider.value = pressure;
        integritySlider.value = integrity;

        instructionText.text =
            $"Left-Click: build pressure\n" +
            $"Right-click + pull down: pry \n" +
            $"Progress: {successfulPulls}/{pullsRequired}" +
            $"ESC: canel";

        if (pressure >= greenMin && pressure <= greenMax) { pressureSlider.fillRect.GetComponent<Image>().color = Color.green; }
        else if (pressure > greenMax) { pressureSlider.fillRect.GetComponent<Image>().color = Color.red; }
        else { pressureSlider.fillRect.GetComponent<Image>().color = Color.yellow; }
    }
}
