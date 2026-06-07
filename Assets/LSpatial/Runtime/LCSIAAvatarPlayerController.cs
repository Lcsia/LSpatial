using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LCSIACameraView
{
    public string name = "Camera";

    public Vector3 localPosition =
        new Vector3(0f, 1.5f, -5f);

    public Vector3 localRotation =
        new Vector3(8f, 0f, 0f);
}

public class LCSIAAvatarPlayerController : MonoBehaviour
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

    public List<LCSIACameraView> cameraViews =
        new List<LCSIACameraView>()
        {
            new LCSIACameraView()
            {
                name = "First Person",
                localPosition =
                    new Vector3(
                        0f,
                        0.4f,
                        -3f),
                localRotation =
                    Vector3.zero
            },

            new LCSIACameraView()
            {
                name = "Third Person",
                localPosition =
                    new Vector3(
                        0f,
                        1.5f,
                        -5f),
                localRotation =
                    new Vector3(
                        8f,
                        0f,
                        0f)
            }
        };

    public int startCameraIndex = 1;

    public KeyCode cameraToggleKey =
        KeyCode.C;

    [Header("Climbing")]
    public float climbRayDistance =
        1.5f;

    public float climbOverDistance =
        2f;

    public float climbOverHeight =
        1.75f;

    public float climbJumpForce =
        5f;

    public float climbJumpBackForce =
        2f;

    public bool IsMoving
    {
        get;
        private set;
    }

    public bool IsRunning
    {
        get;
        private set;
    }

    public bool IsGrounded
    {
        get;
        private set;
    }

    public bool IsClimbing
    {
        get;
        private set;
    }

    public bool IsJumping
    {
        get;
        private set;
    }

    public float ClimbDirection
    {
        get;
        private set;
    }

    private CharacterController controller;

    private float verticalVelocity;

    private bool wasClimbing =
        false;

    private float currentClimbSpeed =
        3f;

    private int currentCameraIndex =
        0;

    void Start()
    {
        controller =
            GetComponent<
                CharacterController>();

        currentCameraIndex =
            Mathf.Clamp(
                startCameraIndex,
                0,
                Mathf.Max(
                    cameraViews.Count - 1,
                    0));

        ApplyCamera(
            currentCameraIndex);
    }

    void Update()
    {
        DetectClimbable();

        IsGrounded =
            controller.isGrounded;

        HandleMovement();

        if (IsClimbing)
        {
            verticalVelocity =
                0f;

            ClimbDirection =
                0f;

            if (
                Input.GetKey(
                    KeyCode.W) ||
                Input.GetKey(
                    KeyCode.UpArrow)
            )
            {
                ClimbDirection =
                    1f;

                controller.Move(
                    Vector3.up *
                    currentClimbSpeed *
                    Time.deltaTime);
            }

            if (
                Input.GetKey(
                    KeyCode.S) ||
                Input.GetKey(
                    KeyCode.DownArrow)
            )
            {
                ClimbDirection =
                    -1f;

                controller.Move(
                    Vector3.down *
                    currentClimbSpeed *
                    Time.deltaTime);
            }

            if (
                Input.GetKeyDown(
                    KeyCode.Space)
            )
            {
                IsClimbing =
                    false;

                IsJumping =
                    true;

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
            IsClimbing;

        IsClimbing =
            false;

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
                hit.collider.GetComponent<
                    LCSIAClimbable>();

            if (
                climbable != null
            )
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

                    IsClimbing =
                        true;
                }
            }
        }

        if (
            previousClimbing &&
            !IsClimbing
        )
        {
            controller.Move(
                transform.forward *
                climbOverDistance +
                Vector3.up *
                climbOverHeight);
        }

        wasClimbing =
            IsClimbing;
    }

    void HandleMovement()
    {
        if (IsClimbing)
        {
            if (
                Input.GetKey(
                    KeyCode.A) ||
                Input.GetKey(
                    KeyCode.LeftArrow)
            )
            {
                transform.Rotate(
                    Vector3.up *
                    -rotationSpeed *
                    Time.deltaTime);
            }

            if (
                Input.GetKey(
                    KeyCode.D) ||
                Input.GetKey(
                    KeyCode.RightArrow)
            )
            {
                transform.Rotate(
                    Vector3.up *
                    rotationSpeed *
                    Time.deltaTime);
            }

            IsMoving =
                false;

            IsRunning =
                false;

            return;
        }

        float move =
            0f;

        if (
            Input.GetKey(
                KeyCode.W) ||
            Input.GetKey(
                KeyCode.UpArrow)
        )
        {
            move =
                1f;
        }

        if (
            Input.GetKey(
                KeyCode.S) ||
            Input.GetKey(
                KeyCode.DownArrow)
        )
        {
            move =
                -1f;
        }

        IsMoving =
            Mathf.Abs(move) > 0f;

        if (
            Input.GetKey(
                KeyCode.A) ||
            Input.GetKey(
                KeyCode.LeftArrow)
        )
        {
            transform.Rotate(
                Vector3.up *
                -rotationSpeed *
                Time.deltaTime);
        }

        if (
            Input.GetKey(
                KeyCode.D) ||
            Input.GetKey(
                KeyCode.RightArrow)
        )
        {
            transform.Rotate(
                Vector3.up *
                rotationSpeed *
                Time.deltaTime);
        }

        float currentSpeed =
            walkSpeed;

        IsRunning =
            false;

        if (
            Input.GetKey(
                KeyCode.LeftShift) ||
            Input.GetKey(
                KeyCode.RightShift)
        )
        {
            currentSpeed =
                runSpeed;

            IsRunning =
                true;
        }

        Vector3 movement =
            transform.forward *
            move *
            currentSpeed;

        movement.y =
            verticalVelocity;

        controller.Move(
            movement *
            Time.deltaTime);
    }

    void HandleJump()
    {
        if (
            controller.isGrounded
        )
        {
            if (
                verticalVelocity < 0f
            )
            {
                verticalVelocity =
                    -2f;
            }

            if (
                IsJumping
            )
            {
                IsJumping =
                    false;
            }

            if (
                Input.GetKeyDown(
                    KeyCode.Space)
            )
            {
                IsJumping =
                    true;

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
        if (
            Input.GetKeyDown(
                cameraToggleKey)
        )
        {
            if (
                cameraViews == null ||
                cameraViews.Count == 0
            )
            {
                return;
            }

            currentCameraIndex++;

            if (
                currentCameraIndex >=
                cameraViews.Count
            )
            {
                currentCameraIndex =
                    0;
            }

            ApplyCamera(
                currentCameraIndex);
        }
    }

    void ApplyCamera(
        int index)
    {
        if (
            playerCamera == null
        )
        {
            return;
        }

        if (
            cameraViews == null ||
            cameraViews.Count == 0
        )
        {
            return;
        }

        index =
            Mathf.Clamp(
                index,
                0,
                cameraViews.Count - 1);

        playerCamera.localPosition =
            cameraViews[index]
            .localPosition;

        playerCamera.localEulerAngles =
            cameraViews[index]
            .localRotation;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.cyan;

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