using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

public static class LCSIAMenu
{
	
	
	[MenuItem("GameObject/LSpatial/Player", false, 10)]
	static void CreatePlayer()
	{
		GameObject existingPlayer =
			GameObject.FindGameObjectWithTag(
				"Player");

		if (existingPlayer != null)
		{
			Selection.activeGameObject =
				existingPlayer;

			Debug.LogWarning(
				"A Player already exists in the scene.");

			return;
		}

		string[] guids =
			AssetDatabase.FindAssets(
				"LCSIA_Player t:Prefab");

		if (guids.Length == 0)
		{
			Debug.LogError(
				"LCSIA_Player prefab not found.");

			return;
		}

		string prefabPath =
			AssetDatabase.GUIDToAssetPath(
				guids[0]);

		GameObject prefab =
			AssetDatabase.LoadAssetAtPath<GameObject>(
				prefabPath);

		if (prefab == null)
		{
			Debug.LogError(
				"Could not load LCSIA_Player.");

			return;
		}

		GameObject player =
			(GameObject)
			PrefabUtility.InstantiatePrefab(
				prefab);

		if (Selection.activeTransform != null)
		{
			player.transform.SetParent(
				Selection.activeTransform);

			player.transform.localPosition =
				Vector3.zero;

			player.transform.localRotation =
				Quaternion.identity;

			player.transform.localScale =
				Vector3.one;
		}

		Undo.RegisterCreatedObjectUndo(
			player,
			"Create Player");

		Selection.activeGameObject =
			player;
	}
	
	[MenuItem("GameObject/LSpatial/Trigger", false, 11)]
	static void CreateTrigger()
	{
		GameObject obj =
			new GameObject(
				"LCSIA_Trigger");

		SphereCollider col =
			obj.AddComponent<SphereCollider>();

		col.isTrigger = true;
		col.radius = 1f;

		LCSIATrigger trigger =
			obj.AddComponent<LCSIATrigger>();

	#if UNITY_EDITOR

		string[] guids =
			AssetDatabase.FindAssets(
				"XRI Default Input Actions t:InputActionAsset");

		if (guids.Length > 0)
		{
			string path =
				AssetDatabase.GUIDToAssetPath(
					guids[0]);

			UnityEngine.InputSystem.InputActionAsset asset =
				AssetDatabase.LoadAssetAtPath<
					UnityEngine.InputSystem.InputActionAsset>(
					path);

			if (asset != null)
			{
				var left =
					asset.FindAction(
						"XRI LeftHand Interaction/Activate",
						false);

				var right =
					asset.FindAction(
						"XRI RightHand Interaction/Activate",
						false);

				if (left != null)
				{
					trigger.leftTrigger =
						UnityEngine.InputSystem.InputActionReference
						.Create(left);
				}

				if (right != null)
				{
					trigger.rightTrigger =
						UnityEngine.InputSystem.InputActionReference
						.Create(right);
				}
			}
		}

	#endif

		if (Selection.activeTransform != null)
		{
			obj.transform.SetParent(
				Selection.activeTransform);

			obj.transform.localPosition =
				Vector3.zero;

			obj.transform.localRotation =
				Quaternion.identity;

			obj.transform.localScale =
				Vector3.one;
		}

		Selection.activeGameObject =
			obj;
	}
    [MenuItem("GameObject/LSpatial/Teleporter", false, 12)]
    static void CreateTeleporter()
    {
        GameObject obj =
            new GameObject(
                "LCSIA_Teleporter");

        SphereCollider col =
            obj.AddComponent<SphereCollider>();

        col.isTrigger = true;
        col.radius = 1f;

        LCSIATeleporter teleporter =
            obj.AddComponent<LCSIATeleporter>();

        GameObject destination =
            new GameObject(
                "Destination");

        destination.transform.SetParent(
            obj.transform);

        destination.transform.localPosition =
            new Vector3(
                0f,
                0f,
                5f);

        destination.transform.localRotation =
            Quaternion.identity;

        teleporter.destination =
            destination.transform;

        if (Selection.activeTransform != null)
        {
            obj.transform.SetParent(
                Selection.activeTransform);

            obj.transform.localPosition =
                Vector3.zero;

            obj.transform.localRotation =
                Quaternion.identity;

            obj.transform.localScale =
                Vector3.one;
        }

        Selection.activeGameObject =
            obj;
    }

