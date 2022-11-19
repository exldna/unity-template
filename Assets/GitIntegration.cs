using System;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[InitializeOnLoad]
class GitIntegration {
    private const int version = 1;
    private static readonly string version_key = $"{version}_{Application.unityVersion}";
    private const string editor_prefs_key_smart_merge_installed = "smart_merge_installed";
    private const string editor_prefs_key_hooks_installed = "githooks_installed";

    static GitIntegration() {
        Debug.Log($"[GitIntegration] initiate in directory {Environment.CurrentDirectory}");
        run_all();
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
            throw new Exception(process.StandardError.ReadLine());
        }

        return process.StandardOutput.ReadLine();
    }

    [MenuItem("Tools/Git/run all")]
    private static void run_all() {
        register_smart_merge();
        register_hooks();
    }

    [MenuItem("Tools/Git/refresh")]
    private static void refresh() {
        EditorPrefs.DeleteKey(editor_prefs_key_smart_merge_installed);
        EditorPrefs.DeleteKey(editor_prefs_key_hooks_installed);
        run_all();
    }

    [MenuItem("Tools/Git/register smart merge")]
    private static void register_smart_merge() {
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
                Debug.Log("[GitIntegration] UnityYAMLMerge registered successful");
            } catch (Exception e) {
                Debug.LogError($"[GitIntegration] Fail to register UnityYAMLMerge with error: {e}");
            }
        } else {
            Debug.Log("[GitIntegration] register UnityYAMLMerge: nothing to do");
        }
    }

    [MenuItem("Tools/Git/register hooks")]
    private static void register_hooks() {
        var installed_version_key = EditorPrefs.GetString(editor_prefs_key_hooks_installed);
        if (installed_version_key != version_key) {
            var hooks_source_path = Path.Combine(Environment.CurrentDirectory, "githooks");
            if (!Directory.Exists(hooks_source_path)) {
                Debug.LogError($"[GitIntegration] hooks source path does not exist ({hooks_source_path})");
                return;
            }

            var hooks_destination_path = Path.Combine(Environment.CurrentDirectory, ".git", "hooks");
            if (!Directory.Exists(hooks_destination_path)) {
                Debug.LogError($"[GitIntegration] hooks destination path does not exist ({hooks_destination_path})");
                return;
            }

            var hooks = Directory.GetFiles(hooks_source_path);

            string hooks_string = "";
            foreach (var hook in hooks) {
                hooks_string += Path.GetFileName(hook) + ", ";
            }

            Debug.Log($"[GitIntegration] copy hooks {hooks_string} to destination {hooks_destination_path}");

            foreach (var hook in hooks) {
                var hook_name = Path.GetFileName(hook);
                var destination = Path.Combine(hooks_destination_path, hook_name);
                if (!File.Exists(destination)) {
                    File.Copy(hook, destination);
                } else {
                    Debug.LogWarning($"[GitIntegration] destination exist {destination}");
                }
            }

            EditorPrefs.SetString(editor_prefs_key_hooks_installed, version_key);
            Debug.Log("[GitIntegration] hooks registered successful");
        } else {
            Debug.Log("[GitIntegration] register hooks: nothing to do");
        }
    }
}

#endif // UNITY_EDITOR