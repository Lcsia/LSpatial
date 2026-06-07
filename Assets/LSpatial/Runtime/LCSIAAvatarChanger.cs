using System.Collections.Generic;
using UnityEngine;

public class LCSIAAvatarChanger : MonoBehaviour
{
    [Header("Avatar")]
    public List<GameObject> avatarPrefabs =
        new List<GameObject>();

    public bool instantiateOnStart =
        false;

    public bool triggerOnce =
        false;

    [Header("Display")]
    public Transform displayPoint;

    public bool rotateDisplay =
        true;

    public float displayRotationSpeed =
        45f;

    [Header("Gizmo")]
    public float gizmoRadius =
        1f;

    private int currentIndex =
        0;

    private bool activated =
        false;

    private GameObject displayedAvatar;

    private void Start()
    {
		
		if (
			displayPoint == null
		)
		{
			displayPoint =
				transform;
		}
        UpdateDisplay();

        if (
            !instantiateOnStart
        )
        {
            return;
        }

        LCSIAPlayerAvatar avatarSystem =
            FindFirstObjectByType<
                LCSIAPlayerAvatar>();

        if (
            avatarSystem == null
        )
        {
            return;
        }

        ApplyCurrentAvatar(
            avatarSystem);
    }

    private void Update()
    {
        if (
            rotateDisplay &&
            displayedAvatar != null
        )
        {
            displayedAvatar.transform.Rotate(
                Vector3.up,
                displayRotationSpeed *
                Time.deltaTime,
                Space.World);
        }
    }

    private void OnTriggerEnter(
        Collider other)
    {
        if (
            triggerOnce &&
            activated
        )
        {
            return;
        }

        LCSIAPlayerAvatar avatarSystem =
            other.GetComponent<
                LCSIAPlayerAvatar>();

        if (
            avatarSystem == null
        )
        {
            avatarSystem =
                other.GetComponentInParent<
                    LCSIAPlayerAvatar>();
        }

        if (
            avatarSystem == null
        )
        {
            return;
        }

        ApplyCurrentAvatar(
            avatarSystem);

        activated =
            true;
    }

    private void ApplyCurrentAvatar(
        LCSIAPlayerAvatar avatarSystem)
    {
        if (
            avatarPrefabs == null ||
            avatarPrefabs.Count == 0
        )
        {
            return;
        }

        GameObject prefab =
            avatarPrefabs[
                currentIndex];

        Transform parent =
            avatarSystem.transform;

        Vector3 localPosition =
            Vector3.zero;

        Quaternion localRotation =
            Quaternion.identity;

        Vector3 localScale =
            Vector3.one;

        if (
            avatarSystem.playerModel != null
        )
        {
            parent =
                avatarSystem.playerModel.parent;

            localPosition =
                avatarSystem.playerModel.localPosition;

            localRotation =
                avatarSystem.playerModel.localRotation;

            localScale =
                avatarSystem.playerModel.localScale;

            Destroy(
                avatarSystem.playerModel.gameObject);
        }

        GameObject newAvatar =
            Instantiate(
                prefab,
                parent);

        newAvatar.transform.localPosition =
            localPosition;

        newAvatar.transform.localRotation =
            localRotation;

        newAvatar.transform.localScale =
            localScale;

        Animator animator =
            newAvatar.GetComponentInChildren<
                Animator>();

        avatarSystem.animator =
            animator;

        avatarSystem.playerModel =
            newAvatar.transform;

        currentIndex++;

        if (
            currentIndex >=
            avatarPrefabs.Count
        )
        {
            currentIndex = 0;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (
            displayPoint == null
        )
        {
            return;
        }

        if (
            displayedAvatar != null
        )
        {
            Destroy(
                displayedAvatar);
        }

        if (
            avatarPrefabs == null ||
            avatarPrefabs.Count == 0
        )
        {
            return;
        }

        displayedAvatar =
            Instantiate(
                avatarPrefabs[
                    currentIndex],
                displayPoint);

        displayedAvatar.transform.localPosition =
            Vector3.zero;

        displayedAvatar.transform.localRotation =
            Quaternion.identity;

        displayedAvatar.transform.localScale =
            Vector3.one;

        Animator animator =
            displayedAvatar.GetComponentInChildren<
                Animator>();

        if (
            animator != null
        )
        {
            animator.enabled =
                true;
        }

        Collider[] colliders =
            displayedAvatar.GetComponentsInChildren<
                Collider>(true);

        foreach (
            Collider col
            in colliders)
        {
            col.enabled =
                false;
        }

        Rigidbody[] rigidbodies =
            displayedAvatar.GetComponentsInChildren<
                Rigidbody>(true);

        foreach (
            Rigidbody rb
            in rigidbodies)
        {
            rb.isKinematic =
                true;
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color =
            Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            gizmoRadius);

        if (
            displayPoint != null
        )
        {
            Gizmos.color =
                Color.yellow;

            Gizmos.DrawWireSphere(
                displayPoint.position,
                0.25f);
        }
    }

#endif
}