using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;

public class LCSIAImageViewer : MonoBehaviour
{
    private GameObject imageViewerCanvas;

    private RawImage largeImage;

    private TMP_Text titleText;
    private TMP_Text descriptionText;

    private Button linkButton;
    private Button closeButton;

    private AspectRatioFitter imageFitter;

    private LCSIAImage imageData;

    private string currentLink =
        "";

    void Awake()
    {
        CacheReferences();

        imageData =
            GetComponent<LCSIAImage>();

        if (
            linkButton != null
        )
        {
            linkButton.onClick.RemoveListener(
                OpenLink);

            linkButton.onClick.AddListener(
                OpenLink);
        }

        if (
            closeButton != null
        )
        {
            closeButton.onClick.RemoveListener(
                Close);

            closeButton.onClick.AddListener(
                Close);
        }

        if (
            imageViewerCanvas != null
        )
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

        if (
            t != null
        )
        {
            imageViewerCanvas =
                t.gameObject;
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/ImagePanel/LargeImage");

        if (
            t != null
        )
        {
            largeImage =
                t.GetComponent<RawImage>();

            imageFitter =
                t.GetComponent<AspectRatioFitter>();
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/InfoPanel/Title");

        if (
            t != null
        )
        {
            titleText =
                t.GetComponent<TMP_Text>();
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/InfoPanel/Description");

        if (
            t != null
        )
        {
            descriptionText =
                t.GetComponent<TMP_Text>();
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/InfoPanel/LinkButton");

        if (
            t != null
        )
        {
            linkButton =
                t.GetComponent<Button>();
        }

        t =
            transform.Find(
                "ViewCanvas/ImageViewerCanvas/Window/Background/CloseButton");

        if (
            t != null
        )
        {
            closeButton =
                t.GetComponent<Button>();
        }
    }

    public void Open()
    {
        if (
            imageData == null
        )
        {
            return;
        }

        if (
            largeImage != null
        )
        {
            largeImage.texture =
                imageData.image;

            if (
                imageFitter != null &&
                imageData.image != null
            )
            {
                imageFitter.aspectRatio =
                    (float)imageData.image.width /
                    imageData.image.height;
            }
        }

        if (
            titleText != null
        )
        {
            titleText.text =
                imageData.title;
        }

        if (
            descriptionText != null
        )
        {
            descriptionText.text =
                imageData.description;
        }

        currentLink =
            imageData.link;

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
					imageViewerCanvas.GetComponent<Canvas>();

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

    public void Close()
    {
        if (
            imageViewerCanvas != null
        )
        {
            imageViewerCanvas.SetActive(
                false);
        }
    }

    public void OpenLink()
    {
        if (
            string.IsNullOrWhiteSpace(
                currentLink)
        )
        {
            return;
        }

        Application.OpenURL(
            currentLink);
    }
}