	[MenuItem("GameObject/LSpatial/Interactable", false, 13)]
	static void CreateInteractable()
	{
		GameObject prefab =
			AssetDatabase.LoadAssetAtPath<GameObject>(
				"Assets/LSpatial/Prefabs/Interactable.prefab");

		if (prefab == null)
		{
			Debug.LogError(
				"Interactable.prefab not found.");

			return;
		}

		GameObject interactable =
			(GameObject)
			PrefabUtility.InstantiatePrefab(
				prefab);

		if (Selection.activeTransform != null)
		{
			interactable.transform.SetParent(
				Selection.activeTransform);

			interactable.transform.localPosition =
				Vector3.zero;

			interactable.transform.localRotation =
				Quaternion.identity;

			interactable.transform.localScale =
				Vector3.one;
		}

		Undo.RegisterCreatedObjectUndo(
			interactable,
			"Create Interactable");

		Selection.activeGameObject =
			interactable;
	}
	
	[MenuItem("GameObject/LSpatial/Point Of Interest", false, 14)]
	static void CreatePointOfInterest()
	{
		GameObject prefab =
			AssetDatabase.LoadAssetAtPath<GameObject>(
				"Assets/LSpatial/Prefabs/PointOfInterest.prefab");

		if (prefab == null)
		{
			Debug.LogError(
				"PointOfInterest.prefab not found.");

			return;
		}

		GameObject pointOfInterest =
			(GameObject)
			PrefabUtility.InstantiatePrefab(
				prefab);

		if (Selection.activeTransform != null)
		{
			pointOfInterest.transform.SetParent(
				Selection.activeTransform);

			pointOfInterest.transform.localPosition =
				Vector3.zero;

			pointOfInterest.transform.localRotation =
				Quaternion.identity;

			pointOfInterest.transform.localScale =
				Vector3.one;
		}

		Undo.RegisterCreatedObjectUndo(
			pointOfInterest,
			"Create Point Of Interest");

		Selection.activeGameObject =
			pointOfInterest;
	}
	
	[MenuItem("GameObject/LSpatial/Avatar Changer", false, 15)]
	static void CreateAvatarChanger()
	{
		GameObject obj =
			new GameObject(
				"LCSIA_AvatarChanger");

		SphereCollider col =
			obj.AddComponent<
				SphereCollider>();

		col.isTrigger =
			true;

		col.radius =
			1f;

		LCSIAAvatarChanger changer =
			obj.AddComponent<
				LCSIAAvatarChanger>();

		GameObject displayPoint =
			new GameObject(
				"DisplayPoint");

		displayPoint.transform.SetParent(
			obj.transform);

		displayPoint.transform.localPosition =
			Vector3.zero;

		displayPoint.transform.localRotation =
			Quaternion.identity;

		displayPoint.transform.localScale =
			Vector3.one;

		changer.displayPoint =
			displayPoint.transform;

		changer.gizmoRadius =
			1f;

		changer.avatarPrefabs =
			new List<GameObject>();

		string[] avatarNames =
		{
			"PlayerModelR",
			"PlayerModel"
		};

		foreach (
			string avatarName
			in avatarNames)
		{
			string[] avatarGuids =
				AssetDatabase.FindAssets(
					avatarName +
					" t:Prefab");

			if (
				avatarGuids.Length == 0
			)
			{
				Debug.LogWarning(
					avatarName +
					" prefab not found.");

				continue;
			}

			string prefabPath =
				AssetDatabase.GUIDToAssetPath(
					avatarGuids[0]);

			GameObject prefab =
				AssetDatabase.LoadAssetAtPath<GameObject>(
					prefabPath);

			if (
				prefab != null
			)
			{
				changer.avatarPrefabs.Add(
					prefab);
			}
		}

		if (
			Selection.activeTransform != null
		)
		{
			obj.transform.SetParent(
				Selection.activeTransform);

			obj.transform.localPosition =
				Vector3.zero;

			obj.transform.localRotation =
				Quaternion.identity;

			obj.transform.localScale =
				Vector3.one;
		}

		Undo.RegisterCreatedObjectUndo(
			obj,
			"Create Avatar Changer");

		Selection.activeGameObject =
			obj;
	}
	
