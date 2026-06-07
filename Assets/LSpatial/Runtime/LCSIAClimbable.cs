using UnityEngine;

public class LCSIAClimbable : MonoBehaviour
{
    [Header("Climbing")]
    public float climbSpeed = 3f;

    [Range(0f, 90f)]
    public float minClimbAngle = 60f;
}