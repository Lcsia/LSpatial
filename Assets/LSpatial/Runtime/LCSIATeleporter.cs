using UnityEngine;
using UnityEngine.Events;

public class LCSIATeleporter : MonoBehaviour
{
    [Header("Destination")]
    public Transform destination;

    [Header("Detection")]
    public string playerTag = "Player";

    [Header("Events")]
    public UnityEvent onEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (destination == null)
            return;

        CharacterController cc =
            other.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        other.transform.position =
            destination.position;

        if (cc != null)
            cc.enabled = true;

        onEnter?.Invoke();
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (destination == null)
            return;

        Gizmos.color = Color.green;

        DrawThickLine(
            transform.position,
            destination.position,
            0.15f);

        Gizmos.DrawSphere(
            transform.position,
            0.30f);

        Gizmos.DrawSphere(
            destination.position,
            0.30f);
    }

    private void DrawThickLine(
        Vector3 start,
        Vector3 end,
        float thickness)
    {
        Vector3 direction =
            (end - start).normalized;

        Vector3 right =
            Vector3.Cross(
                direction,
                Vector3.up);

        if (right == Vector3.zero)
        {
            right =
                Vector3.Cross(
                    direction,
                    Vector3.forward);
        }

        right *= thickness;

        Gizmos.DrawLine(
            start,
            end);

        Gizmos.DrawLine(
            start + right,
            end + right);

        Gizmos.DrawLine(
            start - right,
            end - right);
    }

#endif
}