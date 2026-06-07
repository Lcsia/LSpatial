using UnityEngine;

public class LCSIABillboard : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            return;
        }

        transform.LookAt(cam.transform);

        transform.Rotate(
            0f,
            180f,
            0f);
    }
}