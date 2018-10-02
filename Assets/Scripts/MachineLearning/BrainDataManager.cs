using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class PoseParameter
{
    public int id;
    public int num_joint_detections;
    public List<int> joint_ids;
    public float min_confidence;
}

[Serializable]
public class Poses
{
    public List<PoseParameter> poses;

    public static Poses CreateFromJSON(string jsonString)
    {
        //string jsonString = File.ReadAllText(JsonPath);
        Poses poses_data = JsonUtility.FromJson<Poses>(jsonString);
        return poses_data;
    }
}

public class BrainDataManager : MonoBehaviour
{

    public static BrainDataManager Instance;

    public string brainDataFilePath = "brain_data.json";
    public string trainedDataFolderPath = "MachineLearningModels/TargetPoseRecognizing/TargetPoseTraining_training_"; //Rooted from Resources

    private Academy academy;
    private Brain[] brains;
    public Brain GetBrain(int i)
    {
        return brains[i];
    }

    Poses posesData;
    public PoseParameter GetPoseInfo(int PoseID)
    {
        if(PoseID < NumPoses)
            return posesData.poses[PoseID];
        return null;
    }

    public int NumPoses
    {
        get
        {
            if (posesData == null)
                return 0;
            return posesData.poses.Count;
        }
    }

    void OnEnable()
    {
        academy = GetComponent<Academy>();
        brains = GetComponentsInChildren<Brain>();
        LoadBrainData();
        academy.InitializeEnvironment();
    }

    void Awake ()
    {
        Instance = this;
    }

    private void LoadBrainData()
    {
        string filePath = Application.dataPath + brainDataFilePath;

        AssetBundle ab = AssetBundleManager.Instance.GetAssetBundle(AssetBundles.textdata);
        TextAsset textAsset = ab.LoadAsset<TextAsset>(brainDataFilePath);
        Debug.LogWarning("brain_data: " + textAsset.text);

        if (textAsset != null)
        {
            posesData = Poses.CreateFromJSON(textAsset.text);
        }
        else
        {
            Debug.Log("Brain Data doesn't exist!");
        }

        if (posesData != null)
        {
            for (int i = 0; i < brains.Length; i++)
            {
                brains[i].brainParameters.vectorObservationSize = posesData.poses[i].num_joint_detections * 4;
                TextAsset trained_data = Resources.Load<TextAsset>(trainedDataFolderPath + (i + 1).ToString());
                ((CoreBrain)brains[i].CoreBrains[(int)BrainType.Internal]).SetBrainData(trained_data);
            }
        }
    }
}
