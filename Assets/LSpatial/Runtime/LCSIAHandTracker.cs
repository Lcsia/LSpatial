using UnityEngine;

public class LCSIAHandTracker : MonoBehaviour
{
    [Header("References")]
    public Transform leftHand;
    public Transform rightHand;
    public Transform head;

    [Header("Left Hand")]
    public Vector3 leftPosition;
    public Vector3 leftRotation;

    [Header("Right Hand")]
    public Vector3 rightPosition;
    public Vector3 rightRotation;

    [Header("Head")]
    public Vector3 headPosition;
    public Vector3 headRotation;

    [Header("Status")]
    public bool vrDetected = false;

    void Start()
    {
        AutoAssign();
    }

    void Update()
    {
        if (!vrDetected)
            return;

        if (leftHand != null)
        {
            leftPosition =
                leftHand.position;

            leftRotation =
                leftHand.eulerAngles;
        }

        if (rightHand != null)
        {
            rightPosition =
                rightHand.position;

            rightRotation =
                rightHand.eulerAngles;
        }

        if (head != null)
        {
            headPosition =
                head.position;

            headRotation =
                head.eulerAngles;
        }
    }

    public void AutoAssign()
    {
        GameObject left =
            GameObject.Find(
                "Left Controller");

        if (left != null)
        {
            leftHand =
                left.transform;
        }

        GameObject right =
            GameObject.Find(
                "Right Controller");

        if (right != null)
        {
            rightHand =
                right.transform;
        }

        if (Camera.main != null)
        {
            head =
                Camera.main.transform;
        }

        vrDetected =
            leftHand != null ||
            rightHand != null;
    }

    public Vector3 GetLeftHandPosition()
    {
        return leftPosition;
    }

    public Vector3 GetRightHandPosition()
    {
        return rightPosition;
    }

    public Vector3 GetHeadPosition()
    {
        return headPosition;
    }
}