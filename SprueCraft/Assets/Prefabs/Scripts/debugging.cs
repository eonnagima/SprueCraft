using UnityEngine;
using UnityEngine.InputSystem;

public class XRInputSanityCheck : MonoBehaviour
{
    public InputActionAsset inputActions;

    private InputAction debugAction;

    private void OnEnable()
    {
        var leftMap = inputActions.FindActionMap("XRI LeftHand");
        if (leftMap != null)
        {
            debugAction = leftMap.FindAction("Select");
            if (debugAction != null)
            {
                debugAction.performed += ctx => Debug.Log("🎮 Select Pressed");
                debugAction.Enable();
                Debug.Log("✔ Bound to XRI LeftHand → Select");
            }
            else Debug.LogWarning("⚠️ Couldn't find 'Select' action.");
        }
        else Debug.LogWarning("⚠️ Couldn't find 'XRI LeftHand' map.");
    }

    private void OnDisable()
    {
        if (debugAction != null)
        {
            debugAction.Disable();
            debugAction = null;
        }
    }
}
