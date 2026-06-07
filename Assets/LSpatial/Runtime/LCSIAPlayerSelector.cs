using UnityEngine;
using UnityEngine.XR.Management;

public class LCSIAPlayerSelector : MonoBehaviour
{
    public GameObject avatarPlayer;
    public GameObject vrPlayer;

    void Awake()
    {
        bool vrDetected =
            XRGeneralSettings.Instance != null &&
            XRGeneralSettings.Instance.Manager != null &&
            XRGeneralSettings.Instance.Manager.activeLoader != null;

        avatarPlayer.SetActive(!vrDetected);
        vrPlayer.SetActive(vrDetected);
    }
}