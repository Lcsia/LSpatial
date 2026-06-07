using UnityEngine;

public static class LCSIAPlayer {
    public static GameObject GetPlayer() { return GameObject.FindGameObjectWithTag("Player"); }
    public static bool Exists() { return GetPlayer() != null; }
    public static Transform GetTransform() { GameObject p = GetPlayer(); return p != null ? p.transform : null; }
    public static CharacterController GetCharacterController() { GameObject p = GetPlayer(); return p != null ? p.GetComponent<CharacterController>() : null; }
    public static LCSIAPlayerController GetController() { GameObject p = GetPlayer(); return p != null ? p.GetComponent<LCSIAPlayerController>() : null; }
    public static Camera GetCamera() { return Camera.main; }
    public static Vector3 GetPosition() { Transform t = GetTransform(); return t != null ? t.position : Vector3.zero; }
    public static float GetX() { return GetPosition().x; }
    public static float GetY() { return GetPosition().y; }
    public static float GetZ() { return GetPosition().z; }
    public static Quaternion GetRotation() { Transform t = GetTransform(); return t != null ? t.rotation : Quaternion.identity; }
    public static Vector3 GetEulerAngles() { Transform t = GetTransform(); return t != null ? t.eulerAngles : Vector3.zero; }
    public static Vector3 GetForward() { Transform t = GetTransform(); return t != null ? t.forward : Vector3.forward; }
    public static Vector3 GetRight() { Transform t = GetTransform(); return t != null ? t.right : Vector3.right; }
    public static Vector3 GetUp() { Transform t = GetTransform(); return t != null ? t.up : Vector3.up; }
    public static void SetPosition(Vector3 position) { GameObject p = GetPlayer(); if (p == null) return; CharacterController cc = GetCharacterController(); if (cc != null) cc.enabled = false; p.transform.position = position; if (cc != null) cc.enabled = true; }
    public static void Teleport(float x, float y, float z) { SetPosition(new Vector3(x, y, z)); }
    public static void Translate(Vector3 movement) { Transform t = GetTransform(); if (t != null) t.position += movement; }
    public static void SetRotation(Vector3 eulerAngles) { Transform t = GetTransform(); if (t != null) t.eulerAngles = eulerAngles; }
    public static void LookAt(Vector3 target) { Transform t = GetTransform(); if (t != null) t.LookAt(target); }
    public static float GetDistance(Vector3 position) { return Vector3.Distance(GetPosition(), position); }
    public static float GetDistance(Transform target) { return target != null ? Vector3.Distance(GetPosition(), target.position) : 0f; }
    public static bool IsGrounded() { CharacterController cc = GetCharacterController(); return cc != null && cc.isGrounded; }
    public static string GetName() { GameObject p = GetPlayer(); return p != null ? p.name : ""; }
    public static Vector3 GetScale() { Transform t = GetTransform(); return t != null ? t.localScale : Vector3.one; }
    public static void SetActive(bool value) { GameObject p = GetPlayer(); if (p != null) p.SetActive(value); }
    public static void EnableMovement(bool value) { LCSIAPlayerController c = GetController(); if (c != null) c.enabled = value; }
    public static bool IsMovementEnabled() { LCSIAPlayerController c = GetController(); return c != null && c.enabled; }
    public static Ray GetViewRay() { Camera cam = GetCamera(); return cam != null ? cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)) : new Ray(); }
    public static bool GetLookHit(out RaycastHit hit, float maxDistance = 100f) { return Physics.Raycast(GetViewRay(), out hit, maxDistance); }
    public static GameObject GetLookObject(float maxDistance = 100f) { RaycastHit hit; return GetLookHit(out hit, maxDistance) ? hit.collider.gameObject : null; }
    public static bool IsLookingAt(GameObject target, float maxDistance = 100f) { return target != null && GetLookObject(maxDistance) == target; }
    public static Vector3 GetCameraForward() { Camera cam = GetCamera(); return cam != null ? cam.transform.forward : Vector3.forward; }
    public static Vector3 GetCameraPosition() { Camera cam = GetCamera(); return cam != null ? cam.transform.position : Vector3.zero; }
    public static void DestroyPlayer() { GameObject p = GetPlayer(); if (p != null) Object.Destroy(p); }
}