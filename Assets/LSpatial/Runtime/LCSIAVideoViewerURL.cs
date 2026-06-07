using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using UnityEngine.XR;

public class LCSIAVideoViewerURL : MonoBehaviour
{
    private GameObject imageViewerCanvas;

    private RawImage largeImage;

    private TMP_Text titleText;
    private TMP_Text descriptionText;

    private Button closeButton;

    private Button btnPlayPause;
    private Button btnStop;
    private Button btnBack;
    private Button btnForward;

    private Image imgPlayPause;

    private LCSIAVideoURL videoData;

    private VideoPlayer viewerVideoPlayer;

    private RenderTexture viewerTexture;
	
	private bool sourceWasPlaying;
	private bool viewerShouldPlay;
	
	private VideoPlayer sourceVideoPlayer;

	private double savedTime;

	private bool wasPlaying;

    private Sprite playSprite;
    private Sprite pauseSprite;
	
	

    void Awake()
    {
        CacheReferences();

        videoData =
            GetComponent<LCSIAVideoURL>();

        ConnectButtons();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();

            closeButton.onClick.AddListener(
                Close);
        }

        if (imageViewerCanvas != null)
        {
            imageViewerCanvas.SetActive(
                false);
        }
    }

    void CacheReferences()
    {
        Transform t;

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas");

        if (t != null)
        {
            imageViewerCanvas =
                t.gameObject;
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/ImagePanel/LargeImage");

        if (t != null)
        {
            largeImage =
                t.GetComponent<RawImage>();
        }

        t =
            transform.Find(
                "ViewerVideoPlayer/Video Player");

        if (t != null)
        {
            viewerVideoPlayer =
                t.GetComponent<VideoPlayer>();
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/InfoPanel/Title");

        if (t != null)
        {
            titleText =
                t.GetComponent<TMP_Text>();
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/InfoPanel/Description");

        if (t != null)
        {
            descriptionText =
                t.GetComponent<TMP_Text>();
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/CloseButton");

        if (t != null)
        {
            closeButton =
                t.GetComponent<Button>();
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/BtnPlayPause");

        if (t != null)
        {
            btnPlayPause =
                t.GetComponent<Button>();

            imgPlayPause =
                t.GetComponent<Image>();
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/BtnStop");

        if (t != null)
        {
            btnStop =
                t.GetComponent<Button>();
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/BtnBack");

        if (t != null)
        {
            btnBack =
                t.GetComponent<Button>();
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/BtnForward");

        if (t != null)
        {
            btnForward =
                t.GetComponent<Button>();
        }

        viewerTexture =
            Resources.Load<RenderTexture>(
                "RT_VideoViewer");

        if (
            largeImage != null &&
            viewerTexture != null
        )
        {
            largeImage.texture =
                viewerTexture;
        }
    }

    void ConnectButtons()
    {
        if (btnPlayPause != null)
        {
            btnPlayPause.onClick.RemoveAllListeners();

            btnPlayPause.onClick.AddListener(
                TogglePlayPause);
        }

        if (btnStop != null)
        {
            btnStop.onClick.RemoveAllListeners();

            btnStop.onClick.AddListener(
                StopVideo);
        }

        if (btnBack != null)
        {
            btnBack.onClick.RemoveAllListeners();

            btnBack.onClick.AddListener(
                Back10Seconds);
        }

        if (btnForward != null)
        {
            btnForward.onClick.RemoveAllListeners();

            btnForward.onClick.AddListener(
                Forward10Seconds);
        }
    }

    public void Open()
    {
		if (
			videoData == null ||
			string.IsNullOrWhiteSpace(
				videoData.videoURL)
		)
		{
			return;
		}
		Transform sourceVP =
			transform.Find(
				"Video/Video Player");

		if (sourceVP != null)
		{
			sourceVideoPlayer =
				sourceVP.GetComponent<VideoPlayer>();
		}

		if (sourceVideoPlayer != null)
		{
			savedTime =
				sourceVideoPlayer.time;

			sourceWasPlaying =
				sourceVideoPlayer.isPlaying;
				
			viewerShouldPlay =
				sourceWasPlaying;

			sourceVideoPlayer.Pause();

			LCSIAVideoURL sourceVideo =
				GetComponent<LCSIAVideoURL>();

			if (sourceVideo != null)
			{
				sourceVideo.UpdatePlayPauseIcon();
			}
		}

        if (titleText != null)
        {
            titleText.text =
                videoData.title;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                videoData.description;
        }

        if (
            viewerVideoPlayer == null ||
            viewerTexture == null
        )
        {
            return;
        }

        viewerVideoPlayer.Stop();

		viewerVideoPlayer.source =
			VideoSource.Url;

		viewerVideoPlayer.url =
			videoData.videoURL;

        viewerVideoPlayer.targetTexture =
            viewerTexture;

        viewerVideoPlayer.prepareCompleted -=
            OnPrepared;

        viewerVideoPlayer.prepareCompleted +=
            OnPrepared;

        viewerVideoPlayer.Prepare();

viewerVideoPlayer.Prepare();

if (
    imageViewerCanvas != null
)
{
    bool isVR =
        XRSettings.enabled &&
        XRSettings.isDeviceActive;

    if(isVR)
    {
        Canvas canvas =
            imageViewerCanvas.GetComponentInChildren<Canvas>(
                true);

        if(canvas != null)
        {
            canvas.renderMode =
                RenderMode.WorldSpace;
        }

        Camera cam =
            Camera.main;

        if(cam != null)
        {
            imageViewerCanvas.transform.position =
                cam.transform.position +
                cam.transform.forward * 2f;

            imageViewerCanvas.transform.rotation =
                Quaternion.LookRotation(
                    imageViewerCanvas.transform.position -
                    cam.transform.position);

            imageViewerCanvas.transform.localScale =
                Vector3.one * 0.002f;
        }
    }

    imageViewerCanvas.SetActive(
        true);
}
    }

    void OnPrepared(
        VideoPlayer vp)
    {
        if (
            largeImage != null &&
            vp.width > 0 &&
            vp.height > 0
        )
        {
            RectTransform rt =
                largeImage.GetComponent<RectTransform>();

            if (rt != null)
            {
                float aspect =
                    (float)vp.width /
                    vp.height;

                float width =
                    1200f;

                float height =
                    width / aspect;

                rt.sizeDelta =
                    new Vector2(
                        width,
                        height);
            }
        }

		vp.time =
			savedTime;

		if (sourceWasPlaying)
		{
			vp.Play();
		}
		else
		{
			vp.Play();

			vp.Pause();
		}

		UpdatePlayPauseIcon();
    }

	public void Close()
	{
		if (
			viewerVideoPlayer != null &&
			sourceVideoPlayer != null
		)
		{
			double currentTime =
				viewerVideoPlayer.time;

			bool viewerWasPlaying =
				viewerShouldPlay;

			sourceVideoPlayer.time =
				currentTime;

			if (viewerWasPlaying)
			{
				sourceVideoPlayer.Play();
			}
			else
			{
				sourceVideoPlayer.Pause();
			}

			LCSIAVideoURL sourceVideo =
				GetComponent<LCSIAVideoURL>();

			if (sourceVideo != null)
			{
				sourceVideo.UpdatePlayPauseIcon();
			}

			viewerVideoPlayer.Stop();
		}

		if (
			imageViewerCanvas != null
		)
		{
			imageViewerCanvas.SetActive(
				false);
		}
	}

    void TogglePlayPause()
    {
        if (
            viewerVideoPlayer == null
        )
        {
            return;
        }

		if (
			viewerVideoPlayer.isPlaying
		)
		{
			viewerVideoPlayer.Pause();

			viewerShouldPlay =
				false;
		}
		else
		{
			viewerVideoPlayer.Play();

			viewerShouldPlay =
				true;
		}

        UpdatePlayPauseIcon();
    }

    void StopVideo()
    {
        if (
            viewerVideoPlayer == null
        )
        {
            return;
        }

        viewerVideoPlayer.Pause();

        viewerVideoPlayer.frame = 0;

        UpdatePlayPauseIcon();
    }

    void Back10Seconds()
    {
        if (
            viewerVideoPlayer == null
        )
        {
            return;
        }

        viewerVideoPlayer.time =
            System.Math.Max(
                0d,
                viewerVideoPlayer.time - 10d);
    }

    void Forward10Seconds()
    {
        if (
            viewerVideoPlayer == null
        )
        {
            return;
        }

        viewerVideoPlayer.time +=
            10d;
    }

    void UpdatePlayPauseIcon()
    {
        if (
            imgPlayPause == null ||
            viewerVideoPlayer == null
        )
        {
            return;
        }

        if (
            viewerVideoPlayer.isPlaying
        )
        {
            if (pauseSprite != null)
            {
                imgPlayPause.sprite =
                    pauseSprite;
            }
        }
        else
        {
            if (playSprite != null)
            {
                imgPlayPause.sprite =
                    playSprite;
            }
        }
    }
}