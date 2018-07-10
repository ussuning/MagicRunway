using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class WearablesDataEditor : EditorWindow
{
    public Wearables gameData;
    private string gameDataProjectFilePath = "/StreamingAssets/wearables.json";

    [MenuItem("Window/Wearables Data Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(WearablesDataEditor)).Show();
    }

    void OnGUI()
    {
        if (gameData != null) {
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty serializedProperty = serializedObject.FindProperty("gameData");
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Save Wearables")) {
                SaveGameData();
            }
        }

        if (GUILayout.Button("Load Wearables")) { 
            LoadGameData();
        }
    }

    private void LoadGameData()
    {
        string filePath = Application.dataPath + gameDataProjectFilePath;

        if (File.Exists(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);
            gameData = JsonUtility.FromJson<Wearables>(dataAsJson);
        }
        else { 
            gameData = new Wearables();
        }
    }

    private void SaveGameData()
    {
        string dataAsJson = JsonUtility.ToJson(gameData);
        string filePath = Application.dataPath + gameDataProjectFilePath;
        File.WriteAllText(filePath, dataAsJson);
    }
}
