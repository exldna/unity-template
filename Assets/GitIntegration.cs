using System;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[InitializeOnLoad]
class GitIntegration {
    private const int version = 1;
    private static readonly string version_key = $"{version}_{Application.unityVersion}";
    private const string editor_prefs_key_smart_merge_installed = "smart_merge_installed";

    static GitIntegration() {
        Debug.Log("initiate git integration");
        RegisterSmartMerge();
    }

    [MenuItem("Tools/Git/register smart merge")]
    private static void RegisterSmartMerge() {
        var installed_version_key = EditorPrefs.GetString(editor_prefs_key_smart_merge_installed);
        if (installed_version_key != version_key) {
            try {
                var unity_merge_tool_path = EditorApplication.applicationContentsPath + "\\Tools\\UnityYAMLMerge.exe";
                command("config merge.unityyamlmerge.name \"Unity SmartMerge (UnityYamlMerge)\"");
                string line = "config merge.unityyamlmerge.driver";
                line += $" \"\\\"{unity_merge_tool_path}\\\"";
                line += " merge -h -p --force --fallback none %O %B %A %A\"";
                command(line);
                command("config merge.unityyamlmerge.recursive binary");
                EditorPrefs.SetString(editor_prefs_key_smart_merge_installed, version_key);
                Debug.Log("UnityYAMLMerge registration was successful");
            } catch (Exception e) {
                Debug.Log($"Fail to register UnityYAMLMerge with error: {e}");
            }
        } else {
            Debug.Log("UnityYAMLMerge registration: nothing to do");
        }
    }

    public static string command(string line) {
        var git_process_start_info = new System.Diagnostics.ProcessStartInfo("git");
        var process = new System.Diagnostics.Process();
        
        git_process_start_info.UseShellExecute = false;
        git_process_start_info.WorkingDirectory = Environment.CurrentDirectory;
        git_process_start_info.RedirectStandardOutput = true;
        git_process_start_info.RedirectStandardError = true;

        process.StartInfo = git_process_start_info;
        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = line;

        process.Start();
        process.WaitForExit();
        if (process.ExitCode != 0) {
            throw new System.Exception(process.StandardError.ReadLine());
        }

        return process.StandardOutput.ReadLine();
    }
}

#endif // UNITY_EDITOR