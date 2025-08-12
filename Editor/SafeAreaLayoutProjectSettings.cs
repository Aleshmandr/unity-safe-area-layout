using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.SafeAreaLayout.Editor
{
    public static class SafeAreaLayoutProjectSettings
    {
        private static SerializedObject _cachedSerializedObject;

        [SettingsProvider]
        public static SettingsProvider CreateMyPackageSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Safe Area Layout", SettingsScope.Project)
            {
                label = "Safe Area Layout",
                guiHandler = (_) =>
                {
                    SafeAreaLayoutConfig config = SafeAreaLayoutProjectConfigProvider.Config;
                    if (config == null)
                    {
                        config = CreateProjectConfig(SafeAreaLayoutProjectConfigConstants.ResourceName);
                    }

                    if (_cachedSerializedObject == null || _cachedSerializedObject.targetObject != config)
                    {
                        _cachedSerializedObject = new SerializedObject(config);
                    }

                    _cachedSerializedObject.Update();

                    EditorGUILayout.PropertyField(_cachedSerializedObject.FindProperty(nameof(config.TopMargin)));
                    EditorGUILayout.PropertyField(_cachedSerializedObject.FindProperty(nameof(config.BottomMargin)));
                    EditorGUILayout.PropertyField(_cachedSerializedObject.FindProperty(nameof(config.LeftMargin)));
                    EditorGUILayout.PropertyField(_cachedSerializedObject.FindProperty(nameof(config.RightMargin)));

                    _cachedSerializedObject.ApplyModifiedProperties();
                }
            };

            return provider;
        }

        private static SafeAreaLayoutConfig CreateProjectConfig(string configResourceName)
        {
            // Ensure Resources folder exists
            string dir = Path.GetDirectoryName(SafeAreaLayoutProjectConfigConstants.ProjectPath)?.Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(dir))
            {
                // Create nested folders if needed (e.g., Assets/Resources)
                if (dir != null)
                {
                    string[] parts = dir.Split('/');
                    string current = parts[0]; // "Assets"
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string next = current + "/" + parts[i];
                        if (!AssetDatabase.IsValidFolder(next))
                        {
                            AssetDatabase.CreateFolder(current, parts[i]);
                        }

                        current = next;
                    }
                }
            }

            var config = ScriptableObject.CreateInstance<SafeAreaLayoutConfig>();
            config.name = Path.GetFileNameWithoutExtension(configResourceName);
            AssetDatabase.CreateAsset(config, SafeAreaLayoutProjectConfigConstants.ProjectPath);
            AssetDatabase.SaveAssets();
            return config;
        }
    }
}