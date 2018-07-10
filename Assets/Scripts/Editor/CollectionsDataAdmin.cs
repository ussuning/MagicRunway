using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CollectionsDataEditor : EditorWindow {

    public Collections gameData;
    private string gameDataProjectFilePath = "/StreamingAssets/collections.json";

    [MenuItem("Window/Collections Data Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CollectionsDataEditor)).Show();
    }

    void OnGUI()
    {
        if (gameData != null) { 
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty serializedProperty = serializedObject.FindProperty("gameData");
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Save Collections")) {
                SaveGameData();
            }
        }
        if (GUILayout.Button("Load Collections")) {
            LoadGameData();
        }
    }

    private void LoadGameData()
    {
        string filePath = Application.dataPath + gameDataProjectFilePath;

        if (File.Exists(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);
            gameData = JsonUtility.FromJson<Collections>(dataAsJson);
        }
        else {
            gameData = new Collections();
        }
    }

    private void SaveGameData()
    {
        string dataAsJson = JsonUtility.ToJson(gameData);
        string filePath = Application.dataPath + gameDataProjectFilePath;
        File.WriteAllText(filePath, dataAsJson);
    }
}
