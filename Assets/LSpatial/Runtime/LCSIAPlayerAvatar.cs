using UnityEngine;

public class LCSIAPlayerAvatar : MonoBehaviour
{
    public Animator animator;
    public Transform playerModel;

    private LCSIAAvatarPlayerController controller;
	private bool wasGrounded = true;
	private bool wasJumping = false;

    void Start()
    {
        controller =
            GetComponent<
                LCSIAAvatarPlayerController>();

        if (animator == null)
        {
            animator =
                GetComponentInChildren<
                    Animator>();
        }

        if (
            playerModel == null &&
            animator != null
        )
        {
            playerModel =
                animator.transform;
        }
    }

    void Update()
    {
        if (
            animator == null ||
            controller == null
        )
        {
            return;
        }

        float speed = 0f;

        if (controller.IsMoving)
        {
            speed = 1f;
        }

        if (
            controller.IsRunning &&
            controller.IsMoving
        )
        {
            speed = 2f;
        }

        animator.SetFloat(
            "Speed",
            speed);

        animator.SetBool(
            "Running",
            controller.IsRunning);

        animator.SetBool(
            "Grounded",
            controller.IsGrounded);

        animator.SetBool(
            "Climbing",
            controller.IsClimbing);

        animator.SetFloat(
            "ClimbDirection",
            controller.ClimbDirection);

		if (
			controller.IsJumping &&
			!wasJumping
		)
		{
			animator.SetTrigger(
				"Jump");
		}

        bool forward =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.UpArrow);

        bool backward =
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.DownArrow);

        if (playerModel != null)
        {
            if (forward)
            {
                playerModel.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        0f);
            }

			if (
				backward &&
				!controller.IsClimbing
			)
			{
				playerModel.localRotation =
					Quaternion.Euler(
						0f,
						180f,
						0f);
			}
        }

		wasGrounded =
			controller.IsGrounded;

		wasJumping =
			controller.IsJumping;
    }
}