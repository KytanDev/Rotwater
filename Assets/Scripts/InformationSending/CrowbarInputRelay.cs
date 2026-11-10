using UnityEngine;
using UnityEngine.InputSystem;

public class CrowbarInputRelay : MonoBehaviour
{
    [SerializeField] private CrowbarMinigame crowbarMinigame;

    public void OnApplyPressure(InputValue value)
    {
        if (value.isPressed)
            crowbarMinigame.ApplyPressure();
    }

    public void OnPull(InputValue value)
    {
        if (value.isPressed) {  crowbarMinigame.SetPulling(true); }
    }

    public void OnMouseDelta(InputValue value)
    {
        crowbarMinigame.SetMouseDelta(
            value.Get<Vector2>()
        );
    }

    public void OnCancel(InputValue value)
    {
        if (value.isPressed)
            crowbarMinigame.CancelMinigame();
    }
}
