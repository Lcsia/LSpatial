using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LCSIASaveData : MonoBehaviour
{
    [Header("Server")]
    public string url =
        "https://lcsia.com.mx/VR/VRGetData.php";

    [Header("Experiment")]
    public string ID =
        "TEST";

    [Header("Debug")]
    public bool debug =
        true;

    private string sessionID;

    void Awake()
    {
        sessionID =
            ID + "_" +
            System.DateTime.Now.ToString(
                "ddMMyy") +
            "_" +
            Random.Range(
                100000,
                999999);
    }

    public void Send(
        string data)
    {
        StartCoroutine(
            SendCoroutine(
                data));
    }

    private IEnumerator SendCoroutine(
        string data)
    {
        string fileName =
            sessionID +
            ".csv";

        WWWForm form =
            new WWWForm();

        form.AddField(
            "block_name",
            fileName);

        form.AddField(
            "session_data",
            data);

        if (debug)
        {
            Debug.Log(
                "File: " +
                fileName);

            Debug.Log(
                "Characters: " +
                data.Length);
        }

        UnityWebRequest request =
            UnityWebRequest.Post(
                url,
                form);

        yield return
            request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        bool success =
            request.result ==
            UnityWebRequest.Result.Success;
#else
        bool success =
            !request.isNetworkError &&
            !request.isHttpError;
#endif

        if (success)
        {
            if (debug)
            {
                Debug.Log(
                    "Upload successful");

                Debug.Log(
                    request.downloadHandler.text);
            }
        }
        else
        {
            Debug.LogError(
                "Upload failed: " +
                request.error);
        }
    }
}