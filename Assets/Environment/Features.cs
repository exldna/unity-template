using System;
using UnityEditor;
using UnityEngine;

namespace Environment {
    [InitializeOnLoad]
    public class Features {
        private const int version = 1;
        private static readonly string version_key = $"{version}_{Application.unityVersion}";

        private static void feature(string key, Action behavior) {
            var installed_version_key = EditorPrefs.GetString(key);
            if (installed_version_key != version_key) {
                behavior();
                EditorPrefs.SetString(key, version_key);
            }
        }

        static Features() {
            git_integration_install_smart_merge();
            git_integration_install_hooks();
        }

        [MenuItem("Tools/Git/install smart merge")]
        private static void git_integration_install_smart_merge() {
            feature("Features.git_integration_install_smart_merge", GitIntegration.install_smart_merge);
        }

        [MenuItem("Tools/Git/install hooks")]
        private static void git_integration_install_hooks() {
            feature("Features.git_integration_install_hooks", GitIntegration.install_hooks);
        }

        [MenuItem("Tools/Manifest/install dependencies")]
        private static void install_dependencies() {
            feature("Features.install_dependencies", DependenciesManager.install_dependencies);
        }

        [MenuItem("Tools/Manifest/hard install dependencies")]
        private static void hard_install_dependencies() {
            DependenciesManager.install_dependencies();
        }
    }
}