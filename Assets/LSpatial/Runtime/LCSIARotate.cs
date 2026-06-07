using UnityEngine;

public class LCSIARotate : MonoBehaviour
{
    [Header("Rotation")]
    public bool rotateX = false;

    public bool rotateY = true;

    public bool rotateZ = false;

    public float speed = 90f;

    private void Update()
    {
        Vector3 rotation =
            new Vector3(
                rotateX ? 1f : 0f,
                rotateY ? 1f : 0f,
                rotateZ ? 1f : 0f);

        transform.Rotate(
            rotation *
            speed *
            Time.deltaTime,
            Space.Self);
    }
}