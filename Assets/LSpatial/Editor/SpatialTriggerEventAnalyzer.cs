using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static class SpatialTriggerEventAnalyzer
{
    private const string MenuPath = "LSpatial/Analyze Spatial Trigger Event Components";

    private sealed class ParsedFile
    {
        public readonly Dictionary<long, GameObjectRecord> GameObjects =
            new Dictionary<long, GameObjectRecord>();

        public readonly Dictionary<long, TransformRecord> Transforms =
            new Dictionary<long, TransformRecord>();

        public readonly Dictionary<long, MonoBehaviourRecord> MonoBehaviours =
            new Dictionary<long, MonoBehaviourRecord>();

        public readonly HashSet<long> GameObjectsWithTriggerCollider =
            new HashSet<long>();
    }

    private sealed class GameObjectRecord
    {
        public long FileId;
        public string Name = "";
        public long TransformFileId;
    }

    private sealed class TransformRecord
    {
        public long FileId;
        public long GameObjectFileId;
        public long FatherFileId;
    }

    private sealed class MonoBehaviourRecord
    {
        public long FileId;
        public long GameObjectFileId;
        public string Name = "";
        public string ScriptGuid = "";
        public string ScriptFileId = "";
        public string EditorClassIdentifier = "";
        public readonly HashSet<string> FieldNames =
            new HashSet<string>();
        public readonly Dictionary<string, string> Scalars =
            new Dictionary<string, string>();
        public readonly List<PersistentCallRecord> Calls =
            new List<PersistentCallRecord>();
    }

    private sealed class PersistentCallRecord
    {
        public string EventName = "";
        public string MethodName = "";
        public string TargetAssemblyTypeName = "";
        public string TargetFileId = "";
        public string TargetGuid = "";
    }

    private sealed class PackageScan
    {
        public readonly List<string> YamlAssetFiles =
            new List<string>();

        public readonly Dictionary<string, string> ScriptNamesByGuid =
            new Dictionary<string, string>();
    }

    private sealed class Report
    {
        public int GameObjects;
        public int MonoBehaviours;
        public int TriggerComponents;
        public int TriggerEvents;
        public int MissingScripts;
    }

    private static readonly Regex HeaderRegex =
        new Regex(@"^---\s+!u!(\d+)\s+&(-?\d+)", RegexOptions.Compiled);

    private static readonly Regex FileIdRegex =
        new Regex(@"fileID:\s*(-?\d+)", RegexOptions.Compiled);

    private static readonly Regex GuidRegex =
        new Regex(@"guid:\s*([0-9a-fA-F]+)", RegexOptions.Compiled);

    [MenuItem(MenuPath)]
    private static void AnalyzeTriggerEvents()
    {
        string packagePath = EditorUtility.OpenFilePanel(
            "Select Spatial.io UnityPackage",
            "",
            "unitypackage");

        if (string.IsNullOrEmpty(packagePath))
            return;

        string tempFolder = Path.Combine(
            Path.GetTempPath(),
            "SpatialTriggerEventAnalysis_" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(tempFolder);

        try
        {
            if (!ExtractUnityPackage(packagePath, tempFolder))
                return;

            PackageScan scan = ScanPackage(tempFolder);
            StringBuilder csv = new StringBuilder();
            Report report = new Report();

            csv.AppendLine("Parent,Grandparent,GreatGrandparent,Object,Script,Method");

            foreach (string yamlAssetFile in scan.YamlAssetFiles)
            {
                ParsedFile parsed = ParseYamlFile(yamlAssetFile);
                AppendTriggerRows(csv, parsed, scan.ScriptNamesByGuid, report);
            }

            string savePath = EditorUtility.SaveFilePanel(
                "Save Spatial Trigger Event CSV",
                "",
                "SpatialTriggerEvents.csv",
                "csv");

            string writtenPath = "";
            if (!string.IsNullOrEmpty(savePath))
                writtenPath = WriteCsvWithFallback(savePath, csv.ToString());

            EditorGUIUtility.systemCopyBuffer = csv.ToString();

            UnityEngine.Debug.Log(
                "Spatial Trigger Event analysis finished.\n" +
                "GameObjects: " + report.GameObjects + "\n" +
                "MonoBehaviours: " + report.MonoBehaviours + "\n" +
                "Trigger Event components found: " + report.TriggerComponents + "\n" +
                "Trigger UnityEvents found: " + report.TriggerEvents + "\n" +
                "Missing Scripts found: " + report.MissingScripts + "\n" +
                "CSV copied to clipboard" +
                (string.IsNullOrEmpty(writtenPath) ? "." : " and saved to:\n" + writtenPath));
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, true);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(
                    "Could not delete temporary package extraction folder:\n" +
                    tempFolder + "\n" + ex.Message);
            }
        }
    }

    private static bool ExtractUnityPackage(string packagePath, string tempFolder)
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
                "Unable to extract unitypackage with tar.\n" +
                output + "\n" + error);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(
                "Unable to extract unitypackage. This tool needs the tar command available on the system.\n" +
                ex.Message);
        }

        return false;
    }

    private static string WriteCsvWithFallback(string savePath, string csv)
    {
        try
        {
            File.WriteAllText(savePath, csv, Encoding.UTF8);
            return savePath;
        }
        catch (IOException ex)
        {
            string folder = Path.GetDirectoryName(savePath);
            string name = Path.GetFileNameWithoutExtension(savePath);
            string fallbackPath = Path.Combine(
                folder,
                name + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

            File.WriteAllText(fallbackPath, csv, Encoding.UTF8);

            UnityEngine.Debug.LogWarning(
                "Could not overwrite the selected CSV, probably because it is open in another program.\n" +
                "Original path:\n" + savePath + "\n" +
                "Saved a new copy instead:\n" + fallbackPath + "\n" +
                ex.Message);

            return fallbackPath;
        }
    }

    private static PackageScan ScanPackage(string tempFolder)
    {
        PackageScan scan = new PackageScan();
        string[] directories = Directory.GetDirectories(tempFolder);

        foreach (string directory in directories)
        {
            string assetFile = Path.Combine(directory, "asset");
            string pathnameFile = Path.Combine(directory, "pathname");

            if (!File.Exists(assetFile) || !File.Exists(pathnameFile))
                continue;

            string originalPath = File.ReadAllText(pathnameFile).Trim();
            string packageGuid = Path.GetFileName(directory);

            if (originalPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                string scriptName = Path.GetFileNameWithoutExtension(originalPath);
                if (!string.IsNullOrEmpty(packageGuid) &&
                    !string.IsNullOrEmpty(scriptName))
                {
                    scan.ScriptNamesByGuid[packageGuid] = scriptName;
                }

                continue;
            }

            if (originalPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) ||
                originalPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ||
                originalPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
            {
                scan.YamlAssetFiles.Add(assetFile);
            }
        }

        return scan;
    }

    private static ParsedFile ParseYamlFile(string path)
    {
        ParsedFile parsed = new ParsedFile();
        string[] lines = File.ReadAllLines(path);

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

            ParseObjectBlock(parsed, classId, fileId, lines, start, index);
        }

        foreach (TransformRecord transform in parsed.Transforms.Values)
        {
            GameObjectRecord go;
            if (parsed.GameObjects.TryGetValue(transform.GameObjectFileId, out go))
                go.TransformFileId = transform.FileId;
        }

        return parsed;
    }

    private static void ParseObjectBlock(
        ParsedFile parsed,
        int classId,
        long fileId,
        string[] lines,
        int start,
        int end)
    {
        if (classId == 1)
        {
            parsed.GameObjects[fileId] = new GameObjectRecord
            {
                FileId = fileId,
                Name = FindScalar(lines, start, end, "m_Name")
            };

            return;
        }

        if (classId == 4 || classId == 224)
        {
            parsed.Transforms[fileId] = new TransformRecord
            {
                FileId = fileId,
                GameObjectFileId = FindFileId(lines, start, end, "m_GameObject"),
                FatherFileId = FindFileId(lines, start, end, "m_Father")
            };

            return;
        }

        if (classId == 114)
        {
            MonoBehaviourRecord mono = new MonoBehaviourRecord
            {
                FileId = fileId,
                Name = FindScalar(lines, start, end, "m_Name"),
                EditorClassIdentifier =
                    FindScalar(lines, start, end, "m_EditorClassIdentifier"),
                GameObjectFileId = FindFileId(lines, start, end, "m_GameObject")
            };

            ReadScalars(lines, start, end, mono);
            ReadScriptReference(lines, start, end, mono);
            ReadPersistentCalls(lines, start, end, mono);
            parsed.MonoBehaviours[fileId] = mono;
        }

        if (IsColliderClassId(classId))
        {
            long gameObjectFileId = FindFileId(lines, start, end, "m_GameObject");
            string isTrigger = FindScalar(lines, start, end, "m_IsTrigger");

            if (gameObjectFileId != 0 &&
                (isTrigger == "1" ||
                 isTrigger.Equals("true", StringComparison.OrdinalIgnoreCase)))
            {
                parsed.GameObjectsWithTriggerCollider.Add(gameObjectFileId);
            }
        }
    }

    private static void ReadScalars(
        string[] lines,
        int start,
        int end,
        MonoBehaviourRecord mono)
    {
        for (int i = start; i < end; i++)
        {
            string trimmed = lines[i].Trim();
            int colon = trimmed.IndexOf(':');

            if (colon <= 0)
                continue;

            string key = trimmed.Substring(0, colon).Trim();

            if (!string.IsNullOrEmpty(key))
                mono.FieldNames.Add(key);

            if (colon == trimmed.Length - 1)
                continue;

            string value = trimmed.Substring(colon + 1).Trim();

            if (value.StartsWith("{", StringComparison.Ordinal))
                continue;

            if (!mono.Scalars.ContainsKey(key))
                mono.Scalars[key] = CleanYamlScalar(value);
        }
    }

    private static void ReadScriptReference(
        string[] lines,
        int start,
        int end,
        MonoBehaviourRecord mono)
    {
        for (int i = start; i < end; i++)
        {
            string trimmed = lines[i].Trim();
            if (!trimmed.StartsWith("m_Script:", StringComparison.Ordinal))
                continue;

            Match fileId = FileIdRegex.Match(trimmed);
            Match guid = GuidRegex.Match(trimmed);

            if (fileId.Success)
                mono.ScriptFileId = fileId.Groups[1].Value;

            if (guid.Success)
                mono.ScriptGuid = guid.Groups[1].Value;

            return;
        }
    }

    private static void ReadPersistentCalls(
        string[] lines,
        int start,
        int end,
        MonoBehaviourRecord mono)
    {
        List<string> keyStack = new List<string>();
        string currentEventName = "";
        PersistentCallRecord currentCall = null;
        bool insideCalls = false;

        for (int i = start; i < end; i++)
        {
            string raw = lines[i];
            string trimmed = raw.Trim();
            int indent = CountLeadingSpaces(raw);
            string yamlKey = ExtractYamlKey(trimmed);

            if (!string.IsNullOrEmpty(yamlKey))
            {
                mono.FieldNames.Add(yamlKey);

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
                currentEventName = FindUnityEventFieldName(keyStack);
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
                if (currentCall != null && !string.IsNullOrEmpty(currentCall.MethodName))
                    mono.Calls.Add(currentCall);

                currentCall = new PersistentCallRecord
                {
                    EventName = FriendlyEventName(currentEventName)
                };

                ReadCallLine(trimmed.Substring(1).Trim(), currentCall);
                continue;
            }

            if (currentCall == null)
                continue;

            ReadCallLine(trimmed, currentCall);
        }

        if (currentCall != null && !string.IsNullOrEmpty(currentCall.MethodName))
            mono.Calls.Add(currentCall);
    }

    private static void ReadCallLine(string line, PersistentCallRecord call)
    {
        if (line.StartsWith("m_MethodName:", StringComparison.Ordinal))
        {
            call.MethodName = CleanYamlScalar(
                line.Substring("m_MethodName:".Length));
            return;
        }

        if (line.StartsWith("m_TargetAssemblyTypeName:", StringComparison.Ordinal))
        {
            call.TargetAssemblyTypeName = CleanYamlScalar(
                line.Substring("m_TargetAssemblyTypeName:".Length));
            return;
        }

        if (line.StartsWith("m_Target:", StringComparison.Ordinal))
        {
            Match fileId = FileIdRegex.Match(line);
            Match guid = GuidRegex.Match(line);

            if (fileId.Success)
                call.TargetFileId = fileId.Groups[1].Value;

            if (guid.Success)
                call.TargetGuid = guid.Groups[1].Value;
        }
    }

    private static void AppendTriggerRows(
        StringBuilder csv,
        ParsedFile parsed,
        Dictionary<string, string> scriptNamesByGuid,
        Report report)
    {
        report.GameObjects += parsed.GameObjects.Count;
        report.MonoBehaviours += parsed.MonoBehaviours.Count;

        foreach (MonoBehaviourRecord mono in parsed.MonoBehaviours.Values)
        {
            if (IsMissingScript(mono, scriptNamesByGuid))
                report.MissingScripts++;

            if (!LooksLikeSpatialTriggerEvent(parsed, mono, scriptNamesByGuid))
                continue;

            report.TriggerComponents++;

            GameObjectRecord go;
            parsed.GameObjects.TryGetValue(mono.GameObjectFileId, out go);

            List<string> ancestors = FindAncestorNames(parsed, go, 3);
            string objectName = go != null ? go.Name : "";
            string componentName = "SpatialTriggerEvent";

            if (mono.Calls.Count == 0)
            {
                AppendTriggerRow(
                    csv,
                    ancestors,
                    objectName,
                    componentName,
                    "");
                continue;
            }

            foreach (PersistentCallRecord call in mono.Calls)
            {
                report.TriggerEvents++;

                AppendTriggerRow(
                    csv,
                    ancestors,
                    objectName,
                    componentName,
                    call.MethodName);
            }
        }
    }

    private static void AppendTriggerRow(
        StringBuilder csv,
        List<string> ancestors,
        string objectName,
        string componentName,
        string methodName)
    {
        csv.Append(EscapeCsv(GetAncestorName(ancestors, 0)));
        csv.Append(',');
        csv.Append(EscapeCsv(GetAncestorName(ancestors, 1)));
        csv.Append(',');
        csv.Append(EscapeCsv(GetAncestorName(ancestors, 2)));
        csv.Append(',');
        csv.Append(EscapeCsv(objectName));
        csv.Append(',');
        csv.Append(EscapeCsv(componentName));
        csv.Append(',');
        csv.Append(EscapeCsv(methodName));
        csv.AppendLine();
    }

    private static bool LooksLikeSpatialTriggerEvent(
        ParsedFile parsed,
        MonoBehaviourRecord mono,
        Dictionary<string, string> scriptNamesByGuid)
    {
        string componentName = BuildComponentName(mono, scriptNamesByGuid);
        string haystack =
            componentName + " " +
            mono.EditorClassIdentifier + " " +
            FindTargetTypeName(mono);

        if (ContainsIgnoreCase(haystack, "TriggerEvent") ||
            ContainsIgnoreCase(haystack, "Trigger Event") ||
            ContainsIgnoreCase(haystack, "SpatialTrigger"))
        {
            return HasTriggerColliderIfKnown(parsed, mono);
        }

        return HasSpatialTriggerFieldShape(mono) &&
               HasTriggerColliderIfKnown(parsed, mono);
    }

    private static bool HasSpatialTriggerFieldShape(MonoBehaviourRecord mono)
    {
        bool hasListenFor = HasField(mono, "listenFor");
        bool hasModernEvents =
            HasField(mono, "onEnterEvent") &&
            HasField(mono, "onExitEvent");
        bool hasDeprecatedEvents =
            HasField(mono, "deprecated_onEnter") &&
            HasField(mono, "deprecated_onExit");

        return hasListenFor && (hasModernEvents || hasDeprecatedEvents);
    }

    private static string BuildComponentName(
        MonoBehaviourRecord mono,
        Dictionary<string, string> scriptNamesByGuid)
    {
        if (!string.IsNullOrEmpty(mono.Name))
            return mono.Name;

        string className = ExtractClassIdentifierName(mono.EditorClassIdentifier);
        if (!string.IsNullOrEmpty(className))
            return className;

        string scriptName;
        if (!string.IsNullOrEmpty(mono.ScriptGuid) &&
            scriptNamesByGuid.TryGetValue(mono.ScriptGuid, out scriptName))
        {
            return scriptName;
        }

        string targetType = FindTargetTypeName(mono);
        if (!string.IsNullOrEmpty(targetType))
            return targetType;

        if (!string.IsNullOrEmpty(mono.ScriptGuid))
            return "Missing MonoBehaviour guid:" + mono.ScriptGuid;

        return "Missing MonoBehaviour";
    }

    private static string BuildTargetName(
        ParsedFile parsed,
        PersistentCallRecord call)
    {
        long targetId;
        if (long.TryParse(call.TargetFileId, out targetId))
        {
            GameObjectRecord targetGo;
            if (parsed.GameObjects.TryGetValue(targetId, out targetGo))
                return targetGo.Name;

            MonoBehaviourRecord targetMono;
            if (parsed.MonoBehaviours.TryGetValue(targetId, out targetMono))
            {
                GameObjectRecord owner;
                if (parsed.GameObjects.TryGetValue(targetMono.GameObjectFileId, out owner))
                    return owner.Name;
            }
        }

        string typeName = ExtractTypeName(call.TargetAssemblyTypeName);
        if (!string.IsNullOrEmpty(typeName))
            return typeName;

        if (!string.IsNullOrEmpty(call.TargetGuid))
            return "guid:" + call.TargetGuid;

        return "";
    }

    private static List<string> FindAncestorNames(
        ParsedFile parsed,
        GameObjectRecord go,
        int count)
    {
        List<string> names = new List<string>();

        if (go == null || go.TransformFileId == 0)
            return names;

        long transformFileId = go.TransformFileId;

        for (int i = 0; i < count; i++)
        {
            TransformRecord transform;
            if (!parsed.Transforms.TryGetValue(transformFileId, out transform))
                break;

            TransformRecord parentTransform;
            if (!parsed.Transforms.TryGetValue(transform.FatherFileId, out parentTransform))
                break;

            GameObjectRecord parentGo;
            if (!parsed.GameObjects.TryGetValue(parentTransform.GameObjectFileId, out parentGo))
                break;

            names.Add(parentGo.Name);
            transformFileId = parentTransform.FileId;
        }

        return names;
    }

    private static string FindScalar(
        string[] lines,
        int start,
        int end,
        string key)
    {
        string prefix = key + ":";

        for (int i = start; i < end; i++)
        {
            string trimmed = lines[i].Trim();
            if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                return CleanYamlScalar(trimmed.Substring(prefix.Length));
        }

        return "";
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

    private static string FindScalarByPossibleNames(
        MonoBehaviourRecord mono,
        params string[] names)
    {
        foreach (string name in names)
        {
            string value;
            if (mono.Scalars.TryGetValue(name, out value))
                return value;
        }

        return "";
    }

    private static string FindUnityEventFieldName(List<string> keyStack)
    {
        for (int i = keyStack.Count - 1; i >= 0; i--)
        {
            string key = keyStack[i];
            if (key == "m_PersistentCalls" ||
                key == "m_Calls" ||
                key == "unityEvent" ||
                key == "animatorEvent" ||
                key == "questEvent")
            {
                continue;
            }

            return key;
        }

        return "";
    }

    private static string FriendlyEventName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return "";

        string name = rawName;
        if (name.StartsWith("m_", StringComparison.Ordinal))
            name = name.Substring(2);

        if (name.EndsWith("Event", StringComparison.Ordinal))
            name = name.Substring(0, name.Length - "Event".Length);

        if (name.StartsWith("deprecated_", StringComparison.Ordinal))
            name = name.Substring("deprecated_".Length);

        name = Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
        name = name.Replace("_", " ").Trim();

        return name;
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

    private static string GetAncestorName(List<string> ancestors, int index)
    {
        if (index < ancestors.Count)
            return ancestors[index];

        return "";
    }

    private static string FindTargetTypeName(MonoBehaviourRecord mono)
    {
        foreach (PersistentCallRecord call in mono.Calls)
        {
            string typeName = ExtractTypeName(call.TargetAssemblyTypeName);
            if (!string.IsNullOrEmpty(typeName))
                return typeName;
        }

        return "";
    }

    private static string ExtractTypeName(string assemblyTypeName)
    {
        if (string.IsNullOrEmpty(assemblyTypeName))
            return "";

        string typeName = assemblyTypeName;
        int comma = typeName.IndexOf(',');
        if (comma >= 0)
            typeName = typeName.Substring(0, comma);

        int plus = typeName.LastIndexOf('+');
        if (plus >= 0 && plus + 1 < typeName.Length)
            typeName = typeName.Substring(plus + 1);

        int dot = typeName.LastIndexOf('.');
        if (dot >= 0 && dot + 1 < typeName.Length)
            typeName = typeName.Substring(dot + 1);

        return typeName.Trim();
    }

    private static string ExtractClassIdentifierName(string classIdentifier)
    {
        if (string.IsNullOrEmpty(classIdentifier))
            return "";

        int separator = classIdentifier.LastIndexOf("::", StringComparison.Ordinal);
        if (separator >= 0 && separator + 2 < classIdentifier.Length)
            return classIdentifier.Substring(separator + 2);

        separator = classIdentifier.LastIndexOf('.');
        if (separator >= 0 && separator + 1 < classIdentifier.Length)
            return classIdentifier.Substring(separator + 1);

        return classIdentifier;
    }

    private static bool IsMissingScript(
        MonoBehaviourRecord mono,
        Dictionary<string, string> scriptNamesByGuid)
    {
        return string.IsNullOrEmpty(mono.ScriptGuid) ||
               mono.ScriptFileId == "0" ||
               !scriptNamesByGuid.ContainsKey(mono.ScriptGuid);
    }

    private static bool HasTriggerColliderIfKnown(
        ParsedFile parsed,
        MonoBehaviourRecord mono)
    {
        if (parsed.GameObjectsWithTriggerCollider.Count == 0)
            return true;

        return parsed.GameObjectsWithTriggerCollider.Contains(
            mono.GameObjectFileId);
    }

    private static bool HasField(MonoBehaviourRecord mono, string expectedName)
    {
        foreach (string fieldName in mono.FieldNames)
        {
            if (fieldName == expectedName ||
                fieldName == "m_" + expectedName ||
                fieldName == "_" + expectedName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsColliderClassId(int classId)
    {
        return classId == 64 ||  // MeshCollider
               classId == 65 ||  // BoxCollider
               classId == 135 || // SphereCollider
               classId == 136 || // CapsuleCollider
               classId == 154;   // TerrainCollider
    }

    private static bool ContainsIgnoreCase(string value, string search)
    {
        return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
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

    private static string EscapeCsv(string value)
    {
        if (value == null)
            value = "";

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
