using UnityEngine;

public class LCSIAEventSystemSelector : MonoBehaviour
{
    public GameObject eventSystemHTML;
    public GameObject eventSystemVR;

    void Awake()
    {
        bool isVR =
            UnityEngine.XR.XRSettings.enabled &&
            UnityEngine.XR.XRSettings.isDeviceActive;

        if (eventSystemHTML != null)
        {
            eventSystemHTML.SetActive(
                !isVR);
        }

        if (eventSystemVR != null)
        {
            eventSystemVR.SetActive(
                isVR);
        }

        Debug.Log(
            "LCSIAEventSystemSelector -> " +
            (isVR ? "VR" : "HTML/Desktop"));
    }
}