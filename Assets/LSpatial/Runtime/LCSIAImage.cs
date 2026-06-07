using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class LCSIAImage : MonoBehaviour
{
    [Header("Image")]
    public Texture2D image;

    public float imageWidth =
        2f;

    public bool preserveAspect =
        true;
	
	[Header("Content")]
	public string title = "Title";

	[TextArea(3, 10)]
	public string description = "Description";

	public string link = "Link";
	
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
    public Vector3 imageOffset =
        Vector3.zero;
		

    private Transform imageQuad;
    private Transform passepartoutQuad;

    private Transform frameRoot;

    private Transform frameTop;
    private Transform frameBottom;
    private Transform frameLeft;
    private Transform frameRight;

    private MeshRenderer imageRenderer;
    private MeshRenderer passepartoutRenderer;

    private Material imageMaterialInstance;
    private Material passepartoutMaterialInstance;
    private Material frameMaterialInstance;
	
	private RectTransform clickCanvas;
	private Button button;
	
	private GameObject backgroundFrameTextTitle;
	private TMP_Text frameTextTitle;

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
            imageQuad == null ||
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

        imageRenderer =
            imageQuad.GetComponent<MeshRenderer>();

        passepartoutRenderer =
            passepartoutQuad.GetComponent<MeshRenderer>();

        LoadMaterials();

        UpdateImage();

        UpdatePassepartout();

        UpdateFrame();
		
		UpdateClickCanvas();
		
		UpdateTitle();
		
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
            imageQuad == null
        )
        {
            imageQuad =
                transform.Find(
                    "Image");
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
            imageMaterialInstance == null
        )
        {
            Material source =
                AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/LSpatial/Materials/Image.mat");

            if (
                source != null
            )
            {
                imageMaterialInstance =
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
                    "Assets/LSpatial/Materials/Frame.mat");

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

    void UpdateImage()
    {
        if (
            image == null ||
            imageRenderer == null
        )
        {
            return;
        }

        if (
            imageMaterialInstance != null
        )
        {
            imageRenderer.sharedMaterial =
                imageMaterialInstance;

            if (
                imageMaterialInstance.HasProperty(
                    "_BaseMap")
            )
            {
                imageMaterialInstance.SetTexture(
                    "_BaseMap",
                    image);
            }

            if (
                imageMaterialInstance.HasProperty(
                    "_MainTex")
            )
            {
                imageMaterialInstance.SetTexture(
                    "_MainTex",
                    image);
            }
        }

        float imageHeight =
            imageWidth;

        if (
            preserveAspect
        )
        {
            float aspect =
                (float)image.width /
                image.height;

            imageHeight =
                imageWidth /
                aspect;
        }

        imageQuad.localScale =
            new Vector3(
                imageWidth,
                imageHeight,
                1f);

        imageQuad.localPosition =
            imageOffset;
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

        float imageHeight =
            imageQuad.localScale.y;

        float width =
            imageWidth +
            passepartoutSize * 2f;

        float height =
            imageHeight +
            passepartoutSize * 2f;

        passepartoutQuad.localScale =
            new Vector3(
                width,
                height,
                1f);

        passepartoutQuad.localPosition =
            imageOffset +
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

        float imageHeight =
            imageQuad.localScale.y;

        float passeWidth =
            imageWidth +
            passepartoutSize * 2f;

        float passeHeight =
            imageHeight +
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

		float imageHeight =
			imageQuad.localScale.y;

		float passeWidth =
			imageWidth +
			passepartoutSize * 2f;

		float passeHeight =
			imageHeight +
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

		Debug.Log(
			"Image Clicked",
			this);
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
}