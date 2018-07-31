using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class ComboData
{
    public int combo_num;
    public float pose_time;
}

[Serializable]
public class Combos
{
    public List<ComboData> combos;

    public static Combos CreateFromJSON(string JsonPath)
    {
        string jsonString = File.ReadAllText(JsonPath);
        Combos combo_data = JsonUtility.FromJson<Combos>(jsonString);
        return combo_data;
    }
}

public class PoseMgr : MonoBehaviour {

    public string comboDataFilePath = "/StreamingAssets/combo_data.json";

    public int curPose = 0;
    public int prevPose = 0;
    public int combo = 0;
    public int ComboNum
    {
        get
        {
            if (ComboInfo == null)
                return 0;
            if (combo > ComboInfo.combos.Count - 1)
                return ComboInfo.combos.Count - 1;
            return combo;
        }
    }

    public float poseTimeEllapsed = 0f;

    Combos ComboInfo;

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
    }

    void Awake ()
    {
        LoadComboData();
    }

    void Start ()
    {
        curPose = 0;
        prevPose = 0;
        combo = 0;

        poseTimeEllapsed = 0f;
    }

    void Update ()
    {
        poseTimeEllapsed += Time.deltaTime;

        if (poseTimeEllapsed > ComboInfo.combos[ComboNum].pose_time)
        {
            GenerateNewPose();
            combo = 0;
        }
    }

    public void OnUserPoseMatched(object param, object paramEx)
    {
        long userID = (long)param;

        if(poseTimeEllapsed <= ComboInfo.combos[ComboNum].pose_time)
        {
            GenerateNewPose();
            combo++;
        }
    }

    private void LoadComboData()
    {
        string filePath = Application.dataPath + comboDataFilePath;

        if (File.Exists(filePath))
        {
            ComboInfo = Combos.CreateFromJSON(filePath);
        }
        else
        {
            Debug.Log("Combo Data doesn't exist!");
        }
    }

    private void GenerateNewPose()
    {
        int newPose = 0;
        do
        {
            newPose = UnityEngine.Random.Range(1, 1 + BrainDataManager.Instance.NumPoses);
        } while (newPose == curPose || newPose == prevPose);

        prevPose = curPose;
        curPose = newPose;
        poseTimeEllapsed = 0f;

        EventMsgDispatcher.Instance.TriggerEvent(EventDef.New_Pose_Generated, curPose);
    }
}
