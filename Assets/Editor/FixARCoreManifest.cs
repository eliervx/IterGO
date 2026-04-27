#if UNITY_ANDROID
using System.IO;
using System.IO.Compression;
using UnityEditor.Android;
using UnityEngine;

public class FixARCoreManifest : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 0;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string libsPath = Path.Combine(path, "libs");
        string aarPath = Path.Combine(libsPath, "unityandroidpermissions.aar");

        if (!File.Exists(aarPath))
        {
            Debug.LogWarning("[FixARCoreManifest] unityandroidpermissions.aar not found at: " + aarPath);
            return;
        }

        string tempDir = Path.Combine(Path.GetTempPath(), "unityandroidpermissions_fix");
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);

        // Extraire le .aar
        ZipFile.ExtractToDirectory(aarPath, tempDir);

        // Modifier le AndroidManifest.xml
        string manifestPath = Path.Combine(tempDir, "AndroidManifest.xml");
        if (File.Exists(manifestPath))
        {
            string content = File.ReadAllText(manifestPath);
            content = content.Replace(
                "package=\"com.google.ar.core\"",
                "package=\"com.unity3d.androidpermissions\""
            );
            File.WriteAllText(manifestPath, content);
            Debug.Log("[FixARCoreManifest] AndroidManifest.xml patché avec succès.");
        }

        // Repackager le .aar
        File.Delete(aarPath);
        ZipFile.CreateFromDirectory(tempDir, aarPath);
        Directory.Delete(tempDir, true);

        Debug.Log("[FixARCoreManifest] unityandroidpermissions.aar mis à jour.");
    }
}
#endif