	[MenuItem("GameObject/LSpatial/Image", false, 20)]
	static void CreateImage()
	{
		GameObject prefab =
			AssetDatabase.LoadAssetAtPath<GameObject>(
				"Assets/LSpatial/Prefabs/LCSIAImage.prefab");

		if (prefab == null)
		{
			Debug.LogError(
				"LCSIAImage.prefab not found.");

			return;
		}

		GameObject image =
			(GameObject)
			PrefabUtility.InstantiatePrefab(
				prefab);

		if (Selection.activeTransform != null)
		{
			image.transform.SetParent(
				Selection.activeTransform);

			image.transform.localPosition =
				Vector3.zero;

			image.transform.localRotation =
				Quaternion.identity;

			image.transform.localScale =
				Vector3.one;
		}

		Undo.RegisterCreatedObjectUndo(
			image,
			"Create LCSIA Image");

		Selection.activeGameObject =
			image;
			
	}
	
	
	[MenuItem("GameObject/LSpatial/Video", false, 21)]
	static void CreateVideo()
	{
		GameObject prefab =
			AssetDatabase.LoadAssetAtPath<GameObject>(
				"Assets/LSpatial/Prefabs/LCSIAVideo.prefab");

		if (prefab == null)
		{
			Debug.LogError(
				"LCSIAVideo.prefab not found.");

			return;
		}

		GameObject video =
			(GameObject)
			PrefabUtility.InstantiatePrefab(
				prefab);

		if (Selection.activeTransform != null)
		{
			video.transform.SetParent(
				Selection.activeTransform);

			video.transform.localPosition =
				Vector3.zero;

			video.transform.localRotation =
				Quaternion.identity;

			video.transform.localScale =
				Vector3.one;
		}

		Undo.RegisterCreatedObjectUndo(
			video,
			"Create LCSIA Video");

		Selection.activeGameObject =
			video;
	}
	
	[MenuItem("GameObject/LSpatial/Video URL", false, 22)]
	static void CreateVideoURL()
	{
		GameObject prefab =
			AssetDatabase.LoadAssetAtPath<GameObject>(
				"Assets/LSpatial/Prefabs/LCSIAVideoURL.prefab");

		if (prefab == null)
		{
			Debug.LogError(
				"LCSIAVideoURL.prefab not found.");

			return;
		}

		GameObject video =
			(GameObject)
			PrefabUtility.InstantiatePrefab(
				prefab);

		if (Selection.activeTransform != null)
		{
			video.transform.SetParent(
				Selection.activeTransform);

			video.transform.localPosition =
				Vector3.zero;

			video.transform.localRotation =
				Quaternion.identity;

			video.transform.localScale =
				Vector3.one;
		}

		Undo.RegisterCreatedObjectUndo(
			video,
			"Create LCSIA Video URL");

		Selection.activeGameObject =
			video;
	}
	
	[MenuItem("GameObject/LSpatial/Event System", false, 100)]
	static void CreateEventSystem()
	{
		GameObject existing =
			GameObject.Find("EventSystem");

		if (existing != null)
		{
			Selection.activeGameObject =
				existing;

			Debug.LogWarning(
				"An EventSystem already exists in the scene.");

			return;
		}

		GameObject prefab =
			AssetDatabase.LoadAssetAtPath<GameObject>(
				"Assets/LSpatial/Prefabs/EventSystem.prefab");

		if (prefab == null)
		{
			Debug.LogError(
				"EventSystem.prefab not found.");

			return;
		}

		GameObject eventSystem =
			(GameObject)
			PrefabUtility.InstantiatePrefab(
				prefab);

		Undo.RegisterCreatedObjectUndo(
			eventSystem,
			"Create EventSystem");

		Selection.activeGameObject =
			eventSystem;
	}

}