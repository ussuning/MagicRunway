using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class BrainData
{
    public int observation_size;
    public string trained_data_name;

    public static BrainData CreateFromJSON(string JsonPath)
    {
        string jsonString = File.ReadAllText(JsonPath);
        BrainData bd = JsonUtility.FromJson<BrainData>(jsonString);
        return bd;
    }
}

public class BrainInitializer : MonoBehaviour {

    private string brainDataFilePath = "/StreamingAssets/brain_data.json";

    private Brain brain;

    BrainData brainData;
    TextAsset trained_data;

    void OnEnable()
    {
        if (!brain)
            brain = GetComponentInChildren<Brain>();
        LoadBrainData();
    }

    private void LoadBrainData()
    {
        string filePath = Application.dataPath + brainDataFilePath;

        if (File.Exists(filePath))
        {
            brainData = BrainData.CreateFromJSON(filePath);
            trained_data = Resources.Load<TextAsset>("MachineLearningModels/" + brainData.trained_data_name);
            ((CoreBrain)brain.CoreBrains[(int)BrainType.Internal]).SetBrainData(trained_data);
            brain.brainParameters.vectorObservationSize = brainData.observation_size;
        }
        else
        {
            Debug.Log("Brain Data doesn't exist!");
        }
    }
}
