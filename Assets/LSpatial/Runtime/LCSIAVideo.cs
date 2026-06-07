using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Video;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class LCSIAVideo : MonoBehaviour
{
    [Header("Video")]
    public VideoClip video;

    public float videoWidth =
        2f;

    public bool preserveAspect =
        true;

    [Header("Content")]
    public string title = "Title";

    [TextArea(3, 10)]
    public string description = "Description";

    //public string link = "Link";

    public bool showTitleBackground =
        true;

    [Header("Passepartout")]
    public bool showPassepartout =
        true;

    public float passepartoutSize =
        0.15f;

    public Color passepartoutColor =
        new Color(
            0.92f,
            0.92f,
            0.92f,
            1f);

    [Header("Frame")]
    public bool showFrame =
        true;

    public float frameThickness =
        0.05f;

    public float frameDepth =
        0.03f;

    public Color frameColor =
        Color.black;

    [Header("Offsets")]
    public Vector3 videoOffset =
        Vector3.zero;

    private Transform videoQuad;
    private Transform passepartoutQuad;

    private Transform frameRoot;

    private Transform frameTop;
    private Transform frameBottom;
    private Transform frameLeft;
    private Transform frameRight;

    private MeshRenderer videoRenderer;
    private MeshRenderer passepartoutRenderer;

    private Material videoMaterialInstance;
    private Material passepartoutMaterialInstance;
    private Material frameMaterialInstance;

    private VideoPlayer videoPlayer;

    private RectTransform clickCanvas;
    private Button button;

    private GameObject backgroundFrameTextTitle;
    private TMP_Text frameTextTitle;

    private float currentVideoHeight =
        1f;
		
	private Button btnBack;
	private Button btnPlayPause;
	private Button btnForward;
	private Button btnStop;

	private Image imgPlayPause;

	[Header("Icons")]
	public Sprite playSprite;
	public Sprite pauseSprite;
	public Sprite stopSprite;

    void Awake()
    {
        Build();
    }

    void OnEnable()
    {
        Build();
    }

#if UNITY_EDITOR

    void OnValidate()
    {
        Build();
    }

#endif

    void Build()
    {
        CacheReferences();

        if (
            videoQuad == null ||
            passepartoutQuad == null ||
            frameRoot == null ||
            frameTop == null ||
            frameBottom == null ||
            frameLeft == null ||
            frameRight == null
        )
        {
            return;
        }

        videoRenderer =
            videoQuad.GetComponent<MeshRenderer>();

        passepartoutRenderer =
            passepartoutQuad.GetComponent<MeshRenderer>();

        LoadMaterials();

        PrepareVideo();

        UpdatePassepartout();

        UpdateFrame();

        UpdateClickCanvas();

        UpdateTitle();
		
		ConnectButtons();

        if (
            button != null
        )
        {
            button.onClick.RemoveListener(
                OnClick);

            button.onClick.AddListener(
                OnClick);
        }

        if (
            backgroundFrameTextTitle == null
        )
        {
            Transform t =
                transform.Find(
                    "ClickCanvas/Canvas/BackgroundFrameTextTitle");

            if (
                t != null
            )
            {
                backgroundFrameTextTitle =
                    t.gameObject;
            }
        }
		

    }

    void CacheReferences()
    {
        if (
            videoQuad == null
        )
        {
            videoQuad =
                transform.Find(
                    "Video");
        }

	if (
		videoPlayer == null
	)
	{
		Transform vp =
			transform.Find(
				"Video/Video Player");

		if (
			vp != null
		)
		{
			videoPlayer =
				vp.GetComponent<VideoPlayer>();
		}
	}

        if (
            passepartoutQuad == null
        )
        {
            passepartoutQuad =
                transform.Find(
                    "Passepartout");
        }

        if (
            frameRoot == null
        )
        {
            frameRoot =
                transform.Find(
                    "Frame");
        }

        if (
            frameRoot == null
        )
        {
            return;
        }

        if (
            frameTop == null
        )
        {
            frameTop =
                frameRoot.Find(
                    "FrameTop");
        }

        if (
            frameBottom == null
        )
        {
            frameBottom =
                frameRoot.Find(
                    "FrameBottom");
        }

        if (
            frameLeft == null
        )
        {
            frameLeft =
                frameRoot.Find(
                    "FrameLeft");
        }

        if (
            frameRight == null
        )
        {
            frameRight =
                frameRoot.Find(
                    "FrameRight");
        }

        if (
            clickCanvas == null
        )
        {
            Transform t =
                transform.Find(
                    "ClickCanvas/Canvas");

            if (
                t != null
                )
            {
                clickCanvas =
                    t.GetComponent<RectTransform>();
            }
        }

        if (
            button == null
        )
        {
            Transform t =
                transform.Find(
                    "ClickCanvas/Canvas/Button");

            if (
                t != null
            )
            {
                button =
                    t.GetComponent<Button>();
            }
        }

        if (
            frameTextTitle == null
        )
        {
            Transform t =
                transform.Find(
                    "ClickCanvas/Canvas/BackgroundFrameTextTitle/FrameTextTitle");

            if (
                t != null
            )
            {
                frameTextTitle =
                    t.GetComponent<TMP_Text>();
            }
        }
    }

    void LoadMaterials()
    {
#if UNITY_EDITOR

        if (
            videoMaterialInstance == null
        )
        {
            Material source =
                AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/LSpatial/Materials/Video.mat");

            if (
                source != null
            )
            {
                videoMaterialInstance =
                    new Material(
                        source);
            }
        }

        if (
            passepartoutMaterialInstance == null
        )
        {
            Material source =
                AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/LSpatial/Materials/Paspar.mat");

            if (
                source != null
            )
            {
                passepartoutMaterialInstance =
                    new Material(
                        source);
            }
        }

        if (
            frameMaterialInstance == null
        )
        {
            Material source =
                AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/LSpatial/Materials/Video.mat");

            if (
                source != null
            )
            {
                frameMaterialInstance =
                    new Material(
                        source);
            }
        }

#endif
    }

    void PrepareVideo()
    {
        if (
            videoPlayer == null ||
            video == null
        )
        {
            return;
        }

        videoPlayer.clip =
            video;

        videoPlayer.prepareCompleted -=
            OnVideoPrepared;

        videoPlayer.prepareCompleted +=
            OnVideoPrepared;

        videoPlayer.Prepare();
		
		videoPlayer.isLooping =
			true;
    }

	void OnVideoPrepared(
		VideoPlayer source)
	{
		UpdateVideo();

		UpdatePassepartout();

		UpdateFrame();

		UpdateClickCanvas();

		if (
			Application.isPlaying
		)
		{
			source.Play();

			source.Pause();

			source.frame = 0;
			
			UpdatePlayPauseIcon();
		}
	}
    void UpdateVideo()
    {
        if (
            videoQuad == null
        )
        {
            return;
        }

        float aspect =
            16f / 9f;

        if (
            preserveAspect &&
            videoPlayer != null &&
            videoPlayer.width > 0 &&
            videoPlayer.height > 0
        )
        {
            aspect =
                (float)videoPlayer.width /
                videoPlayer.height;
        }

        currentVideoHeight =
            videoWidth /
            aspect;

        videoQuad.localScale =
            new Vector3(
                videoWidth,
                currentVideoHeight,
                1f);

        videoQuad.localPosition =
            videoOffset;

        if (
            videoRenderer != null &&
            videoMaterialInstance != null
        )
        {
            videoRenderer.sharedMaterial =
                videoMaterialInstance;
        }
    }

    void UpdatePassepartout()
    {
        passepartoutQuad.gameObject.SetActive(
            showPassepartout);

        if (
            !showPassepartout
        )
        {
            return;
        }

        float width =
            videoWidth +
            passepartoutSize * 2f;

        float height =
            currentVideoHeight +
            passepartoutSize * 2f;

        passepartoutQuad.localScale =
            new Vector3(
                width,
                height,
                1f);

        passepartoutQuad.localPosition =
            videoOffset +
            new Vector3(
                0f,
                0f,
                0.01f);

        if (
            passepartoutRenderer != null &&
            passepartoutMaterialInstance != null
        )
        {
            passepartoutRenderer.sharedMaterial =
                passepartoutMaterialInstance;

            passepartoutRenderer.sharedMaterial.color =
                passepartoutColor;
        }
    }

    void UpdateFrame()
    {
        frameRoot.gameObject.SetActive(
            showFrame);

        if (
            !showFrame
        )
        {
            return;
        }

        float passeWidth =
            videoWidth +
            passepartoutSize * 2f;

        float passeHeight =
            currentVideoHeight +
            passepartoutSize * 2f;

        float outerWidth =
            passeWidth +
            frameThickness * 2f;

        float z =
            frameDepth * 0.5f +
            0.02f;

        frameTop.localScale =
            new Vector3(
                outerWidth,
                frameThickness,
                frameDepth);

        frameTop.localPosition =
            new Vector3(
                0f,
                passeHeight * 0.5f +
                frameThickness * 0.5f,
                z);

        frameBottom.localScale =
            new Vector3(
                outerWidth,
                frameThickness,
                frameDepth);

        frameBottom.localPosition =
            new Vector3(
                0f,
                -passeHeight * 0.5f -
                frameThickness * 0.5f,
                z);

        frameLeft.localScale =
            new Vector3(
                frameThickness,
                passeHeight,
                frameDepth);

        frameLeft.localPosition =
            new Vector3(
                -passeWidth * 0.5f -
                frameThickness * 0.5f,
                0f,
                z);

        frameRight.localScale =
            new Vector3(
                frameThickness,
                passeHeight,
                frameDepth);

        frameRight.localPosition =
            new Vector3(
                passeWidth * 0.5f +
                frameThickness * 0.5f,
                0f,
                z);

        SetFrameMaterial(frameTop);
        SetFrameMaterial(frameBottom);
        SetFrameMaterial(frameLeft);
        SetFrameMaterial(frameRight);
    }

    void SetFrameMaterial(
        Transform part)
    {
        if (
            part == null ||
            frameMaterialInstance == null
        )
        {
            return;
        }

        MeshRenderer renderer =
            part.GetComponent<MeshRenderer>();

        if (
            renderer == null
        )
        {
            return;
        }

        renderer.sharedMaterial =
            frameMaterialInstance;

        renderer.sharedMaterial.color =
            frameColor;
    }

    void UpdateClickCanvas()
    {
        if (
            clickCanvas == null
        )
        {
            return;
        }

        float passeWidth =
            videoWidth +
            passepartoutSize * 2f;

        float passeHeight =
            currentVideoHeight +
            passepartoutSize * 2f;

        float outerWidth =
            passeWidth +
            frameThickness * 2f;

        float outerHeight =
            passeHeight +
            frameThickness * 2f;

        clickCanvas.sizeDelta =
            new Vector2(
                outerWidth * 100f,
                outerHeight * 100f);

        clickCanvas.localPosition =
            Vector3.zero;
    }

	void OnClick()
	{
		if (
			button == null
		)
		{
			return;
		}

		button.Select();

		if (
			Application.isPlaying
		)
		{
			StartCoroutine(
				ClearSelection());
		}

		LCSIAVideoViewer viewer =
			GetComponent<LCSIAVideoViewer>();

		if (
			viewer != null
		)
		{
			viewer.Open();
		}
	}

    IEnumerator ClearSelection()
    {
        yield return null;

        if (
            EventSystem.current != null
        )
        {
            EventSystem.current.SetSelectedGameObject(
                null);
        }
    }

    void UpdateTitle()
    {
        if (
            backgroundFrameTextTitle != null
        )
        {
            backgroundFrameTextTitle.SetActive(
                showTitleBackground);
        }

        if (
            frameTextTitle != null
        )
        {
            frameTextTitle.text =
                title;
        }
    }
	
	void ConnectButtons()
	{
		Transform t;
		
		t = transform.Find(
			"ClickCanvas/Canvas/Stop/BtnStop");

		if(t != null)
		{
			btnStop =
				t.GetComponent<Button>();

			if(btnStop != null)
			{
				btnStop.onClick.RemoveAllListeners();

				btnStop.onClick.AddListener(
					StopVideo);
			}
		}

		t = transform.Find(
			"ClickCanvas/Canvas/PlayPause/BtnPlayPause");

		if(t != null)
		{
			btnPlayPause =
				t.GetComponent<Button>();

			if(btnPlayPause != null)
			{
				btnPlayPause.onClick.RemoveAllListeners();
				btnPlayPause.onClick.AddListener(
					TogglePlayPause);
			}

			imgPlayPause =
				t.GetComponent<Image>();
		}

		t = transform.Find(
			"ClickCanvas/Canvas/Back/BtnBack");

		if(t != null)
		{
			btnBack =
				t.GetComponent<Button>();

			if(btnBack != null)
			{
				btnBack.onClick.RemoveAllListeners();
				btnBack.onClick.AddListener(
					Back10Seconds);
			}
		}

		t = transform.Find(
			"ClickCanvas/Canvas/Forward/BtnForward");

		if(t != null)
		{
			btnForward =
				t.GetComponent<Button>();

			if(btnForward != null)
			{
				btnForward.onClick.RemoveAllListeners();

				btnForward.onClick.AddListener(
					Forward10Seconds);
			}
		}
	}

	void Back10Seconds()
	{
		if(videoPlayer == null)
		{
			return;
		}

		videoPlayer.time =
			System.Math.Max(
				0d,
				videoPlayer.time - 10d);
	}

	void Forward10Seconds()
	{
		if(videoPlayer == null)
		{
			return;
		}

		videoPlayer.time += 10d;
	}
		
	void StopVideo()
	{
		if(videoPlayer == null)
		{
			return;
		}

		videoPlayer.Pause();

		videoPlayer.frame = 0;

		UpdatePlayPauseIcon();
	}

	void TogglePlayPause()
	{
		if(videoPlayer == null)
		{
			return;
		}

		if(videoPlayer.isPlaying)
		{
			videoPlayer.Pause();
		}
		else
		{
			videoPlayer.Play();
		}

		UpdatePlayPauseIcon();
	}

	public void UpdatePlayPauseIcon()
	{
		if(
			imgPlayPause == null ||
			videoPlayer == null
		)
		{
			return;
		}

		if(videoPlayer.isPlaying)
		{
			imgPlayPause.sprite =
				pauseSprite;
		}
		else
		{
			imgPlayPause.sprite =
				playSprite;
		}
	}
}