using UnityEngine;
using UnityEngine.InputSystem;

public class LCSIAVRController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;

    [Tooltip("Arrastra aquí el objeto Move que tiene el Dynamic Move Provider")]
    public MonoBehaviour moveProvider;

    [Tooltip("Camera Offset o Main Camera del XR Origin")]
    public Transform xrCamera;

    [Header("Input")]
    public InputActionReference runAction;
    public InputActionReference jumpAction;
    public InputActionReference moveAction;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    [Header("Jump")]
    public float jumpForce = 5f;

    [Header("Climbing")]
    public float climbRayDistance = 1.5f;
    public float climbOverDistance = 1.5f;
    public float climbOverHeight = 1.2f;

    private float verticalVelocity = 0f;

    private bool isClimbing = false;
    private bool wasClimbing = false;
    private float currentClimbSpeed = 3f;
	
	private float jumpMoveProviderDelay = 0.25f;
	private float jumpTimer = 0f;

    void Update()
    {
		
		if (jumpTimer > 0f)
		{
			jumpTimer -= Time.deltaTime;

			if (moveProvider != null)
			{
				moveProvider.enabled = false;
			}
		}
		else
		{
			if (moveProvider != null)
			{
				moveProvider.enabled =
					!isClimbing;
			}
		}
		
        DetectClimbable();

        HandleRun();
		
		if (moveProvider != null)
		{
			moveProvider.enabled =
				!isClimbing;
		}

        if (isClimbing)
        {
            HandleClimbing();
            return;
        }

        HandleJump();
    }

    void HandleRun()
    {
        if (moveProvider == null)
            return;

        float speed = walkSpeed;

        if (
            runAction != null &&
            runAction.action != null &&
            runAction.action.IsPressed()
        )
        {
            speed = runSpeed;
        }

        var field =
            moveProvider.GetType().GetField("moveSpeed");

        if (field != null)
        {
            field.SetValue(
                moveProvider,
                speed);
        }

        var property =
            moveProvider.GetType().GetProperty("moveSpeed");

        if (
            property != null &&
            property.CanWrite
        )
        {
            property.SetValue(
                moveProvider,
                speed);
        }
    }

    void DetectClimbable()
    {
        bool previousClimbing =
            isClimbing;

        isClimbing = false;

        if (xrCamera == null)
            return;

        RaycastHit hit;

        Vector3 origin =
            xrCamera.position;

        Vector3 direction =
            xrCamera.forward;

        if (
            Physics.Raycast(
                origin,
                direction,
                out hit,
                climbRayDistance)
        )
        {
            LCSIAClimbable climbable =
                hit.collider.GetComponent<LCSIAClimbable>();

            if (climbable != null)
            {
                float angle =
                    Vector3.Angle(
                        hit.normal,
                        Vector3.up);

                if (
                    angle >=
                    climbable.minClimbAngle
                )
                {
                    currentClimbSpeed =
                        climbable.climbSpeed;

                    isClimbing = true;
                }
            }
        }

        if (
            previousClimbing &&
            !isClimbing
        )
        {
            controller.Move(
                xrCamera.forward *
                climbOverDistance +
                Vector3.up *
                climbOverHeight);
        }

        wasClimbing =
            isClimbing;
    }

    void HandleClimbing()
    {
        if (
            moveAction == null ||
            moveAction.action == null
        )
        {
            return;
        }

        Vector2 moveInput =
            moveAction.action.ReadValue<Vector2>();

        controller.Move(
            Vector3.up *
            moveInput.y *
            currentClimbSpeed *
            Time.deltaTime);

        if (
            jumpAction != null &&
            jumpAction.action != null &&
            jumpAction.action.WasPressedThisFrame()
        )
        {
            isClimbing = false;

            verticalVelocity =
                jumpForce;

            controller.Move(
                -xrCamera.forward *
                0.5f);
        }
    }

    void HandleJump()
    {
        if (controller == null)
            return;

        if (
            controller.isGrounded &&
            verticalVelocity < 0f
        )
        {
            verticalVelocity = -2f;
        }

        if (
            jumpAction != null &&
            jumpAction.action != null &&
            jumpAction.action.WasPressedThisFrame() &&
            controller.isGrounded
        )
        {
            verticalVelocity =
                jumpForce;
        }

        verticalVelocity +=
            Physics.gravity.y *
            Time.deltaTime;

        controller.Move(
            Vector3.up *
            verticalVelocity *
            Time.deltaTime);
    }
}