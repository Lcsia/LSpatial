using UnityEngine;

public class LCSIAPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 120f;

    [Header("Jump")]
    public float jumpHeight = 3f;
    public float gravity = -20f;

    [Header("Camera")]
    public Transform playerCamera;

    public Vector3 firstPersonPosition = new Vector3(0f, 0.4f, -3f);
    public Vector3 firstPersonRotation = Vector3.zero;

    public Vector3 thirdPersonPosition = new Vector3(0f, 1.5f, -5f);
    public Vector3 thirdPersonRotation = new Vector3(8f, 0f, 0f);

    public KeyCode cameraToggleKey = KeyCode.C;

    [Header("Climbing")]
    public float climbRayDistance = 1.5f;
    public float climbOverDistance = 2f;
    public float climbOverHeight = 1.75f;

    public float climbJumpForce = 5f;
    public float climbJumpBackForce = 2f;

    private CharacterController controller;
    private float verticalVelocity;
    private bool firstPerson = false;

    private bool isClimbing = false;
    private bool wasClimbing = false;
    private float currentClimbSpeed = 3f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera != null)
        {
            SetThirdPerson();
        }
    }

    void Update()
    {
        DetectClimbable();

        HandleMovement();

        if (isClimbing)
        {
            verticalVelocity = 0f;

            if (
                Input.GetKey(KeyCode.W) ||
                Input.GetKey(KeyCode.UpArrow)
            )
            {
                controller.Move(
                    Vector3.up *
                    currentClimbSpeed *
                    Time.deltaTime);
            }

            if (
                Input.GetKey(KeyCode.S) ||
                Input.GetKey(KeyCode.DownArrow)
            )
            {
                controller.Move(
                    Vector3.down *
                    currentClimbSpeed *
                    Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isClimbing = false;

                verticalVelocity =
                    Mathf.Sqrt(
                        climbJumpForce *
                        -2f *
                        gravity);

                controller.Move(
                    -transform.forward *
                    climbJumpBackForce);

                return;
            }

            HandleCameraToggle();
            return;
        }

        HandleJump();
        HandleCameraToggle();
    }

    void DetectClimbable()
    {
        bool previousClimbing =
            isClimbing;

        isClimbing = false;

        RaycastHit hit;

        Vector3 origin =
            transform.position +
            Vector3.up * 1f;

        if (
            Physics.Raycast(
                origin,
                transform.forward,
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
                transform.forward *
                climbOverDistance +
                Vector3.up *
                climbOverHeight);
        }

        wasClimbing = isClimbing;
    }

    void HandleMovement()
    {
        if (isClimbing)
        {
            if (
                Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.LeftArrow)
            )
            {
                transform.Rotate(
                    Vector3.up *
                    -rotationSpeed *
                    Time.deltaTime);
            }

            if (
                Input.GetKey(KeyCode.D) ||
                Input.GetKey(KeyCode.RightArrow)
            )
            {
                transform.Rotate(
                    Vector3.up *
                    rotationSpeed *
                    Time.deltaTime);
            }

            return;
        }

        float move = 0f;

        if (
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.UpArrow)
        )
        {
            move = 1f;
        }

        if (
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.DownArrow)
        )
        {
            move = -1f;
        }

        if (
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow)
        )
        {
            transform.Rotate(
                Vector3.up *
                -rotationSpeed *
                Time.deltaTime);
        }

        if (
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow)
        )
        {
            transform.Rotate(
                Vector3.up *
                rotationSpeed *
                Time.deltaTime);
        }

        float currentSpeed = walkSpeed;

        if (
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift)
        )
        {
            currentSpeed = runSpeed;
        }

        Vector3 movement =
            transform.forward *
            move *
            currentSpeed;

        movement.y = verticalVelocity;

        controller.Move(
            movement *
            Time.deltaTime);
    }

    void HandleJump()
    {
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity =
                    Mathf.Sqrt(
                        jumpHeight *
                        -2f *
                        gravity);
            }
        }

        verticalVelocity +=
            gravity *
            Time.deltaTime;
    }

    void HandleCameraToggle()
    {
        if (Input.GetKeyDown(cameraToggleKey))
        {
            firstPerson = !firstPerson;

            if (firstPerson)
            {
                SetFirstPerson();
            }
            else
            {
                SetThirdPerson();
            }
        }
    }

    void SetFirstPerson()
    {
        if (playerCamera == null)
            return;

        playerCamera.localPosition =
            firstPersonPosition;

        playerCamera.localEulerAngles =
            firstPersonRotation;
    }

    void SetThirdPerson()
    {
        if (playerCamera == null)
            return;

        playerCamera.localPosition =
            thirdPersonPosition;

        playerCamera.localEulerAngles =
            thirdPersonRotation;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 origin =
            transform.position +
            Vector3.up * 1f;

        Gizmos.DrawLine(
            origin,
            origin +
            transform.forward *
            climbRayDistance);
    }

#endif
}