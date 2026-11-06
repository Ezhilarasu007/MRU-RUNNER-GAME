#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MRU.InfiniteRunner.Editor
{
    public static class ProjectSetup
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string AdConfigurationPath = "Assets/Resources/MRUAdConfiguration.asset";

        [MenuItem("MRU Runner/1. Generate Main Scene & App Settings")]
        public static void GenerateProject()
        {
            Directory.CreateDirectory("Assets/Scenes");
            EnsureAdConfiguration();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject bootstrap = new GameObject("MRU Runner Bootstrap");
            bootstrap.AddComponent<GameBootstrap>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            // Player Settings Configuration
            PlayerSettings.productName = "MRU Infinite Runner";
            PlayerSettings.companyName = "MRU Games";
            PlayerSettings.applicationIdentifier = "com.mru.infiniterunner";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

            AssetDatabase.SaveAssets();
            Debug.Log("[MRU Setup] Successfully generated " + ScenePath + " and configured player settings!");
        }

        private static void EnsureAdConfiguration()
        {
            if (AssetDatabase.LoadAssetAtPath<AdConfiguration>(AdConfigurationPath) != null) return;
            Directory.CreateDirectory("Assets/Resources");
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AdConfiguration>(), AdConfigurationPath);
        }

        [MenuItem("MRU Runner/2. Build Android APK (.apk)")]
        public static void BuildAndroidApk()
        {
            BuildAndroid(false);
        }

        [MenuItem("MRU Runner/3. Build Android App Bundle (.aab)")]
        public static void BuildAndroidAppBundle()
        {
            BuildAndroid(true);
        }

        [MenuItem("MRU Runner/4. Build Windows Standalone (.exe)")]
        public static void BuildWindowsExe()
        {
            if (!File.Exists(ScenePath)) GenerateProject();

            Directory.CreateDirectory("Build/Windows");
            string outputPath = "Build/Windows/MRU-Infinite-Runner.exe";

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("[MRU Build] Windows build failed: " + report.summary.result);
            }
            else
            {
                Debug.Log("[MRU Build] Windows EXE build SUCCESS! Output: " + outputPath);
            }
        }

        private static void BuildAndroid(bool appBundle)
        {
            if (!File.Exists(ScenePath)) GenerateProject();

            Directory.CreateDirectory("Build/Android");
            EditorUserBuildSettings.buildAppBundle = appBundle;
            string extension = appBundle ? ".aab" : ".apk";
            string outputPath = "Build/Android/MRU-Infinite-Runner-v1.0.0" + extension;

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("[MRU Build] Android build failed: " + report.summary.result + " - " + report.summary.totalErrors + " error(s).");
            }
            else
            {
                Debug.Log("[MRU Build] Android build SUCCESS! Output: " + outputPath + " (" + report.summary.totalSize + " bytes).");
            }
        }
    }
}
#endif
