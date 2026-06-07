using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LCSIANotification : MonoBehaviour
{
    private static LCSIANotification instance;

    private Canvas canvas1;

    private RectTransform container1;

    private readonly List<GameObject> notifications1 =
        new List<GameObject>();

    /// <summary>
    /// Shows a notification.
    /// </summary>
    public static void Show(
        string message1,
        float duration1 = 3f)
    {
        EnsureInstance();

        instance.CreateNotification(
            message1,
            duration1);
    }

    /// <summary>
    /// Removes all notifications.
    /// </summary>
    public static void Clear()
    {
        if (instance == null)
            return;

        foreach (GameObject obj1 in instance.notifications1)
        {
            if (obj1 != null)
                Destroy(obj1);
        }

        instance.notifications1.Clear();
    }

    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        GameObject obj1 =
            new GameObject(
                "LCSIA_Notifications");

        instance =
            obj1.AddComponent<
                LCSIANotification>();

        DontDestroyOnLoad(
            obj1);

        instance.CreateCanvas();
    }

    private void CreateCanvas()
    {
        canvas1 =
            gameObject.AddComponent<
                Canvas>();

        canvas1.renderMode =
            RenderMode.ScreenSpaceOverlay;

        gameObject.AddComponent<
            CanvasScaler>();

        gameObject.AddComponent<
            GraphicRaycaster>();

        GameObject containerObj1 =
            new GameObject(
                "Container");

        containerObj1.transform.SetParent(
            transform,
            false);

        container1 =
            containerObj1.AddComponent<
                RectTransform>();

        container1.anchorMin =
            new Vector2(
                0.5f,
                0f);

        container1.anchorMax =
            new Vector2(
                0.5f,
                0f);

        container1.pivot =
            new Vector2(
                0.5f,
                0f);

        container1.anchoredPosition =
            new Vector2(
                0f,
                30f);

        container1.sizeDelta =
            new Vector2(
                800f,
                500f);
    }

    private void CreateNotification(
        string message1,
        float duration1)
    {
        GameObject panel1 =
            new GameObject(
                "Notification");

        panel1.transform.SetParent(
            container1,
            false);

        Image image1 =
            panel1.AddComponent<
                Image>();

        image1.color =
            new Color32(
                0,
                0,
                0,
                200);

        RectTransform rect1 =
            panel1.GetComponent<
                RectTransform>();

        rect1.anchorMin =
            new Vector2(
                0.5f,
                0f);

        rect1.anchorMax =
            new Vector2(
                0.5f,
                0f);

        rect1.pivot =
            new Vector2(
                0.5f,
                0f);

        rect1.sizeDelta =
            new Vector2(
                500f,
                40f);

        GameObject textObj1 =
            new GameObject(
                "Text");

        textObj1.transform.SetParent(
            panel1.transform,
            false);

        TextMeshProUGUI tmp1 =
            textObj1.AddComponent<
                TextMeshProUGUI>();

        tmp1.text =
            message1;

        tmp1.color =
            Color.white;

        tmp1.alignment =
            TextAlignmentOptions.Center;

        tmp1.enableAutoSizing =
            true;

        tmp1.fontSizeMin =
            12f;

        tmp1.fontSizeMax =
            24f;

        RectTransform textRect1 =
            textObj1.GetComponent<
                RectTransform>();

        textRect1.anchorMin =
            Vector2.zero;

        textRect1.anchorMax =
            Vector2.one;

        textRect1.offsetMin =
            new Vector2(
                10f,
                0f);

        textRect1.offsetMax =
            new Vector2(
                -10f,
                0f);

        notifications1.Insert(
            0,
            panel1);

        RefreshPositions();

        StartCoroutine(
            DestroyNotification(
                panel1,
                duration1));
    }

    private IEnumerator DestroyNotification(
        GameObject panel1,
        float duration1)
    {
        yield return
            new WaitForSeconds(
                duration1);

        notifications1.Remove(
            panel1);

        Destroy(
            panel1);

        RefreshPositions();
    }

    private void RefreshPositions()
    {
        for (
            int i1 = 0;
            i1 < notifications1.Count;
            i1++)
        {
            RectTransform rect1 =
                notifications1[i1]
                .GetComponent<
                    RectTransform>();

            rect1.anchoredPosition =
                new Vector2(
                    0f,
                    i1 * 45f);
        }
    }
}