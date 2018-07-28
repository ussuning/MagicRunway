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
}

[Serializable]
public class Poses
{
    public List<PoseParameter> poses;

    public static Poses CreateFromJSON(string JsonPath)
    {
        string jsonString = File.ReadAllText(JsonPath);
        Poses poses_data = JsonUtility.FromJson<Poses>(jsonString);
        return poses_data;
    }
}

public class BrainDataManager : MonoBehaviour
{

    public static BrainDataManager Instance;

    public string poseDataFilePath = "/StreamingAssets/pose_data.json";
    public string trainedDataFolderPath = "MachineLearningModels/TargetPoseRecognizing/TargetPoseTraining_training_"; //Rooted from Resources

    private Brain[] brains;
    public Brain GetBrain(int i)
    {
        return brains[i];
    }

    Poses posesData;
    public PoseParameter GetPoseInfo(int PoseID)
    {
        return posesData.poses[PoseID];
    }

    public int NumPoses
    {
        get
        {
            return posesData.poses.Count;
        }
    }

    void OnEnable()
    {
        brains = GetComponentsInChildren<Brain>();
        LoadBrainData();
    }

    void Awake ()
    {
        Instance = this;
    }

    private void LoadBrainData()
    {
        string filePath = Application.dataPath + poseDataFilePath;

        if (File.Exists(filePath))
        {
            posesData = Poses.CreateFromJSON(filePath);
        }
        else
        {
            Debug.Log("Brain Data doesn't exist!");
        }

        if (posesData != null)
        {
            for (int i = 0; i < brains.Length; i++)
            {
                brains[i].brainParameters.vectorObservationSize = posesData.poses[i].num_joint_detections * 3;
                //TextAsset trained_data = Resources.Load<TextAsset>(trainedDataFolderPath + (i + 1).ToString());
                TextAsset trained_data = Resources.Load<TextAsset>(trainedDataFolderPath + 1.ToString());
                ((CoreBrain)brains[i].CoreBrains[(int)BrainType.Internal]).SetBrainData(trained_data);
            }
        }
    }
}
