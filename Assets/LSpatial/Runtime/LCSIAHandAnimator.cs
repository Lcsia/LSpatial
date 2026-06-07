using UnityEngine;
using UnityEngine.InputSystem;

public class LCSIAHandAnimator : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference triggerAction;

    [Header("Animator")]
    public string parameterName = "Close";

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning(
                "LCSIAHandAnimator: No se encontró Animator en " +
                gameObject.name);
        }
    }

    void Update()
    {
        if (
            animator == null ||
            triggerAction == null ||
            triggerAction.action == null
        )
        {
            return;
        }

        bool pressed =
            triggerAction.action.IsPressed();

        animator.SetBool(
            parameterName,
            pressed);
    }
}