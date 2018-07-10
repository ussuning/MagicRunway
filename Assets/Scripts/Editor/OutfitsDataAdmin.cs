using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class OutfitsDataEditor : EditorWindow {

    public Outfits gameData;

    private string gameDataProjectFilePath = "/StreamingAssets/outfits.json";

    [MenuItem("Window/Outfits Data Admin Panel")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(OutfitsDataEditor)).Show();
    }

    void OnGUI()
    {
        if (gameData != null)
        {
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty serializedProperty = serializedObject.FindProperty("gameData");
            EditorGUILayout.PropertyField(serializedProperty, true);

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Save data"))
            {
                SaveGameData();
            }
        }

        if (GUILayout.Button("Load data"))
        {
            LoadGameData();
        }
    }

    private void LoadGameData()
    {
        string filePath = Application.dataPath + gameDataProjectFilePath;

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            gameData = JsonUtility.FromJson<Outfits>(dataAsJson);
        }
        else
        {
            gameData = new Outfits();
        }
    }

    private void SaveGameData()
    {

        string dataAsJson = JsonUtility.ToJson(gameData);

        string filePath = Application.dataPath + gameDataProjectFilePath;
        File.WriteAllText(filePath, dataAsJson);

    }
}
