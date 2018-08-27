// C# example.
using UnityEditor;
using System.Diagnostics;
using System;

public class ScriptBatch
{
    [MenuItem("Build Tools/Build StandaloneWindows64")]
    public static void BuildGame()
    {
        // Get filename.
        string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        string[] levels = new string[] { "Assets/Scenes/Main.unity" };

        // Move Files that can't compile with Windows.
        string[] excludeFiles = new string[]
        {
            "Assets/Obi/Resources/ObiMaterials/ParticleShader.shader"
        };

        foreach (string excludeFile in excludeFiles)
        {
            string fileName = ExtractFileName(excludeFile);
            UnityEngine.Debug.Log("Moving "+ excludeFile + " to Temp...");

            // Delete it from Temp just in case there's an old one there.
            FileUtil.DeleteFileOrDirectory("Temp/" + fileName);
            FileUtil.MoveFileOrDirectory(excludeFile, "Temp/" + fileName);
        }

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + "/MagicRunway.exe", BuildTarget.StandaloneWindows64, BuildOptions.ShowBuiltPlayer);

        // Restore removed files that couldn't compile with Windows.

        foreach (string excludeFile in excludeFiles)
        {
            string fileName = ExtractFileName(excludeFile);
            UnityEngine.Debug.Log("Restoring " + excludeFile + " from Temp...");
            FileUtil.MoveFileOrDirectory("Temp/" + fileName, excludeFile);
        }

        // Run the game (Process class from System.Diagnostics).
        //Process proc = new Process();
        //proc.StartInfo.FileName = path + "/MagicRunway.exe";
        //proc.Start();
    }

    protected static string ExtractFileName(string fullFileName)
    {
        string[] splitStrings = fullFileName.Split('/');
        string fileName = splitStrings[splitStrings.Length - 1];
        return fileName;
    }
}