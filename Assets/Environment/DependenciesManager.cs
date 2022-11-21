using System;
using System.IO;

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

#if UNITY_EDITOR

namespace Environment {
    static class DependenciesManager {
        [Serializable]
        class Dependence {
            public string name;
            public string version;
        }

        [Serializable]
        class Dependencies {
            public Dependence[] list;
        }

        private static AddRequest add_request;
        
        public static void install_dependencies() {
            var packages_dir_path = Path.Combine(System.Environment.CurrentDirectory, "Packages");
            var dependencies_path = Path.Combine(packages_dir_path, "dependencies.json");
            using StreamReader r = new StreamReader(dependencies_path);
            string dependencies_text = r.ReadToEnd();
            var dependencies = new Dependencies();
            EditorJsonUtility.FromJsonOverwrite(dependencies_text, dependencies);
            foreach (var dependence in dependencies.list) {
                add_request = Client.Add(dependence.name + "@" + dependence.version);
                EditorApplication.update += Progress;
                while (!add_request.IsCompleted) {}
            }
        }

        static void Progress() {
            if (add_request.IsCompleted) {
                if (add_request.Status == StatusCode.Success) {
                    Debug.Log("Embedded: " + add_request.Result.packageId);
                } else if (add_request.Status >= StatusCode.Failure) {
                    Debug.Log(add_request.Error.message);
                }

                EditorApplication.update -= Progress;
            }
        }
    }
}

#endif // UNITY_EDITOR