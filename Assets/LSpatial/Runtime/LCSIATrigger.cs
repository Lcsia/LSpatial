using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LCSIATrigger : MonoBehaviour
{
    [Header("Detection")]
    public string playerTag = "Player";

    [Header("Keyboard Press")]
    public KeyCode keyPress = KeyCode.E;

    [Header("VR Input")]
    public InputActionReference leftTrigger;
    public InputActionReference rightTrigger;

    [Header("Events")]
    public UnityEvent onEnter;
    public UnityEvent onExit;
    public UnityEvent onEnterAndKeyPress;

    private bool playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = true;

        onEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = false;

        onExit?.Invoke();
    }

    private void Update()
    {
        if (!playerInside)
            return;

        bool keyboardPressed =
            Input.GetKeyDown(keyPress);

        bool leftPressed =
            leftTrigger != null &&
            leftTrigger.action != null &&
            leftTrigger.action.WasPressedThisFrame();

        bool rightPressed =
            rightTrigger != null &&
            rightTrigger.action != null &&
            rightTrigger.action.WasPressedThisFrame();

        if (
            keyboardPressed ||
            leftPressed ||
            rightPressed
        )
        {
            onEnterAndKeyPress?.Invoke();
        }
    }

    public bool IsPlayerInside()
    {
        return playerInside;
    }
}