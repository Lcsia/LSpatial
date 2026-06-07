using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LCSIAInteractable : MonoBehaviour
{
    [Header("Content")]
    public string title = "Title";

    [TextArea]
    public string description = "Description";

    public LCSIAInteractionKey interactionKey =
        LCSIAInteractionKey.E;

    [Header("Distance")]
    public float visibleDistance = 10f;

    public float interactDistance = 3f;

    [Header("Events")]
    public UnityEvent onInteract;

    private Transform player;

    private TMP_Text titleText;
    private TMP_Text descriptionText;
    private TMP_Text keyText;

    private GameObject point;
    private GameObject background;
    private GameObject keyBackground;

    private Image backgroundImage;

    private Vector3 pointScale;

    private Color normalColor;
    private Color hoverColor;

    private Button button;

    void Start()
    {
        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player =
                playerObj.transform;
        }

        Transform canvas =
            transform.Find("Canvas");

        if (canvas != null)
        {
            Transform buttonObj =
                canvas.Find("Button");

            if (buttonObj != null)
            {
                button =
                    buttonObj.GetComponent<Button>();

                if (button != null)
                {
                    button.onClick.AddListener(
                        Interact);
                }
            }

            point =
                canvas.Find("Point")?.gameObject;

            background =
                canvas.Find("Background")?.gameObject;

            keyBackground =
                canvas.Find("KeyBackground")?.gameObject;

            Transform titleObj =
                canvas.Find("Title");

            Transform textObj =
                canvas.Find("Text");

            Transform keyObj =
                canvas.Find("KeyBackground/Key");

            if (titleObj != null)
            {
                titleText =
                    titleObj.GetComponent<TMP_Text>();
            }

            if (textObj != null)
            {
                descriptionText =
                    textObj.GetComponent<TMP_Text>();
            }

            if (keyObj != null)
            {
                keyText =
                    keyObj.GetComponent<TMP_Text>();
            }

            if (background != null)
            {
                backgroundImage =
                    background.GetComponent<Image>();
            }
        }

        UpdateTexts();

        if (point != null)
        {
            pointScale =
                point.transform.localScale;
        }

        if (backgroundImage != null)
        {
            normalColor =
                backgroundImage.color;

            hoverColor =
                new Color(
                    normalColor.r,
                    normalColor.g,
                    normalColor.b,
                    Mathf.Min(
                        normalColor.a + 0.25f,
                        1f));
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position);

        if (distance > visibleDistance)
        {
            if (point != null)
                point.SetActive(true);

            if (background != null)
                background.SetActive(false);
			
			

            if (titleText != null)
                titleText.gameObject.SetActive(false);

            if (descriptionText != null)
                descriptionText.gameObject.SetActive(false);

            if (keyBackground != null)
                keyBackground.SetActive(false);
			
			if (button != null)
			{
				Image img =
					button.GetComponent<Image>();

				if (img != null)
				{
					Color c =
						img.color;

					c.a = 0f;

					img.color = c;
				}
			}

            AnimatePoint();
        }
        else
        {
            if (point != null)
                point.SetActive(false);

            if (background != null)
                background.SetActive(true);

            if (titleText != null)
                titleText.gameObject.SetActive(true);

            if (descriptionText != null)
                descriptionText.gameObject.SetActive(true);

            if (keyBackground != null)
            {
                keyBackground.SetActive(
                    distance <= interactDistance &&
                    interactionKey !=
                    LCSIAInteractionKey.None);
            }
			
			if (button != null)
			{
				Image img =
					button.GetComponent<Image>();

				if (img != null)
				{
					Color c =
						img.color;

					c.a = .20f;

					img.color = c;
				}
			}

        }

        if (
            distance <= interactDistance &&
            interactionKey !=
            LCSIAInteractionKey.None
        )
        {
            if (
                Input.GetKeyDown(
                    GetKeyCode(
                        interactionKey))
            )
            {
                Interact();
            }
        }

    }

    public void Interact()
    {
        onInteract.Invoke();

		if (button != null)
		{
			button.Select();

			StartCoroutine(
				ClearSelection());
		}
    }
	
	IEnumerator ClearSelection()
	{
		yield return null;

		button.OnDeselect(null);
	}

    void AnimatePoint()
    {
        if (point == null)
            return;

        float scale =
            1f +
            Mathf.Sin(
                Time.time * 2f)
            * 0.05f;

        point.transform.localScale =
            pointScale * scale;
    }


    void UpdateTexts()
    {
        if (titleText != null)
        {
            titleText.text =
                title;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                description;
        }

        if (keyText != null)
        {
            if (
                interactionKey ==
                LCSIAInteractionKey.None
            )
            {
                keyText.gameObject
                    .SetActive(false);
            }
            else
            {
                keyText.gameObject
                    .SetActive(true);

                keyText.text =
                    "" +
                    interactionKey.ToString() +
                    "";
            }
        }
    }

    KeyCode GetKeyCode(
        LCSIAInteractionKey key)
    {
        return (KeyCode)
            System.Enum.Parse(
                typeof(KeyCode),
                key.ToString());
    }

    void OnValidate()
    {
        UpdateTexts();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            visibleDistance);

        Gizmos.color =
            Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            interactDistance);
    }
}