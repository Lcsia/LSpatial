using System.Collections;
using UnityEngine;

public class LCSIASpawnParabola : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject prefab;

    [Header("Area")]
    public float radius = 3f;

    [Header("Jump")]
    public float jumpHeight = 2f;
    public float duration = 1f;

    [Header("Scale")]
    public float startScale = 0.1f;
    public float endScale = 1f;

    public void Spawn()
    {
        if (prefab == null)
            return;

        Vector3 startPos = transform.position;

        Vector2 randomCircle = Random.insideUnitCircle.normalized * radius;

        Vector3 endPos = transform.position +
                         new Vector3(randomCircle.x, 0f, randomCircle.y);

        GameObject obj = Instantiate(prefab, startPos, Quaternion.identity);

        obj.transform.localScale = Vector3.one * startScale;

        StartCoroutine(JumpRoutine(obj.transform, startPos, endPos));
    }

    private IEnumerator JumpRoutine(Transform target, Vector3 startPos, Vector3 endPos)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float progress = Mathf.Clamp01(t / duration);

            Vector3 pos = Vector3.Lerp(startPos, endPos, progress);

            float height = 4f * jumpHeight * progress * (1f - progress);

            pos.y += height;

            target.position = pos;

            float scale = Mathf.Lerp(startScale, endScale, progress);

            target.localScale = Vector3.one * scale;

            yield return null;
        }

        target.position = endPos;
        target.localScale = Vector3.one * endScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}