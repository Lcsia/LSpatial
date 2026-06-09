using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static class SpatialPackageAnalyzer
{
    private const string MenuPath = "LSpatial/Analyze Spatial UnityPackage Events";

    private sealed class ParsedFile
    {
        public readonly Dictionary<long, GameObjectRecord> GameObjects =
            new Dictionary<long, GameObjectRecord>();

        public readonly Dictionary<long, TransformRecord> Transforms =
            new Dictionary<long, TransformRecord>();

        public readonly Dictionary<long, MonoBehaviourRecord> MonoBehaviours =
            new Dictionary<long, MonoBehaviourRecord>();
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
        public string ScriptType = "";
        public string EditorClassIdentifier = "";
        public readonly List<PersistentCallRecord> Calls =
            new List<PersistentCallRecord>();
    }

    private sealed class PersistentCallRecord
    {
        public string MethodName = "";
        public string TargetAssemblyTypeName = "";
        public string TargetFileId = "";
        public string TargetGuid = "";
    }

    private sealed class PackageAsset
    {
        public string AssetPath = "";
        public string OriginalPath = "";
    }

    private sealed class Report
    {
        public int GameObjects;
        public int MonoBehaviours;
        public int UnityEvents;
        public int MissingScripts;
    }

    private sealed class PackageScan
    {
        public readonly List<PackageAsset> YamlAssets =
            new List<PackageAsset>();

        public readonly Dictionary<string, string> ScriptNamesByGuid =
            new Dictionary<string, string>();
    }

    private static readonly Regex HeaderRegex =
        new Regex(@"^---\s+!u!(\d+)\s+&(-?\d+)", RegexOptions.Compiled);

    private static readonly Regex FileIdRegex =
        new Regex(@"fileID:\s*(-?\d+)", RegexOptions.Compiled);

    private static readonly Regex GuidRegex =
        new Regex(@"guid:\s*([0-9a-fA-F]+)", RegexOptions.Compiled);

    private static readonly Regex TypeRegex =
        new Regex(@"type:\s*(-?\d+)", RegexOptions.Compiled);

    [MenuItem(MenuPath)]
    private static void AnalyzePackage()
    {
        string packagePath = EditorUtility.OpenFilePanel(
            "Select Spatial.io UnityPackage",
            "",
            "unitypackage");

        if (string.IsNullOrEmpty(packagePath))
            return;

        string tempFolder = Path.Combine(
            Path.GetTempPath(),
            "SpatialPackageAnalysis_" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(tempFolder);

        try
        {
            if (!ExtractUnityPackage(packagePath, tempFolder))
                return;

            PackageScan packageScan = ScanPackageAssets(tempFolder);
            StringBuilder csv = new StringBuilder();
            Report report = new Report();

            csv.AppendLine("Parent,Grandparent,GreatGrandparent,Object,Script,Method");

            foreach (PackageAsset asset in packageScan.YamlAssets)
            {
                ParsedFile parsed = ParseYamlFile(asset.AssetPath);
                AppendEvents(csv, parsed, packageScan.ScriptNamesByGuid, report);
            }

            string savePath = EditorUtility.SaveFilePanel(
                "Save Spatial Events CSV",
                "",
                "SpatialUnityEvents.csv",
                "csv");

            if (!string.IsNullOrEmpty(savePath))
                File.WriteAllText(savePath, csv.ToString(), Encoding.UTF8);

            EditorGUIUtility.systemCopyBuffer = csv.ToString();

            UnityEngine.Debug.Log(
                "Spatial package analysis finished.\n" +
                "GameObjects: " + report.GameObjects + "\n" +
                "MonoBehaviours: " + report.MonoBehaviours + "\n" +
                "UnityEvents found: " + report.UnityEvents + "\n" +
                "Missing Scripts found: " + report.MissingScripts + "\n" +
                "CSV copied to clipboard" +
                (string.IsNullOrEmpty(savePath) ? "." : " and saved to:\n" + savePath));
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
                "Unable to extract unitypackage. Unity packages are gzipped tar archives; this tool needs the tar command available on the system.\n" +
                ex.Message);
        }

        return false;
    }

    private static PackageScan ScanPackageAssets(string tempFolder)
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

            if (!IsWantedUnityYamlPath(originalPath))
            {
                if (originalPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    string scriptName = Path.GetFileNameWithoutExtension(originalPath);
                    if (!string.IsNullOrEmpty(packageGuid) &&
                        !string.IsNullOrEmpty(scriptName))
                    {
                        scan.ScriptNamesByGuid[packageGuid] = scriptName;
                    }
                }

                continue;
            }

            scan.YamlAssets.Add(new PackageAsset
            {
                AssetPath = assetFile,
                OriginalPath = originalPath
            });
        }

        return scan;
    }

    private static bool IsWantedUnityYamlPath(string path)
    {
        return path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase);
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
            if (transform.GameObjectFileId == 0)
                continue;

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
            GameObjectRecord go = new GameObjectRecord { FileId = fileId };
            go.Name = FindScalar(lines, start, end, "m_Name");
            parsed.GameObjects[fileId] = go;
            return;
        }

        if (classId == 4 || classId == 224)
        {
            TransformRecord transform = new TransformRecord { FileId = fileId };
            transform.GameObjectFileId = FindFileId(lines, start, end, "m_GameObject");
            transform.FatherFileId = FindFileId(lines, start, end, "m_Father");
            parsed.Transforms[fileId] = transform;
            return;
        }

        if (classId == 114)
        {
            MonoBehaviourRecord mono = new MonoBehaviourRecord { FileId = fileId };
            mono.Name = FindScalar(lines, start, end, "m_Name");
            mono.EditorClassIdentifier =
                FindScalar(lines, start, end, "m_EditorClassIdentifier");
            mono.GameObjectFileId = FindFileId(lines, start, end, "m_GameObject");
            ReadScriptReference(lines, start, end, mono);
            ReadPersistentCalls(lines, start, end, mono);
            parsed.MonoBehaviours[fileId] = mono;
        }
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
            if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
                continue;

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
            Match type = TypeRegex.Match(trimmed);

            if (fileId.Success)
                mono.ScriptFileId = fileId.Groups[1].Value;

            if (guid.Success)
                mono.ScriptGuid = guid.Groups[1].Value;

            if (type.Success)
                mono.ScriptType = type.Groups[1].Value;

            return;
        }
    }

    private static void ReadPersistentCalls(
        string[] lines,
        int start,
        int end,
        MonoBehaviourRecord mono)
    {
        bool insidePersistentCalls = false;
        bool insideCalls = false;
        PersistentCallRecord currentCall = null;

        for (int i = start; i < end; i++)
        {
            string trimmed = lines[i].Trim();

            if (trimmed.StartsWith("m_PersistentCalls:", StringComparison.Ordinal))
            {
                insidePersistentCalls = true;
                continue;
            }

            if (!insidePersistentCalls)
                continue;

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

                currentCall = new PersistentCallRecord();
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

    private static void AppendEvents(
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

            if (mono.Calls.Count == 0)
                continue;

            GameObjectRecord go;
            parsed.GameObjects.TryGetValue(mono.GameObjectFileId, out go);

            string objectName = go != null ? go.Name : "";
            List<string> ancestors = FindAncestorNames(parsed, go, 3);
            string scriptName = BuildScriptName(mono, scriptNamesByGuid);

            foreach (PersistentCallRecord call in mono.Calls)
            {
                report.UnityEvents++;

                csv.Append(EscapeCsv(GetAncestorName(ancestors, 0)));
                csv.Append(',');
                csv.Append(EscapeCsv(GetAncestorName(ancestors, 1)));
                csv.Append(',');
                csv.Append(EscapeCsv(GetAncestorName(ancestors, 2)));
                csv.Append(',');
                csv.Append(EscapeCsv(objectName));
                csv.Append(',');
                csv.Append(EscapeCsv(scriptName));
                csv.Append(',');
                csv.Append(EscapeCsv(call.MethodName));
                csv.AppendLine();
            }
        }
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

            if (transform.FatherFileId == 0)
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

    private static string GetAncestorName(List<string> ancestors, int index)
    {
        if (index < ancestors.Count)
            return ancestors[index];

        return "";
    }

    private static string BuildScriptName(
        MonoBehaviourRecord mono,
        Dictionary<string, string> scriptNamesByGuid)
    {
        if (!string.IsNullOrEmpty(mono.Name))
            return mono.Name;

        string classIdentifierName = ExtractClassIdentifierName(
            mono.EditorClassIdentifier);
        if (!string.IsNullOrEmpty(classIdentifierName))
            return classIdentifierName;

        string scriptName;
        if (!string.IsNullOrEmpty(mono.ScriptGuid) &&
            scriptNamesByGuid.TryGetValue(mono.ScriptGuid, out scriptName))
        {
            return scriptName;
        }

        string targetTypeName = FindTargetTypeName(mono);
        if (!string.IsNullOrEmpty(targetTypeName))
            return targetTypeName;

        if (!string.IsNullOrEmpty(mono.ScriptGuid))
            return "Missing MonoBehaviour guid:" + mono.ScriptGuid +
                   BuildTargetHint(mono);

        if (!string.IsNullOrEmpty(mono.ScriptFileId) && mono.ScriptFileId != "0")
            return "Missing MonoBehaviour fileID:" + mono.ScriptFileId +
                   BuildTargetHint(mono);

        foreach (PersistentCallRecord call in mono.Calls)
        {
            if (!string.IsNullOrEmpty(call.TargetAssemblyTypeName))
                return call.TargetAssemblyTypeName;
        }

        return "Missing MonoBehaviour";
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

    private static string BuildTargetHint(MonoBehaviourRecord mono)
    {
        foreach (PersistentCallRecord call in mono.Calls)
        {
            if (!string.IsNullOrEmpty(call.TargetAssemblyTypeName))
                return " targetType:" + call.TargetAssemblyTypeName;

            if (!string.IsNullOrEmpty(call.TargetGuid))
                return " targetGuid:" + call.TargetGuid;
        }

        return "";
    }

    private static bool IsMissingScript(
        MonoBehaviourRecord mono,
        Dictionary<string, string> scriptNamesByGuid)
    {
        return string.IsNullOrEmpty(mono.ScriptGuid) ||
               mono.ScriptFileId == "0" ||
               !scriptNamesByGuid.ContainsKey(mono.ScriptGuid);
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
