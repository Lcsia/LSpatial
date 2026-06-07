using UnityEngine;
using UnityEngine.InputSystem;

public class LCSIAJumpTest : MonoBehaviour
{
    public CharacterController controller;

    public InputActionReference jumpAction;

    public float jumpForce = 5f;

    private float verticalVelocity = 0f;

    void Update()
    {
        if (jumpAction.action.WasPressedThisFrame())
        {
            verticalVelocity = jumpForce;
        }

        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        controller.Move(
            new Vector3(
                0,
                verticalVelocity,
                0
            ) * Time.deltaTime
        );
    }
}