using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LCSIAGrabObject : MonoBehaviour
{
    [Header("Keys")]
    public KeyCode takeKey = KeyCode.E;

    public KeyCode leaveKey = KeyCode.Q;

    [Header("Grab")]
    public float grabDistance = 2f;

    public bool holdWhileKeyPressed = false;

    [Header("Avatar Hand Offset")]
    public Vector3 positionOffset = Vector3.zero;

    public Vector3 rotationOffset = Vector3.zero;

    [Header("VR Controllers")]
    public Transform leftController;

    public Transform rightController;

    public InputActionReference leftTrigger;

    public InputActionReference rightTrigger;

    [Header("VR Left Hand Offset")]
    public Vector3 leftPositionOffset =
        new Vector3(
            0f,
            -0.05f,
            0.12f);

    public Vector3 leftRotationOffset =
        new Vector3(
            0f,
            -90f,
            0f);

    [Header("VR Right Hand Offset")]
    public Vector3 rightPositionOffset =
        new Vector3(
            0f,
            -0.05f,
            0.12f);

    public Vector3 rightRotationOffset =
        new Vector3(
            0f,
            90f,
            0f);

    private Transform player;

    private Transform hand;

    private Transform activeHand;

    private bool grabbed = false;

    private bool grabbedByVR = false;

    private bool grabbedByLeftHand = false;

    void Start()
    {
        AutoAssignVRReferences();

        FindPlayer();

        FindHand();
    }

    void Update()
    {
        if (player == null)
            FindPlayer();

        if (hand == null)
            FindHand();

        float distance =
            player != null ?
            Vector3.Distance(
                player.position,
                transform.position) :
            999f;

        bool leftPressed =
            leftTrigger != null &&
            leftTrigger.action != null &&
            leftTrigger.action.IsPressed();

        bool rightPressed =
            rightTrigger != null &&
            rightTrigger.action != null &&
            rightTrigger.action.IsPressed();

        if (!grabbed)
        {
            if (
                distance <= grabDistance &&
                Input.GetKeyDown(
                    takeKey))
            {
                grabbed = true;

                grabbedByVR = false;

                activeHand = hand;

                IgnorePlayerCollision(
                    true);
            }

            else if (
                distance <= grabDistance &&
                leftPressed &&
                leftController != null)
            {
                grabbed = true;

                grabbedByVR = true;

                grabbedByLeftHand = true;

                activeHand =
                    leftController;

                IgnorePlayerCollision(
                    true);
            }

            else if (
                distance <= grabDistance &&
                rightPressed &&
                rightController != null)
            {
                grabbed = true;

                grabbedByVR = true;

                grabbedByLeftHand = false;

                activeHand =
                    rightController;

                IgnorePlayerCollision(
                    true);
            }
        }

        if (!grabbed)
            return;

        if (
            activeHand == null)
        {
            Drop();

            return;
        }

        if (grabbedByVR)
        {
            if (grabbedByLeftHand)
            {
                transform.position =
                    activeHand.position +
                    activeHand.TransformDirection(
                        leftPositionOffset);

                transform.rotation =
                    activeHand.rotation *
                    Quaternion.Euler(
                        leftRotationOffset);

                if (!leftPressed)
                {
                    Drop();
                }
            }
            else
            {
                transform.position =
                    activeHand.position +
                    activeHand.TransformDirection(
                        rightPositionOffset);

                transform.rotation =
                    activeHand.rotation *
                    Quaternion.Euler(
                        rightRotationOffset);

                if (!rightPressed)
                {
                    Drop();
                }
            }
        }
        else
        {
            if (hand == null)
                return;

            transform.position =
                hand.position +
                hand.TransformDirection(
                    positionOffset);

            transform.rotation =
                hand.rotation *
                Quaternion.Euler(
                    rotationOffset);

            if (holdWhileKeyPressed)
            {
                if (!Input.GetKey(
                    takeKey))
                {
                    Drop();
                }
            }
            else
            {
                if (Input.GetKeyDown(
                    leaveKey))
                {
                    Drop();
                }
            }
        }
    }

    void AutoAssignVRReferences()
    {
        if (leftController == null)
        {
            GameObject left =
                GameObject.Find(
                    "Left Controller");

            if (left != null)
            {
                leftController =
                    left.transform;
            }
        }

        if (rightController == null)
        {
            GameObject right =
                GameObject.Find(
                    "Right Controller");

            if (right != null)
            {
                rightController =
                    right.transform;
            }
        }
    }

    void FindPlayer()
    {
        GameObject obj =
            GameObject.FindGameObjectWithTag(
                "Player");

        if (obj != null)
        {
            player =
                obj.transform;
        }
    }

    void FindHand()
    {
        if (player == null)
            return;

        Transform[] bones =
            player.GetComponentsInChildren<Transform>(
                true);

        foreach (Transform bone in bones)
        {
            string n =
                bone.name.ToLower();

            if (
                n.Contains(
                    "righthand"))
            {
                hand = bone;

                return;
            }

            if (
                n.Contains(
                    "right_hand"))
            {
                hand = bone;

                return;
            }

            if (
                n.Contains(
                    "mixamorig:righthand"))
            {
                hand = bone;

                return;
            }
        }
    }

    void IgnorePlayerCollision(
        bool ignore)
    {
        if (player == null)
            return;

        Collider[] playerColliders =
            player.GetComponentsInChildren<Collider>(
                true);

        Collider[] objectColliders =
            GetComponentsInChildren<Collider>(
                true);

        foreach (
            Collider pc
            in playerColliders)
        {
            foreach (
                Collider oc
                in objectColliders)
            {
                if (
                    pc == null ||
                    oc == null)
                    continue;

                Physics.IgnoreCollision(
                    pc,
                    oc,
                    ignore);
            }
        }
    }

    public bool IsGrabbed()
    {
        return grabbed;
    }

    public void Drop()
    {
        grabbed = false;

        grabbedByVR = false;

        activeHand = null;

        IgnorePlayerCollision(
            false);
    }

    public void Grab()
    {
        grabbed = true;

        IgnorePlayerCollision(
            true);
    }

    private void OnDisable()
    {
        IgnorePlayerCollision(
            false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            grabbed ?
            Color.green :
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            grabDistance);
    }

    void Reset()
    {
#if UNITY_EDITOR

        AutoAssignVRReferences();

        if (
            leftTrigger != null &&
            rightTrigger != null)
        {
            return;
        }

        string[] guids =
            AssetDatabase.FindAssets(
                "XRI Default Input Actions t:InputActionAsset");

        if (guids.Length == 0)
            return;

        string path =
            AssetDatabase.GUIDToAssetPath(
                guids[0]);

        InputActionAsset asset =
            AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                path);

        if (asset == null)
            return;

        var leftAction =
            asset.FindAction(
                "XRI LeftHand Interaction/Activate",
                false);

        var rightAction =
            asset.FindAction(
                "XRI RightHand Interaction/Activate",
                false);

        if (leftAction != null)
        {
            leftTrigger =
                InputActionReference.Create(
                    leftAction);
        }

        if (rightAction != null)
        {
            rightTrigger =
                InputActionReference.Create(
                    rightAction);
        }

#endif
    }
}