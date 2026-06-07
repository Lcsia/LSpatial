using UnityEngine;
using UnityEngine.Video;

public class TestVideoSize : MonoBehaviour
{
    private VideoPlayer vp;

    void Start()
    {
        vp =
            GetComponent<VideoPlayer>();

        vp.Prepare();

        vp.prepareCompleted +=
            OnPrepared;
    }

    void OnPrepared(
        VideoPlayer source)
    {
        float width =
            source.width;

        float height =
            source.height;

        float aspect =
            width / height;

        float videoWidth =
            2f;

        float videoHeight =
            videoWidth / aspect;

        transform.localScale =
            new Vector3(
                videoWidth,
                videoHeight,
                1f);
    }
}