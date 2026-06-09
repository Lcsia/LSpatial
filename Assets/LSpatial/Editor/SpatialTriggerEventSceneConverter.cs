using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static class SpatialTriggerEventSceneConverter
{
    private const string MenuPath =
        "LSpatial/Convert Active Scene Spatial Trigger Events";

    private sealed class SpatialTriggerRecord
    {
        public long FileId;
        public long GameObjectFileId;
        public readonly List<PersistentCallRecord> OnEnterCalls =
            new List<PersistentCallRecord>();
        public readonly List<PersistentCallRecord> OnExitCalls =
            new List<PersistentCallRecord>();
    }

    private sealed class PersistentCallRecord
    {
        public string MethodName = "";
        public int Mode = 1;
        public int CallState = 2;
        public long TargetFileId;
        public string TargetGuid = "";
        public string TargetAssemblyTypeName = "";
        public long ObjectArgumentFileId;
        public string ObjectArgumentGuid = "";
        public string ObjectArgumentAssemblyTypeName = "";
        public int IntArgument;
        public float FloatArgument;
        public string StringArgument = "";
        public bool BoolArgument;
    }

    private sealed class ConversionResult
    {
        public readonly List<string> Converted =
            new List<string>();
        public readonly List<string> Failed =
            new List<string>();
        public readonly List<string> Skipped =
            new List<string>();
        public readonly List<ReportRow> Rows =
            new List<ReportRow>();
    }

    private sealed class ReportRow
    {
        public string Status = "";
        public string Parent = "";
        public string Grandparent = "";
        public string GreatGrandparent = "";
        public string ObjectName = "";
        public string Script = "SpatialTriggerEvent";
        public string Method = "";
        public string Detail = "";
    }

    private static readonly Regex HeaderRegex =
        new Regex(@"^---\s+!u!(\d+)\s+&(-?\d+)", RegexOptions.Compiled);

    private static readonly Regex FileIdRegex =
        new Regex(@"fileID:\s*(-?\d+)", RegexOptions.Compiled);

    private static readonly Regex GuidRegex =
        new Regex(@"guid:\s*([0-9a-fA-F]+)", RegexOptions.Compiled);

    [MenuItem(MenuPath)]
    private static void ConvertActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (string.IsNullOrEmpty(scene.path))
        {
            EditorUtility.DisplayDialog(
                "Convert Spatial Trigger Events",
                "Save the active scene before converting Spatial Trigger Events.",
                "OK");
            return;
        }

        if (scene.isDirty)
        {
            bool save = EditorUtility.DisplayDialog(
                "Convert Spatial Trigger Events",
                "The active scene has unsaved changes. Save it before conversion?",
                "Save and Convert",
                "Cancel");

            if (!save)
                return;

            EditorSceneManager.SaveScene(scene);
        }

        string sceneText = File.ReadAllText(scene.path);
        if (!sceneText.Contains("%YAML") ||
            !sceneText.Contains("MonoBehaviour:"))
        {
            EditorUtility.DisplayDialog(
                "Convert Spatial Trigger Events",
                "The active scene file does not look like text YAML. Enable Force Text serialization in Unity before running this converter.",
                "OK");
            return;
        }

        ConversionResult result = new ConversionResult();

        List<SpatialTriggerRecord> records =
            ParseSpatialTriggerRecords(sceneText);

        if (records.Count == 0)
        {
            records = LoadRecordsFromOriginalPackage();
        }

        Dictionary<long, UnityEngine.Object> objectsByFileId =
            BuildSceneObjectMap(scene);

        ConversionResult sceneResult =
            ConvertRecords(records, objectsByFileId);

        MergeResult(result, sceneResult, "Scene: " + scene.path);

        int detectedCount = records.Count;
        int prefabDetectedCount = ConvertProjectPrefabs(result);
        detectedCount += prefabDetectedCount;

        EditorSceneManager.MarkSceneDirty(scene);

        string report =
            BuildReport(result, detectedCount);

        SaveConversionCsv(result);

        EditorUtility.DisplayDialog(
            "Spatial Trigger Event Conversion",
            report,
            "OK");

        UnityEngine.Debug.Log(report);
    }

    private static List<SpatialTriggerRecord> LoadRecordsFromOriginalPackage()
    {
        bool usePackage = EditorUtility.DisplayDialog(
            "Convert Spatial Trigger Events",
            "No Spatial Trigger Event data was found in the active scene. This usually means Unity kept only the Missing Script marker after import.\n\nSelect the original Spatial.io unitypackage as the source of the serialized On Enter / On Exit events?",
            "Select Package",
            "Skip");

        if (!usePackage)
            return new List<SpatialTriggerRecord>();

        string packagePath = EditorUtility.OpenFilePanel(
            "Select Original Spatial.io UnityPackage",
            "",
            "unitypackage");

        if (string.IsNullOrEmpty(packagePath))
            return new List<SpatialTriggerRecord>();

        string tempFolder = Path.Combine(
            Path.GetTempPath(),
            "SpatialTriggerConvert_" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(tempFolder);

        try
        {
            if (!ExtractUnityPackage(packagePath, tempFolder))
                return new List<SpatialTriggerRecord>();

            List<SpatialTriggerRecord> packageRecords =
                new List<SpatialTriggerRecord>();

            foreach (string yamlFile in FindUnityYamlAssets(tempFolder))
            {
                string text = File.ReadAllText(yamlFile);
                packageRecords.AddRange(ParseSpatialTriggerRecords(text));
            }

            return packageRecords;
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, true);
            }
            catch
            {
            }
        }
    }

    private static bool ExtractUnityPackage(
        string packagePath,
        string tempFolder)
    {
        Process process = new Process();
        process.StartInfo.FileName = "tar";
        process.StartInfo.Arguments =
            "-xzf \"" + packagePath + "\" -C \"" + tempFolder + "\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;

        try
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
                return true;

            UnityEngine.Debug.LogError(
                "Could not extract unitypackage.\n" +
                output + "\n" + error);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(
                "Could not extract unitypackage. This tool needs the tar command available.\n" +
                ex.Message);
        }

        return false;
    }

    private static List<string> FindUnityYamlAssets(string tempFolder)
    {
        List<string> result = new List<string>();

        foreach (string directory in Directory.GetDirectories(tempFolder))
        {
            string assetFile = Path.Combine(directory, "asset");
            string pathnameFile = Path.Combine(directory, "pathname");

            if (!File.Exists(assetFile) || !File.Exists(pathnameFile))
                continue;

            string originalPath = File.ReadAllText(pathnameFile).Trim();

            if (originalPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) ||
                originalPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ||
                originalPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(assetFile);
            }
        }

        return result;
    }

    private static int ConvertProjectPrefabs(ConversionResult result)
    {
        int detectedCount = 0;
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string prefabGuid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            if (string.IsNullOrEmpty(prefabPath) ||
                !File.Exists(prefabPath))
            {
                continue;
            }

            string prefabText = File.ReadAllText(prefabPath);
            if (!prefabText.Contains("MonoBehaviour:") ||
                !prefabText.Contains("listenFor"))
            {
                continue;
            }

            List<SpatialTriggerRecord> records =
                ParseSpatialTriggerRecords(prefabText);

            if (records.Count == 0)
                continue;

            detectedCount += records.Count;

            GameObject prefabRoot = null;
            try
            {
                prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

                Dictionary<long, UnityEngine.Object> objectsByFileId =
                    BuildGameObjectMap(prefabRoot);

                ConversionResult prefabResult =
                    ConvertRecords(records, objectsByFileId);

                if (prefabResult.Converted.Count > 0)
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

                MergeResult(result, prefabResult, "Prefab: " + prefabPath);
            }
            catch (Exception ex)
            {
                result.Failed.Add(
                    "Prefab: " + prefabPath + " failed: " + ex.Message);
            }
            finally
            {
                if (prefabRoot != null)
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        return detectedCount;
    }

    private static void MergeResult(
        ConversionResult target,
        ConversionResult source,
        string prefix)
    {
        AppendPrefixed(target.Converted, source.Converted, prefix);
        AppendPrefixed(target.Skipped, source.Skipped, prefix);
        AppendPrefixed(target.Failed, source.Failed, prefix);

        foreach (ReportRow row in source.Rows)
        {
            row.Detail = prefix + " | " + row.Detail;
            target.Rows.Add(row);
        }
    }

    private static void AppendPrefixed(
        List<string> target,
        List<string> source,
        string prefix)
    {
        foreach (string line in source)
            target.Add(prefix + " | " + line);
    }

    private static List<SpatialTriggerRecord> ParseSpatialTriggerRecords(
        string sceneText)
    {
        List<SpatialTriggerRecord> records =
            new List<SpatialTriggerRecord>();
        string[] lines = sceneText.Split('\n');

        int index = 0;
        while (index < lines.Length)
        {
            Match header = HeaderRegex.Match(lines[index].Trim());
            if (!header.Success)
            {
                index++;
                continue;
            }

            int classId = int.Parse(header.Groups[1].Value);
            long fileId = long.Parse(header.Groups[2].Value);
            int start = index;
            index++;

            while (index < lines.Length &&
                   !HeaderRegex.IsMatch(lines[index].Trim()))
            {
                index++;
            }

            if (classId != 114)
                continue;

            SpatialTriggerRecord record =
                ParseMonoBehaviourBlock(lines, start, index, fileId);

            if (record != null)
                records.Add(record);
        }

        return records;
    }

    private static SpatialTriggerRecord ParseMonoBehaviourBlock(
        string[] lines,
        int start,
        int end,
        long fileId)
    {
        HashSet<string> fields = new HashSet<string>();

        for (int i = start; i < end; i++)
        {
            string key = ExtractYamlKey(lines[i].Trim());
            if (!string.IsNullOrEmpty(key))
                fields.Add(key);
        }

        if (!LooksLikeSpatialTriggerEvent(fields))
            return null;

        SpatialTriggerRecord record =
            new SpatialTriggerRecord
            {
                FileId = fileId,
                GameObjectFileId = FindFileId(lines, start, end, "m_GameObject")
            };

        ReadPersistentCalls(lines, start, end, record);

        return record;
    }

    private static bool LooksLikeSpatialTriggerEvent(HashSet<string> fields)
    {
        bool hasListenFor =
            HasField(fields, "listenFor");

        bool hasModernEvents =
            HasField(fields, "onEnterEvent") &&
            HasField(fields, "onExitEvent");

        bool hasDeprecatedEvents =
            HasField(fields, "deprecated_onEnter") &&
            HasField(fields, "deprecated_onExit");

        return hasListenFor && (hasModernEvents || hasDeprecatedEvents);
    }

    private static void ReadPersistentCalls(
        string[] lines,
        int start,
        int end,
        SpatialTriggerRecord record)
    {
        List<string> keyStack = new List<string>();
        string currentEventName = "";
        PersistentCallRecord currentCall = null;
        bool insideCalls = false;

        for (int i = start; i < end; i++)
        {
            string raw = lines[i].Replace("\r", "");
            string trimmed = raw.Trim();
            int indent = CountLeadingSpaces(raw);
            string yamlKey = ExtractYamlKey(trimmed);

            if (!string.IsNullOrEmpty(yamlKey))
            {
                int depth = indent / 2;
                while (keyStack.Count > depth)
                    keyStack.RemoveAt(keyStack.Count - 1);

                while (keyStack.Count < depth)
                    keyStack.Add("");

                if (keyStack.Count == depth)
                    keyStack.Add(yamlKey);
                else
                    keyStack[depth] = yamlKey;
            }

            if (trimmed.StartsWith("m_PersistentCalls:", StringComparison.Ordinal))
            {
                currentEventName = FindSpatialEventName(keyStack);
                insideCalls = false;
                continue;
            }

            if (trimmed.StartsWith("m_Calls:", StringComparison.Ordinal))
            {
                insideCalls = true;
                continue;
            }

            if (!insideCalls)
                continue;

            if (trimmed.StartsWith("-", StringComparison.Ordinal))
            {
                AddCallToRecord(record, currentEventName, currentCall);
                currentCall = new PersistentCallRecord();
                ReadCallLine(trimmed.Substring(1).Trim(), currentCall);
                continue;
            }

            if (currentCall == null)
                continue;

            ReadCallLine(trimmed, currentCall);
        }

        AddCallToRecord(record, currentEventName, currentCall);
    }

    private static void AddCallToRecord(
        SpatialTriggerRecord record,
        string eventName,
        PersistentCallRecord call)
    {
        if (call == null || string.IsNullOrEmpty(call.MethodName))
            return;

        if (IsEnterEvent(eventName))
            record.OnEnterCalls.Add(call);
        else if (IsExitEvent(eventName))
            record.OnExitCalls.Add(call);
    }

    private static void ReadCallLine(
        string line,
        PersistentCallRecord call)
    {
        if (line.StartsWith("m_Target:", StringComparison.Ordinal))
        {
            ReadObjectReference(line, out call.TargetFileId, out call.TargetGuid);
            return;
        }

        if (line.StartsWith("m_TargetAssemblyTypeName:", StringComparison.Ordinal))
        {
            call.TargetAssemblyTypeName =
                CleanYamlScalar(line.Substring("m_TargetAssemblyTypeName:".Length));
            return;
        }

        if (line.StartsWith("m_MethodName:", StringComparison.Ordinal))
        {
            call.MethodName =
                CleanYamlScalar(line.Substring("m_MethodName:".Length));
            return;
        }

        if (line.StartsWith("m_Mode:", StringComparison.Ordinal))
        {
            int.TryParse(
                CleanYamlScalar(line.Substring("m_Mode:".Length)),
                out call.Mode);
            return;
        }

        if (line.StartsWith("m_CallState:", StringComparison.Ordinal))
        {
            int.TryParse(
                CleanYamlScalar(line.Substring("m_CallState:".Length)),
                out call.CallState);
            return;
        }

        if (line.StartsWith("m_ObjectArgument:", StringComparison.Ordinal))
        {
            ReadObjectReference(
                line,
                out call.ObjectArgumentFileId,
                out call.ObjectArgumentGuid);
            return;
        }

        if (line.StartsWith("m_ObjectArgumentAssemblyTypeName:", StringComparison.Ordinal))
        {
            call.ObjectArgumentAssemblyTypeName =
                CleanYamlScalar(
                    line.Substring("m_ObjectArgumentAssemblyTypeName:".Length));
            return;
        }

        if (line.StartsWith("m_IntArgument:", StringComparison.Ordinal))
        {
            int.TryParse(
                CleanYamlScalar(line.Substring("m_IntArgument:".Length)),
                out call.IntArgument);
            return;
        }

        if (line.StartsWith("m_FloatArgument:", StringComparison.Ordinal))
        {
            float.TryParse(
                CleanYamlScalar(line.Substring("m_FloatArgument:".Length)),
                out call.FloatArgument);
            return;
        }

        if (line.StartsWith("m_StringArgument:", StringComparison.Ordinal))
        {
            call.StringArgument =
                CleanYamlScalar(line.Substring("m_StringArgument:".Length));
            return;
        }

        if (line.StartsWith("m_BoolArgument:", StringComparison.Ordinal))
        {
            string value =
                CleanYamlScalar(line.Substring("m_BoolArgument:".Length));
            call.BoolArgument = value == "1" ||
                                value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }

    private static ConversionResult ConvertRecords(
        List<SpatialTriggerRecord> records,
        Dictionary<long, UnityEngine.Object> objectsByFileId)
    {
        ConversionResult result = new ConversionResult();

        foreach (SpatialTriggerRecord record in records)
        {
            UnityEngine.Object ownerObject;
            if (!objectsByFileId.TryGetValue(record.GameObjectFileId, out ownerObject))
            {
                result.Failed.Add(
                    "GameObject fileID " + record.GameObjectFileId +
                    " was not found in the loaded scene.");
                result.Rows.Add(new ReportRow
                {
                    Status = "Failed",
                    Method = BuildMethodsSummary(record),
                    Detail = "GameObject fileID " + record.GameObjectFileId +
                             " was not found in the loaded scene."
                });
                continue;
            }

            GameObject go = ownerObject as GameObject;
            if (go == null)
            {
                result.Failed.Add(
                    "fileID " + record.GameObjectFileId +
                    " is not a GameObject.");
                result.Rows.Add(new ReportRow
                {
                    Status = "Failed",
                    Method = BuildMethodsSummary(record),
                    Detail = "fileID " + record.GameObjectFileId +
                             " is not a GameObject."
                });
                continue;
            }

            Collider collider = go.GetComponent<Collider>();
            if (collider == null)
            {
                result.Failed.Add(
                    GetHierarchyPath(go) +
                    " has SpatialTriggerEvent data but no Collider.");
                AddRecordRows(
                    result,
                    "Failed",
                    go,
                    record,
                    "Object has SpatialTriggerEvent data but no Collider.");
                continue;
            }

            if (!collider.isTrigger)
                collider.isTrigger = true;

            LCSIATrigger trigger = go.GetComponent<LCSIATrigger>();
            if (trigger == null)
            {
                Undo.AddComponent<LCSIATrigger>(go);
                trigger = go.GetComponent<LCSIATrigger>();
            }

            if (trigger == null)
            {
                result.Failed.Add(
                    "Could not add LCSIATrigger to " +
                    GetHierarchyPath(go));
                AddRecordRows(
                    result,
                    "Failed",
                    go,
                    record,
                    "Could not add LCSIATrigger.");
                continue;
            }

            SerializedObject serializedTrigger =
                new SerializedObject(trigger);

            List<PersistentCallRecord> copiedEnterCalls =
                new List<PersistentCallRecord>();
            List<PersistentCallRecord> copiedExitCalls =
                new List<PersistentCallRecord>();

            bool enterOk = CopyCallsToUnityEvent(
                serializedTrigger,
                "onEnter",
                record.OnEnterCalls,
                copiedEnterCalls,
                objectsByFileId,
                result,
                GetHierarchyPath(go) + " On Enter");

            bool exitOk = CopyCallsToUnityEvent(
                serializedTrigger,
                "onExit",
                record.OnExitCalls,
                copiedExitCalls,
                objectsByFileId,
                result,
                GetHierarchyPath(go) + " On Exit");

            serializedTrigger.ApplyModifiedProperties();
            EditorUtility.SetDirty(trigger);
            EditorUtility.SetDirty(go);

            if (enterOk || exitOk ||
                (record.OnEnterCalls.Count == 0 && record.OnExitCalls.Count == 0))
            {
                result.Converted.Add(
                    GetHierarchyPath(go) +
                    " enter:" + record.OnEnterCalls.Count +
                    " exit:" + record.OnExitCalls.Count);
                AddRecordRows(
                    result,
                    "Converted",
                    go,
                    record,
                    "LCSIATrigger added/configured.",
                    copiedEnterCalls,
                    copiedExitCalls);
            }
            else
            {
                result.Skipped.Add(
                    GetHierarchyPath(go) +
                    " had calls, but none could be copied.");
                AddRecordRows(
                    result,
                    "Skipped",
                    go,
                    record,
                    "Object had calls, but none could be copied.");
            }
        }

        return result;
    }

    private static bool CopyCallsToUnityEvent(
        SerializedObject serializedObject,
        string eventPropertyName,
        List<PersistentCallRecord> calls,
        List<PersistentCallRecord> copiedCalls,
        Dictionary<long, UnityEngine.Object> objectsByFileId,
        ConversionResult result,
        string context)
    {
        SerializedProperty unityEvent =
            serializedObject.FindProperty(eventPropertyName);

        if (unityEvent == null)
        {
            result.Failed.Add(
                context + ": LCSIATrigger has no " +
                eventPropertyName + " UnityEvent.");
            return false;
        }

        SerializedProperty persistentCalls =
            unityEvent.FindPropertyRelative("m_PersistentCalls.m_Calls");

        if (persistentCalls == null)
        {
            result.Failed.Add(
                context + ": could not access persistent calls.");
            return false;
        }

        persistentCalls.ClearArray();

        int copied = 0;
        foreach (PersistentCallRecord call in calls)
        {
            UnityEngine.Object target = ResolveReference(
                call.TargetFileId,
                call.TargetGuid,
                objectsByFileId);

            if (target == null)
            {
                result.Failed.Add(
                    context + ": target for method " +
                    call.MethodName + " was not found.");
                continue;
            }

            persistentCalls.InsertArrayElementAtIndex(
                persistentCalls.arraySize);

            SerializedProperty item =
                persistentCalls.GetArrayElementAtIndex(
                    persistentCalls.arraySize - 1);

            item.FindPropertyRelative("m_Target").objectReferenceValue = target;
            item.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue =
                call.TargetAssemblyTypeName;
            item.FindPropertyRelative("m_MethodName").stringValue =
                call.MethodName;
            item.FindPropertyRelative("m_Mode").intValue =
                call.Mode;
            item.FindPropertyRelative("m_CallState").intValue =
                call.CallState;

            SerializedProperty arguments =
                item.FindPropertyRelative("m_Arguments");

            UnityEngine.Object objectArgument = ResolveReference(
                call.ObjectArgumentFileId,
                call.ObjectArgumentGuid,
                objectsByFileId);

            arguments.FindPropertyRelative("m_ObjectArgument").objectReferenceValue =
                objectArgument;
            arguments.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue =
                call.ObjectArgumentAssemblyTypeName;
            arguments.FindPropertyRelative("m_IntArgument").intValue =
                call.IntArgument;
            arguments.FindPropertyRelative("m_FloatArgument").floatValue =
                call.FloatArgument;
            arguments.FindPropertyRelative("m_StringArgument").stringValue =
                call.StringArgument;
            arguments.FindPropertyRelative("m_BoolArgument").boolValue =
                call.BoolArgument;

            copied++;
            copiedCalls.Add(call);
        }

        return copied > 0;
    }

    private static void AddRecordRows(
        ConversionResult result,
        string status,
        GameObject go,
        SpatialTriggerRecord record,
        string detail,
        List<PersistentCallRecord> copiedEnterCalls = null,
        List<PersistentCallRecord> copiedExitCalls = null)
    {
        List<string> ancestors = GetAncestorNames(go, 3);

        if (record.OnEnterCalls.Count == 0 &&
            record.OnExitCalls.Count == 0)
        {
            result.Rows.Add(new ReportRow
            {
                Status = status,
                Parent = GetAncestorName(ancestors, 0),
                Grandparent = GetAncestorName(ancestors, 1),
                GreatGrandparent = GetAncestorName(ancestors, 2),
                ObjectName = go.name,
                Script = "SpatialTriggerEvent",
                Method = "",
                Detail = detail
            });
            return;
        }

        foreach (PersistentCallRecord call in record.OnEnterCalls)
        {
            string callStatus = ResolveCallStatus(
                status,
                call,
                copiedEnterCalls);

            AddCallRow(
                result,
                callStatus,
                go,
                ancestors,
                call,
                "On Enter: " + detail);
        }

        foreach (PersistentCallRecord call in record.OnExitCalls)
        {
            string callStatus = ResolveCallStatus(
                status,
                call,
                copiedExitCalls);

            AddCallRow(
                result,
                callStatus,
                go,
                ancestors,
                call,
                "On Exit: " + detail);
        }
    }

    private static string ResolveCallStatus(
        string objectStatus,
        PersistentCallRecord call,
        List<PersistentCallRecord> copiedCalls)
    {
        if (objectStatus != "Converted" || copiedCalls == null)
            return objectStatus;

        foreach (PersistentCallRecord copiedCall in copiedCalls)
        {
            if (object.ReferenceEquals(copiedCall, call))
                return "Converted";
        }

        return "Failed";
    }

    private static void AddCallRow(
        ConversionResult result,
        string status,
        GameObject go,
        List<string> ancestors,
        PersistentCallRecord call,
        string detail)
    {
        result.Rows.Add(new ReportRow
        {
            Status = status,
            Parent = GetAncestorName(ancestors, 0),
            Grandparent = GetAncestorName(ancestors, 1),
            GreatGrandparent = GetAncestorName(ancestors, 2),
            ObjectName = go.name,
            Script = "SpatialTriggerEvent",
            Method = call.MethodName,
            Detail = detail
        });
    }

    private static string BuildMethodsSummary(SpatialTriggerRecord record)
    {
        List<string> methods = new List<string>();

        foreach (PersistentCallRecord call in record.OnEnterCalls)
            methods.Add(call.MethodName);

        foreach (PersistentCallRecord call in record.OnExitCalls)
            methods.Add(call.MethodName);

        return string.Join(" | ", methods.ToArray());
    }

    private static Dictionary<long, UnityEngine.Object> BuildSceneObjectMap(
        Scene scene)
    {
        Dictionary<long, UnityEngine.Object> result =
            new Dictionary<long, UnityEngine.Object>();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            AddObjectAndChildrenToMap(root, result);
        }

        return result;
    }

    private static Dictionary<long, UnityEngine.Object> BuildGameObjectMap(
        GameObject root)
    {
        Dictionary<long, UnityEngine.Object> result =
            new Dictionary<long, UnityEngine.Object>();

        AddObjectAndChildrenToMap(root, result);

        return result;
    }

    private static void AddObjectAndChildrenToMap(
        GameObject go,
        Dictionary<long, UnityEngine.Object> result)
    {
        AddObjectToMap(go, result);

        Component[] components = go.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (component != null)
                AddObjectToMap(component, result);
        }

        foreach (Transform child in go.transform)
        {
            AddObjectAndChildrenToMap(child.gameObject, result);
        }
    }

    private static void AddObjectToMap(
        UnityEngine.Object obj,
        Dictionary<long, UnityEngine.Object> result)
    {
        GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(obj);
        long localId = unchecked((long)id.targetObjectId);

        if (localId != 0 && !result.ContainsKey(localId))
            result.Add(localId, obj);
    }

    private static UnityEngine.Object ResolveReference(
        long fileId,
        string guid,
        Dictionary<long, UnityEngine.Object> sceneObjectsByFileId)
    {
        if (fileId == 0)
            return null;

        if (!string.IsNullOrEmpty(guid))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
                return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        }

        UnityEngine.Object obj;
        sceneObjectsByFileId.TryGetValue(fileId, out obj);
        return obj;
    }

    private static string BuildReport(
        ConversionResult result,
        int detectedCount)
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("Spatial Trigger Event conversion finished.");
        report.AppendLine("Detected: " + detectedCount);
        report.AppendLine("Converted: " + result.Converted.Count);
        report.AppendLine("Skipped: " + result.Skipped.Count);
        report.AppendLine("Failed: " + result.Failed.Count);

        AppendReportSection(report, "Converted", result.Converted);
        AppendReportSection(report, "Skipped", result.Skipped);
        AppendReportSection(report, "Failed", result.Failed);

        return report.ToString();
    }

    private static void SaveConversionCsv(ConversionResult result)
    {
        string savePath = EditorUtility.SaveFilePanel(
            "Save Trigger Conversion Report CSV",
            "",
            "SpatialTriggerConversionReport.csv",
            "csv");

        if (string.IsNullOrEmpty(savePath))
            return;

        StringBuilder csv = new StringBuilder();
        csv.AppendLine(
            "Status,Parent,Grandparent,GreatGrandparent,Object,Script,Method,Detail");

        foreach (ReportRow row in result.Rows)
        {
            csv.Append(EscapeCsv(row.Status));
            csv.Append(',');
            csv.Append(EscapeCsv(row.Parent));
            csv.Append(',');
            csv.Append(EscapeCsv(row.Grandparent));
            csv.Append(',');
            csv.Append(EscapeCsv(row.GreatGrandparent));
            csv.Append(',');
            csv.Append(EscapeCsv(row.ObjectName));
            csv.Append(',');
            csv.Append(EscapeCsv(row.Script));
            csv.Append(',');
            csv.Append(EscapeCsv(row.Method));
            csv.Append(',');
            csv.Append(EscapeCsv(row.Detail));
            csv.AppendLine();
        }

        File.WriteAllText(savePath, csv.ToString(), Encoding.UTF8);
        EditorGUIUtility.systemCopyBuffer = csv.ToString();
    }

    private static void AppendReportSection(
        StringBuilder report,
        string title,
        List<string> lines)
    {
        if (lines.Count == 0)
            return;

        report.AppendLine();
        report.AppendLine(title + ":");

        int count = Mathf.Min(lines.Count, 30);
        for (int i = 0; i < count; i++)
            report.AppendLine("- " + lines[i]);

        if (lines.Count > count)
            report.AppendLine("- ... " + (lines.Count - count) + " more");
    }

    private static string FindSpatialEventName(List<string> keyStack)
    {
        for (int i = keyStack.Count - 1; i >= 0; i--)
        {
            string key = keyStack[i];

            if (key == "m_PersistentCalls" ||
                key == "m_Calls" ||
                key == "unityEvent")
            {
                continue;
            }

            return key;
        }

        return "";
    }

    private static bool IsEnterEvent(string eventName)
    {
        return eventName == "onEnterEvent" ||
               eventName == "deprecated_onEnter" ||
               eventName == "onEnter";
    }

    private static bool IsExitEvent(string eventName)
    {
        return eventName == "onExitEvent" ||
               eventName == "deprecated_onExit" ||
               eventName == "onExit";
    }

    private static bool HasField(
        HashSet<string> fields,
        string expectedName)
    {
        return fields.Contains(expectedName) ||
               fields.Contains("m_" + expectedName) ||
               fields.Contains("_" + expectedName);
    }

    private static long FindFileId(
        string[] lines,
        int start,
        int end,
        string key)
    {
        string prefix = key + ":";

        for (int i = start; i < end; i++)
        {
            string trimmed = lines[i].Trim();
            if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            Match match = FileIdRegex.Match(trimmed);
            if (match.Success)
            {
                long value;
                if (long.TryParse(match.Groups[1].Value, out value))
                    return value;
            }
        }

        return 0;
    }

    private static void ReadObjectReference(
        string line,
        out long fileId,
        out string guid)
    {
        fileId = 0;
        guid = "";

        Match fileIdMatch = FileIdRegex.Match(line);
        if (fileIdMatch.Success)
            long.TryParse(fileIdMatch.Groups[1].Value, out fileId);

        Match guidMatch = GuidRegex.Match(line);
        if (guidMatch.Success)
            guid = guidMatch.Groups[1].Value;
    }

    private static string ExtractYamlKey(string trimmedLine)
    {
        if (trimmedLine.StartsWith("-", StringComparison.Ordinal))
            trimmedLine = trimmedLine.Substring(1).Trim();

        int colon = trimmedLine.IndexOf(':');
        if (colon <= 0)
            return "";

        string key = trimmedLine.Substring(0, colon).Trim();
        if (key.IndexOf(' ') >= 0 || key.IndexOf('{') >= 0)
            return "";

        return key;
    }

    private static int CountLeadingSpaces(string value)
    {
        int count = 0;
        while (count < value.Length && value[count] == ' ')
            count++;

        return count;
    }

    private static string CleanYamlScalar(string value)
    {
        value = value.Trim();

        if (value.Length >= 2 &&
            ((value[0] == '"' && value[value.Length - 1] == '"') ||
             (value[0] == '\'' && value[value.Length - 1] == '\'')))
        {
            value = value.Substring(1, value.Length - 2);
        }

        return value.Replace("\\\"", "\"");
    }

    private static string GetHierarchyPath(GameObject go)
    {
        List<string> names = new List<string>();
        Transform current = go.transform;

        while (current != null)
        {
            names.Add(current.name);
            current = current.parent;
        }

        names.Reverse();
        return string.Join("/", names.ToArray());
    }

    private static List<string> GetAncestorNames(GameObject go, int count)
    {
        List<string> names = new List<string>();
        Transform current = go.transform.parent;

        while (current != null && names.Count < count)
        {
            names.Add(current.name);
            current = current.parent;
        }

        return names;
    }

    private static string GetAncestorName(List<string> ancestors, int index)
    {
        if (index < ancestors.Count)
            return ancestors[index];

        return "";
    }

    private static string EscapeCsv(string value)
    {
        if (value == null)
            value = "";

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
