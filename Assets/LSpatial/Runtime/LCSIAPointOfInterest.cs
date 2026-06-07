using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LCSIAPointOfInterest : MonoBehaviour
{
    [Header("Content")]
    public string title = "Title";

    [TextArea]
    public string description = "Description";

    [Header("Display")]
    public float visibleDistance = 10f;

    private int maxCharacters = 350;

    private Transform player;

    private TMP_Text titleText;
    private TMP_Text descriptionText;

    private GameObject point;
    private GameObject background;

    private Vector3 pointScale;

    void Start()
    {
        GameObject playerObj =
            GameObject.FindGameObjectWithTag(
                "Player");

        if (playerObj != null)
        {
            player =
                playerObj.transform;
        }

        Transform canvas =
            transform.Find("Canvas");

        if (canvas != null)
        {
            point =
                canvas.Find("Point")?.gameObject;

            background =
                canvas.Find("Background")?.gameObject;

            Transform titleObj =
                canvas.Find("Title");

            Transform textObj =
                canvas.Find("Text");

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
        }

        UpdateTexts();

        if (point != null)
        {
            pointScale =
                point.transform.localScale;
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
        }
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
            string text =
                description;

            if (
                maxCharacters > 0 &&
                text.Length > maxCharacters
            )
            {
                text =
                    text.Substring(
                        0,
                        maxCharacters) +
                    "...";
            }

            descriptionText.text =
                text;
        }
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
    }
}